import React from 'react'
import ReactDOM from 'react-dom/client'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter } from 'react-router-dom'
import { AuthProvider } from './contexts/AuthContext'
import App from './App.tsx'
import './index.css'

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: false, // Disable retries to prevent loops
      staleTime: 5 * 60 * 1000, // Cache for 5 minutes
      gcTime: 10 * 60 * 1000, // Keep in cache for 10 minutes (formerly cacheTime)
    },
    mutations: {
      retry: false, // Don't retry mutations
    },
  },
})

// API Health Check on startup
const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:7000/api'
const API_SERVER_URL = API_BASE_URL.replace('/api', '')
console.log(`🔗 API Base URL: ${API_BASE_URL}`)

// Optional: Try to ping the API server (non-blocking)
// Check root URL where Swagger UI is mounted in development
fetch(`${API_SERVER_URL}/`, { method: 'HEAD', mode: 'no-cors' })
  .then(() => {
    console.log('✅ API server is reachable')
  })
  .catch(() => {
    // Silently fail - this is just a health check, not critical
    // Don't log as error since Swagger might not be available in production
  })

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <AuthProvider>
          <App />
        </AuthProvider>
      </BrowserRouter>
    </QueryClientProvider>
  </React.StrictMode>,
)
