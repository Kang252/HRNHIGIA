import { apiBasePath } from '../config'

export class ApiError extends Error {
  public readonly status: number
  public readonly code: string | null
  public readonly requestId: string | null
  public readonly details: unknown

  public constructor(
    status: number,
    message: string,
    details: unknown = null,
    code: string | null = null,
    requestId: string | null = null
  ) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.code = code
    this.requestId = requestId
    this.details = details
  }
}

type ApiRequestOptions = Omit<RequestInit, 'body' | 'headers'> & {
  body?: unknown
  csrfToken?: string
  headers?: HeadersInit
}

function requestUrl(path: string): string {
  if (!path.startsWith('/')) {
    throw new Error('Đường dẫn API phải bắt đầu bằng dấu /.')
  }

  return `${apiBasePath}${path}`
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value)
}

function optionalText(value: unknown): string | null {
  return typeof value === 'string' && value.trim().length > 0 ? value : null
}

function apiErrorFromPayload(status: number, payload: unknown): ApiError {
  const defaultMessage = `Yêu cầu API không thành công (HTTP ${status}).`
  if (!isRecord(payload)) {
    return new ApiError(status, defaultMessage, payload)
  }

  const error = isRecord(payload.error) ? payload.error : null
  const message = optionalText(error?.message) ?? optionalText(payload.message) ?? defaultMessage
  const code = optionalText(error?.code) ?? optionalText(payload.code)
  const requestId = optionalText(payload.requestId)
  return new ApiError(status, message, payload, code, requestId)
}

async function getResponseBody(response: Response): Promise<unknown> {
  const contentType = response.headers.get('content-type') || ''
  if (contentType.includes('application/json')) {
    return response.json().catch(() => null)
  }

  return response.text().catch(() => '')
}

export async function apiRequest<T>(path: string, options: ApiRequestOptions = {}): Promise<T> {
  const { body, csrfToken, headers, ...requestOptions } = options
  const requestHeaders = new Headers(headers)
  requestHeaders.set('Accept', 'application/json')
  requestHeaders.set('X-Requested-With', 'XMLHttpRequest')

  if (csrfToken) {
    requestHeaders.set('X-CSRF-TOKEN', csrfToken)
  }

  let requestBody: BodyInit | undefined
  if (body !== undefined) {
    requestHeaders.set('Content-Type', 'application/json')
    requestBody = JSON.stringify(body)
  }

  const response = await fetch(requestUrl(path), {
    ...requestOptions,
    headers: requestHeaders,
    body: requestBody,
    credentials: 'same-origin'
  })
  const responseBody = await getResponseBody(response)

  if (!response.ok) {
    throw apiErrorFromPayload(response.status, responseBody)
  }

  if (isRecord(responseBody) && responseBody.success === false) {
    throw apiErrorFromPayload(response.status, responseBody)
  }

  if (isRecord(responseBody) && responseBody.success === true && 'data' in responseBody) {
    return responseBody.data as T
  }

  return responseBody as T
}

export function isApiError(error: unknown): error is ApiError {
  return error instanceof ApiError
}
