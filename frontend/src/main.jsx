import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router'
import App from './App.jsx'
import AuthProvider from './context/AuthProvider.jsx'
import { Toaster } from './components/ui/sonner.jsx'
import './index.css'

createRoot(document.getElementById('root')).render(
  <StrictMode>
    <BrowserRouter>
      <AuthProvider>
        <App />
        {/* Debajo de la barra superior (3,5 rem): así no tapa "Cambiar contraseña" ni "Cerrar sesión" */}
        <Toaster position="top-right" closeButton offset={{ top: '4.5rem' }} />
      </AuthProvider>
    </BrowserRouter>
  </StrictMode>,
)
