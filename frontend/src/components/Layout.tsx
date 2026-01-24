import { Outlet, Link, useLocation } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'

export default function Layout() {
  const { user, logout } = useAuth()
  const location = useLocation()

  const navigation = [
    { name: 'Dashboard', href: '/dashboard' },
    { name: 'Customers', href: '/customers' },
    { name: 'Appointments', href: '/appointments' },
    { name: 'Services', href: '/services' },
    { name: 'Inventory', href: '/inventory' },
    { name: 'Invoices', href: '/invoices' },
    { name: 'Reports', href: '/reports' },
  ]

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="flex">
        {/* Sidebar */}
        <div className="w-64 bg-gray-900 min-h-screen">
          <div className="p-4">
            <h1 className="text-white text-xl font-bold">ServiceHub Pro</h1>
          </div>
          <nav className="mt-8">
            {navigation.map((item) => {
              const isActive = location.pathname === item.href
              return (
                <Link
                  key={item.name}
                  to={item.href}
                  className={`block px-4 py-2 text-sm ${
                    isActive
                      ? 'bg-gray-800 text-white'
                      : 'text-gray-300 hover:bg-gray-800 hover:text-white'
                  }`}
                >
                  {item.name}
                </Link>
              )
            })}
          </nav>
        </div>

        {/* Main content */}
        <div className="flex-1">
          {/* Top bar */}
          <div className="bg-white shadow">
            <div className="px-4 py-4 flex justify-between items-center">
              <h2 className="text-xl font-semibold text-gray-800">
                {navigation.find((item) => location.pathname === item.href)?.name || 'Dashboard'}
              </h2>
              <div className="flex items-center space-x-4">
                <span className="text-sm text-gray-600">
                  {user?.firstName} {user?.lastName}
                </span>
                <button
                  onClick={logout}
                  className="text-sm text-gray-600 hover:text-gray-900"
                >
                  Logout
                </button>
              </div>
            </div>
          </div>

          {/* Page content */}
          <main className="p-6">
            <Outlet />
          </main>
        </div>
      </div>
    </div>
  )
}
