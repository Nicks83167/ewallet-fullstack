import React, { useState, useEffect } from 'react';
import api from '../api/axios';
import { formatCurrency, formatDateTime, txBadge, txAmountClass } from '../utils/format';

const TYPE_ICON = { Deposit: '↓', Transfer: '→', Withdrawal: '↑' };

const Transactions = () => {
  const [transactions, setTransactions] = useState([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [error, setError] = useState(null);
  const [loading, setLoading] = useState(false);
  const pageSize = 10;

  useEffect(() => {
    const load = async () => {
      setLoading(true);
      setError(null);
      try {
        const res = await api.get(`/transactions?page=${page}&pageSize=${pageSize}`);
        if (!res.data.success) throw new Error(res.data.message);
        const { transactions: txs, totalPages: tp, totalCount: tc } = res.data.data;
        setTransactions(txs);
        setTotalPages(tp);
        setTotalCount(tc);
      } catch (err) {
        setError(err.response?.data?.message ?? err.message ?? 'Failed to load transactions.');
      } finally {
        setLoading(false);
      }
    };
    load();
  }, [page]);

  const from = (page - 1) * pageSize + 1;
  const to = Math.min(page * pageSize, totalCount);

  return (
    <div className="page-body">
      <div className="topbar" style={{ marginLeft: '-2.5rem', marginRight: '-2.5rem', marginTop: '-2rem', marginBottom: '2rem', paddingLeft: '2.5rem' }}>
        <span className="topbar-title">Transactions</span>
        {totalCount > 0 && <span className="topbar-badge">{totalCount} total</span>}
      </div>

      {error && <div className="alert alert-error"><span>⚠</span> {error}</div>}

      <div className="card">
        <div className="section-header">
          <div>
            <div className="section-title">Transaction History</div>
            <div className="section-subtitle">All your deposits, withdrawals, and transfers</div>
          </div>
        </div>

        {loading ? (
          <div className="page-loading" style={{ height: '20vh' }}>
            <div className="spinner" /><span>Loading transactions…</span>
          </div>
        ) : transactions.length === 0 ? (
          <div className="empty-state">
            <div className="empty-state-icon">📭</div>
            <p>No transactions found.</p>
          </div>
        ) : (
          <>
            <div className="table-wrap">
              <table>
                <thead>
                  <tr>
                    <th>Transaction</th>
                    <th>Date & Time</th>
                    <th>Counterparty</th>
                    <th>Ref Code</th>
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
                            {TYPE_ICON[tx.type] ?? (tx.direction === 'IN' ? '↓' : '↑')}
                          </div>
                          <div>
                            <div style={{ fontWeight: 500, color: 'var(--text-1)', fontSize: '0.875rem' }}>{tx.type}</div>
                            <div className="text-xs text-muted">{tx.direction === 'IN' ? 'Received' : 'Sent'}</div>
                          </div>
                        </div>
                      </td>
                      <td className="text-sm">{formatDateTime(tx.createdAt)}</td>
                      <td className="text-sm">{tx.counterpartyName ?? <span className="text-muted">—</span>}</td>
                      <td><span className="mono text-muted">{tx.referenceCode ?? '—'}</span></td>
                      <td className={txAmountClass(tx.direction)}>
                        {tx.direction === 'IN' ? '+' : '−'}{formatCurrency(tx.amount)}
                      </td>
                      <td><span className={`badge ${txBadge(tx.status)}`}>{tx.status}</span></td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            <div className="pagination">
              <span className="pagination-info">
                {totalCount > 0 ? `Showing ${from}–${to} of ${totalCount}` : ''}
              </span>
              <div className="pagination-controls">
                <button className="btn btn-secondary btn-sm" disabled={page === 1} onClick={() => setPage(p => p - 1)}>
                  ← Prev
                </button>
                <span className="text-sm text-muted" style={{ padding: '0 0.5rem' }}>
                  {page} / {totalPages}
                </span>
                <button className="btn btn-secondary btn-sm" disabled={page === totalPages} onClick={() => setPage(p => p + 1)}>
                  Next →
                </button>
              </div>
            </div>
          </>
        )}
      </div>
    </div>
  );
};

export default Transactions;
