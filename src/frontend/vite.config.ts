import { loadEnv } from 'vite'
import { defineConfig } from 'vitest/config'
import react from '@vitejs/plugin-react'

export default defineConfig(({ mode }) => {
  const apiProxyTarget = loadEnv(mode, '.', '').VITE_API_PROXY_TARGET

  return {
    plugins: [react()],
    server: {
      port: 5173,
      proxy: apiProxyTarget
        ? {
            '/api': apiProxyTarget,
            '/health': apiProxyTarget,
          }
        : undefined,
    },
    test: {
      environment: 'jsdom',
      setupFiles: './src/test-setup.ts',
    },
  }
})
