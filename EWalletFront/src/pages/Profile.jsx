import React, { useState, useContext } from 'react';
import api from '../api/axios';
import { AuthContext } from '../context/AuthContext';
import { formatDate } from '../utils/format';

const Profile = () => {
  const { user, updateUser } = useContext(AuthContext);

  const [nameForm, setNameForm] = useState({ fullName: user?.fullName ?? '' });
  const [nameStatus, setNameStatus] = useState(null);
  const [nameSaving, setNameSaving] = useState(false);

  const [pwForm, setPwForm] = useState({ currentPassword: '', newPassword: '', confirmPassword: '' });
  const [pwStatus, setPwStatus] = useState(null);
  const [pwSaving, setPwSaving] = useState(false);
  const [showPw, setShowPw] = useState(false);

  const handleNameSave = async (e) => {
    e.preventDefault();
    setNameStatus(null);
    setNameSaving(true);
    try {
      const res = await api.put('/auth/profile', { fullName: nameForm.fullName });
      if (!res.data.success) throw new Error(res.data.message);
      updateUser({ ...user, fullName: res.data.data.fullName });
      setNameStatus({ type: 'success', message: 'Name updated successfully.' });
    } catch (err) {
      setNameStatus({ type: 'error', message: err.response?.data?.message ?? err.message ?? 'Update failed.' });
    } finally {
      setNameSaving(false);
    }
  };

  const handlePwSave = async (e) => {
    e.preventDefault();
    setPwStatus(null);
    if (pwForm.newPassword !== pwForm.confirmPassword) {
      setPwStatus({ type: 'error', message: 'New passwords do not match.' });
      return;
    }
    setPwSaving(true);
    try {
      const res = await api.post('/auth/change-password', {
        currentPassword: pwForm.currentPassword,
        newPassword: pwForm.newPassword
      });
      if (!res.data.success) throw new Error(res.data.message);
      setPwStatus({ type: 'success', message: 'Password changed successfully.' });
      setPwForm({ currentPassword: '', newPassword: '', confirmPassword: '' });
    } catch (err) {
      setPwStatus({ type: 'error', message: err.response?.data?.message ?? err.message ?? 'Failed to change password.' });
    } finally {
      setPwSaving(false);
    }
  };

  const initials = user?.fullName
    ? user.fullName.split(' ').map(n => n[0]).slice(0, 2).join('').toUpperCase()
    : user?.email?.[0]?.toUpperCase() ?? '?';

  return (
    <div className="page-body">
      <div className="topbar" style={{ marginLeft: '-2.5rem', marginRight: '-2.5rem', marginTop: '-2rem', marginBottom: '2rem', paddingLeft: '2.5rem' }}>
        <span className="topbar-title">Profile</span>
      </div>

      <div style={{ maxWidth: '580px', display: 'flex', flexDirection: 'column', gap: '1.25rem' }}>

        {/* Profile header */}
        <div className="card" style={{ display: 'flex', alignItems: 'center', gap: '1.25rem' }}>
          <div className="avatar" style={{ width: 60, height: 60, fontSize: '1.35rem' }}>{initials}</div>
          <div>
            <div style={{ fontWeight: 700, fontSize: '1.1rem', color: 'var(--text-1)' }}>{user?.fullName}</div>
            <div className="text-sm text-muted">{user?.email}</div>
            <div className="text-xs text-muted" style={{ marginTop: '0.25rem' }}>
              Member since {user?.createdAt ? formatDate(user.createdAt) : '—'} · {user?.role}
            </div>
          </div>
        </div>

        {/* Edit name */}
        <div className="card">
          <div className="section-title" style={{ marginBottom: '1rem' }}>Edit Name</div>
          {nameStatus && (
            <div className={`alert alert-${nameStatus.type}`}>
              <span>{nameStatus.type === 'success' ? '✓' : '⚠'}</span> {nameStatus.message}
            </div>
          )}
          <form onSubmit={handleNameSave}>
            <div className="form-group">
              <label className="form-label">Full name</label>
              <input
                type="text"
                className="form-input"
                value={nameForm.fullName}
                onChange={e => setNameForm({ fullName: e.target.value })}
                required
                minLength={2}
                maxLength={100}
              />
            </div>
            <button type="submit" className="btn" disabled={nameSaving || nameForm.fullName === user?.fullName}>
              {nameSaving ? <><span className="spinner" /> Saving…</> : 'Save Name'}
            </button>
          </form>
        </div>

        {/* Change password */}
        <div className="card">
          <div className="section-title" style={{ marginBottom: '1rem' }}>Change Password</div>
          {pwStatus && (
            <div className={`alert alert-${pwStatus.type}`}>
              <span>{pwStatus.type === 'success' ? '✓' : '⚠'}</span> {pwStatus.message}
            </div>
          )}
          <form onSubmit={handlePwSave}>
            <div className="form-group">
              <label className="form-label" style={{ display: 'flex', justifyContent: 'space-between' }}>
                Current password
                <button type="button" className="btn-ghost btn-sm" style={{ padding: 0, fontSize: '0.75rem', color: 'var(--text-3)' }} onClick={() => setShowPw(v => !v)}>
                  {showPw ? 'Hide' : 'Show'}
                </button>
              </label>
              <input
                type={showPw ? 'text' : 'password'}
                className="form-input"
                placeholder="••••••••"
                value={pwForm.currentPassword}
                onChange={e => setPwForm(f => ({ ...f, currentPassword: e.target.value }))}
                required
              />
            </div>
            <div className="form-group">
              <label className="form-label">New password</label>
              <input
                type={showPw ? 'text' : 'password'}
                className="form-input"
                placeholder="Min 8 chars, upper, lower, digit, symbol"
                value={pwForm.newPassword}
                onChange={e => setPwForm(f => ({ ...f, newPassword: e.target.value }))}
                required
                minLength={8}
              />
            </div>
            <div className="form-group">
              <label className="form-label">Confirm new password</label>
              <input
                type={showPw ? 'text' : 'password'}
                className="form-input"
                placeholder="Re-enter new password"
                value={pwForm.confirmPassword}
                onChange={e => setPwForm(f => ({ ...f, confirmPassword: e.target.value }))}
                required
              />
              {pwForm.confirmPassword && pwForm.newPassword !== pwForm.confirmPassword && (
                <span className="form-hint text-danger">Passwords do not match</span>
              )}
            </div>
            <button type="submit" className="btn" disabled={pwSaving}>
              {pwSaving ? <><span className="spinner" /> Saving…</> : 'Change Password'}
            </button>
          </form>
        </div>

      </div>
    </div>
  );
};

export default Profile;
