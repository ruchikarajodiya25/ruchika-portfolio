import { InvoiceDto } from '../types'
import { format } from 'date-fns'

interface InvoicePreviewProps {
  invoice: InvoiceDto
}

export function InvoicePreview({ invoice }: InvoicePreviewProps) {
  return (
    <div className="invoice-preview bg-white p-8 max-w-4xl mx-auto" style={{ fontFamily: 'Arial, sans-serif' }}>
      {/* Header */}
      <div className="text-center mb-8">
        <h1 className="text-3xl font-bold mb-2">INVOICE</h1>
        <p className="text-lg text-gray-600">Invoice #{invoice.invoiceNumber}</p>
      </div>

      {/* Invoice Info */}
      <div className="grid grid-cols-2 gap-8 mb-8">
        <div>
          <h3 className="font-semibold text-sm text-gray-700 mb-2">Bill To:</h3>
          <p className="text-gray-900">{invoice.customerName || 'N/A'}</p>
        </div>
        <div className="text-right">
          <div className="mb-2">
            <span className="text-sm text-gray-700">Date: </span>
            <span className="text-gray-900">{format(new Date(invoice.invoiceDate), 'MMM dd, yyyy')}</span>
          </div>
          {invoice.dueDate && (
            <div className="mb-2">
              <span className="text-sm text-gray-700">Due Date: </span>
              <span className="text-gray-900">{format(new Date(invoice.dueDate), 'MMM dd, yyyy')}</span>
            </div>
          )}
          <div>
            <span className="text-sm text-gray-700">Status: </span>
            <span className="text-gray-900">{invoice.status}</span>
          </div>
        </div>
      </div>

      {/* Items Table */}
      <table className="w-full border-collapse mb-6">
        <thead>
          <tr className="border-b-2 border-gray-300">
            <th className="text-left py-3 px-4 font-semibold text-gray-700">Description</th>
            <th className="text-right py-3 px-4 font-semibold text-gray-700">Quantity</th>
            <th className="text-right py-3 px-4 font-semibold text-gray-700">Unit Price</th>
            <th className="text-right py-3 px-4 font-semibold text-gray-700">Total</th>
          </tr>
        </thead>
        <tbody>
          {invoice.items && invoice.items.length > 0 ? (
            invoice.items.map((item, index) => (
              <tr key={item.id || index} className="border-b border-gray-200">
                <td className="py-3 px-4 text-gray-900">{item.description || 'N/A'}</td>
                <td className="py-3 px-4 text-right text-gray-900">{item.quantity}</td>
                <td className="py-3 px-4 text-right text-gray-900">${item.unitPrice.toFixed(2)}</td>
                <td className="py-3 px-4 text-right text-gray-900 font-semibold">${item.totalAmount.toFixed(2)}</td>
              </tr>
            ))
          ) : (
            <tr>
              <td colSpan={4} className="py-4 px-4 text-center text-gray-500">
                No items
              </td>
            </tr>
          )}
        </tbody>
      </table>

      {/* Totals */}
      <div className="flex justify-end">
        <div className="w-64">
          <div className="flex justify-between py-2 border-b border-gray-200">
            <span className="text-gray-700">Subtotal:</span>
            <span className="text-gray-900 font-semibold">${invoice.subTotal.toFixed(2)}</span>
          </div>
          {invoice.discountAmount > 0 && (
            <div className="flex justify-between py-2 border-b border-gray-200">
              <span className="text-gray-700">Discount:</span>
              <span className="text-gray-900">${invoice.discountAmount.toFixed(2)}</span>
            </div>
          )}
          <div className="flex justify-between py-2 border-b border-gray-200">
            <span className="text-gray-700">Tax:</span>
            <span className="text-gray-900">${invoice.taxAmount.toFixed(2)}</span>
          </div>
          <div className="flex justify-between py-3 border-t-2 border-gray-300 mt-2">
            <span className="text-lg font-bold text-gray-900">Total:</span>
            <span className="text-lg font-bold text-gray-900">${invoice.totalAmount.toFixed(2)}</span>
          </div>
          <div className="flex justify-between py-2 border-b border-gray-200">
            <span className="text-gray-700">Paid:</span>
            <span className="text-gray-900">${invoice.paidAmount.toFixed(2)}</span>
          </div>
          <div className="flex justify-between py-2">
            <span className="font-semibold text-gray-900">Balance:</span>
            <span className="font-semibold text-gray-900">
              ${(invoice.totalAmount - invoice.paidAmount).toFixed(2)}
            </span>
          </div>
        </div>
      </div>
    </div>
  )
}
