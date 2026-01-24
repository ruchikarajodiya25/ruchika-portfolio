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
    const response = await authApi.login({ email, password })
    if (response.data?.data) {
      const { accessToken, user } = response.data.data
      setToken(accessToken)
      setUser(user)
      localStorage.setItem('accessToken', accessToken)
      localStorage.setItem('refreshToken', response.data.data.refreshToken)
      localStorage.setItem('user', JSON.stringify(user))
    }
  }

  const register = async (data: RegisterData) => {
    const response = await authApi.register(data)
    if (response.data?.data) {
      const { accessToken, user } = response.data.data
      setToken(accessToken)
      setUser(user)
      localStorage.setItem('accessToken', accessToken)
      localStorage.setItem('refreshToken', response.data.data.refreshToken)
      localStorage.setItem('user', JSON.stringify(user))
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
