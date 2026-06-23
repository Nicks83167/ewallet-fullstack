import React, { useState, useContext } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import api from '../api/axios';
import { AuthContext } from '../context/AuthContext';

const Login = () => {
  const [form, setForm] = useState({ email: '', password: '' });
  const [error, setError] = useState(null);
  const [loading, setLoading] = useState(false);
  const [showPw, setShowPw] = useState(false);
  const { login } = useContext(AuthContext);
  const navigate = useNavigate();

  const set = (field) => (e) => setForm(f => ({ ...f, [field]: e.target.value }));

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      const res = await api.post('/auth/login', form);
      if (!res.data.success) throw new Error(res.data.errors?.[0] ?? res.data.message);
      const { token, user } = res.data.data;
      login(user, token);
      navigate('/dashboard');
    } catch (err) {
      setError(err.response?.data?.message ?? err.message ?? 'Login failed. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-page">
      <div className="auth-box">
        <div className="auth-logo">
          <span>💳</span> EWallet
        </div>
        <div className="auth-card">
          <h1 className="auth-title">Welcome back</h1>
          <p className="auth-subtitle">Apne account mein sign in karein</p>

          {error && (
            <div className="alert alert-error">
              <span>⚠</span> {error}
            </div>
          )}

          <form onSubmit={handleSubmit}>
            <div className="form-group">
              <label className="form-label">Email address</label>
              <input
                type="email"
                className="form-input"
                placeholder="rahul@example.com"
                value={form.email}
                onChange={set('email')}
                autoComplete="email"
                required
              />
            </div>
            <div className="form-group">
              <label className="form-label" style={{ display: 'flex', justifyContent: 'space-between' }}>
                Password
                <button
                  type="button"
                  className="btn-ghost btn-sm"
                  style={{ padding: 0, fontSize: '0.75rem', color: 'var(--text-3)' }}
                  onClick={() => setShowPw(v => !v)}
                >
                  {showPw ? 'Hide' : 'Show'}
                </button>
              </label>
              <input
                type={showPw ? 'text' : 'password'}
                className="form-input"
                placeholder="••••••••"
                value={form.password}
                onChange={set('password')}
                autoComplete="current-password"
                required
              />
            </div>
            <button type="submit" className="btn btn-full" disabled={loading} style={{ marginTop: '0.5rem' }}>
              {loading ? <><span className="spinner" /> Signing in…</> : 'Sign In'}
            </button>
          </form>
        </div>
        <div className="auth-footer">
          Don't have an account? <Link to="/register">Create one</Link>
        </div>
      </div>
    </div>
  );
};

export default Login;
