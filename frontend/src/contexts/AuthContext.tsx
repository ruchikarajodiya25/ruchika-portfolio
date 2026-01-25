import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react'
import { useNavigate } from 'react-router-dom'
import { authApi } from '../services/api'
import { AuthResponseDto, UserDto } from '../types'

interface AuthContextType {
  user: UserDto | null
  token: string | null
  isAuthenticated: boolean
  login: (email: string, password: string) => Promise<void>
  register: (data: RegisterData) => Promise<void>
  logout: () => void
  loading: boolean
}

interface RegisterData {
  email: string
  password: string
  firstName: string
  lastName: string
  businessName: string
  phone?: string
}

const AuthContext = createContext<AuthContextType | undefined>(undefined)

export const AuthProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<UserDto | null>(null)
  const [token, setToken] = useState<string | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    const storedToken = localStorage.getItem('accessToken')
    const storedUser = localStorage.getItem('user')
    
    if (storedToken && storedUser) {
      setToken(storedToken)
      setUser(JSON.parse(storedUser))
    }
    setLoading(false)
  }, [])

  const login = async (email: string, password: string) => {
    try {
      console.log('🔑 Attempting login...')
      const response = await authApi.login({ email, password })
      console.log('📥 Login response received:', response.status, response.statusText)
      const apiResponse = response.data
      
      // Check if request was successful
      if (!apiResponse?.success) {
        console.error('❌ Login failed:', apiResponse?.message)
        throw new Error(apiResponse?.message || 'Login failed. Please try again.')
      }
      
      // Extract data from response
      if (!apiResponse.data) {
        console.error('❌ Invalid response format:', apiResponse)
        throw new Error('Invalid response format from server')
      }
      
      const { accessToken, refreshToken, user } = apiResponse.data
      
      if (!accessToken || !user) {
        console.error('❌ Missing auth data:', { accessToken: !!accessToken, user: !!user })
        throw new Error('Missing authentication data in response')
      }
      
      // Store tokens and user
      setToken(accessToken)
      setUser(user)
      localStorage.setItem('accessToken', accessToken)
      if (refreshToken) {
        localStorage.setItem('refreshToken', refreshToken)
      }
      localStorage.setItem('user', JSON.stringify(user))
      console.log('✅ Login successful, tokens stored')
    } catch (error: any) {
      console.error('❌ Login error:', error)
      
      // Handle network errors (cert issues, CORS, etc.)
      if (error.code === 'ERR_NETWORK' || error.message?.includes('Network Error') || error.code === 'ERR_CONNECTION_REFUSED') {
        console.error('🌐 Network error - API server may not be running')
        throw new Error('Cannot connect to server. Make sure the API is running on http://localhost:7000. Check console for details.')
      }
      
      // Handle HTTP errors
      if (error.response) {
        console.error('📡 HTTP error:', error.response.status, error.response.data)
        const apiResponse = error.response.data
        const message = apiResponse?.message || error.response.statusText || 'Login failed. Please try again.'
        throw new Error(message)
      }
      
      // Re-throw other errors
      throw error
    }
  }

  const register = async (data: RegisterData) => {
    try {
      const response = await authApi.register(data)
      const apiResponse = response.data
      
      if (!apiResponse?.success) {
        throw new Error(apiResponse?.message || 'Registration failed. Please try again.')
      }
      
      if (!apiResponse.data) {
        throw new Error('Invalid response format from server')
      }
      
      const { accessToken, refreshToken, user } = apiResponse.data
      
      if (!accessToken || !user) {
        throw new Error('Missing authentication data in response')
      }
      
      setToken(accessToken)
      setUser(user)
      localStorage.setItem('accessToken', accessToken)
      if (refreshToken) {
        localStorage.setItem('refreshToken', refreshToken)
      }
      localStorage.setItem('user', JSON.stringify(user))
    } catch (error: any) {
      if (error.code === 'ERR_NETWORK' || error.message?.includes('Network Error')) {
        throw new Error('Cannot connect to server. Make sure the API is running on http://localhost:7000 or trust the HTTPS certificate.')
      }
      
      if (error.response) {
        const apiResponse = error.response.data
        const message = apiResponse?.message || error.response.statusText || 'Registration failed. Please try again.'
        throw new Error(message)
      }
      
      throw error
    }
  }

  const logout = () => {
    setToken(null)
    setUser(null)
    localStorage.removeItem('accessToken')
    localStorage.removeItem('refreshToken')
    localStorage.removeItem('user')
  }

  return (
    <AuthContext.Provider
      value={{
        user,
        token,
        isAuthenticated: !!token && !!user,
        login,
        register,
        logout,
        loading,
      }}
    >
      {children}
    </AuthContext.Provider>
  )
}

export const useAuth = () => {
  const context = useContext(AuthContext)
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider')
  }
  return context
}
