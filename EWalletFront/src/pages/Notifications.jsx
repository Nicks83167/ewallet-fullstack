import React, { useState, useEffect } from 'react';
import api from '../api/axios';
import { formatDateTime } from '../utils/format';

const TYPE_ICONS = {
  TransferSuccessful: '✅', TransferFailed: '❌', MoneyReceived: '💰', MoneySent: '📤',
  WalletCredited: '💳', WalletDebited: '💸', CashbackEarned: '🎁', BillReminder: '💡',
  RechargeReminder: '📱', SecurityAlert: '🔒', PasswordChanged: '🔑',
  LoginFromNewDevice: '💻', ProfileUpdated: '👤', SystemAnnouncement: '📢',
  MaintenanceAlert: '🔧', Promotion: '🎉',
};

const Notifications = () => {
  const [notifications, setNotifications] = useState([]);
  const [loading, setLoading]             = useState(true);
  const [error, setError]                 = useState('');
  const [filter, setFilter]               = useState('all');
  const [page, setPage]                   = useState(1);
  const [totalPages, setTotalPages]       = useState(1);
  const [unreadCount, setUnreadCount]     = useState(0);

  const load = async (p = page, f = filter) => {
    try {
      setLoading(true);
      const res = await api.get(`/notifications?page=${p}&pageSize=20&unreadOnly=${f === 'unread'}`);
      if (res.data.success) {
        setNotifications(res.data.data.items ?? []);
        setTotalPages(res.data.data.totalPages ?? 1);
        setUnreadCount(res.data.data.unreadCount ?? 0);
        setError('');
      } else setError(res.data.message || 'Failed to load notifications');
    } catch (e) {
      setError(e.response?.data?.message || 'Failed to load notifications');
    } finally { setLoading(false); }
  };

  useEffect(() => { load(page, filter); }, [page, filter]);

  const changeFilter = (f) => { setFilter(f); setPage(1); };

  const markRead = async (id) => {
    try { await api.put(`/notifications/${id}/read`); load(); } catch { /* ignore */ }
  };

  const markAllRead = async () => {
    try { await api.put('/notifications/mark-all-read'); load(); } catch { /* ignore */ }
  };

  const deleteNotif = async (e, id) => {
    e.stopPropagation();
    try { await api.delete(`/notifications/${id}`); load(); } catch { /* ignore */ }
  };

  return (
    <div className="page-container">
      <div className="page-header">
        <div>
          <h1 className="page-title">Notifications</h1>
          {unreadCount > 0 && (
            <p className="page-subtitle">{unreadCount} unread notification{unreadCount !== 1 ? 's' : ''}</p>
          )}
        </div>
        <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap' }}>
          <button className={`tab${filter === 'all' ? ' active' : ''}`}    onClick={() => changeFilter('all')}>All</button>
          <button className={`tab${filter === 'unread' ? ' active' : ''}`} onClick={() => changeFilter('unread')}>Unread</button>
          {unreadCount > 0 && (
            <button className="btn btn-sm btn-secondary" onClick={markAllRead}>Mark All Read</button>
          )}
        </div>
      </div>

      {error && <div className="alert alert-error">❌ {error}</div>}

      <div className="card">
        {loading && notifications.length === 0 ? (
          <div className="page-loading"><div className="spinner" /><span>Loading…</span></div>
        ) : notifications.length === 0 ? (
          <div className="empty-state">
            <div className="empty-state-icon">🔔</div>
            <h3>{filter === 'unread' ? 'No unread notifications' : 'No notifications yet'}</h3>
            <p>You'll be notified about transactions, security events and more</p>
          </div>
        ) : (
          <>
            <div style={{ display: 'flex', flexDirection: 'column' }}>
              {notifications.map(n => (
                <div
                  key={n.id}
                  onClick={() => !n.isRead && markRead(n.id)}
                  style={{
                    display: 'flex', alignItems: 'flex-start', gap: '1rem',
                    padding: '1rem 1.5rem',
                    borderBottom: '1px solid var(--border)',
                    background: n.isRead ? 'transparent' : 'var(--primary-dim)',
                    cursor: n.isRead ? 'default' : 'pointer',
                    transition: 'background 0.15s',
                  }}
                >
                  <div style={{ fontSize: '1.5rem', flexShrink: 0, marginTop: '0.1rem' }}>
                    {n.icon || TYPE_ICONS[n.type] || '🔔'}
                  </div>
                  <div style={{ flex: 1, minWidth: 0 }}>
                    <div style={{ fontWeight: n.isRead ? 500 : 700, color: 'var(--text-1)', fontSize: '0.9rem', marginBottom: '0.2rem' }}>
                      {n.title}
                      {!n.isRead && (
                        <span style={{ display: 'inline-block', width: 7, height: 7, borderRadius: '50%', background: 'var(--primary)', marginLeft: '0.5rem', verticalAlign: 'middle' }} />
                      )}
                    </div>
                    <div style={{ fontSize: '0.825rem', color: 'var(--text-2)', marginBottom: '0.35rem', lineHeight: 1.5 }}>{n.message}</div>
                    <div style={{ fontSize: '0.72rem', color: 'var(--text-3)' }}>{formatDateTime(n.createdAt)}</div>
                  </div>
                  <button
                    onClick={(e) => deleteNotif(e, n.id)}
                    style={{ background: 'transparent', border: 'none', color: 'var(--text-3)', cursor: 'pointer', padding: '0.25rem', fontSize: '1rem', flexShrink: 0 }}
                    title="Delete"
                  >
                    ✕
                  </button>
                </div>
              ))}
            </div>

            {totalPages > 1 && (
              <div className="pagination" style={{ padding: '1rem 1.5rem' }}>
                <div className="pagination-info">Page {page} of {totalPages}</div>
                <div className="pagination-controls">
                  <button className="btn btn-sm btn-secondary" disabled={page === 1}          onClick={() => setPage(p => p - 1)}>← Prev</button>
                  <button className="btn btn-sm btn-secondary" disabled={page === totalPages} onClick={() => setPage(p => p + 1)}>Next →</button>
                </div>
              </div>
            )}
          </>
        )}
      </div>
    </div>
  );
};

export default Notifications;
