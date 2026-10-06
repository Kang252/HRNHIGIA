import type { ReactNode } from 'react'

export function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value)
}

const labels: Record<string, string> = {
  attendance: 'Chấm công',
  category: 'Danh mục',
  commentCount: 'Bình luận',
  content: 'Nội dung',
  createdAt: 'Ngày tạo',
  date: 'Ngày',
  description: 'Mô tả',
  displayName: 'Tên hiển thị',
  email: 'Email',
  employeeCount: 'Nhân viên',
  leaveBalance: 'Phép còn lại',
  name: 'Tên',
  pendingRequests: 'Đơn chờ xử lý',
  publishedAt: 'Ngày đăng',
  role: 'Vai trò',
  status: 'Trạng thái',
  statusCode: 'Trạng thái',
  submittedAt: 'Ngày gửi',
  summary: 'Tổng quan',
  title: 'Tiêu đề',
  total: 'Tổng số',
  updatedAt: 'Cập nhật lúc',
  username: 'Tên đăng nhập'
}

export function labelForKey(key: string): string {
  if (labels[key]) return labels[key]
  return key
    .replace(/([a-z0-9])([A-Z])/g, '$1 $2')
    .replace(/[_-]+/g, ' ')
    .replace(/^./, (value) => value.toUpperCase())
}

export function firstText(record: Record<string, unknown>, keys: string[]): string | null {
  for (const key of keys) {
    const value = record[key]
    if (typeof value === 'string' && value.trim()) return value.trim()
    if (typeof value === 'number' || typeof value === 'boolean') return String(value)
  }
  return null
}

function tryFormatDate(value: string): string | null {
  if (!/^\d{4}-\d{2}-\d{2}(?:[T\s].*)?$/.test(value)) return null
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return null
  const hasTime = value.includes('T') || value.includes(':')
  return new Intl.DateTimeFormat('vi-VN', hasTime
    ? { dateStyle: 'medium', timeStyle: 'short' }
    : { dateStyle: 'medium' }).format(date)
}

export function formatValue(value: unknown): string {
  if (value === null || value === undefined || value === '') return '—'
  if (typeof value === 'boolean') return value ? 'Có' : 'Không'
  if (typeof value === 'number') return new Intl.NumberFormat('vi-VN').format(value)
  if (typeof value === 'string') return tryFormatDate(value) ?? value
  if (Array.isArray(value)) return `${value.length} mục`
  if (isRecord(value)) return `${Object.keys(value).length} trường dữ liệu`
  return String(value)
}

function ScalarValue({ value }: { value: unknown }) {
  return <span>{formatValue(value)}</span>
}

function ObjectValue({ value }: { value: Record<string, unknown> }) {
  const entries = Object.entries(value).slice(0, 12)
  return (
    <dl className="data-list">
      {entries.map(([key, item]) => (
        <div key={key}>
          <dt>{labelForKey(key)}</dt>
          <dd>{isRecord(item) || Array.isArray(item) ? formatValue(item) : <ScalarValue value={item} />}</dd>
        </div>
      ))}
    </dl>
  )
}

export function DataValue({ value }: { value: unknown }): ReactNode {
  if (isRecord(value)) return <ObjectValue value={value} />
  if (Array.isArray(value)) return <CollectionPreview value={value} />
  return <ScalarValue value={value} />
}

export function CollectionPreview({ value }: { value: unknown[] }) {
  if (value.length === 0) return <p className="muted-copy">Chưa có dữ liệu.</p>

  const records = value.filter(isRecord)
  if (records.length === 0) {
    return <p className="data-inline-value">{value.slice(0, 8).map(formatValue).join(', ')}</p>
  }

  const columns = Array.from(new Set(records.slice(0, 4).flatMap((item) => Object.keys(item)))).slice(0, 5)
  return (
    <div className="data-table-wrap" tabIndex={0}>
      <table>
        <thead>
          <tr>{columns.map((column) => <th key={column}>{labelForKey(column)}</th>)}</tr>
        </thead>
        <tbody>
          {records.slice(0, 8).map((record, index) => (
            <tr key={String(record.id ?? record.key ?? index)}>
              {columns.map((column) => <td key={column}>{formatValue(record[column])}</td>)}
            </tr>
          ))}
        </tbody>
      </table>
      {value.length > 8 && <p className="muted-copy">Hiển thị 8 trong {value.length} mục API trả về.</p>}
    </div>
  )
}
