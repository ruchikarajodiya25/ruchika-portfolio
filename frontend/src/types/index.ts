export interface UserDto {
  id: string
  email: string
  firstName: string
  lastName: string
  tenantId?: string
  locationId?: string
  roles: string[]
}

export interface AuthResponseDto {
  accessToken: string
  refreshToken: string
  expiresAt: string
  user: UserDto
}

export interface CustomerDto {
  id: string
  firstName: string
  lastName: string
  email?: string
  phone?: string
  mobile?: string
  address?: string
  city?: string
  state?: string
  zipCode?: string
  country?: string
  dateOfBirth?: string
  notes?: string
  tags?: string
  totalSpent: number
  totalVisits: number
  lastVisitAt?: string
  createdAt: string
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

export interface ApiResponse<T> {
  success: boolean
  data?: T
  message?: string
  errors: string[]
}
