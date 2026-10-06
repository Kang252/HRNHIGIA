import { useCallback, useEffect, useMemo, useState } from 'react'
import { getCurrentUser, login, logout, type ApiUser, type LoginCredentials } from './api/hrm'
import { CommunicationsView } from './components/CommunicationsView'
import { DashboardView } from './components/DashboardView'
import { LoginForm } from './components/LoginForm'
import { apiBasePath } from './config'
import { errorMessage, errorRequestId, isUnauthorized } from './ui/errors'

type ViewKey = 'dashboard' | 'communications'

type AuthState =
  | { status: 'loading'; user: null; error: null }
  | { status: 'anonymous'; user: null; error: null }
  | { status: 'authenticated'; user: ApiUser; error: null }
  | { status: 'error'; user: null; error: unknown }

const navigation: Array<{ key: ViewKey; label: string; description: string }> = [
  { key: 'dashboard', label: 'Tổng quan', description: 'Dữ liệu vận hành từ API' },
  { key: 'communications', label: 'Truyền thông', description: 'Bài viết nội bộ' }
]

function readHash(): ViewKey {
  const hash = window.location.hash.replace('#', '')
  return navigation.some((item) => item.key === hash) ? (hash as ViewKey) : 'dashboard'
}

function avatarText(name: string): string {
  return name.trim().slice(0, 1).toLocaleUpperCase('vi-VN') || 'N'
}

function SessionLoading() {
  return (
    <main className="session-loading" aria-busy="true" aria-live="polite">
      <span className="brand-mark" aria-hidden="true">N</span>
      <p>Đang kiểm tra phiên làm việc…</p>
    </main>
  )
}

function SessionError({ error, onRetry }: { error: unknown; onRetry: () => void }) {
  return (
    <main className="login-page">
      <section className="login-card error-card" role="alert">
        <p className="eyebrow">Không thể mở phiên</p>
        <h1>Không thể kết nối dịch vụ xác thực</h1>
        <p>{errorMessage(error)}</p>
        {errorRequestId(error) && <small>Mã yêu cầu: {errorRequestId(error)}</small>}
        <button type="button" className="primary-button" onClick={onRetry}>Thử lại</button>
      </section>
    </main>
  )
}

export default function App() {
  const [view, setView] = useState<ViewKey>(readHash)
  const [auth, setAuth] = useState<AuthState>({ status: 'loading', user: null, error: null })
  const [logoutError, setLogoutError] = useState<unknown>(null)
  const [isLoggingOut, setIsLoggingOut] = useState(false)

  const establishSession = useCallback(async () => {
    setAuth({ status: 'loading', user: null, error: null })
    try {
      const user = await getCurrentUser()
      setAuth({ status: 'authenticated', user, error: null })
    } catch (error) {
      if (isUnauthorized(error)) setAuth({ status: 'anonymous', user: null, error: null })
      else setAuth({ status: 'error', user: null, error })
    }
  }, [])

  useEffect(() => {
    void establishSession()
  }, [establishSession])

  useEffect(() => {
    const updateView = () => setView(readHash())
    window.addEventListener('hashchange', updateView)
    return () => window.removeEventListener('hashchange', updateView)
  }, [])

  const navigate = useCallback((nextView: ViewKey) => {
    setView(nextView)
    window.location.hash = nextView
  }, [])

  const sessionExpired = useCallback(() => {
    setAuth({ status: 'anonymous', user: null, error: null })
    setLogoutError(null)
  }, [])

  async function submitLogin(credentials: LoginCredentials): Promise<void> {
    await login(credentials)
    const user = await getCurrentUser()
    setAuth({ status: 'authenticated', user, error: null })
    navigate('dashboard')
  }

  async function submitLogout(): Promise<void> {
    setLogoutError(null)
    setIsLoggingOut(true)
    try {
      await logout()
      sessionExpired()
    } catch (error) {
      if (isUnauthorized(error)) sessionExpired()
      else setLogoutError(error)
    } finally {
      setIsLoggingOut(false)
    }
  }

  const currentItem = useMemo(
    () => navigation.find((item) => item.key === view) ?? navigation[0],
    [view]
  )

  if (auth.status === 'loading') return <SessionLoading />
  if (auth.status === 'error') return <SessionError error={auth.error} onRetry={() => void establishSession()} />
  if (auth.status === 'anonymous') return <LoginForm apiBasePath={apiBasePath} onSubmit={submitLogin} />

  return (
    <div className="app-shell">
      <aside className="sidebar" aria-label="Điều hướng hệ thống">
        <a className="brand" href="#dashboard" onClick={() => navigate('dashboard')}>
          <span className="brand-mark" aria-hidden="true">N</span>
          <span><strong>NHỊ GIA</strong><small>HRM Modern</small></span>
        </a>

        <nav className="navigation">
          <p className="navigation-heading">Không gian làm việc</p>
          {navigation.map((item) => (
            <a
              className={item.key === view ? 'nav-link active' : 'nav-link'}
              href={`#${item.key}`}
              key={item.key}
              aria-current={item.key === view ? 'page' : undefined}
            >
              <span>{item.label}</span>
              <small>{item.description}</small>
            </a>
          ))}
        </nav>

        <div className="sidebar-note">
          <span className="dot" aria-hidden="true" />
          Dữ liệu được kiểm soát tại API
        </div>
      </aside>

      <main className="content">
        <header className="topbar">
          <div>
            <p className="eyebrow">NHỊ GIA HRM</p>
            <h1>{currentItem.label}</h1>
            <p>{currentItem.description}</p>
          </div>
          <div className="topbar-actions">
            <div className="account-chip" aria-label={`Đăng nhập với ${auth.user.displayName}`}>
              <span className="avatar" aria-hidden="true">{avatarText(auth.user.displayName)}</span>
              <span>
                <strong>{auth.user.displayName}</strong>
                <small>{auth.user.roles.length > 0 ? auth.user.roles.join(', ') : auth.user.username ?? auth.user.email ?? 'Phiên API'}</small>
              </span>
            </div>
            <button type="button" className="logout-button" onClick={() => void submitLogout()} disabled={isLoggingOut}>
              {isLoggingOut ? 'Đang đăng xuất…' : 'Đăng xuất'}
            </button>
          </div>
        </header>

        {logoutError !== null && (
          <div className="form-alert inline-alert" role="alert">
            <strong>Không thể đăng xuất.</strong>
            <span>{errorMessage(logoutError)}</span>
            {errorRequestId(logoutError) && <small>Mã yêu cầu: {errorRequestId(logoutError)}</small>}
          </div>
        )}

        {view === 'dashboard' && <DashboardView onSessionExpired={sessionExpired} />}
        {view === 'communications' && <CommunicationsView onSessionExpired={sessionExpired} />}
      </main>
    </div>
  )
}
