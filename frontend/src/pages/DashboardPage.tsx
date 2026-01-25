import { useQuery } from '@tanstack/react-query'
import { dashboardApi } from '../services/api'
import { format } from 'date-fns'

export default function DashboardPage() {
  const { data, isLoading } = useQuery({
    queryKey: ['dashboard-stats'],
    queryFn: () => dashboardApi.getStats(),
  })

  const stats = data?.data?.data

  if (isLoading) {
    return <div className="text-center py-8">Loading dashboard...</div>
  }

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-900 mb-6">Dashboard</h1>
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
        <div className="bg-white p-6 rounded-lg shadow">
          <h3 className="text-sm font-medium text-gray-500">Total Revenue</h3>
          <p className="text-2xl font-bold text-gray-900 mt-2">
            ${stats?.totalRevenue.toFixed(2) || '0.00'}
          </p>
        </div>
        <div className="bg-white p-6 rounded-lg shadow">
          <h3 className="text-sm font-medium text-gray-500">Active Appointments</h3>
          <p className="text-2xl font-bold text-gray-900 mt-2">{stats?.activeAppointments || 0}</p>
        </div>
        <div className="bg-white p-6 rounded-lg shadow">
          <h3 className="text-sm font-medium text-gray-500">Total Customers</h3>
          <p className="text-2xl font-bold text-gray-900 mt-2">{stats?.totalCustomers || 0}</p>
        </div>
        <div className="bg-white p-6 rounded-lg shadow">
          <h3 className="text-sm font-medium text-gray-500">Pending Invoices</h3>
          <p className="text-2xl font-bold text-gray-900 mt-2">{stats?.pendingInvoices || 0}</p>
          <p className="text-sm text-gray-500 mt-1">
            ${stats?.pendingInvoiceAmount.toFixed(2) || '0.00'}
          </p>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="bg-white p-6 rounded-lg shadow">
          <h2 className="text-lg font-semibold mb-4">Top Services</h2>
          {stats?.topServices && stats.topServices.length > 0 ? (
            <ul className="space-y-2">
              {stats.topServices.map((service) => (
                <li key={service.serviceId} className="flex justify-between">
                  <span>{service.serviceName}</span>
                  <span className="font-medium">{service.count} bookings</span>
                </li>
              ))}
            </ul>
          ) : (
            <p className="text-gray-500">No service data available</p>
          )}
        </div>

        <div className="bg-white p-6 rounded-lg shadow">
          <h2 className="text-lg font-semibold mb-4">Recent Appointments</h2>
          {stats?.recentAppointments && stats.recentAppointments.length > 0 ? (
            <ul className="space-y-2">
              {stats.recentAppointments.map((apt) => (
                <li key={apt.id} className="flex justify-between">
                  <div>
                    <span className="font-medium">{apt.customerName}</span>
                    <span className="text-sm text-gray-500 ml-2">
                      {format(new Date(apt.scheduledStart), 'MMM dd, h:mm a')}
                    </span>
                  </div>
                  <span className="text-sm text-gray-500">{apt.status}</span>
                </li>
              ))}
            </ul>
          ) : (
            <p className="text-gray-500">No recent appointments</p>
          )}
        </div>
      </div>

      {stats?.lowStockProducts !== undefined && stats.lowStockProducts > 0 && (
        <div className="mt-6 bg-yellow-50 border border-yellow-200 rounded-lg p-4">
          <p className="text-yellow-800">
            <strong>Alert:</strong> {stats.lowStockProducts} product(s) are running low on stock.
          </p>
        </div>
      )}
    </div>
  )
}
