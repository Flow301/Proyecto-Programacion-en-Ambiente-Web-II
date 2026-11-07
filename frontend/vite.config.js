import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// Puerto fijo: es el origen que la API permite en CORS (appsettings.json, Cors:Origenes)
export default defineConfig({
  plugins: [react()],
  server: { port: 5173, strictPort: true },
})
