import React, { useState, useEffect, useContext } from 'react';
import { Link } from 'react-router-dom';
import api from '../api/axios';
import { AuthContext } from '../context/AuthContext';
import { formatCurrency, formatDate } from '../utils/format';
import BarChartComponent from '../components/charts/BarChartComponent';
import LineChartComponent from '../components/charts/LineChartComponent';
import PieChartComponent from '../components/charts/PieChartComponent';
import AreaChartComponent from '../components/charts/AreaChartComponent';

const DashboardNew = () => {
  const { user } = useContext(AuthContext);
  const [analytics, setAnalytics] = useState(null);
  const [error, setError] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const load = async () => {
      try {
        const res = await api.get('/analytics/dashboard');
        if (res.data.success) {
          setAnalytics(res.data.data);
        } else {
          setError(res.data.message);
        }
      } catch (err) {
        setError(err.response?.data?.message ?? 'Failed to load analytics.');
      } finally {
        setLoading(false);
      }
    };
    load();
  }, []);

  if (loading) return (
    <div className="page-body">
      <div className="page-loading"><div className="spinner" /><span>Loading analytics…</span></div>
    </div>
  );

  if (error || !analytics) return (
    <div className="page-body">
      <div className="alert alert-error"><span>⚠</span> {error || 'No data available'}</div>
    </div>
  );

  const { walletOverview, monthlyAnalytics, quickInsights, recentActivities } = analytics;

  // Prepare chart data
  const incomeExpenseData = [];
  const months = [...new Set(monthlyAnalytics.incomeVsExpense.map(d => d.label))];
  months.forEach(month => {
    const income = monthlyAnalytics.incomeVsExpense.find(d => d.label === month && d.color === '#10b981')?.value || 0;
    const expense = monthlyAnalytics.incomeVsExpense.find(d => d.label === month && d.color === '#ef4444')?.value || 0;
    incomeExpenseData.push({ label: month, income, expense });
  });

  const categoryData = monthlyAnalytics.categoryDistribution.map(c => ({
    label: c.label,
    value: c.value
  }));

  const weeklyData = monthlyAnalytics.weeklySpending.map(w => ({
    label: w.label,
    spending: w.value
  }));

  return (
    <div className="page-body">
      {/* Top bar */}
      <div className="topbar" style={{ marginLeft: '-2.5rem', marginRight: '-2.5rem', marginTop: '-2rem', marginBottom: '2rem', paddingLeft: '2.5rem', paddingRight: '2.5rem' }}>
        <span className="topbar-title">Dashboard</span>
        <span className="topbar-badge">Live</span>
      </div>

      {/* Greeting */}
      <div style={{ marginBottom: '1.5rem' }}>
        <h2 style={{ fontSize: '1.4rem', fontWeight: 700, color: 'var(--text-1)' }}>
          Good {getGreeting()}, {user?.fullName?.split(' ')[0] ?? 'there'} 👋
        </h2>
        <p className="text-muted text-sm" style={{ marginTop: '0.25rem' }}>
          Aaj aapke wallet ka complete overview dekhein, charts ke saath.
        </p>
      </div>

      {/* Main Stats */}
      <div className="stats-grid">
        <div className="stat-card">
          <div className="stat-icon blue">💳</div>
          <div className="stat-body">
            <div className="stat-label">Total Balance</div>
            <div className="stat-value">{formatCurrency(walletOverview.currentBalance)}</div>
            <div className="stat-sub">
              {walletOverview.isVerified ? '✓ Verified' : 'Not verified'} · {walletOverview.walletStatus}
            </div>
          </div>
        </div>
        <div className="stat-card">
          <div className="stat-icon green">↓</div>
          <div className="stat-body">
            <div className="stat-label">Money In (This Month)</div>
            <div className="stat-value" style={{ color: 'var(--success)' }}>
              {formatCurrency(walletOverview.moneyIn)}
            </div>
            <div className="stat-sub">Income received</div>
          </div>
        </div>
        <div className="stat-card">
          <div className="stat-icon amber">↑</div>
          <div className="stat-body">
            <div className="stat-label">Money Out (This Month)</div>
            <div className="stat-value" style={{ color: 'var(--danger)' }}>
              {formatCurrency(walletOverview.moneyOut)}
            </div>
            <div className="stat-sub">Total spending</div>
          </div>
        </div>
        <div className="stat-card">
          <div className="stat-icon purple">💰</div>
          <div className="stat-body">
            <div className="stat-label">Net Savings</div>
            <div className="stat-value" style={{ color: monthlyAnalytics.netSavings >= 0 ? 'var(--success)' : 'var(--danger)' }}>
              {formatCurrency(monthlyAnalytics.netSavings)}
            </div>
            <div className="stat-sub">This month</div>
          </div>
        </div>
      </div>

      {/* Quick Actions */}
      <div className="card mb-3">
        <div className="card-header">
          <div className="card-title">Quick Actions</div>
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

      {/* Charts Grid */}
      <div className="grid-2">
        {/* Income vs Expense */}
        <div className="card">
          <div className="card-header">
            <div>
              <div className="card-title">Income vs Expense</div>
              <div className="card-subtitle">Last 6 months trend</div>
            </div>
          </div>
          <BarChartComponent 
            data={incomeExpenseData} 
            dataKey1="income" 
            dataKey2="expense"
            color1="#10b981"
            color2="#ef4444"
          />
        </div>

        {/* Category Distribution */}
        <div className="card">
          <div className="card-header">
            <div>
              <div className="card-title">Spending by Category</div>
              <div className="card-subtitle">This month breakdown</div>
            </div>
          </div>
          {categoryData.length > 0 ? (
            <PieChartComponent data={categoryData} />
          ) : (
            <div className="empty-state" style={{ padding: '2rem' }}>
              <div className="empty-state-icon">📊</div>
              <p>No spending data available</p>
            </div>
          )}
        </div>
      </div>

      {/* Weekly Spending */}
      <div className="card mt-3">
        <div className="card-header">
          <div>
            <div className="card-title">Weekly Spending Trend</div>
            <div className="card-subtitle">Last 4 weeks activity</div>
          </div>
        </div>
        <AreaChartComponent 
          data={weeklyData} 
          dataKey1="spending"
          color1="#4f8ef7"
        />
      </div>

      {/* Quick Insights */}
      <div className="card mt-3">
        <div className="card-header">
          <div className="card-title">Quick Insights</div>
        </div>
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '1rem' }}>
          <InsightBox label="Highest Transaction" value={formatCurrency(quickInsights.highestTransaction)} icon="🔝" />
          <InsightBox label="Average Transaction" value={formatCurrency(quickInsights.averageTransaction)} icon="📊" />
          <InsightBox label="Daily Spending Avg" value={formatCurrency(quickInsights.averageDailySpending)} icon="📅" />
          <InsightBox label="Monthly Spending Avg" value={formatCurrency(quickInsights.averageMonthlySpending)} icon="📆" />
          <InsightBox label="Wallet Age" value={`${quickInsights.walletAgeDays} days`} icon="🎂" />
          <InsightBox label="Total Transactions" value={quickInsights.totalTransactions} icon="💳" />
          <InsightBox label="Saved Beneficiaries" value={quickInsights.savedBeneficiaries} icon="👥" />
          <InsightBox label="Linked Accounts" value={quickInsights.linkedAccounts} icon="🔗" />
        </div>
      </div>

      {/* Recent Activity Timeline */}
      <div className="card mt-3">
        <div className="section-header">
          <div>
            <div className="section-title">Recent Activity</div>
            <div className="section-subtitle">Your latest transactions</div>
          </div>
          <Link to="/transactions" className="btn btn-secondary btn-sm">View All</Link>
        </div>

        {recentActivities.length === 0 ? (
          <div className="empty-state">
            <div className="empty-state-icon">📭</div>
            <p>Abhi tak koi activity nahi hai.</p>
            <Link to="/add-money" className="btn btn-sm" style={{ marginTop: '0.5rem' }}>Paise Add Karein</Link>
          </div>
        ) : (
          <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
            {recentActivities.map(activity => (
              <div key={activity.id} style={{ 
                display: 'flex', 
                alignItems: 'center', 
                gap: '1rem', 
                padding: '0.875rem',
                background: 'var(--surface-2)',
                borderRadius: 'var(--radius-sm)',
                border: '1px solid var(--border)'
              }}>
                <div style={{ 
                  width: '40px', 
                  height: '40px', 
                  borderRadius: '50%', 
                  background: 'var(--primary-dim)',
                  display: 'flex', 
                  alignItems: 'center', 
                  justifyContent: 'center',
                  fontSize: '1.25rem',
                  flexShrink: 0
                }}>
                  {activity.icon}
                </div>
                <div style={{ flex: 1, minWidth: 0 }}>
                  <div style={{ fontWeight: 500, color: 'var(--text-1)', fontSize: '0.875rem' }}>
                    {activity.title}
                  </div>
                  <div className="text-xs text-muted" style={{ marginTop: '0.15rem' }}>
                    {activity.description}
                  </div>
                </div>
                <div style={{ textAlign: 'right', flexShrink: 0 }}>
                  <div style={{ fontWeight: 600, color: 'var(--text-1)', fontSize: '0.9rem' }}>
                    {formatCurrency(activity.amount)}
                  </div>
                  <div className="text-xs text-muted" style={{ marginTop: '0.15rem' }}>
                    {activity.timeAgo}
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Top Transactions */}
      {monthlyAnalytics.largestTransactions.length > 0 && (
        <div className="card mt-3">
          <div className="card-header">
            <div>
              <div className="card-title">Top Transactions</div>
              <div className="card-subtitle">Largest amounts this month</div>
            </div>
          </div>
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Description</th>
                  <th>Type</th>
                  <th>Date</th>
                  <th>Amount</th>
                </tr>
              </thead>
              <tbody>
                {monthlyAnalytics.largestTransactions.map(tx => (
                  <tr key={tx.id}>
                    <td className="font-semibold">{tx.description}</td>
                    <td className="text-sm">{tx.type}</td>
                    <td className="text-sm">{formatDate(tx.date)}</td>
                    <td className="font-bold">{formatCurrency(tx.amount)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
};

const InsightBox = ({ label, value, icon }) => (
  <div style={{
    background: 'var(--surface-2)',
    padding: '1rem',
    borderRadius: 'var(--radius-sm)',
    border: '1px solid var(--border)'
  }}>
    <div style={{ fontSize: '1.5rem', marginBottom: '0.5rem' }}>{icon}</div>
    <div className="text-xs text-muted" style={{ marginBottom: '0.35rem', textTransform: 'uppercase', letterSpacing: '0.05em', fontWeight: 600 }}>
      {label}
    </div>
    <div style={{ fontSize: '1.25rem', fontWeight: 700, color: 'var(--text-1)' }}>
      {value}
    </div>
  </div>
);

const getGreeting = () => {
  const h = new Date().getHours();
  if (h < 12) return 'morning';
  if (h < 17) return 'afternoon';
  return 'evening';
};

export default DashboardNew;
