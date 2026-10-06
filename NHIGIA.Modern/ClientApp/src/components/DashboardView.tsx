import { useCallback, useEffect, useMemo, useState } from 'react'
import { getDashboard } from '../api/hrm'
import { DataValue, formatValue, isRecord, labelForKey } from '../ui/data'
import { errorMessage, errorRequestId, isUnauthorized } from '../ui/errors'

type DashboardViewProps = {
  onSessionExpired: () => void
}

type DashboardState =
  | { status: 'loading'; data: null; error: null }
  | { status: 'ready'; data: unknown; error: null }
  | { status: 'error'; data: null; error: unknown }

function DashboardContent({ data }: { data: unknown }) {
  const record = useMemo(() => isRecord(data) ? data : { data }, [data])
  const entries = Object.entries(record)
  const scalarEntries = entries.filter(([, value]) => !isRecord(value) && !Array.isArray(value))
  const detailEntries = entries.filter(([, value]) => isRecord(value) || Array.isArray(value))

  if (entries.length === 0) {
    return <div className="empty-state compact"><h2>API chưa trả về dữ liệu tổng quan.</h2></div>
  }

  return (
    <div className="dashboard-data">
      {scalarEntries.length > 0 && (
        <section className="metric-grid" aria-label="Chỉ số từ dashboard">
          {scalarEntries.map(([key, value]) => (
            <article className="metric-card" key={key}>
              <p>{labelForKey(key)}</p>
              <strong>{formatValue(value)}</strong>
            </article>
          ))}
        </section>
      )}
      {detailEntries.map(([key, value]) => (
        <section className="data-panel" key={key} aria-label={labelForKey(key)}>
          <h2>{labelForKey(key)}</h2>
          <DataValue value={value} />
        </section>
      ))}
    </div>
  )
}

export function DashboardView({ onSessionExpired }: DashboardViewProps) {
  const [state, setState] = useState<DashboardState>({ status: 'loading', data: null, error: null })

  const load = useCallback(async (signal?: AbortSignal) => {
    setState({ status: 'loading', data: null, error: null })
    try {
      const data = await getDashboard(signal)
      if (!signal?.aborted) setState({ status: 'ready', data, error: null })
    } catch (error) {
      if (signal?.aborted) return
      if (isUnauthorized(error)) onSessionExpired()
      else setState({ status: 'error', data: null, error })
    }
  }, [onSessionExpired])

  useEffect(() => {
    const controller = new AbortController()
    void load(controller.signal)
    return () => controller.abort()
  }, [load])

  if (state.status === 'loading') {
    return (
      <section className="loading-section" aria-busy="true" aria-live="polite">
        <p>Đang tải dữ liệu tổng quan…</p>
        <div className="loading-grid" aria-hidden="true"><span /><span /><span /></div>
      </section>
    )
  }

  if (state.status === 'error') {
    const requestId = errorRequestId(state.error)
    return (
      <section className="error-state" role="alert">
        <h2>Không thể tải tổng quan</h2>
        <p>{errorMessage(state.error)}</p>
        {requestId && <small>Mã yêu cầu: {requestId}</small>}
        <button type="button" className="primary-button" onClick={() => void load()}>Tải lại</button>
      </section>
    )
  }

  return <DashboardContent data={state.data} />
}
