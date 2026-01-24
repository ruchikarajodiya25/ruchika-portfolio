import { useQuery } from '@tanstack/react-query'
import { format } from 'date-fns'

export default function InvoicesPage() {
  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-900 mb-6">Invoices</h1>
      <div className="bg-white p-6 rounded-lg shadow">
        <p className="text-gray-500">Invoice management coming soon. Create invoices from completed work orders.</p>
      </div>
    </div>
  )
}
