import React, { useState } from 'react';
import api from '../api/axios';

const Security = () => {
  const [pwForm, setPwForm]     = useState({ currentPassword: '', newPassword: '', confirmPassword: '' });
  const [pwLoading, setPwLoading] = useState(false);
  const [pwError, setPwError]   = useState('');
  const [pwSuccess, setPwSuccess] = useState('');

  const handleChangePassword = async (e) => {
    e.preventDefault();
    setPwError(''); setPwSuccess('');
    if (pwForm.newPassword !== pwForm.confirmPassword) {
      setPwError('New passwords do not match'); return;
    }
    if (pwForm.newPassword.length < 8) {
      setPwError('Password must be at least 8 characters'); return;
    }
    setPwLoading(true);
    try {
      const res = await api.post('/auth/change-password', {
        currentPassword: pwForm.currentPassword,
        newPassword: pwForm.newPassword,
      });
      if (res.data.success) {
        setPwSuccess('Password changed successfully!');
        setPwForm({ currentPassword: '', newPassword: '', confirmPassword: '' });
      } else setPwError(res.data.message || 'Failed to change password');
    } catch (e) {
      setPwError(e.response?.data?.message || 'Failed to change password');
    } finally { setPwLoading(false); }
  };

  const securityChecklist = [
    { label: 'Strong Password Set',        done: true,  icon: '🔑' },
    { label: 'Email Verified',             done: true,  icon: '📧' },
    { label: 'Two-Factor Authentication',  done: false, icon: '📱' },
    { label: 'Trusted Devices Reviewed',   done: false, icon: '💻' },
    { label: 'Regular Password Updates',   done: false, icon: '🔄' },
  ];

  const score = Math.round((securityChecklist.filter(i => i.done).length / securityChecklist.length) * 100);
  const scoreColor = score >= 80 ? 'var(--success)' : score >= 50 ? 'var(--warning)' : 'var(--danger)';

  return (
    <div className="page-container">
      <div className="page-header">
        <div>
          <h1 className="page-title">Security Center</h1>
          <p className="page-subtitle">Manage your account security and privacy settings</p>
        </div>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(320px, 1fr))', gap: '1.25rem' }}>

        {/* Security Score */}
        <div className="card">
          <div className="card-header"><h3 className="card-title">🛡️ Security Score</h3></div>
          <div className="card-content" style={{ textAlign: 'center' }}>
            <div style={{ fontSize: '3rem', fontWeight: 700, color: scoreColor, marginBottom: '0.5rem' }}>
              {score}%
            </div>
            <div style={{ height: 8, background: 'var(--surface-3)', borderRadius: 4, margin: '0.75rem 0 1.25rem' }}>
              <div style={{ width: `${score}%`, height: '100%', background: scoreColor, borderRadius: 4, transition: 'width 0.5s ease' }} />
            </div>
            <div style={{ display: 'flex', flexDirection: 'column', gap: '0.65rem', textAlign: 'left' }}>
              {securityChecklist.map(item => (
                <div key={item.label} style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
                  <span style={{ fontSize: '1.1rem' }}>{item.icon}</span>
                  <span style={{ flex: 1, fontSize: '0.875rem', color: 'var(--text-2)' }}>{item.label}</span>
                  <span style={{ color: item.done ? 'var(--success)' : 'var(--text-3)', fontSize: '1rem' }}>
                    {item.done ? '✅' : '○'}
                  </span>
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* Change Password */}
        <div className="card">
          <div className="card-header"><h3 className="card-title">🔑 Change Password</h3></div>
          <div className="card-content">
            {pwError   && <div className="alert alert-error"   style={{ marginBottom: '1rem' }}>❌ {pwError}</div>}
            {pwSuccess && <div className="alert alert-success" style={{ marginBottom: '1rem' }}>✅ {pwSuccess}</div>}
            <form onSubmit={handleChangePassword}>
              <div className="form-group">
                <label>Current Password *</label>
                <input type="password" className="form-input" value={pwForm.currentPassword}
                  onChange={e => setPwForm({ ...pwForm, currentPassword: e.target.value })}
                  placeholder="Enter current password" required />
              </div>
              <div className="form-group">
                <label>New Password *</label>
                <input type="password" className="form-input" value={pwForm.newPassword}
                  onChange={e => setPwForm({ ...pwForm, newPassword: e.target.value })}
                  placeholder="Min 8 chars, include uppercase, number, symbol" required />
              </div>
              <div className="form-group">
                <label>Confirm New Password *</label>
                <input type="password" className="form-input" value={pwForm.confirmPassword}
                  onChange={e => setPwForm({ ...pwForm, confirmPassword: e.target.value })}
                  placeholder="Repeat new password" required />
              </div>
              <button type="submit" className="btn btn-primary btn-full" disabled={pwLoading}>
                {pwLoading ? 'Updating…' : 'Change Password'}
              </button>
            </form>
          </div>
        </div>

        {/* Security Tips */}
        <div className="card">
          <div className="card-header"><h3 className="card-title">💡 Security Tips</h3></div>
          <div className="card-content">
            <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
              {[
                ['🔒', 'Strong Password', 'Use a mix of uppercase, lowercase, numbers and symbols. Never share your password.'],
                ['📧', 'Email Alerts', 'You will receive an email alert for every login and transaction.'],
                ['🚨', 'Report Suspicious Activity', 'If you notice anything unusual, contact support immediately.'],
                ['📱', 'Log Out on Shared Devices', 'Always log out when using a public or shared computer.'],
                ['🔄', 'Update Regularly', 'Change your password every 3–6 months for best security.'],
              ].map(([icon, title, desc]) => (
                <div key={title} style={{ display: 'flex', gap: '0.75rem', alignItems: 'flex-start' }}>
                  <span style={{ fontSize: '1.25rem', flexShrink: 0 }}>{icon}</span>
                  <div>
                    <div style={{ fontWeight: 600, color: 'var(--text-1)', fontSize: '0.875rem', marginBottom: '0.2rem' }}>{title}</div>
                    <div style={{ color: 'var(--text-3)', fontSize: '0.8rem', lineHeight: 1.5 }}>{desc}</div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* Active Sessions Info */}
        <div className="card">
          <div className="card-header"><h3 className="card-title">💻 Active Session</h3></div>
          <div className="card-content">
            <div style={{ background: 'var(--surface-2)', borderRadius: 'var(--radius-sm)', padding: '1rem', border: '1px solid var(--border)' }}>
              <div style={{ display: 'flex', gap: '0.75rem', alignItems: 'center', marginBottom: '0.75rem' }}>
                <span style={{ fontSize: '1.5rem' }}>🖥️</span>
                <div>
                  <div style={{ fontWeight: 600, color: 'var(--text-1)' }}>Current Browser Session</div>
                  <div style={{ fontSize: '0.78rem', color: 'var(--success)' }}>● Active Now</div>
                </div>
              </div>
              <div style={{ fontSize: '0.8rem', color: 'var(--text-3)', lineHeight: 1.8 }}>
                <div>Login time: {new Date().toLocaleString('en-IN')}</div>
                <div>Browser: {navigator.userAgent.includes('Chrome') ? 'Chrome' : navigator.userAgent.includes('Firefox') ? 'Firefox' : 'Browser'}</div>
                <div>Platform: {navigator.platform}</div>
              </div>
            </div>
            <p style={{ fontSize: '0.8rem', color: 'var(--text-3)', marginTop: '1rem' }}>
              For full device and session management, advanced security features will be available in a future update.
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Security;
