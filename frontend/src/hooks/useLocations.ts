import { useQuery } from '@tanstack/react-query'
import { locationsApi } from '../services/api'
import { LocationDto } from '../types'

export function useLocations() {
  return useQuery({
    queryKey: ['locations', 'all'],
    queryFn: async () => {
      try {
        const response = await locationsApi.getLocations({ pageNumber: 1, pageSize: 1000 })
        return response.data?.data?.items || []
      } catch (err: any) {
        console.error('❌ Failed to load locations:', err)
        return []
      }
    },
    staleTime: 5 * 60 * 1000, // Cache for 5 minutes
  })
}
