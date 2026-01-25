import { useQuery } from '@tanstack/react-query'
import { servicesApi } from '../services/api'
import { ServiceDto } from '../types'

export function useServices() {
  return useQuery({
    queryKey: ['services', 'all'],
    queryFn: async () => {
      try {
        const response = await servicesApi.getServices({ pageNumber: 1, pageSize: 1000, isActive: true })
        return response.data?.data?.items || []
      } catch (err: any) {
        console.error('❌ Failed to load services:', err)
        return []
      }
    },
    staleTime: 5 * 60 * 1000, // Cache for 5 minutes
  })
}
