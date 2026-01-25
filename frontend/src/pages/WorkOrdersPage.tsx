import React, { useState, useMemo } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { workOrdersApi, appointmentsApi } from '../services/api'
import { WorkOrderDto, CustomerDto, LocationDto, UserDto, AppointmentDto, ServiceDto, WorkOrderItemDto } from '../types'
import { useCustomers } from '../hooks/useCustomers'
import { useLocations } from '../hooks/useLocations'
import { useUsers } from '../hooks/useUsers'
import { useServices } from '../hooks/useServices'
import { getWorkOrderTotal, formatCurrency, getDecimalFraction, calculateItemTotal, formatTaxRate } from '../utils/workOrderUtils'

export default function WorkOrdersPage() {
  const [page, setPage] = useState(1)
  const [showCreateModal, setShowCreateModal] = useState(false)
  const [expandedOrderId, setExpandedOrderId] = useState<string | null>(null)
  const [showAddItemModal, setShowAddItemModal] = useState<string | null>(null)
  const [itemFormData, setItemFormData] = useState({
    serviceId: '',
    quantity: '1',
    unitPrice: '',
    taxRate: '0',
    description: '',
  })
  
  // Client-side storage for work order items (temporary until backend endpoint exists)
  const [workOrderItems, setWorkOrderItems] = useState<Map<string, WorkOrderItemDto[]>>(new Map())
  
  const [formData, setFormData] = useState({
    customerId: '',
    appointmentId: '',
    assignedToUserId: '',
    locationId: '',
    description: '',
    internalNotes: '',
  })
  const queryClient = useQueryClient()

  // Fetch work orders
  const { data, isLoading, error } = useQuery({
    queryKey: ['workorders', page],
    queryFn: async () => {
      try {
        const response = await workOrdersApi.getWorkOrders({ pageNumber: page, pageSize: 10 })
        console.log('📥 Work orders response:', response.data)
        return response
      } catch (err: any) {
        console.error('❌ Failed to load work orders:', err)
        if (err.response) {
          console.error('Response:', err.response.status, err.response.data)
        }
        throw err
      }
    },
  })

  // Fetch lookups using hooks
  const { data: customers = [], isLoading: customersLoading } = useCustomers()
  const { data: locations = [], isLoading: locationsLoading } = useLocations()
  const { data: users = [], isLoading: usersLoading } = useUsers()
  const { data: services = [], isLoading: servicesLoading } = useServices()

  // Fetch appointments for dropdown
  const { data: appointmentsData } = useQuery({
    queryKey: ['appointments', 'all'],
    queryFn: async () => {
      try {
        const response = await appointmentsApi.getAppointments({ pageNumber: 1, pageSize: 100 })
        return response.data?.data?.items || []
      } catch (err: any) {
        console.error('❌ Failed to load appointments:', err)
        return []
      }
    },
  })
  const appointments = appointmentsData || []

  // Create memoized lookup maps
  const customerById = useMemo(
    () => new Map(customers.map((c: CustomerDto) => [c.id, `${c.firstName} ${c.lastName}`])),
    [customers]
  )
  const locationById = useMemo(() => new Map(locations.map((l: LocationDto) => [l.id, l.name])), [locations])
  const userById = useMemo(
    () => new Map(users.map((u: UserDto) => [u.id, `${u.firstName} ${u.lastName}`])),
    [users]
  )
  const serviceById = useMemo(() => new Map(services.map((s: ServiceDto) => [s.id, s])), [services])

  // Check if any lookups are still loading
  const lookupsLoading = customersLoading || locationsLoading || usersLoading || servicesLoading

  // Use backend totals directly - backend calculates from items
  // Only use client-side calculation as temporary fallback for unsaved items
  const workOrdersWithComputedTotals = useMemo(() => {
    if (!data?.data?.data?.items) return []
    return data.data.data.items.map((order: WorkOrderDto) => {
      // Backend items (persisted)
      const backendItems = order.items || []
      // Client-side items (temporary, not yet saved)
      const clientItems = workOrderItems.get(order.id) || []
      
      // If backend has items and total, use backend total directly
      if (backendItems.length > 0 && order.totalAmount > 0) {
        return {
          ...order,
          computedTotal: order.totalAmount, // Use backend calculated total
          items: backendItems,
        }
      }
      
      // If only client-side items exist (temporary), calculate from them
      if (clientItems.length > 0) {
        const computedTotal = getWorkOrderTotal(order, clientItems)
        return {
          ...order,
          computedTotal,
          items: clientItems,
        }
      }
      
      // Fallback: use backend total even if 0 (no items)
      return {
        ...order,
        computedTotal: order.totalAmount || 0,
        items: backendItems,
      }
    })
  }, [data, workOrderItems])

  const deleteMutation = useMutation({
    mutationFn: (id: string) => workOrdersApi.deleteWorkOrder(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['workorders'] })
    },
    onError: (err: any) => {
      console.error('❌ Failed to delete work order:', err)
      if (err.response) {
        console.error('Response:', err.response.status, err.response.data)
        const apiResponse = err.response.data
        alert(apiResponse?.message || `Failed to delete work order: ${err.response.statusText}`)
      }
    },
  })

  const updateStatusMutation = useMutation({
    mutationFn: ({ id, status }: { id: string; status: string }) =>
      workOrdersApi.updateWorkOrderStatus(id, status),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['workorders'] })
    },
    onError: (err: any) => {
      console.error('❌ Failed to update work order status:', err)
      if (err.response) {
        console.error('Response:', err.response.status, err.response.data)
        const apiResponse = err.response.data
        alert(apiResponse?.message || `Failed to update status: ${err.response.statusText}`)
      } else {
        alert('Failed to update status. Check console for details.')
      }
    },
  })

  const handleStatusChange = (orderId: string, newStatus: string) => {
    updateStatusMutation.mutate({ id: orderId, status: newStatus })
  }

  const addItemMutation = useMutation({
    mutationFn: ({ workOrderId, item }: { workOrderId: string; item: any }) =>
      workOrdersApi.addWorkOrderItem(workOrderId, item),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['workorders'] })
      setShowAddItemModal(null)
      setItemFormData({
        serviceId: '',
        quantity: '1',
        unitPrice: '',
        taxRate: '0',
        description: '',
      })
    },
    onError: (err: any) => {
      console.error('❌ Failed to add work order item:', err)
      if (err.response) {
        const apiResponse = err.response.data
        alert(apiResponse?.message || 'Failed to add item. Please try again.')
      } else {
        alert('Failed to add item. Check console for details.')
      }
    },
  })

  const removeItemMutation = useMutation({
    mutationFn: ({ workOrderId, itemId }: { workOrderId: string; itemId: string }) =>
      workOrdersApi.removeWorkOrderItem(workOrderId, itemId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['workorders'] })
    },
    onError: (err: any) => {
      console.error('❌ Failed to remove work order item:', err)
      if (err.response) {
        const apiResponse = err.response.data
        alert(apiResponse?.message || 'Failed to remove item. Please try again.')
      } else {
        alert('Failed to remove item. Check console for details.')
      }
    },
  })

  const handleAddItem = (workOrderId: string) => {
    const service = serviceById.get(itemFormData.serviceId)
    if (!service) {
      alert('Please select a service')
      return
    }

    const quantity = parseFloat(itemFormData.quantity) || 1
    const unitPrice = parseFloat(itemFormData.unitPrice) || service.price
    // Tax rate from form is percentage (e.g., 8 for 8%)
    // Backend expects percent (0-100), so send as-is
    const taxRatePercent = parseFloat(itemFormData.taxRate) || 0

    // Send to backend - backend will calculate total
    addItemMutation.mutate({
      workOrderId,
      item: {
        itemType: 'Service',
        serviceId: service.id,
        description: itemFormData.description || service.name,
        quantity,
        unitPrice,
        taxRate: taxRatePercent, // Send as percent (0-100), e.g., 8 for 8%
      },
    })
  }

  const handleRemoveItem = (workOrderId: string, itemId: string) => {
    // Check if it's a temporary client-side item (starts with "temp-")
    if (itemId.startsWith('temp-')) {
      // Remove from client-side state only
      const existingItems = workOrderItems.get(workOrderId) || []
      setWorkOrderItems(
        new Map(workOrderItems.set(workOrderId, existingItems.filter((item) => item.id !== itemId)))
      )
    } else {
      // Remove from backend
      removeItemMutation.mutate({ workOrderId, itemId })
    }
  }

  const handleServiceChange = (serviceId: string) => {
    const service = serviceById.get(serviceId)
    if (service) {
      // Service taxRate is stored as decimal (0.08), convert to percentage for display (8)
      const taxRateForDisplay = service.taxRate ? (service.taxRate * 100).toString() : '0'
      setItemFormData({
        ...itemFormData,
        serviceId,
        unitPrice: service.price.toString(),
        taxRate: taxRateForDisplay,
        description: service.name,
      })
    } else {
      setItemFormData({ ...itemFormData, serviceId })
    }
  }

  const createMutation = useMutation({
    mutationFn: (data: {
      customerId: string
      appointmentId?: string
      assignedToUserId?: string
      locationId: string
      description?: string
      internalNotes?: string
    }) => workOrdersApi.createWorkOrder(data),
    onSuccess: (response) => {
      console.log('✅ Work order created successfully:', response.data)
      queryClient.invalidateQueries({ queryKey: ['workorders'] })
      setShowCreateModal(false)
      setFormData({
        customerId: '',
        appointmentId: '',
        assignedToUserId: '',
        locationId: '',
        description: '',
        internalNotes: '',
      })
    },
    onError: (err: any) => {
      console.error('❌ Failed to create work order:', err)
      if (err.response) {
        console.error('Response:', err.response.status, err.response.data)
        const apiResponse = err.response.data
        alert(apiResponse?.message || `Failed to create work order: ${err.response.statusText}`)
      } else {
        alert('Failed to create work order. Check console for details.')
      }
    },
  })

  const handleCreate = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault()
    createMutation.mutate({
      customerId: formData.customerId,
      appointmentId: formData.appointmentId || undefined,
      assignedToUserId: formData.assignedToUserId || undefined,
      locationId: formData.locationId,
      description: formData.description || undefined,
      internalNotes: formData.internalNotes || undefined,
    })
  }

  return (
    <div>
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Work Orders</h1>
        <button
          onClick={() => setShowCreateModal(true)}
          className="bg-indigo-600 text-white px-4 py-2 rounded-md hover:bg-indigo-700"
        >
          Create Work Order
        </button>
      </div>

      {isLoading ? (
        <div className="text-center py-8">Loading...</div>
      ) : error ? (
        <div className="bg-red-50 border border-red-400 text-red-700 px-4 py-3 rounded">
          Failed to load work orders. Check console for details.
        </div>
      ) : (
        <div className="bg-white shadow overflow-hidden sm:rounded-md">
          <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Order #</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Customer</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Location</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Assigned To</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Status</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Total</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Created</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Actions</th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {workOrdersWithComputedTotals.length > 0 ? (
                workOrdersWithComputedTotals.map((order) => {
                  const items = order.items || []
                  const hasItems = items.length > 0
                  return (
                    <React.Fragment key={order.id}>
                      <tr className="hover:bg-gray-50">
                        <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                          <div className="flex items-center gap-2">
                            {hasItems && (
                              <button
                                onClick={() => setExpandedOrderId(expandedOrderId === order.id ? null : order.id)}
                                className="text-gray-400 hover:text-gray-600"
                              >
                                {expandedOrderId === order.id ? '▼' : '▶'}
                              </button>
                            )}
                            {order.workOrderNumber}
                          </div>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                          {lookupsLoading
                            ? 'Loading...'
                            : order.customerId
                            ? customerById.get(order.customerId) ?? order.customerName ?? 'Unknown'
                            : order.customerName ?? 'Unknown'}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                          {lookupsLoading
                            ? 'Loading...'
                            : order.locationId
                            ? locationById.get(order.locationId) ?? 'Unknown'
                            : 'Unknown'}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                          {lookupsLoading
                            ? 'Loading...'
                            : order.assignedToUserId
                            ? userById.get(order.assignedToUserId) ?? 'Unknown'
                            : 'Unknown'}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <span
                            className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${
                              order.status === 'Completed'
                                ? 'bg-green-100 text-green-800'
                                : order.status === 'Cancelled'
                                ? 'bg-red-100 text-red-800'
                                : order.status === 'InProgress'
                                ? 'bg-blue-100 text-blue-800'
                                : order.status === 'OnHold'
                                ? 'bg-yellow-100 text-yellow-800'
                                : 'bg-gray-100 text-gray-800'
                            }`}
                          >
                            {order.status}
                          </span>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900 font-semibold">
                          {formatCurrency(order.computedTotal)}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                          {new Date(order.createdAt).toLocaleDateString()}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                          <div className="flex items-center gap-2">
                            <select
                              value={order.status}
                              onChange={(e) => handleStatusChange(order.id, e.target.value)}
                              disabled={updateStatusMutation.isPending}
                              className="text-xs border border-gray-300 rounded-md px-2 py-1 bg-white hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                            >
                              <option value="Draft">Draft</option>
                              <option value="InProgress">InProgress</option>
                              <option value="OnHold">OnHold</option>
                              <option value="Completed">Completed</option>
                              <option value="Cancelled">Cancelled</option>
                            </select>
                            <button
                              onClick={() => setShowAddItemModal(order.id)}
                              className="text-xs text-blue-600 hover:text-blue-900 px-2 py-1 border border-blue-300 rounded hover:bg-blue-50"
                              title="Add Item"
                            >
                              + Item
                            </button>
                            <button
                              onClick={() => {
                                if (confirm('Are you sure you want to delete this work order?')) {
                                  deleteMutation.mutate(order.id)
                                  // Also remove client-side items
                                  const newItems = new Map(workOrderItems)
                                  newItems.delete(order.id)
                                  setWorkOrderItems(newItems)
                                }
                              }}
                              className="text-red-600 hover:text-red-900 text-xs"
                            >
                              Delete
                            </button>
                          </div>
                        </td>
                      </tr>
                      {expandedOrderId === order.id && hasItems && (
                        <tr>
                          <td colSpan={8} className="px-6 py-4 bg-gray-50">
                            <div className="mb-2">
                              <h4 className="text-sm font-semibold text-gray-700 mb-2">Items</h4>
                              <table className="min-w-full text-xs">
                                <thead>
                                  <tr className="border-b">
                                    <th className="text-left py-2">Service</th>
                                    <th className="text-left py-2">Description</th>
                                    <th className="text-right py-2">Qty</th>
                                    <th className="text-right py-2">Unit Price</th>
                                    <th className="text-right py-2">Tax Rate</th>
                                    <th className="text-right py-2">Total</th>
                                    <th className="text-right py-2">Actions</th>
                                  </tr>
                                </thead>
                                <tbody>
                                  {items.map((item) => (
                                    <tr key={item.id} className="border-b">
                                      <td className="py-2">
                                        {serviceById.get(item.serviceId || '')?.name || 'Unknown'}
                                      </td>
                                      <td className="py-2">{item.description}</td>
                                      <td className="text-right py-2">{item.quantity}</td>
                                      <td className="text-right py-2">${item.unitPrice.toFixed(2)}</td>
                                      <td className="text-right py-2">{(item.taxRate * 100).toFixed(2)}%</td>
                                      <td className="text-right py-2 font-semibold">
                                        ${item.totalAmount.toFixed(2)}
                                      </td>
                                      <td className="text-right py-2">
                                        <button
                                          onClick={() => handleRemoveItem(order.id, item.id)}
                                          className="text-red-600 hover:text-red-900"
                                        >
                                          Remove
                                        </button>
                                      </td>
                                    </tr>
                                  ))}
                                </tbody>
                                <tfoot>
                                  <tr className="font-semibold">
                                    <td colSpan={5} className="text-right py-2">Subtotal:</td>
                                    <td className="text-right py-2">
                                      {formatCurrency(items.reduce((sum, item) => sum + item.totalAmount, 0))}
                                    </td>
                                    <td></td>
                                  </tr>
                                </tfoot>
                              </table>
                            </div>
                          </td>
                        </tr>
                      )}
                    </React.Fragment>
                  )
                })
              ) : (
                <tr>
                  <td colSpan={8} className="px-6 py-4 text-center text-sm text-gray-500">
                    No work orders found
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      )}

      {showCreateModal && (
        <div className="fixed inset-0 bg-gray-600 bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white p-6 rounded-lg max-w-md w-full max-h-[90vh] overflow-y-auto">
            <h2 className="text-xl font-bold mb-4">Create Work Order</h2>
            <form onSubmit={handleCreate}>
              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700">Customer *</label>
                  <select
                    name="customerId"
                    required
                    value={formData.customerId}
                    onChange={(e) => setFormData({ ...formData, customerId: e.target.value })}
                    className="mt-1 block w-full border border-gray-300 rounded-md px-3 py-2"
                  >
                    <option value="">Select a customer</option>
                    {customers.map((customer: CustomerDto) => (
                      <option key={customer.id} value={customer.id}>
                        {customer.firstName} {customer.lastName} {customer.email ? `(${customer.email})` : ''}
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700">Location *</label>
                  <select
                    name="locationId"
                    required
                    value={formData.locationId}
                    onChange={(e) => setFormData({ ...formData, locationId: e.target.value })}
                    className="mt-1 block w-full border border-gray-300 rounded-md px-3 py-2"
                  >
                    <option value="">Select a location</option>
                    {locations.map((location: LocationDto) => (
                      <option key={location.id} value={location.id}>
                        {location.name} {location.city ? `(${location.city})` : ''}
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700">Appointment (Optional)</label>
                  <select
                    name="appointmentId"
                    value={formData.appointmentId}
                    onChange={(e) => setFormData({ ...formData, appointmentId: e.target.value })}
                    className="mt-1 block w-full border border-gray-300 rounded-md px-3 py-2"
                  >
                    <option value="">None</option>
                    {appointments.map((appointment: AppointmentDto) => (
                      <option key={appointment.id} value={appointment.id}>
                        {appointment.customerName || 'Customer'} - {new Date(appointment.scheduledStart).toLocaleString()}
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700">Assign To User (Optional)</label>
                  <select
                    name="assignedToUserId"
                    value={formData.assignedToUserId}
                    onChange={(e) => setFormData({ ...formData, assignedToUserId: e.target.value })}
                    className="mt-1 block w-full border border-gray-300 rounded-md px-3 py-2"
                  >
                    <option value="">None</option>
                    {users.map((user: UserDto) => (
                      <option key={user.id} value={user.id}>
                        {user.firstName} {user.lastName} ({user.email})
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700">Description</label>
                  <textarea
                    name="description"
                    value={formData.description}
                    onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                    rows={3}
                    className="mt-1 block w-full border border-gray-300 rounded-md px-3 py-2"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700">Internal Notes</label>
                  <textarea
                    name="internalNotes"
                    value={formData.internalNotes}
                    onChange={(e) => setFormData({ ...formData, internalNotes: e.target.value })}
                    rows={2}
                    className="mt-1 block w-full border border-gray-300 rounded-md px-3 py-2"
                  />
                </div>
              </div>
              <div className="mt-6 flex justify-end space-x-2">
                <button
                  type="button"
                  onClick={() => {
                    setShowCreateModal(false)
                    setFormData({
                      customerId: '',
                      appointmentId: '',
                      assignedToUserId: '',
                      locationId: '',
                      description: '',
                      internalNotes: '',
                    })
                  }}
                  className="px-4 py-2 border rounded-md"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={createMutation.isPending}
                  className="px-4 py-2 bg-indigo-600 text-white rounded-md hover:bg-indigo-700 disabled:opacity-50"
                >
                  {createMutation.isPending ? 'Creating...' : 'Create'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Add Item Modal */}
      {showAddItemModal && (
        <div className="fixed inset-0 bg-gray-600 bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white p-6 rounded-lg max-w-md w-full">
            <h2 className="text-xl font-bold mb-4">Add Service Item</h2>
            <form
              onSubmit={(e) => {
                e.preventDefault()
                handleAddItem(showAddItemModal)
              }}
            >
              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700">Service *</label>
                  <select
                    required
                    value={itemFormData.serviceId}
                    onChange={(e) => handleServiceChange(e.target.value)}
                    className="mt-1 block w-full border border-gray-300 rounded-md px-3 py-2"
                  >
                    <option value="">Select a service</option>
                    {services.map((service: ServiceDto) => (
                      <option key={service.id} value={service.id}>
                        {service.name} - ${service.price.toFixed(2)}
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700">Description</label>
                  <input
                    type="text"
                    value={itemFormData.description}
                    onChange={(e) => setItemFormData({ ...itemFormData, description: e.target.value })}
                    className="mt-1 block w-full border border-gray-300 rounded-md px-3 py-2"
                    placeholder="Service description"
                  />
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700">Quantity *</label>
                    <input
                      type="number"
                      required
                      min="0.01"
                      step="0.01"
                      value={itemFormData.quantity}
                      onChange={(e) => setItemFormData({ ...itemFormData, quantity: e.target.value })}
                      className="mt-1 block w-full border border-gray-300 rounded-md px-3 py-2"
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700">Unit Price *</label>
                    <input
                      type="number"
                      required
                      min="0"
                      step="0.01"
                      value={itemFormData.unitPrice}
                      onChange={(e) => setItemFormData({ ...itemFormData, unitPrice: e.target.value })}
                      className="mt-1 block w-full border border-gray-300 rounded-md px-3 py-2"
                    />
                  </div>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700">Tax Rate (%)</label>
                  <input
                    type="number"
                    min="0"
                    max="100"
                    step="0.01"
                    value={itemFormData.taxRate}
                    onChange={(e) => setItemFormData({ ...itemFormData, taxRate: e.target.value })}
                    className="mt-1 block w-full border border-gray-300 rounded-md px-3 py-2"
                  />
                </div>

                {itemFormData.serviceId && itemFormData.quantity && itemFormData.unitPrice && (
                  <div className="bg-gray-50 p-3 rounded">
                    <div className="text-sm">
                      <div className="flex justify-between mb-1">
                        <span>Subtotal:</span>
                        <span>
                          ${(
                            (parseFloat(itemFormData.quantity) || 0) * (parseFloat(itemFormData.unitPrice) || 0)
                          ).toFixed(2)}
                        </span>
                      </div>
                      <div className="flex justify-between mb-1">
                        <span>Tax ({(parseFloat(itemFormData.taxRate) || 0).toFixed(2)}%):</span>
                        <span>
                          $
                          {(
                            (parseFloat(itemFormData.quantity) || 0) *
                            (parseFloat(itemFormData.unitPrice) || 0) *
                            ((parseFloat(itemFormData.taxRate) || 0) / 100)
                          ).toFixed(2)}
                        </span>
                      </div>
                      <div className="flex justify-between font-semibold pt-2 border-t">
                        <span>Total:</span>
                        <span>
                          $
                          {(
                            (parseFloat(itemFormData.quantity) || 0) *
                            (parseFloat(itemFormData.unitPrice) || 0) *
                            (1 + (parseFloat(itemFormData.taxRate) || 0) / 100)
                          ).toFixed(2)}
                        </span>
                      </div>
                    </div>
                  </div>
                )}
              </div>
              <div className="mt-6 flex justify-end space-x-2">
                <button
                  type="button"
                  onClick={() => {
                    setShowAddItemModal(null)
                    setItemFormData({
                      serviceId: '',
                      quantity: '1',
                      unitPrice: '',
                      taxRate: '0',
                      description: '',
                    })
                  }}
                  className="px-4 py-2 border rounded-md"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="px-4 py-2 bg-indigo-600 text-white rounded-md hover:bg-indigo-700"
                >
                  Add Item
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  )
}
