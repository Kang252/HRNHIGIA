import { useCallback, useEffect, useState, type FormEvent } from 'react'
import { getCommunications, type CommunicationFilters, type CommunicationResult } from '../api/hrm'
import { firstText, formatValue, isRecord, labelForKey } from '../ui/data'
import { errorMessage, errorRequestId, isUnauthorized } from '../ui/errors'

type CommunicationsViewProps = {
  onSessionExpired: () => void
}

type CommunicationsState =
  | { status: 'loading'; result: null; error: null }
  | { status: 'ready'; result: CommunicationResult; error: null }
  | { status: 'error'; result: null; error: unknown }

const emptyFilters: CommunicationFilters = { keyword: '', category: '', status: '' }

function CommunicationCard({ item, index }: { item: Record<string, unknown>; index: number }) {
  const title = firstText(item, ['title', 'name', 'subject']) ?? 'Không có tiêu đề'
  const content = firstText(item, ['excerpt', 'summary', 'content', 'body', 'description'])
  const category = firstText(item, ['categoryName', 'category'])
  const status = firstText(item, ['statusName', 'statusCode', 'status'])
  const author = firstText(item, ['authorName', 'createdByName', 'author', 'createdBy'])
  const createdAt = firstText(item, ['publishedAt', 'submittedAt', 'createdAt', 'date'])
  const stableKey = String(item.id ?? item.communicationId ?? item.postId ?? `${title}-${index}`)
  const hiddenKeys = new Set(['id', 'communicationId', 'postId', 'title', 'name', 'subject', 'excerpt', 'summary', 'content', 'body', 'description', 'categoryName', 'category', 'statusName', 'statusCode', 'status', 'authorName', 'createdByName', 'author', 'createdBy', 'publishedAt', 'submittedAt', 'createdAt', 'date'])
  const extraEntries = Object.entries(item).filter(([key]) => !hiddenKeys.has(key)).slice(0, 5)

  return (
    <article className="communication-card" key={stableKey}>
      <div className="communication-meta">
        {category && <span className="tag">{category}</span>}
        {status && <span className="tag muted-tag">{status}</span>}
      </div>
      <h2>{title}</h2>
      {content && <p className="communication-content">{content}</p>}
      {(author || createdAt) && (
        <p className="communication-byline">
          {author && <span>{author}</span>}
          {author && createdAt && <span aria-hidden="true">·</span>}
          {createdAt && <time>{formatValue(createdAt)}</time>}
        </p>
      )}
      {extraEntries.length > 0 && (
        <details className="communication-details">
          <summary>Thông tin bài viết</summary>
          <dl className="data-list">
            {extraEntries.map(([key, value]) => (
              <div key={key}>
                <dt>{labelForKey(key)}</dt>
                <dd>{isRecord(value) || Array.isArray(value) ? formatValue(value) : formatValue(value)}</dd>
              </div>
            ))}
          </dl>
        </details>
      )}
    </article>
  )
}

export function CommunicationsView({ onSessionExpired }: CommunicationsViewProps) {
  const [inputFilters, setInputFilters] = useState<CommunicationFilters>(emptyFilters)
  const [appliedFilters, setAppliedFilters] = useState<CommunicationFilters>(emptyFilters)
  const [state, setState] = useState<CommunicationsState>({ status: 'loading', result: null, error: null })

  const load = useCallback(async (filters: CommunicationFilters, signal?: AbortSignal) => {
    setState({ status: 'loading', result: null, error: null })
    try {
      const result = await getCommunications(filters, signal)
      if (!signal?.aborted) setState({ status: 'ready', result, error: null })
    } catch (error) {
      if (signal?.aborted) return
      if (isUnauthorized(error)) onSessionExpired()
      else setState({ status: 'error', result: null, error })
    }
  }, [onSessionExpired])

  useEffect(() => {
    const controller = new AbortController()
    void load(appliedFilters, controller.signal)
    return () => controller.abort()
  }, [appliedFilters, load])

  function applyFilters(event: FormEvent<HTMLFormElement>): void {
    event.preventDefault()
    setAppliedFilters({ ...inputFilters })
  }

  function resetFilters(): void {
    setInputFilters(emptyFilters)
    setAppliedFilters(emptyFilters)
  }

  const appliedCount = Object.values(appliedFilters).filter((value) => value.trim()).length
  return (
    <section className="communications-page" aria-label="Danh sách truyền thông nội bộ">
      <form className="filter-panel" onSubmit={applyFilters} aria-busy={state.status === 'loading'}>
        <label className="filter-keyword">
          <span>Tìm bài viết</span>
          <input value={inputFilters.keyword} onChange={(event) => setInputFilters((current) => ({ ...current, keyword: event.target.value }))} placeholder="Tiêu đề hoặc nội dung" />
        </label>
        <label>
          <span>Danh mục</span>
          <input value={inputFilters.category} onChange={(event) => setInputFilters((current) => ({ ...current, category: event.target.value }))} placeholder="Mã hoặc tên danh mục" />
        </label>
        <label>
          <span>Trạng thái</span>
          <select value={inputFilters.status} onChange={(event) => setInputFilters((current) => ({ ...current, status: event.target.value }))}>
            <option value="">Tất cả trạng thái</option>
            <option value="PUBLISHED">PUBLISHED</option>
            <option value="PENDING">PENDING</option>
            <option value="REJECTED">REJECTED</option>
          </select>
        </label>
        <div className="filter-actions">
          <button type="submit" className="primary-button" disabled={state.status === 'loading'}>Lọc bài viết</button>
          {appliedCount > 0 && <button type="button" className="text-button" onClick={resetFilters}>Xóa bộ lọc</button>}
        </div>
      </form>

      {state.status === 'loading' && (
        <div className="loading-section compact" aria-live="polite" aria-busy="true">
          <p>Đang tải bài viết…</p>
          <div className="loading-grid" aria-hidden="true"><span /><span /><span /></div>
        </div>
      )}

      {state.status === 'error' && (
        <div className="error-state" role="alert">
          <h2>Không thể tải truyền thông nội bộ</h2>
          <p>{errorMessage(state.error)}</p>
          {errorRequestId(state.error) && <small>Mã yêu cầu: {errorRequestId(state.error)}</small>}
          <button type="button" className="primary-button" onClick={() => void load(appliedFilters)}>Tải lại</button>
        </div>
      )}

      {state.status === 'ready' && (
        <div className="communications-result">
          <p className="result-count" aria-live="polite">
            {state.result.total === null
              ? `Đang hiển thị ${state.result.items.length} bài viết API trả về.`
              : `Tổng cộng ${state.result.total} bài viết.`}
          </p>
          {state.result.items.length === 0 ? (
            <div className="empty-state compact"><h2>Không tìm thấy bài viết phù hợp.</h2></div>
          ) : (
            <div className="communications-grid">
              {state.result.items.map((item, index) => <CommunicationCard item={item} index={index} key={String(item.id ?? item.communicationId ?? item.postId ?? index)} />)}
            </div>
          )}
        </div>
      )}
    </section>
  )
}
