import React, { useState, useEffect } from 'react';
import api from '../api/axios';
import { formatDateTime } from '../utils/format';

const Notifications = () => {
  const [notifications, setNotifications] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [filter, setFilter] = useState('all'); // all, unread
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [unreadCount, setUnreadCount] = useState(0);

  const loadNotifications = async () => {
    try {
      setLoading(true);
      const res = await api.get(`/notifications?page=${page}&pageSize=20&unreadOnly=${filter === 'unread'}`);
      if (res.data.success) {
        setNotifications(res.data.data.items);
        setTotalPages(res.data.data.totalPages);
        setUnreadCount(res.data.data.unreadCount);
      } else {
        setError(res.data.message);
      }
    } catch (err) {
      setError(err.response?.data?.message ?? 'Failed to load notifications.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadNotifications();
  }, [page, filter]);

  const markAsRead = async (id) => {
    try {
      await api.put(`/notifications/${id}/read`);
      loadNotifications();
    } catch (err) {
      console.error('Failed to mark as read');
    }
  };

  const markAllAsRead = async () => {
    try {
      await api.put('/notifications/mark-all-read');
      loadNotifications();
    } catch (err) {
      console.error('Failed to mark all as read');
    }
  };

  const deleteNotification = async (id) => {
    try {
      await api.delete(`/notifications/${id}`);
      loadNotifications();
    } catch (err) {
      console.error('Failed to delete notification');
    }
  };

  if (loading && notifications.length === 0) return (
    <div className="page-body">
      <div className="page-loading"><div className="spinner" /><span>Loading…</span></div>
    </div>
  );

  return (
    <div className="page-body">
      <div className="topbar" style={{ marginLeft: '-2.5rem', marginRight: '-2.5rem', marginTop: '-2rem', marginBottom: '2rem', paddingLeft: '2.5rem', paddingRight: '2.5rem' }}>
        <span className="topbar-title">Notifications</span>
        {unreadCount > 0 && (
          <span className="topbar-badge" style={{ background: 'var(--danger-dim)', color: 'var(--danger)' }}>
            {unreadCount} Unread
          </span>
        )}
      </div>

      {error && <div className="alert alert-error"><span>⚠</span> {error}</div>}

      <div className="card">
        <div className="card-header">
          <div className="card-title">All Notifications</div>
          <div style={{ display: 'flex', gap: '0.5rem' }}>
            <button
              className={`btn btn-sm ${filter === 'all' ? 'btn-primary' : 'btn-secondary'}`}
              onClick={() => { setFilter('all'); setPage(1); }}
            >
              All
            </button>
            <button
              className={`btn btn-sm ${filter === 'unread' ? 'btn-primary' : 'btn-secondary'}`}
              onClick={() => { setFilter('unread'); setPage(1); }}
            >
              Unread
            </button>
            {unreadCount > 0 && (
              <button className="btn btn-sm btn-secondary" onClick={markAllAsRead}>
                Mark All Read
              </button>
            )}
          </div>
        </div>

        {notifications.length === 0 ? (
          <div className="empty-state">
            <div className="empty-state-icon">🔔</div>
            <p>{filter === 'unread' ? 'No unread notifications' : 'No notifications yet'}</p>
          </div>
        ) : (
          <div style={{ display: 'flex', flexDirection: 'column', gap: '0' }}>
            {notifications.map((notification) => (
              <div
                key={notification.id}
                style={{
                  display: 'flex',
                  alignItems: 'flex-start',
                  gap: '1rem',
                  padding: '1rem 1.5rem',
                  borderBottom: '1px solid var(--border)',
                  background: notification.isRead ? 'transparent' : 'var(--primary-dim)',
                  cursor: 'pointer',
                  transition: 'background 0.15s'
                }}
                onClick={() => !notification.isRead && markAsRead(notification.id)}
              >
                <div style={{ fontSize: '1.5rem', flexShrink: 0, marginTop: '0.25rem' }}>
                  {notification.icon || '🔔'}
                </div>
                <div style={{ flex: 1, minWidth: 0 }}>
                  <div style={{
                    fontWeight: notification.isRead ? 500 : 700,
                    color: 'var(--text-1)',
                    marginBottom: '0.25rem',
                    fontSize: '0.9rem'
                  }}>
                    {notification.title}
                  </div>
                  <div className="text-sm text-2" style={{ marginBottom: '0.5rem' }}>
                    {notification.message}
                  </div>
                  <div className="text-xs text-muted">
                    {formatDateTime(notification.createdAt)}
                  </div>
                </div>
                <button
                  className="btn btn-ghost btn-sm"
                  onClick={(e) => {
                    e.stopPropagation();
                    deleteNotification(notification.id);
                  }}
                  style={{ color: 'var(--text-3)' }}
                >
                  ✕
                </button>
              </div>
            ))}
          </div>
        )}

        {totalPages > 1 && (
          <div className="pagination">
            <div className="pagination-info">
              Page {page} of {totalPages}
            </div>
            <div className="pagination-controls">
              <button
                className="btn btn-sm btn-secondary"
                disabled={page === 1}
                onClick={() => setPage(p => Math.max(1, p - 1))}
              >
                Previous
              </button>
              <button
                className="btn btn-sm btn-secondary"
                disabled={page === totalPages}
                onClick={() => setPage(p => Math.min(totalPages, p + 1))}
              >
                Next
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default Notifications;
