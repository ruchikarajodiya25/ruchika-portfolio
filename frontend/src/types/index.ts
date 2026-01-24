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

export interface ServiceDto {
  id: string
  name: string
  description?: string
  price: number
  durationMinutes: number
  category?: string
  taxCategory?: string
  taxRate: number
  isActive: boolean
  createdAt: string
  updatedAt?: string
}

export interface AppointmentDto {
  id: string
  customerId: string
  customerName?: string
  serviceId?: string
  serviceName?: string
  staffId?: string
  staffName?: string
  locationId: string
  locationName?: string
  scheduledStart: string
  scheduledEnd: string
  status: string
  notes?: string
  internalNotes?: string
  createdAt: string
  updatedAt?: string
}

export interface WorkOrderDto {
  id: string
  workOrderNumber: string
  customerId: string
  customerName?: string
  appointmentId?: string
  assignedToUserId?: string
  locationId: string
  status: string
  description?: string
  internalNotes?: string
  totalAmount: number
  startedAt?: string
  completedAt?: string
  createdAt: string
}

export interface ProductDto {
  id: string
  name: string
  description?: string
  sku?: string
  category?: string
  unitPrice: number
  costPrice: number
  stockQuantity: number
  lowStockThreshold: number
  unit?: string
  isActive: boolean
  locationId: string
  createdAt: string
}

export interface InvoiceDto {
  id: string
  invoiceNumber: string
  customerId: string
  customerName?: string
  workOrderId?: string
  invoiceDate: string
  dueDate?: string
  status: string
  subTotal: number
  taxAmount: number
  discountAmount: number
  totalAmount: number
  paidAmount: number
  items: InvoiceItemDto[]
  createdAt: string
}

export interface InvoiceItemDto {
  id: string
  itemType: string
  serviceId?: string
  productId?: string
  description: string
  quantity: number
  unitPrice: number
  taxRate: number
  totalAmount: number
}

export interface DashboardStatsDto {
  totalRevenue: number
  totalAppointments: number
  activeAppointments: number
  totalCustomers: number
  pendingInvoices: number
  pendingInvoiceAmount: number
  lowStockProducts: number
  topServices: TopServiceDto[]
  recentAppointments: RecentAppointmentDto[]
}

export interface TopServiceDto {
  serviceId: string
  serviceName: string
  count: number
  revenue: number
}

export interface RecentAppointmentDto {
  id: string
  customerName: string
  scheduledStart: string
  status: string
}

export interface PaymentDto {
  id: string
  paymentNumber: string
  invoiceId: string
  paymentDate: string
  amount: number
  paymentMethod: string
  referenceNumber?: string
  notes?: string
  createdAt: string
}

export interface NotificationDto {
  id: string
  type: string
  title: string
  message: string
  isRead: boolean
  readAt?: string
  linkUrl?: string
  relatedEntityType?: string
  relatedEntityId?: string
  createdAt: string
}

export interface LocationDto {
  id: string
  name: string
  address?: string
  city?: string
  state?: string
  zipCode?: string
  phone?: string
  email?: string
  isActive: boolean
  createdAt: string
}
