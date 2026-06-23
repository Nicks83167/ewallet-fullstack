import React, { useState, useEffect, useContext } from 'react';
import { Link } from 'react-router-dom';
import api from '../api/axios';
import { AuthContext } from '../context/AuthContext';
import { formatCurrency, formatDate, txBadge, txAmountClass } from '../utils/format';

const Dashboard = () => {
  const { user } = useContext(AuthContext);
  const [balance, setBalance] = useState(null);
  const [transactions, setTransactions] = useState([]);
  const [error, setError] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const load = async () => {
      try {
        const [balRes, txRes] = await Promise.all([
          api.get('/wallet/balance'),
          api.get('/transactions?page=1&pageSize=5')
        ]);
        if (balRes.data.success) setBalance(balRes.data.data);
        if (txRes.data.success) setTransactions(txRes.data.data.transactions);
      } catch (err) {
        setError(err.response?.data?.message ?? 'Failed to load dashboard.');
      } finally {
        setLoading(false);
      }
    };
    load();
  }, []);

  if (loading) return (
    <div className="page-body">
      <div className="page-loading"><div className="spinner" /><span>Loading…</span></div>
    </div>
  );

  const inbound = transactions.filter(t => t.direction === 'IN').reduce((s, t) => s + t.amount, 0);
  const outbound = transactions.filter(t => t.direction === 'OUT').reduce((s, t) => s + t.amount, 0);

  return (
    <div className="page-body">
      {/* Top bar */}
      <div className="topbar" style={{ marginLeft: '-2.5rem', marginRight: '-2.5rem', marginTop: '-2rem', marginBottom: '2rem', paddingLeft: '2.5rem', paddingRight: '2.5rem' }}>
        <span className="topbar-title">Dashboard</span>
        <span className="topbar-badge">Live</span>
      </div>

      {error && <div className="alert alert-error"><span>⚠</span> {error}</div>}

      {/* Greeting */}
      <div style={{ marginBottom: '1.5rem' }}>
        <h2 style={{ fontSize: '1.4rem', fontWeight: 700, color: 'var(--text-1)' }}>
          Good {getGreeting()}, {user?.fullName?.split(' ')[0] ?? 'there'} 👋
        </h2>
        <p className="text-muted text-sm" style={{ marginTop: '0.25rem' }}>
          Aaj aapke wallet mein kya chal raha hai, dekhein.
        </p>
      </div>

      {/* Stats */}
      <div className="stats-grid">
        <div className="stat-card">
          <div className="stat-icon blue">💳</div>
          <div className="stat-body">
            <div className="stat-label">Total Balance</div>
            <div className="stat-value">{formatCurrency(balance?.balance ?? 0)}</div>
            <div className="stat-sub">Updated {balance ? formatDate(balance.updatedAt) : '—'}</div>
          </div>
        </div>
        <div className="stat-card">
          <div className="stat-icon green">↓</div>
          <div className="stat-body">
            <div className="stat-label">Money In (recent)</div>
            <div className="stat-value" style={{ color: 'var(--success)' }}>{formatCurrency(inbound)}</div>
            <div className="stat-sub">Last 5 transactions</div>
          </div>
        </div>
        <div className="stat-card">
          <div className="stat-icon amber">↑</div>
          <div className="stat-body">
            <div className="stat-label">Money Out (recent)</div>
            <div className="stat-value" style={{ color: 'var(--danger)' }}>{formatCurrency(outbound)}</div>
            <div className="stat-sub">Last 5 transactions</div>
          </div>
        </div>
        <div className="stat-card">
          <div className="stat-icon purple">🪙</div>
          <div className="stat-body">
            <div className="stat-label">Wallet ID</div>
            <div className="stat-value" style={{ fontSize: '0.75rem', fontFamily: 'monospace', letterSpacing: '0.02em', marginTop: '0.25rem', color: 'var(--text-2)' }}>
              {balance?.walletId ? `${balance.walletId.slice(0, 8)}…` : '—'}
            </div>
            <div className="stat-sub">{balance?.ownerName}</div>
          </div>
        </div>
      </div>

      {/* Quick Actions */}
      <div className="card mb-3">
        <div className="card-header">
          <div>
            <div className="card-title">Quick Actions</div>
          </div>
        </div>
        <div className="quick-actions">
          <Link to="/add-money" className="quick-action-btn">
            <span className="quick-action-icon">＋</span>Add Money
          </Link>
          <Link to="/withdraw" className="quick-action-btn">
            <span className="quick-action-icon">↑</span>Withdraw
          </Link>
          <Link to="/transfer" className="quick-action-btn">
            <span className="quick-action-icon">→</span>Transfer
          </Link>
          <Link to="/transactions" className="quick-action-btn">
            <span className="quick-action-icon">≡</span>History
          </Link>
        </div>
      </div>

      {/* Recent Transactions */}
      <div className="card">
        <div className="section-header">
          <div>
            <div className="section-title">Recent Transactions</div>
            <div className="section-subtitle">Your last 5 transactions</div>
          </div>
          <Link to="/transactions" className="btn btn-secondary btn-sm">View All</Link>
        </div>

        {transactions.length === 0 ? (
          <div className="empty-state">
            <div className="empty-state-icon">📭</div>
            <p>Abhi tak koi transaction nahi. Paise add karein aur shuru karein.</p>
            <Link to="/add-money" className="btn btn-sm" style={{ marginTop: '0.5rem' }}>Paise Add Karein</Link>
          </div>
        ) : (
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Transaction</th>
                  <th>Date</th>
                  <th>Counterparty</th>
                  <th>Amount</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {transactions.map(tx => (
                  <tr key={tx.id}>
                    <td>
                      <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
                        <div className={`tx-icon ${tx.direction === 'IN' ? 'in' : 'out'}`}>
                          {tx.direction === 'IN' ? '↓' : '↑'}
                        </div>
                        <div>
                          <div style={{ fontWeight: 500, color: 'var(--text-1)', fontSize: '0.875rem' }}>{tx.type}</div>
                          <div className="text-xs text-muted mono">{tx.referenceCode}</div>
                        </div>
                      </div>
                    </td>
                    <td className="text-sm">{formatDate(tx.createdAt)}</td>
                    <td className="text-sm">{tx.counterpartyName ?? <span className="text-muted">—</span>}</td>
                    <td className={txAmountClass(tx.direction)}>
                      {tx.direction === 'IN' ? '+' : '−'}{formatCurrency(tx.amount)}
                    </td>
                    <td><span className={`badge ${txBadge(tx.status)}`}>{tx.status}</span></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
};

const getGreeting = () => {
  const h = new Date().getHours();
  if (h < 12) return 'morning';
  if (h < 17) return 'afternoon';
  return 'evening';
};

export default Dashboard;
