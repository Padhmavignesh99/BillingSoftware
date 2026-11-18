import path from 'path'
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    proxy: {
      '/api': { target: 'http://localhost:5224', changeOrigin: true },
      '/health': { target: 'http://localhost:5224', changeOrigin: true },
    },
  },
  resolve: {
    alias: {
      '@': path.resolve(import.meta.dirname ?? path.dirname(new URL(import.meta.url).pathname), './src'),
    },
  },
})
