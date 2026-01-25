import { useState, useMemo } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { appointmentsApi } from '../services/api'
import { format } from 'date-fns'
import { AppointmentDto, CustomerDto, LocationDto, ServiceDto, UserDto } from '../types'
import { useCustomers } from '../hooks/useCustomers'
import { useServices } from '../hooks/useServices'
import { useLocations } from '../hooks/useLocations'
import { useUsers } from '../hooks/useUsers'

export default function AppointmentsPage() {
  const [page, setPage] = useState(1)
  const [showCreateModal, setShowCreateModal] = useState(false)
  const [formData, setFormData] = useState({
    customerId: '',
    serviceId: '',
    staffId: '',
    locationId: '',
    scheduledStart: '',
    durationMinutes: '30',
    notes: '',
    internalNotes: '',
  })
  const queryClient = useQueryClient()

  const startDate = new Date()
  startDate.setDate(startDate.getDate() - 7)
  const endDate = new Date()
  endDate.setDate(endDate.getDate() + 30)

  // Fetch appointments
  const { data, isLoading } = useQuery({
    queryKey: ['appointments', page],
    queryFn: () =>
      appointmentsApi.getAppointments({
        pageNumber: page,
        pageSize: 20,
        startDate: startDate.toISOString(),
        endDate: endDate.toISOString(),
      }),
  })

  // Fetch lookups using hooks
  const { data: customers = [], isLoading: customersLoading } = useCustomers()
  const { data: services = [], isLoading: servicesLoading } = useServices()
  const { data: locations = [], isLoading: locationsLoading } = useLocations()
  const { data: users = [], isLoading: usersLoading } = useUsers()

  // Create memoized lookup maps
  const customerById = useMemo(
    () => new Map(customers.map((c: CustomerDto) => [c.id, `${c.firstName} ${c.lastName}`])),
    [customers]
  )
  const serviceById = useMemo(() => new Map(services.map((s: ServiceDto) => [s.id, s.name])), [services])
  const locationById = useMemo(() => new Map(locations.map((l: LocationDto) => [l.id, l.name])), [locations])
  const userById = useMemo(
    () => new Map(users.map((u: UserDto) => [u.id, `${u.firstName} ${u.lastName}`])),
    [users]
  )

  // Check if any lookups are still loading
  const lookupsLoading = customersLoading || servicesLoading || locationsLoading || usersLoading

  const createMutation = useMutation({
    mutationFn: (appointment: {
      customerId: string
      serviceId?: string
      staffId?: string
      locationId: string
      scheduledStart: string
      scheduledEnd: string
      notes?: string
      internalNotes?: string
    }) => appointmentsApi.createAppointment(appointment),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['appointments'] })
      setShowCreateModal(false)
      setFormData({
        customerId: '',
        serviceId: '',
        staffId: '',
        locationId: '',
        scheduledStart: '',
        durationMinutes: '30',
        notes: '',
        internalNotes: '',
      })
    },
    onError: (err: any) => {
      console.error('❌ Failed to create appointment:', err)
      if (err.response) {
        console.error('Response:', err.response.status, err.response.data)
        const apiResponse = err.response.data
        alert(apiResponse?.message || `Failed to create appointment: ${err.response.statusText}`)
      } else {
        alert('Failed to create appointment. Check console for details.')
      }
    },
  })

  const deleteMutation = useMutation({
    mutationFn: (id: string) => appointmentsApi.deleteAppointment(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['appointments'] })
    },
    onError: (err: any) => {
      console.error('❌ Failed to delete appointment:', err)
      if (err.response) {
        console.error('Response:', err.response.status, err.response.data)
        const apiResponse = err.response.data
        alert(apiResponse?.message || `Failed to delete appointment: ${err.response.statusText}`)
      }
    },
  })

  const handleCreate = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault()
    const start = new Date(formData.scheduledStart)
    const duration = parseInt(formData.durationMinutes)
    const end = new Date(start.getTime() + duration * 60000)

    createMutation.mutate({
      customerId: formData.customerId,
      serviceId: formData.serviceId || undefined,
      staffId: formData.staffId || undefined,
      locationId: formData.locationId,
      scheduledStart: start.toISOString(),
      scheduledEnd: end.toISOString(),
      notes: formData.notes || undefined,
      internalNotes: formData.internalNotes || undefined,
    })
  }

  return (
    <div>
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Appointments</h1>
        <button
          onClick={() => setShowCreateModal(true)}
          className="bg-indigo-600 text-white px-4 py-2 rounded-md hover:bg-indigo-700"
        >
          Book Appointment
        </button>
      </div>

      {isLoading ? (
        <div className="text-center py-8">Loading...</div>
      ) : (
        <>
          <div className="bg-white shadow overflow-hidden sm:rounded-md">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Date & Time</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Customer</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Service</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Status</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Notes</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Actions</th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {data?.data?.data?.items.map((appointment) => (
                  <tr key={appointment.id}>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm font-medium text-gray-900">
                        {format(new Date(appointment.scheduledStart), 'MMM dd, yyyy')}
                      </div>
                      <div className="text-sm text-gray-500">
                        {format(new Date(appointment.scheduledStart), 'h:mm a')} -{' '}
                        {format(new Date(appointment.scheduledEnd), 'h:mm a')}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {lookupsLoading
                        ? 'Loading...'
                        : appointment.customerId
                        ? customerById.get(appointment.customerId) ?? appointment.customerName ?? 'Unknown'
                        : appointment.customerName ?? 'Unknown'}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      {lookupsLoading
                        ? 'Loading...'
                        : appointment.serviceId
                        ? serviceById.get(appointment.serviceId) ?? appointment.serviceName ?? 'Unknown'
                        : appointment.serviceName ?? 'Unknown'}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span
                        className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${
                          appointment.status === 'Completed'
                            ? 'bg-green-100 text-green-800'
                            : appointment.status === 'Cancelled' || appointment.status === 'NoShow'
                            ? 'bg-red-100 text-red-800'
                            : 'bg-blue-100 text-blue-800'
                        }`}
                      >
                        {appointment.status}
                      </span>
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-500">
                      {appointment.notes || '-'}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                      <button
                        onClick={() => {
                          if (confirm('Are you sure you want to delete this appointment?')) {
                            deleteMutation.mutate(appointment.id)
                          }
                        }}
                        className="text-red-600 hover:text-red-900"
                      >
                        Delete
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </>
      )}

      {showCreateModal && (
        <div className="fixed inset-0 bg-gray-600 bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white p-6 rounded-lg max-w-md w-full max-h-[90vh] overflow-y-auto">
            <h2 className="text-xl font-bold mb-4">Book Appointment</h2>
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
                  <label className="block text-sm font-medium text-gray-700">Service (Optional)</label>
                  <select
                    name="serviceId"
                    value={formData.serviceId}
                    onChange={(e) => setFormData({ ...formData, serviceId: e.target.value })}
                    className="mt-1 block w-full border border-gray-300 rounded-md px-3 py-2"
                  >
                    <option value="">None</option>
                    {services.map((service: ServiceDto) => (
                      <option key={service.id} value={service.id}>
                        {service.name} {service.price ? `($${service.price.toFixed(2)})` : ''}
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700">Staff Member (Optional)</label>
                  <select
                    name="staffId"
                    value={formData.staffId}
                    onChange={(e) => setFormData({ ...formData, staffId: e.target.value })}
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
                  <label className="block text-sm font-medium text-gray-700">Scheduled Start *</label>
                  <input
                    name="scheduledStart"
                    type="datetime-local"
                    required
                    value={formData.scheduledStart}
                    onChange={(e) => setFormData({ ...formData, scheduledStart: e.target.value })}
                    className="mt-1 block w-full border border-gray-300 rounded-md px-3 py-2"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700">Duration (minutes) *</label>
                  <input
                    name="durationMinutes"
                    type="number"
                    required
                    min="15"
                    step="15"
                    value={formData.durationMinutes}
                    onChange={(e) => setFormData({ ...formData, durationMinutes: e.target.value })}
                    className="mt-1 block w-full border border-gray-300 rounded-md px-3 py-2"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700">Notes</label>
                  <textarea
                    name="notes"
                    value={formData.notes}
                    onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
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
                      serviceId: '',
                      staffId: '',
                      locationId: '',
                      scheduledStart: '',
                      durationMinutes: '30',
                      notes: '',
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
                  {createMutation.isPending ? 'Booking...' : 'Book'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  )
}
