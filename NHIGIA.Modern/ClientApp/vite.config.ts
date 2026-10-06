import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'

function normalizeBasePath(value: string | undefined): string {
  const candidate = value?.trim() || '/modern-ui/'
  const withLeadingSlash = candidate.startsWith('/') ? candidate : `/${candidate}`
  return withLeadingSlash.endsWith('/') ? withLeadingSlash : `${withLeadingSlash}/`
}

export default defineConfig(({ mode }) => {
  const environment = loadEnv(mode, process.cwd(), '')
  const apiOrigin = environment.HRM_DEV_API_ORIGIN?.trim() || 'http://localhost:10000'
  const apiRequestOrigin = new URL(apiOrigin).origin

  return {
    base: normalizeBasePath(environment.VITE_APP_BASE_PATH),
    plugins: [react()],
    build: {
      outDir: environment.HRM_CLIENT_OUT_DIR?.trim() || 'dist',
      emptyOutDir: true,
      sourcemap: environment.HRM_CLIENT_SOURCEMAP === 'true'
    },
    server: {
      host: '127.0.0.1',
      port: 5173,
      strictPort: true,
      proxy: {
        '/api': {
          target: apiOrigin,
          changeOrigin: true,
          secure: false,
          // The Nest API only accepts same-origin cookie mutations. During
          // development Vite is the local reverse proxy, so forward the API
          // origin rather than the browser's Vite origin.
          configure: (proxy) => {
            proxy.on('proxyReq', (proxyRequest) => {
              proxyRequest.setHeader('origin', apiRequestOrigin)
            })
          }
        }
      }
    }
  }
})
