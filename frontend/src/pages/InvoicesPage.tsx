import { useState, useMemo, useRef } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { invoicesApi, workOrdersApi } from '../services/api'
import { InvoiceDto, WorkOrderDto, CustomerDto, LocationDto } from '../types'
import { format } from 'date-fns'
import { useCustomers } from '../hooks/useCustomers'
import { useLocations } from '../hooks/useLocations'
import jsPDF from 'jspdf'
import { getWorkOrderTotal, formatCurrency } from '../utils/workOrderUtils'
import { InvoicePreview } from '../components/InvoicePreview'

export default function InvoicesPage() {
  const [pageNumber, setPageNumber] = useState(1)
  const [statusFilter, setStatusFilter] = useState<string>('all')
  const [showCreateModal, setShowCreateModal] = useState(false)
  const [selectedWorkOrderId, setSelectedWorkOrderId] = useState<string>('')
  const [createdInvoice, setCreatedInvoice] = useState<InvoiceDto | null>(null)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)
  const printRef = useRef<HTMLDivElement>(null)
  const navigate = useNavigate()
  const queryClient = useQueryClient()

  // Fetch lookups
  const { data: customers = [], isLoading: customersLoading } = useCustomers()
  const { data: locations = [], isLoading: locationsLoading } = useLocations()

  // Create memoized lookup maps
  const customerById = useMemo(
    () => new Map(customers.map((c: CustomerDto) => [c.id, `${c.firstName} ${c.lastName}`])),
    [customers]
  )
  const locationById = useMemo(() => new Map(locations.map((l: LocationDto) => [l.id, l.name])), [locations])

  const { data: invoicesData, isLoading: invoicesLoading } = useQuery({
    queryKey: ['invoices', pageNumber, statusFilter],
    queryFn: () =>
      invoicesApi.getInvoices({
        pageNumber,
        pageSize: 10,
        status: statusFilter === 'all' ? undefined : statusFilter,
      }),
  })

  // Fetch all work orders and filter client-side for "Completed" status (case-insensitive)
  const { data: workOrdersData, isLoading: workOrdersLoading } = useQuery({
    queryKey: ['workorders', 'all'],
    queryFn: () => workOrdersApi.getWorkOrders({ pageSize: 1000 }),
  })

  // Filter completed work orders client-side (case-insensitive)
  const completedWorkOrders = useMemo(() => {
    if (!workOrdersData?.data?.data?.items) return []
    return workOrdersData.data.data.items.filter(
      (wo: WorkOrderDto) => wo.status?.toLowerCase() === 'completed'
    )
  }, [workOrdersData])

  const lookupsLoading = customersLoading || locationsLoading

  const createInvoiceMutation = useMutation({
    mutationFn: invoicesApi.createInvoiceFromWorkOrder,
    onSuccess: async (response) => {
      const invoiceData = response.data?.data
      if (invoiceData) {
        setCreatedInvoice(invoiceData)
        setErrorMessage(null)
        queryClient.invalidateQueries({ queryKey: ['invoices'] })
        setShowCreateModal(false)
        setSelectedWorkOrderId('')
      } else {
        setErrorMessage('Invoice was created but no data was returned. Please refresh the page.')
      }
    },
    onError: (err: any) => {
      console.error('❌ Failed to create invoice:', err)
      let errorMsg = 'Failed to create invoice. Please try again.'
      
      if (err.response) {
        const apiResponse = err.response.data
        if (apiResponse?.message) {
          errorMsg = apiResponse.message
        } else if (apiResponse?.errors && Array.isArray(apiResponse.errors) && apiResponse.errors.length > 0) {
          errorMsg = apiResponse.errors.join(', ')
        } else if (err.response.status === 400) {
          errorMsg = 'Invalid request. Please check the work order selection.'
        } else if (err.response.status === 404) {
          errorMsg = 'Work order not found. It may have been deleted.'
        } else if (err.response.status === 409) {
          errorMsg = 'An invoice already exists for this work order.'
        }
      } else if (err.request) {
        errorMsg = 'Unable to connect to server. Please check your connection.'
      }
      
      setErrorMessage(errorMsg)
      setCreatedInvoice(null)
    },
  })

  const invoices = invoicesData?.data?.data?.items || []
  const totalPages = invoicesData?.data?.data?.totalPages || 0

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Paid':
        return 'bg-green-100 text-green-800'
      case 'PartiallyPaid':
        return 'bg-yellow-100 text-yellow-800'
      case 'Overdue':
        return 'bg-red-100 text-red-800'
      default:
        return 'bg-gray-100 text-gray-800'
    }
  }

  const getWorkOrderStatusColor = (status: string) => {
    const normalizedStatus = status?.toLowerCase() || ''
    switch (normalizedStatus) {
      case 'completed':
        return 'bg-green-100 text-green-800'
      case 'cancelled':
        return 'bg-red-100 text-red-800'
      case 'inprogress':
        return 'bg-blue-100 text-blue-800'
      case 'onhold':
        return 'bg-yellow-100 text-yellow-800'
      default:
        return 'bg-gray-100 text-gray-800'
    }
  }

  const handleCreateInvoice = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault()
    if (!selectedWorkOrderId) {
      alert('Please select a work order')
      return
    }
    createInvoiceMutation.mutate(selectedWorkOrderId)
  }

  const handleDownloadPdf = async (invoice: InvoiceDto) => {
    try {
      // Try backend PDF endpoint first
      try {
        const blob = await invoicesApi.downloadInvoicePdf(invoice.id)
        // Check if blob is actually a PDF (backend might return HTML)
        const blobType = blob.type || ''
        if (blobType.includes('pdf') || blob.size > 1000) {
          const url = window.URL.createObjectURL(blob)
          const link = document.createElement('a')
          link.href = url
          link.download = `Invoice-${invoice.invoiceNumber}.pdf`
          document.body.appendChild(link)
          link.click()
          document.body.removeChild(link)
          window.URL.revokeObjectURL(url)
          return
        }
        // If blob is HTML (backend returns HTML), fall through to print
      } catch (backendError: any) {
        console.warn('Backend PDF download failed, using fallback:', backendError)
        // Fall through to frontend generation
      }

      // Fallback 1: Use jsPDF for PDF generation
      try {
        const pdf = new jsPDF()
        const pageWidth = pdf.internal.pageSize.getWidth()
        const margin = 20
        let yPos = margin
        pdf.setFontSize(20)
        pdf.text('INVOICE', pageWidth / 2, yPos, { align: 'center' })
        yPos += 10
        pdf.setFontSize(12)
        pdf.text(`Invoice #: ${invoice.invoiceNumber}`, pageWidth / 2, yPos, { align: 'center' })
        yPos += 20
        pdf.setFontSize(10)
        pdf.text(`Date: ${format(new Date(invoice.invoiceDate), 'MMM dd, yyyy')}`, margin, yPos)
        if (invoice.dueDate) {
          yPos += 7
          pdf.text(`Due Date: ${format(new Date(invoice.dueDate), 'MMM dd, yyyy')}`, margin, yPos)
        }
        yPos += 7
        pdf.text(`Status: ${invoice.status}`, margin, yPos)
        yPos += 15
        pdf.setFontSize(12)
        pdf.text('Bill To:', margin, yPos)
        yPos += 7
        pdf.setFontSize(10)
        pdf.text(invoice.customerName ?? 'N/A', margin, yPos)
        yPos += 20
        pdf.setFontSize(10)
        pdf.setFont(undefined, 'bold')
        pdf.text('Description', margin, yPos)
        pdf.text('Qty', margin + 80, yPos)
        pdf.text('Unit Price', margin + 100, yPos)
        pdf.text('Total', pageWidth - margin - 20, yPos, { align: 'right' })
        yPos += 7
        pdf.setDrawColor(200, 200, 200)
        pdf.line(margin, yPos, pageWidth - margin, yPos)
        yPos += 5
        pdf.setFont(undefined, 'normal')
        invoice.items?.forEach((item) => {
          if (yPos > 250) {
            pdf.addPage()
            yPos = margin
          }
          pdf.text(item.description ?? 'N/A', margin, yPos)
          pdf.text(item.quantity.toString(), margin + 80, yPos)
          pdf.text(`$${item.unitPrice.toFixed(2)}`, margin + 100, yPos)
          pdf.text(`$${item.totalAmount.toFixed(2)}`, pageWidth - margin - 20, yPos, { align: 'right' })
          yPos += 7
        })
        yPos += 10
        pdf.line(margin, yPos, pageWidth - margin, yPos)
        yPos += 10
        pdf.setFont(undefined, 'bold')
        pdf.text(`Subtotal: $${invoice.subTotal.toFixed(2)}`, pageWidth - margin - 20, yPos, { align: 'right' })
        yPos += 7
        if (invoice.discountAmount > 0) {
          pdf.setFont(undefined, 'normal')
          pdf.text(`Discount: $${invoice.discountAmount.toFixed(2)}`, pageWidth - margin - 20, yPos, { align: 'right' })
          yPos += 7
        }
        pdf.setFont(undefined, 'normal')
        pdf.text(`Tax: $${invoice.taxAmount.toFixed(2)}`, pageWidth - margin - 20, yPos, { align: 'right' })
        yPos += 7
        pdf.setFont(undefined, 'bold')
        pdf.setFontSize(12)
        pdf.text(`Total: $${invoice.totalAmount.toFixed(2)}`, pageWidth - margin - 20, yPos, { align: 'right' })
        yPos += 10
        pdf.setFontSize(10)
        pdf.setFont(undefined, 'normal')
        pdf.text(`Paid: $${invoice.paidAmount.toFixed(2)}`, pageWidth - margin - 20, yPos, { align: 'right' })
        yPos += 7
        pdf.setFont(undefined, 'bold')
        pdf.text(
          `Balance: $${(invoice.totalAmount - invoice.paidAmount).toFixed(2)}`,
          pageWidth - margin - 20,
          yPos,
          { align: 'right' }
        )
        pdf.save(`Invoice-${invoice.invoiceNumber}.pdf`)
        return
      } catch (pdfError) {
        console.warn('jsPDF generation failed, using print fallback:', pdfError)
        // Fall through to print
      }

      // Fallback 2: Print/Save as PDF using browser print
      handlePrintInvoice(invoice)
    } catch (err) {
      console.error('❌ Failed to download PDF:', err)
      alert('Failed to download PDF. Please try the Print option instead.')
    }
  }

  const handlePrintInvoice = (invoice: InvoiceDto) => {
    // Create a new window with the invoice preview
    const printWindow = window.open('', '_blank')
    if (!printWindow) {
      alert('Please allow popups to print the invoice.')
      return
    }

    printWindow.document.write(`
      <!DOCTYPE html>
      <html>
        <head>
          <title>Invoice ${invoice.invoiceNumber}</title>
          <style>
            body { font-family: Arial, sans-serif; margin: 40px; }
            .header { text-align: center; margin-bottom: 30px; }
            .invoice-info { display: flex; justify-content: space-between; margin-bottom: 30px; }
            .info-section { flex: 1; }
            table { width: 100%; border-collapse: collapse; margin-bottom: 20px; }
            th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }
            th { background-color: #f2f2f2; }
            .total-section { text-align: right; margin-top: 20px; }
            .total-row { margin: 5px 0; }
            @media print {
              body { margin: 0; }
              .no-print { display: none; }
            }
          </style>
        </head>
        <body>
          <div class="header">
            <h1>INVOICE</h1>
            <p>Invoice #: ${invoice.invoiceNumber}</p>
          </div>
          <div class="invoice-info">
            <div class="info-section">
              <h3>Bill To:</h3>
              <p>${invoice.customerName || 'N/A'}</p>
            </div>
            <div class="info-section">
              <h3>Invoice Details:</h3>
              <p>Date: ${format(new Date(invoice.invoiceDate), 'MMM dd, yyyy')}</p>
              ${invoice.dueDate ? `<p>Due Date: ${format(new Date(invoice.dueDate), 'MMM dd, yyyy')}</p>` : ''}
              <p>Status: ${invoice.status}</p>
            </div>
          </div>
          <table>
            <thead>
              <tr>
                <th>Description</th>
                <th>Quantity</th>
                <th>Unit Price</th>
                <th>Total</th>
              </tr>
            </thead>
            <tbody>
              ${invoice.items?.map(item => `
                <tr>
                  <td>${item.description || 'N/A'}</td>
                  <td>${item.quantity}</td>
                  <td>$${item.unitPrice.toFixed(2)}</td>
                  <td>$${item.totalAmount.toFixed(2)}</td>
                </tr>
              `).join('') || '<tr><td colSpan="4">No items</td></tr>'}
            </tbody>
          </table>
          <div class="total-section">
            <div class="total-row"><strong>Subtotal: $${invoice.subTotal.toFixed(2)}</strong></div>
            ${invoice.discountAmount > 0 ? `<div class="total-row">Discount: $${invoice.discountAmount.toFixed(2)}</div>` : ''}
            <div class="total-row">Tax: $${invoice.taxAmount.toFixed(2)}</div>
            <div class="total-row"><strong>Total: $${invoice.totalAmount.toFixed(2)}</strong></div>
            <div class="total-row">Paid: $${invoice.paidAmount.toFixed(2)}</div>
            <div class="total-row"><strong>Balance: $${(invoice.totalAmount - invoice.paidAmount).toFixed(2)}</strong></div>
          </div>
        </body>
      </html>
    `)
    printWindow.document.close()
    printWindow.focus()
    setTimeout(() => {
      printWindow.print()
    }, 250)
  }

  return (
    <div className="p-6">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold text-gray-900">Invoices</h1>
        <button
          onClick={() => setShowCreateModal(true)}
          className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
        >
          Create Invoice
        </button>
      </div>

      <div className="mb-4 flex gap-2">
        <button
          onClick={() => setStatusFilter('all')}
          className={`px-4 py-2 rounded-lg ${
            statusFilter === 'all' ? 'bg-blue-600 text-white' : 'bg-gray-100 text-gray-700'
          }`}
        >
          All
        </button>
        <button
          onClick={() => setStatusFilter('Draft')}
          className={`px-4 py-2 rounded-lg ${
            statusFilter === 'Draft' ? 'bg-blue-600 text-white' : 'bg-gray-100 text-gray-700'
          }`}
        >
          Draft
        </button>
        <button
          onClick={() => setStatusFilter('Sent')}
          className={`px-4 py-2 rounded-lg ${
            statusFilter === 'Sent' ? 'bg-blue-600 text-white' : 'bg-gray-100 text-gray-700'
          }`}
        >
          Sent
        </button>
        <button
          onClick={() => setStatusFilter('PartiallyPaid')}
          className={`px-4 py-2 rounded-lg ${
            statusFilter === 'PartiallyPaid' ? 'bg-blue-600 text-white' : 'bg-gray-100 text-gray-700'
          }`}
        >
          Partially Paid
        </button>
        <button
          onClick={() => setStatusFilter('Paid')}
          className={`px-4 py-2 rounded-lg ${
            statusFilter === 'Paid' ? 'bg-blue-600 text-white' : 'bg-gray-100 text-gray-700'
          }`}
        >
          Paid
        </button>
      </div>

      {invoicesLoading ? (
        <div className="text-center py-12">Loading invoices...</div>
      ) : invoices.length === 0 ? (
        <div className="text-center py-12 text-gray-500">No invoices found</div>
      ) : (
        <>
          <div className="bg-white rounded-lg shadow overflow-hidden">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Invoice #
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Date
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Due Date
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Amount
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Paid
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Status
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {invoices.map((invoice: InvoiceDto) => (
                  <tr key={invoice.id}>
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                      {invoice.invoiceNumber}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      {format(new Date(invoice.invoiceDate), 'MMM dd, yyyy')}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      {invoice.dueDate ? format(new Date(invoice.dueDate), 'MMM dd, yyyy') : '-'}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                      ${invoice.totalAmount.toFixed(2)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      ${invoice.paidAmount.toFixed(2)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className={`px-2 py-1 text-xs rounded-full ${getStatusColor(invoice.status)}`}>
                        {invoice.status}
                      </span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                      <button
                        onClick={() => invoice && handleDownloadPdf(invoice)}
                        className="text-blue-600 hover:text-blue-900 mr-4"
                      >
                        Download PDF
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {totalPages > 1 && (
            <div className="mt-4 flex justify-center gap-2">
              <button
                onClick={() => setPageNumber((p) => Math.max(1, p - 1))}
                disabled={pageNumber === 1}
                className="px-4 py-2 border rounded disabled:opacity-50"
              >
                Previous
              </button>
              <span className="px-4 py-2">
                Page {pageNumber} of {totalPages}
              </span>
              <button
                onClick={() => setPageNumber((p) => Math.min(totalPages, p + 1))}
                disabled={pageNumber === totalPages}
                className="px-4 py-2 border rounded disabled:opacity-50"
              >
                Next
              </button>
            </div>
          )}
        </>
      )}

      {showCreateModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 w-full max-w-md">
            <h2 className="text-xl font-bold mb-4">Create Invoice from Work Order</h2>
            
            {/* Error Message */}
            {errorMessage && (
              <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-md">
                <p className="text-sm text-red-800">{errorMessage}</p>
                <button
                  onClick={() => setErrorMessage(null)}
                  className="mt-2 text-xs text-red-600 hover:text-red-800 underline"
                >
                  Dismiss
                </button>
              </div>
            )}

            {workOrdersLoading ? (
              <div className="text-center py-8">Loading work orders...</div>
            ) : completedWorkOrders.length === 0 ? (
              <div className="text-center py-8">
                <p className="text-gray-500 mb-4">No completed work orders. Complete one first from Work Orders page.</p>
                <button
                  onClick={() => {
                    setShowCreateModal(false)
                    setErrorMessage(null)
                    navigate('/workorders')
                  }}
                  className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
                >
                  Go to Work Orders
                </button>
              </div>
            ) : (
              <form onSubmit={handleCreateInvoice}>
                <div className="mb-4">
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Select Work Order *
                  </label>
                  <select
                    required
                    value={selectedWorkOrderId}
                    onChange={(e) => {
                      setSelectedWorkOrderId(e.target.value)
                      setErrorMessage(null)
                    }}
                    className="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  >
                    <option value="">Select a work order</option>
                    {completedWorkOrders.map((wo: WorkOrderDto) => {
                      const customerName = lookupsLoading
                        ? 'Loading...'
                        : wo.customerId
                        ? customerById.get(wo.customerId) ?? wo.customerName ?? 'Unknown'
                        : wo.customerName ?? 'Unknown'
                      const locationName = lookupsLoading
                        ? 'Loading...'
                        : wo.locationId
                        ? locationById.get(wo.locationId) ?? 'Unknown'
                        : 'Unknown'
                      // Calculate total: getWorkOrderTotal handles totalAmount correctly
                      // If totalAmount is 0 or missing, it returns 0
                      // If totalAmount exists and > 0, it returns that value
                      const total = getWorkOrderTotal(wo)
                      return (
                        <option key={wo.id} value={wo.id}>
                          {wo.workOrderNumber} - {customerName} - {formatCurrency(total)} ({format(new Date(wo.createdAt), 'MMM dd, yyyy')})
                        </option>
                      )
                    })}
                  </select>
                </div>

                {selectedWorkOrderId && (
                  <div className="mb-4 p-3 bg-gray-50 rounded-md">
                    {(() => {
                      const wo = completedWorkOrders.find((w: WorkOrderDto) => w.id === selectedWorkOrderId)
                      if (!wo) return null
                      // Use getWorkOrderTotal for consistency
                      const total = getWorkOrderTotal(wo)
                      return (
                        <div className="text-sm text-gray-600">
                          <div className="grid grid-cols-2 gap-2">
                            <div>
                              <span className="font-medium">Customer:</span>{' '}
                              {lookupsLoading
                                ? 'Loading...'
                                : wo.customerId
                                ? customerById.get(wo.customerId) ?? wo.customerName ?? 'N/A'
                                : wo.customerName ?? 'N/A'}
                            </div>
                            <div>
                              <span className="font-medium">Location:</span>{' '}
                              {lookupsLoading
                                ? 'Loading...'
                                : wo.locationId
                                ? locationById.get(wo.locationId) ?? '-'
                                : '-'}
                            </div>
                            <div>
                              <span className="font-medium">Created:</span>{' '}
                              {format(new Date(wo.createdAt), 'MMM dd, yyyy')}
                            </div>
                            <div>
                              <span className="font-medium">Total:</span>{' '}
                              <span className="font-semibold text-gray-900">{formatCurrency(total)}</span>
                            </div>
                          </div>
                          {wo.description && (
                            <div className="mt-2">
                              <span className="font-medium">Description:</span> {wo.description}
                            </div>
                          )}
                        </div>
                      )
                    })()}
                  </div>
                )}

                <div className="flex gap-2">
                  <button
                    type="button"
                    onClick={() => {
                      setShowCreateModal(false)
                      setSelectedWorkOrderId('')
                      setErrorMessage(null)
                    }}
                    className="flex-1 px-4 py-2 border rounded-lg hover:bg-gray-50"
                  >
                    Cancel
                  </button>
                  <button
                    type="submit"
                    disabled={createInvoiceMutation.isPending || !selectedWorkOrderId}
                    className="flex-1 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    {createInvoiceMutation.isPending ? 'Creating...' : 'Create Invoice'}
                  </button>
                </div>
              </form>
            )}
          </div>
        </div>
      )}

      {/* Success Modal with PDF Download */}
      {createdInvoice && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 w-full max-w-2xl max-h-[90vh] overflow-y-auto">
            <h2 className="text-xl font-bold mb-4 text-green-600">Invoice Created Successfully!</h2>
            
            {/* Invoice Details */}
            <div className="mb-6 p-4 bg-gray-50 rounded-md">
              <div className="grid grid-cols-2 gap-4 text-sm">
                <div>
                  <span className="font-medium text-gray-700">Invoice Number:</span>
                  <p className="text-gray-900">{createdInvoice.invoiceNumber || '—'}</p>
                </div>
                <div>
                  <span className="font-medium text-gray-700">Date:</span>
                  <p className="text-gray-900">{format(new Date(createdInvoice.invoiceDate), 'MMM dd, yyyy')}</p>
                </div>
                {createdInvoice.dueDate && (
                  <div>
                    <span className="font-medium text-gray-700">Due Date:</span>
                    <p className="text-gray-900">{format(new Date(createdInvoice.dueDate), 'MMM dd, yyyy')}</p>
                  </div>
                )}
                <div>
                  <span className="font-medium text-gray-700">Status:</span>
                  <p className="text-gray-900">{createdInvoice.status}</p>
                </div>
                <div>
                  <span className="font-medium text-gray-700">Customer:</span>
                  <p className="text-gray-900">{createdInvoice.customerName || 'N/A'}</p>
                </div>
                <div>
                  <span className="font-medium text-gray-700">Total Amount:</span>
                  <p className="text-gray-900 font-semibold">
                    {formatCurrency(createdInvoice.totalAmount || 0)}
                  </p>
                </div>
              </div>
            </div>

            {/* Action Buttons */}
            <div className="flex gap-2">
              <button
                onClick={() => setCreatedInvoice(null)}
                className="flex-1 px-4 py-2 border rounded-lg hover:bg-gray-50"
              >
                Close
              </button>
              <button
                onClick={() => {
                  if (createdInvoice) {
                    handleDownloadPdf(createdInvoice)
                  }
                }}
                className="flex-1 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
              >
                Download PDF
              </button>
              <button
                onClick={() => {
                  if (createdInvoice) {
                    handlePrintInvoice(createdInvoice)
                  }
                }}
                className="flex-1 px-4 py-2 bg-gray-600 text-white rounded-lg hover:bg-gray-700"
              >
                Print / Save as PDF
              </button>
            </div>

            {/* Hidden Invoice Preview for Print */}
            <div ref={printRef} className="hidden">
              {createdInvoice && <InvoicePreview invoice={createdInvoice} />}
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
