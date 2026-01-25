import axios, { AxiosInstance, AxiosError, AxiosRequestConfig } from 'axios'
import {
  ApiResponse,
  AuthResponseDto,
  CustomerDto,
  PagedResult,
  ServiceDto,
  AppointmentDto,
  WorkOrderDto,
  WorkOrderItemDto,
  ProductDto,
  InvoiceDto,
  DashboardStatsDto,
  PaymentDto,
  NotificationDto,
  LocationDto,
  UserDto,
} from '../types'

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:7000/api'

// Debug: Log API configuration
console.log('🔧 API Configuration:', {
  envVar: import.meta.env.VITE_API_URL,
  resolved: API_BASE_URL,
  mode: import.meta.env.MODE,
})

class ApiClient {
  private client: AxiosInstance

  constructor() {
    console.log(`📡 Creating API client with baseURL: ${API_BASE_URL}`)
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
          console.log(`🔑 Request [${config.method?.toUpperCase()}] ${config.url}`, {
            hasAuth: true,
            baseURL: config.baseURL,
          })
        } else {
          console.warn(`⚠️ Request [${config.method?.toUpperCase()}] ${config.url} - No auth token`)
        }
        return config
      },
      (error) => Promise.reject(error)
    )

    // Add response interceptor for token refresh and error logging
    this.client.interceptors.response.use(
      (response) => response,
      async (error: AxiosError) => {
        // Log all errors
        if (error.response) {
          console.error(`❌ API Error [${error.response.status}]:`, {
            url: error.config?.url,
            method: error.config?.method,
            status: error.response.status,
            statusText: error.response.statusText,
            data: error.response.data,
          })
        } else if (error.request) {
          console.error('❌ Network Error:', {
            url: error.config?.url,
            method: error.config?.method,
            message: error.message,
            code: error.code,
          })
        }

        if (error.response?.status === 401) {
          // Try to refresh token
          const refreshToken = localStorage.getItem('refreshToken')
          if (refreshToken) {
            try {
              const response = await axios.post<ApiResponse<AuthResponseDto>>(
                `${API_BASE_URL}/Auth/refresh-token`,
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
  login: (data: { email: string; password: string }) => {
    const url = `${API_BASE_URL}/Auth/login`
    console.log(`🔐 Login request: POST ${url}`, { email: data.email })
    return apiClient.post<AuthResponseDto>('/Auth/login', data)
  },
  register: (data: {
    email: string
    password: string
    firstName: string
    lastName: string
    businessName: string
    phone?: string
  }) => apiClient.post<AuthResponseDto>('/Auth/register', data),
  refreshToken: (data: { accessToken: string; refreshToken: string }) =>
    apiClient.post<AuthResponseDto>('/Auth/refresh-token', data),
}

export const customersApi = {
  getCustomers: (params?: {
    pageNumber?: number
    pageSize?: number
    searchTerm?: string
    sortBy?: string
    sortDescending?: boolean
  }) => apiClient.get<PagedResult<CustomerDto>>('/customers', params),
  getCustomer: (id: string) => apiClient.get<CustomerDto>(`/customers/${id}`),
  createCustomer: (data: Partial<CustomerDto>) =>
    apiClient.post<CustomerDto>('/customers', data),
  updateCustomer: (id: string, data: Partial<CustomerDto>) =>
    apiClient.put<CustomerDto>(`/customers/${id}`, data),
  deleteCustomer: (id: string) => apiClient.delete(`/customers/${id}`),
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
  createAppointment: (data: {
    customerId: string
    serviceId?: string
    staffId?: string
    locationId: string
    scheduledStart: string
    scheduledEnd: string
    notes?: string
    internalNotes?: string
  }) => {
    console.log('📅 Creating appointment:', data)
    return apiClient.post<AppointmentDto>('/appointments', {
      customerId: data.customerId,
      serviceId: data.serviceId,
      staffId: data.staffId,
      locationId: data.locationId,
      scheduledStart: data.scheduledStart,
      scheduledEnd: data.scheduledEnd,
      notes: data.notes,
      internalNotes: data.internalNotes,
    })
  },
  updateAppointment: (id: string, data: Partial<AppointmentDto>) =>
    apiClient.put<AppointmentDto>(`/appointments/${id}`, data),
  deleteAppointment: (id: string) => {
    console.log('🗑️ Deleting appointment:', id)
    return apiClient.delete(`/appointments/${id}`)
  },
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
  }) => {
    console.log('📋 Fetching work orders:', params)
    return apiClient.get<PagedResult<WorkOrderDto>>('/WorkOrders', params)
  },
  createWorkOrder: (data: {
    customerId: string
    appointmentId?: string
    assignedToUserId?: string
    locationId: string
    description?: string
    internalNotes?: string
  }) => {
    console.log('➕ Creating work order:', data)
    return apiClient.post<WorkOrderDto>('/WorkOrders', {
      customerId: data.customerId,
      appointmentId: data.appointmentId,
      assignedToUserId: data.assignedToUserId,
      locationId: data.locationId,
      description: data.description,
      internalNotes: data.internalNotes,
    })
  },
  updateWorkOrder: (id: string, data: Partial<WorkOrderDto>) =>
    apiClient.put<WorkOrderDto>(`/WorkOrders/${id}`, data),
  updateWorkOrderStatus: (id: string, status: string) => {
    console.log('🔄 Updating work order status:', { id, status })
    return apiClient.put<WorkOrderDto>(`/WorkOrders/${id}`, { status })
  },
  deleteWorkOrder: (id: string) => {
    console.log('🗑️ Deleting work order:', id)
    return apiClient.delete(`/WorkOrders/${id}`)
  },
  addWorkOrderItem: (workOrderId: string, item: {
    itemType: string
    serviceId?: string
    productId?: string
    description: string
    quantity: number
    unitPrice: number
    taxRate: number // Should be decimal fraction (0.08 for 8%)
  }) => {
    console.log('➕ Adding work order item:', { workOrderId, item })
    return apiClient.post<WorkOrderItemDto>(`/WorkOrders/${workOrderId}/items`, item)
  },
  removeWorkOrderItem: (workOrderId: string, itemId: string) => {
    console.log('🗑️ Removing work order item:', { workOrderId, itemId })
    return apiClient.delete(`/WorkOrders/${workOrderId}/items/${itemId}`)
  },
}

export const productsApi = {
  getProducts: (params?: {
    pageNumber?: number
    pageSize?: number
    searchTerm?: string
    locationId?: string
    lowStock?: boolean
  }) => apiClient.get<PagedResult<ProductDto>>('/products', params),
  getProduct: (id: string) => apiClient.get<ProductDto>(`/products/${id}`),
  createProduct: (data: Partial<ProductDto>) => apiClient.post<ProductDto>('/products', data),
  updateProduct: (id: string, data: Partial<ProductDto>) =>
    apiClient.put<ProductDto>(`/products/${id}`, data),
  deleteProduct: (id: string) => apiClient.delete(`/products/${id}`),
}

export const invoicesApi = {
  getInvoices: (params?: {
    pageNumber?: number
    pageSize?: number
    customerId?: string
    status?: string
    startDate?: string
    endDate?: string
  }) => apiClient.get<PagedResult<InvoiceDto>>('/invoices', params),
  getInvoice: (id: string) => apiClient.get<InvoiceDto>(`/invoices/${id}`),
  createInvoiceFromWorkOrder: (workOrderId: string) =>
    apiClient.post<InvoiceDto>(`/invoices/from-workorder/${workOrderId}`),
  downloadInvoicePdf: async (id: string): Promise<Blob> => {
    // Use axios directly for blob response
    const token = localStorage.getItem('accessToken')
    const response = await axios.get(`${API_BASE_URL}/invoices/${id}/pdf`, {
      responseType: 'blob',
      headers: {
        Authorization: token ? `Bearer ${token}` : '',
      },
    })
    return response.data as Blob
  },
}

export const dashboardApi = {
  getStats: (params?: { startDate?: string; endDate?: string }) =>
    apiClient.get<DashboardStatsDto>('/dashboard/stats', params),
}

export const paymentsApi = {
  getPayments: (params?: {
    pageNumber?: number
    pageSize?: number
    invoiceId?: string
    startDate?: string
    endDate?: string
    paymentMethod?: string
  }) => apiClient.get<PagedResult<PaymentDto>>('/payments', params),
  createPayment: (data: {
    invoiceId: string
    amount: number
    paymentMethod: string
    referenceNumber?: string
    notes?: string
    paymentDate?: string
  }) => apiClient.post<PaymentDto>('/payments', data),
}

export const notificationsApi = {
  getNotifications: (params?: {
    pageNumber?: number
    pageSize?: number
    isRead?: boolean
    type?: string
  }) => apiClient.get<PagedResult<NotificationDto>>('/notifications', params),
  markAsRead: (id: string) => apiClient.put(`/notifications/${id}/read`),
  markAllAsRead: () => apiClient.put('/notifications/mark-all-read'),
}

export const locationsApi = {
  getLocations: (params?: {
    pageNumber?: number
    pageSize?: number
    isActive?: boolean
  }) => apiClient.get<PagedResult<LocationDto>>('/locations', params),
  createLocation: (data: Partial<LocationDto>) => apiClient.post<LocationDto>('/locations', data),
  updateLocation: (id: string, data: Partial<LocationDto>) =>
    apiClient.put<LocationDto>(`/locations/${id}`, data),
  deleteLocation: (id: string) => apiClient.delete(`/locations/${id}`),
}

export const usersApi = {
  getUsers: (params?: {
    pageNumber?: number
    pageSize?: number
    searchTerm?: string
    isActive?: boolean
    role?: string
  }) => apiClient.get<PagedResult<UserDto>>('/users', params),
}
