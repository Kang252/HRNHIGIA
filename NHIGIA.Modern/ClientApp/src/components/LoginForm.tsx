import { useState, type FormEvent } from 'react'
import type { LoginCredentials } from '../api/hrm'
import { errorMessage, errorRequestId } from '../ui/errors'

type LoginFormProps = {
  onSubmit: (credentials: LoginCredentials) => Promise<void>
}

export function LoginForm({ onSubmit }: LoginFormProps) {
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [rememberMe, setRememberMe] = useState(false)
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState<unknown>(null)

  async function submit(event: FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault()
    setSubmitting(true)
    setError(null)
    try {
      await onSubmit({ username: username.trim(), password, rememberMe })
    } catch (nextError) {
      setError(nextError)
    } finally {
      setSubmitting(false)
    }
  }

  const requestId = errorRequestId(error)
  return (
    <main className="login-page">
      <section className="login-card" aria-labelledby="login-title">
        <a className="brand login-brand" href="#dashboard">
          <span className="brand-mark" aria-hidden="true">N</span>
          <span><strong>NHỊ GIA</strong><small>HRM Modern</small></span>
        </a>
        <p className="eyebrow">Đăng nhập</p>
        <h1 id="login-title">Đăng nhập hệ thống</h1>
        <p className="login-intro">Sử dụng tài khoản Nhị Gia của bạn để tiếp tục.</p>

        {error !== null && (
          <div className="form-alert" role="alert">
            <strong>Không thể đăng nhập.</strong>
            <span>{errorMessage(error)}</span>
            {requestId && <small>Mã yêu cầu: {requestId}</small>}
          </div>
        )}

        <form className="login-form" onSubmit={submit} aria-busy={submitting}>
          <label>
            <span>Tên đăng nhập</span>
            <input
              name="username"
              value={username}
              onChange={(event) => setUsername(event.target.value)}
              autoComplete="username"
              required
              disabled={submitting}
            />
          </label>
          <label>
            <span>Mật khẩu</span>
            <input
              name="password"
              type="password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              autoComplete="current-password"
              required
              disabled={submitting}
            />
          </label>
          <label className="remember-choice">
            <input
              name="rememberMe"
              type="checkbox"
              checked={rememberMe}
              onChange={(event) => setRememberMe(event.target.checked)}
              disabled={submitting}
            />
            <span>Ghi nhớ đăng nhập trên thiết bị này</span>
          </label>
          <button className="primary-button login-button" type="submit" disabled={submitting}>
            {submitting ? 'Đang đăng nhập…' : 'Đăng nhập'}
          </button>
        </form>
      </section>
    </main>
  )
}
