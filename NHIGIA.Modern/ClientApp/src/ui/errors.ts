import { isApiError } from '../api/client'

export function errorMessage(error: unknown): string {
  if (isApiError(error)) {
    return error.message
  }
  if (error instanceof Error && error.message.trim()) {
    return error.message
  }
  return 'Không thể hoàn tất yêu cầu. Vui lòng thử lại.'
}

export function errorRequestId(error: unknown): string | null {
  return isApiError(error) ? error.requestId : null
}

export function isUnauthorized(error: unknown): boolean {
  return isApiError(error) && error.status === 401
}
