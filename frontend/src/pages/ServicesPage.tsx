import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { servicesApi } from '../services/api'
import { ServiceDto } from '../types'

export default function ServicesPage() {
  const [page, setPage] = useState(1)
  const [searchTerm, setSearchTerm] = useState('')
  const [showCreateModal, setShowCreateModal] = useState(false)
  const queryClient = useQueryClient()

  const { data, isLoading } = useQuery({
    queryKey: ['services', page, searchTerm],
    queryFn: () =>
      servicesApi.getServices({
        pageNumber: page,
        pageSize: 10,
        searchTerm: searchTerm || undefined,
      }),
  })

  const createMutation = useMutation({
    mutationFn: (service: Partial<ServiceDto>) => servicesApi.createService(service),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['services'] })
      setShowCreateModal(false)
    },
  })

  const deleteMutation = useMutation({
    mutationFn: (id: string) => servicesApi.deleteService(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['services'] })
    },
  })

  const handleCreate = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault()
    const formData = new FormData(e.currentTarget)
    createMutation.mutate({
      name: formData.get('name') as string,
      description: formData.get('description') as string,
      price: parseFloat(formData.get('price') as string),
      durationMinutes: parseInt(formData.get('durationMinutes') as string),
      category: formData.get('category') as string,
      taxRate: parseFloat(formData.get('taxRate') as string) || 0,
      isActive: true,
    })
  }

  return (
    <div>
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Services</h1>
        <button
          onClick={() => setShowCreateModal(true)}
          className="bg-indigo-600 text-white px-4 py-2 rounded-md hover:bg-indigo-700"
        >
          Add Service
        </button>
      </div>

      <div className="mb-4">
        <input
          type="text"
          placeholder="Search services..."
          value={searchTerm}
          onChange={(e) => {
            setSearchTerm(e.target.value)
            setPage(1)
          }}
          className="w-full max-w-md px-4 py-2 border border-gray-300 rounded-md"
        />
      </div>

      {isLoading ? (
        <div className="text-center py-8">Loading...</div>
      ) : (
        <>
          <div className="bg-white shadow overflow-hidden sm:rounded-md">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Name</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Price</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Duration</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Category</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Status</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Actions</th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {data?.data?.data?.items.map((service) => (
                  <tr key={service.id}>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm font-medium text-gray-900">{service.name}</div>
                      {service.description && (
                        <div className="text-sm text-gray-500">{service.description}</div>
                      )}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      ${service.price.toFixed(2)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {service.durationMinutes} min
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      {service.category || '-'}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span
                        className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${
                          service.isActive
                            ? 'bg-green-100 text-green-800'
                            : 'bg-gray-100 text-gray-800'
                        }`}
                      >
                        {service.isActive ? 'Active' : 'Inactive'}
                      </span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                      <button
                        onClick={() => deleteMutation.mutate(service.id)}
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

          {data?.data?.data && data.data.data.totalPages > 1 && (
            <div className="mt-4 flex justify-center space-x-2">
              <button
                onClick={() => setPage((p) => Math.max(1, p - 1))}
                disabled={page === 1}
                className="px-4 py-2 border rounded-md disabled:opacity-50"
              >
                Previous
              </button>
              <span className="px-4 py-2">
                Page {page} of {data.data.data.totalPages}
              </span>
              <button
                onClick={() => setPage((p) => Math.min(data.data.data!.totalPages, p + 1))}
                disabled={page === data.data.data.totalPages}
                className="px-4 py-2 border rounded-md disabled:opacity-50"
              >
                Next
              </button>
            </div>
          )}
        </>
      )}

      {showCreateModal && (
        <div className="fixed inset-0 bg-gray-600 bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white p-6 rounded-lg max-w-md w-full">
            <h2 className="text-xl font-bold mb-4">Create Service</h2>
            <form onSubmit={handleCreate}>
              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700">Name *</label>
                  <input
                    name="name"
                    required
                    className="mt-1 block w-full border border-gray-300 rounded-md px-3 py-2"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">Description</label>
                  <textarea
                    name="description"
                    className="mt-1 block w-full border border-gray-300 rounded-md px-3 py-2"
                  />
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700">Price *</label>
                    <input
                      name="price"
                      type="number"
                      step="0.01"
                      required
                      className="mt-1 block w-full border border-gray-300 rounded-md px-3 py-2"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700">Duration (min) *</label>
                    <input
                      name="durationMinutes"
                      type="number"
                      required
                      className="mt-1 block w-full border border-gray-300 rounded-md px-3 py-2"
                    />
                  </div>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">Category</label>
                  <input
                    name="category"
                    className="mt-1 block w-full border border-gray-300 rounded-md px-3 py-2"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">Tax Rate (%)</label>
                  <input
                    name="taxRate"
                    type="number"
                    step="0.01"
                    defaultValue="0"
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
                  {createMutation.isPending ? 'Creating...' : 'Create'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  )
}
