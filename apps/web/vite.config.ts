import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'path'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  // O arquivo .env está na raiz do monorepo (dois níveis acima de apps/web)
  envDir: path.resolve(__dirname, '../..'),
  server: {
    host: '127.0.0.1',
    port: 5173,
  },
})
