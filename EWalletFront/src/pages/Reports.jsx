import React, { useState, useEffect } from 'react';
import api from '../api/axios';
import { formatCurrency, formatDate } from '../utils/format';
import BarChartComponent from '../components/charts/BarChartComponent';
import PieChartComponent from '../components/charts/PieChartComponent';

const PERIODS = [
  { id: 'monthly', label: 'This Month' },
  { id: 'weekly',  label: 'This Week'  },
  { id: 'yearly',  label: 'This Year'  },
];

const Reports = () => {
  const [period, setPeriod]     = useState('monthly');
  const [report, setReport]     = useState(null);
  const [loading, setLoading]   = useState(true);
  const [error, setError]       = useState('');

  useEffect(() => {
    const load = async () => {
      setLoading(true); setError('');
      try {
        const res = await api.get(`/analytics/report?period=${period}`);
        if (res.data.success) setReport(res.data.data);
        else setError(res.data.message || 'Failed to load report');
      } catch (e) {
        setError(e.response?.data?.message || 'Failed to load report');
      } finally { setLoading(false); }
    };
    load();
  }, [period]);

  const incomeExpenseData = report
    ? [{ label: 'Income', income: report.totalIncome, expense: 0 }, { label: 'Expense', income: 0, expense: report.totalExpense }]
    : [];

  const categoryData = report?.categoryBreakdown?.map(c => ({ label: c.label, value: c.value })) ?? [];

  return (
    <div className="page-container">
      <div className="page-header">
        <div>
          <h1 className="page-title">Reports</h1>
          <p className="page-subtitle">Detailed financial summary and spending insights</p>
        </div>
        <div style={{ display: 'flex', gap: '0.5rem' }}>
          {PERIODS.map(p => (
            <button key={p.id} className={`tab${period === p.id ? ' active' : ''}`} onClick={() => setPeriod(p.id)}>
              {p.label}
            </button>
          ))}
        </div>
      </div>

      {error && <div className="alert alert-error">❌ {error}</div>}

      {loading ? (
        <div className="page-loading"><div className="spinner" /><span>Generating report…</span></div>
      ) : !report ? (
        <div className="card"><div className="empty-state"><div className="empty-state-icon">📊</div><h3>No Data</h3><p>No transactions found for this period</p></div></div>
      ) : (
        <>
          {/* Summary Cards */}
          <div className="stats-grid" style={{ marginBottom: '1.5rem' }}>
            <div className="stat-card">
              <div className="stat-icon green">↓</div>
              <div className="stat-body">
                <div className="stat-label">Total Income</div>
                <div className="stat-value" style={{ color: 'var(--success)' }}>{formatCurrency(report.totalIncome)}</div>
              </div>
            </div>
            <div className="stat-card">
              <div className="stat-icon amber">↑</div>
              <div className="stat-body">
                <div className="stat-label">Total Expenses</div>
                <div className="stat-value" style={{ color: 'var(--danger)' }}>{formatCurrency(report.totalExpense)}</div>
              </div>
            </div>
            <div className="stat-card">
              <div className="stat-icon blue">💰</div>
              <div className="stat-body">
                <div className="stat-label">Net Savings</div>
                <div className="stat-value" style={{ color: report.totalSavings >= 0 ? 'var(--success)' : 'var(--danger)' }}>
                  {formatCurrency(report.totalSavings)}
                </div>
              </div>
            </div>
          </div>

          {/* Charts */}
          <div className="grid-2" style={{ marginBottom: '1.5rem' }}>
            <div className="card">
              <div className="card-header"><h3 className="card-title">Income vs Expense</h3></div>
              {incomeExpenseData.length > 0
                ? <BarChartComponent data={incomeExpenseData} dataKey1="income" dataKey2="expense" color1="#10b981" color2="#ef4444" />
                : <div className="empty-state" style={{ padding: '2rem' }}><p>No data</p></div>}
            </div>
            <div className="card">
              <div className="card-header"><h3 className="card-title">Spending by Category</h3></div>
              {categoryData.length > 0
                ? <PieChartComponent data={categoryData} />
                : <div className="empty-state" style={{ padding: '2rem' }}><div className="empty-state-icon">🗂️</div><p>No category data</p></div>}
            </div>
          </div>

          {/* Largest Transactions */}
          <div className="card">
            <div className="card-header"><h3 className="card-title">Largest Transactions</h3></div>
            <div className="card-content">
              {!report.largestTransactions?.length ? (
                <div className="empty-state"><div className="empty-state-icon">💳</div><h3>No Transactions</h3></div>
              ) : (
                <div className="table-wrap">
                  <table>
                    <thead>
                      <tr>
                        <th>Description</th>
                        <th>Type</th>
                        <th>Direction</th>
                        <th>Amount</th>
                        <th>Date</th>
                      </tr>
                    </thead>
                    <tbody>
                      {report.largestTransactions.map(tx => (
                        <tr key={tx.id}>
                          <td>{tx.description || tx.type}</td>
                          <td className="text-sm">{tx.type}</td>
                          <td>
                            <span className={`badge ${tx.direction === 'IN' ? 'badge-success' : 'badge-danger'}`}>
                              {tx.direction === 'IN' ? '↓ IN' : '↑ OUT'}
                            </span>
                          </td>
                          <td className={tx.direction === 'IN' ? 'amount-credit' : 'amount-debit'}>
                            {tx.direction === 'IN' ? '+' : '-'}{formatCurrency(tx.amount)}
                          </td>
                          <td className="text-sm">{formatDate(tx.createdAt)}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </div>
          </div>
        </>
      )}
    </div>
  );
};

export default Reports;
