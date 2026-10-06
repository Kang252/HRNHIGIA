const defaultApiBasePath = '/api/v1'

function normalizeApiBasePath(value: string | undefined): string {
  const candidate = value?.trim() || defaultApiBasePath

  if (!candidate.startsWith('/') || candidate.startsWith('//')) {
    throw new Error('VITE_API_BASE_PATH phải là đường dẫn cùng tên miền, ví dụ /api/v1.')
  }

  return candidate.replace(/\/+$/, '') || defaultApiBasePath
}

export const apiBasePath = normalizeApiBasePath(import.meta.env.VITE_API_BASE_PATH)
export const appBasePath = import.meta.env.BASE_URL
