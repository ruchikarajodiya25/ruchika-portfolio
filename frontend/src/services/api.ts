import axios, { AxiosInstance, AxiosError } from 'axios'
import {
  ApiResponse,
  AuthResponseDto,
  CustomerDto,
  PagedResult,
  ServiceDto,
  AppointmentDto,
  WorkOrderDto,
  ProductDto,
  InvoiceDto,
  DashboardStatsDto,
} from '../types'

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api'

class ApiClient {
  private client: AxiosInstance

  constructor() {
    this.client = axios.create({
      baseURL: API_BASE_URL,
      headers: {
        'Content-Type': 'application/json',
      },
    })

    // Add request interceptor to include auth token
    this.client.interceptors.request.use(
      (config) => {
        const token = localStorage.getItem('accessToken')
        if (token) {
          config.headers.Authorization = `Bearer ${token}`
        }
        return config
      },
      (error) => Promise.reject(error)
    )

    // Add response interceptor for token refresh
    this.client.interceptors.response.use(
      (response) => response,
      async (error: AxiosError) => {
        if (error.response?.status === 401) {
          // Try to refresh token
          const refreshToken = localStorage.getItem('refreshToken')
          if (refreshToken) {
            try {
              const response = await axios.post<ApiResponse<AuthResponseDto>>(
                `${API_BASE_URL}/auth/refresh-token`,
                {
                  accessToken: localStorage.getItem('accessToken'),
                  refreshToken,
                }
              )
              if (response.data.data) {
                localStorage.setItem('accessToken', response.data.data.accessToken)
                localStorage.setItem('refreshToken', response.data.data.refreshToken)
                // Retry original request
                if (error.config) {
                  error.config.headers.Authorization = `Bearer ${response.data.data.accessToken}`
                  return this.client.request(error.config)
                }
              }
            } catch {
              // Refresh failed, logout
              localStorage.removeItem('accessToken')
              localStorage.removeItem('refreshToken')
              localStorage.removeItem('user')
              window.location.href = '/login'
            }
          }
        }
        return Promise.reject(error)
      }
    )
  }

  get<T>(url: string, params?: any) {
    return this.client.get<ApiResponse<T>>(url, { params })
  }

  post<T>(url: string, data?: any) {
    return this.client.post<ApiResponse<T>>(url, data)
  }

  put<T>(url: string, data?: any) {
    return this.client.put<ApiResponse<T>>(url, data)
  }

  delete<T>(url: string) {
    return this.client.delete<ApiResponse<T>>(url)
  }
}

export const apiClient = new ApiClient()

export const authApi = {
  login: (data: { email: string; password: string }) =>
    apiClient.post<AuthResponseDto>('/auth/login', data),
  register: (data: {
    email: string
    password: string
    firstName: string
    lastName: string
    businessName: string
    phone?: string
  }) => apiClient.post<AuthResponseDto>('/auth/register', data),
  refreshToken: (data: { accessToken: string; refreshToken: string }) =>
    apiClient.post<AuthResponseDto>('/auth/refresh-token', data),
}

export const customersApi = {
  getCustomers: (params?: {
    pageNumber?: number
    pageSize?: number
    searchTerm?: string
    sortBy?: string
    sortDescending?: boolean
  }) => apiClient.get<PagedResult<CustomerDto>>('/customers', params),
  createCustomer: (data: Partial<CustomerDto>) =>
    apiClient.post<CustomerDto>('/customers', data),
}

export const servicesApi = {
  getServices: (params?: {
    pageNumber?: number
    pageSize?: number
    searchTerm?: string
    category?: string
    isActive?: boolean
    sortBy?: string
    sortDescending?: boolean
  }) => apiClient.get<PagedResult<ServiceDto>>('/services', params),
  getService: (id: string) => apiClient.get<ServiceDto>(`/services/${id}`),
  createService: (data: Partial<ServiceDto>) => apiClient.post<ServiceDto>('/services', data),
  updateService: (id: string, data: Partial<ServiceDto>) => apiClient.put<ServiceDto>(`/services/${id}`, data),
  deleteService: (id: string) => apiClient.delete(`/services/${id}`),
}

export const appointmentsApi = {
  getAppointments: (params?: {
    pageNumber?: number
    pageSize?: number
    startDate?: string
    endDate?: string
    customerId?: string
    staffId?: string
    locationId?: string
    status?: string
  }) => apiClient.get<PagedResult<AppointmentDto>>('/appointments', params),
  createAppointment: (data: Partial<AppointmentDto>) => apiClient.post<AppointmentDto>('/appointments', data),
  updateAppointmentStatus: (id: string, status: string, notes?: string) =>
    apiClient.put<AppointmentDto>(`/appointments/${id}/status`, { status, notes }),
}

export const workOrdersApi = {
  getWorkOrders: (params?: {
    pageNumber?: number
    pageSize?: number
    status?: string
    customerId?: string
    assignedToUserId?: string
  }) => apiClient.get<PagedResult<WorkOrderDto>>('/workorders', params),
  createWorkOrder: (data: Partial<WorkOrderDto>) => apiClient.post<WorkOrderDto>('/workorders', data),
}

export const productsApi = {
  getProducts: (params?: {
    pageNumber?: number
    pageSize?: number
    searchTerm?: string
    locationId?: string
    lowStock?: boolean
  }) => apiClient.get<PagedResult<ProductDto>>('/products', params),
}

export const invoicesApi = {
  createInvoiceFromWorkOrder: (workOrderId: string) =>
    apiClient.post<InvoiceDto>(`/invoices/from-workorder/${workOrderId}`),
}

export const dashboardApi = {
  getStats: (params?: { startDate?: string; endDate?: string }) =>
    apiClient.get<DashboardStatsDto>('/dashboard/stats', params),
}
