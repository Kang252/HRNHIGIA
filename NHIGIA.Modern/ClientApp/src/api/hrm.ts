import { apiRequest } from './client'

export type ApiUser = {
  id: string | null
  username: string | null
  displayName: string
  email: string | null
  roles: string[]
}

export type LoginCredentials = {
  username: string
  password: string
  rememberMe?: boolean
}

export type CommunicationFilters = {
  keyword: string
  category: string
  status: string
}

export type CommunicationResult = {
  items: Record<string, unknown>[]
  total: number | null
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value)
}

function firstText(record: Record<string, unknown>, keys: string[]): string | null {
  for (const key of keys) {
    const value = record[key]
    if (typeof value === 'string' && value.trim().length > 0) {
      return value.trim()
    }
    if (typeof value === 'number') {
      return String(value)
    }
  }
  return null
}

function textList(value: unknown): string[] {
  if (Array.isArray(value)) {
    return value.filter((item): item is string => typeof item === 'string' && item.trim().length > 0)
  }
  if (typeof value === 'string' && value.trim().length > 0) {
    return [value.trim()]
  }
  return []
}

export function normalizeUser(payload: unknown): ApiUser {
  const data = isRecord(payload) && isRecord(payload.user) ? payload.user : payload
  if (!isRecord(data)) {
    throw new Error('API /auth/me không trả về thông tin người dùng hợp lệ.')
  }

  const id = firstText(data, ['id', 'userId', 'employeeId'])
  const username = firstText(data, ['username', 'userName', 'loginName'])
  const email = firstText(data, ['email', 'emailAddress'])
  const displayName = firstText(data, ['displayName', 'fullName', 'name', 'employeeName']) ?? username ?? email ?? id
  if (!displayName) {
    throw new Error('API /auth/me thiếu tên định danh của người dùng.')
  }

  return {
    id,
    username,
    displayName,
    email,
    roles: textList(data.roles ?? data.roleNames ?? data.role ?? data.roleCode)
  }
}

export async function login(credentials: LoginCredentials): Promise<void> {
  await apiRequest<unknown>('/auth/login', {
    method: 'POST',
    body: credentials
  })
}

export async function getCurrentUser(signal?: AbortSignal): Promise<ApiUser> {
  const payload = await apiRequest<unknown>('/auth/me', { method: 'GET', signal })
  return normalizeUser(payload)
}

export async function logout(): Promise<void> {
  await apiRequest<unknown>('/auth/logout', { method: 'POST' })
}

export function getDashboard(signal?: AbortSignal): Promise<unknown> {
  return apiRequest<unknown>('/dashboard', { method: 'GET', signal })
}

function readNumber(value: unknown): number | null {
  if (typeof value === 'number' && Number.isFinite(value)) {
    return value
  }
  if (typeof value === 'string' && value.trim() !== '' && Number.isFinite(Number(value))) {
    return Number(value)
  }
  return null
}

const communicationStatuses = new Set(['PUBLISHED', 'PENDING', 'REJECTED'])

function normalizeCommunicationStatus(value: string): string {
  const status = value.trim().toUpperCase()
  if (status && !communicationStatuses.has(status)) {
    throw new Error('Trạng thái truyền thông chỉ nhận PUBLISHED, PENDING hoặc REJECTED.')
  }
  return status
}

function findCollection(payload: unknown): unknown[] {
  if (Array.isArray(payload)) {
    return payload
  }
  if (!isRecord(payload)) {
    return []
  }

  for (const key of ['items', 'records', 'results', 'communications', 'posts']) {
    if (Array.isArray(payload[key])) {
      return payload[key]
    }
  }

  return []
}

export async function getCommunications(filters: CommunicationFilters, signal?: AbortSignal): Promise<CommunicationResult> {
  const query = new URLSearchParams()
  if (filters.keyword.trim()) query.set('keyword', filters.keyword.trim())
  if (filters.category.trim()) query.set('category', filters.category.trim())
  const status = normalizeCommunicationStatus(filters.status)
  if (status) query.set('status', status)

  const suffix = query.size > 0 ? `?${query.toString()}` : ''
  const payload = await apiRequest<unknown>(`/communications${suffix}`, { method: 'GET', signal })
  const pagination = isRecord(payload) && isRecord(payload.pagination) ? payload.pagination : isRecord(payload) && isRecord(payload.meta) ? payload.meta : null
  const total = isRecord(payload) ? readNumber(payload.total ?? payload.count ?? pagination?.total) : null
  const items = findCollection(payload).filter(isRecord)
  return { items, total }
}
