import { useQuery } from '@tanstack/react-query'
import { customersApi } from '../services/api'
import { CustomerDto } from '../types'

export function useCustomers() {
  return useQuery({
    queryKey: ['customers', 'all'],
    queryFn: async () => {
      try {
        const response = await customersApi.getCustomers({ pageNumber: 1, pageSize: 1000 })
        return response.data?.data?.items || []
      } catch (err: any) {
        console.error('❌ Failed to load customers:', err)
        return []
      }
    },
    staleTime: 5 * 60 * 1000, // Cache for 5 minutes
  })
}
