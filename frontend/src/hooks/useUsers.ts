import { useQuery } from '@tanstack/react-query'
import { usersApi } from '../services/api'
import { UserDto } from '../types'

export function useUsers() {
  return useQuery({
    queryKey: ['users', 'all'],
    queryFn: async () => {
      try {
        const response = await usersApi.getUsers({ pageNumber: 1, pageSize: 1000, isActive: true })
        return response.data?.data?.items || []
      } catch (err: any) {
        console.error('❌ Failed to load users:', err)
        return []
      }
    },
    staleTime: 5 * 60 * 1000, // Cache for 5 minutes
  })
}
