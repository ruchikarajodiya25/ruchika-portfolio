import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { appointmentsApi } from '../services/api'
import { format } from 'date-fns'
import { AppointmentDto } from '../types'

export default function AppointmentsPage() {
  const [page, setPage] = useState(1)
  const [showCreateModal, setShowCreateModal] = useState(false)
  const queryClient = useQueryClient()

  const startDate = new Date()
  startDate.setDate(startDate.getDate() - 7)
  const endDate = new Date()
  endDate.setDate(endDate.getDate() + 30)

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

  const createMutation = useMutation({
    mutationFn: (appointment: Partial<AppointmentDto>) => appointmentsApi.createAppointment(appointment),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['appointments'] })
      setShowCreateModal(false)
    },
  })

  const deleteMutation = useMutation({
    mutationFn: (id: string) => appointmentsApi.deleteAppointment(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['appointments'] })
    },
  })

  const handleCreate = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault()
    const formData = new FormData(e.currentTarget)
    const start = new Date(formData.get('scheduledStart') as string)
    const duration = parseInt(formData.get('durationMinutes') as string)
    const end = new Date(start.getTime() + duration * 60000)

    createMutation.mutate({
      customerId: formData.get('customerId') as string,
      serviceId: formData.get('serviceId') as string || undefined,
      staffId: formData.get('staffId') as string || undefined,
      locationId: formData.get('locationId') as string,
      scheduledStart: start.toISOString(),
      scheduledEnd: end.toISOString(),
      notes: formData.get('notes') as string || undefined,
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
                      {appointment.customerName || 'N/A'}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      {appointment.serviceName || '-'}
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

      {showCreateModal && (
        <div className="fixed inset-0 bg-gray-600 bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white p-6 rounded-lg max-w-md w-full">
            <h2 className="text-xl font-bold mb-4">Book Appointment</h2>
            <form onSubmit={handleCreate}>
              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700">Customer ID *</label>
                  <input
                    name="customerId"
                    required
                    className="mt-1 block w-full border border-gray-300 rounded-md px-3 py-2"
                    placeholder="Enter customer ID"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">Location ID *</label>
                  <input
                    name="locationId"
                    required
                    className="mt-1 block w-full border border-gray-300 rounded-md px-3 py-2"
                    placeholder="Enter location ID"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">Service ID</label>
                  <input
                    name="serviceId"
                    className="mt-1 block w-full border border-gray-300 rounded-md px-3 py-2"
                    placeholder="Optional"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">Scheduled Start *</label>
                  <input
                    name="scheduledStart"
                    type="datetime-local"
                    required
                    className="mt-1 block w-full border border-gray-300 rounded-md px-3 py-2"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">Duration (minutes) *</label>
                  <input
                    name="durationMinutes"
                    type="number"
                    required
                    defaultValue="30"
                    className="mt-1 block w-full border border-gray-300 rounded-md px-3 py-2"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">Notes</label>
                  <textarea
                    name="notes"
                    className="mt-1 block w-full border border-gray-300 rounded-md px-3 py-2"
                  />
                </div>
              </div>
              <div className="mt-6 flex justify-end space-x-2">
                <button
                  type="button"
                  onClick={() => setShowCreateModal(false)}
                  className="px-4 py-2 border rounded-md"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={createMutation.isPending}
                  className="px-4 py-2 bg-indigo-600 text-white rounded-md hover:bg-indigo-700"
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
