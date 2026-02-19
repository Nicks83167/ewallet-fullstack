import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import api from '../api/axios';

const Dashboard = () => {
  const [balanceData, setBalanceData] = useState(null);
  const [transactions, setTransactions] = useState([]);
  const [error, setError] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchDashboardData = async () => {
      try {
        const [balanceRes, txRes] = await Promise.all([
          api.get('/wallet/balance'),
          api.get('/transactions?page=1&pageSize=5')
        ]);

        if (!balanceRes.data.success) throw new Error(balanceRes.data.errors[0]);
        if (!txRes.data.success) throw new Error(txRes.data.errors[0]);

        setBalanceData(balanceRes.data.data);
        setTransactions(txRes.data.data.transactions);
      } catch (err) {
        if (err.response?.data?.errors?.length > 0) {
          setError(err.response.data.errors[0]);
        } else {
          setError(err.message || "Failed to load dashboard data.");
        }
      } finally {
        setLoading(false);
      }
    };

    fetchDashboardData();
  }, []);

  if (loading) return <div className="container mt-2">Loading dashboard...</div>;
  if (error) return <div className="container mt-2"><div className="alert alert-error">{error}</div></div>;

  return (
    <div className="container mt-2">
      <div className="card">
        <h2 className="title">Wallet Balance</h2>
        <h1 style={{ fontSize: '3rem', margin: '1rem 0' }}>${balanceData?.balance?.toFixed(2)}</h1>
        <p style={{ color: 'var(--text-secondary)' }}>
          Wallet ID: {balanceData?.walletId} <br/>
          Last Updated: {new Date(balanceData?.updatedAt).toLocaleString()}
        </p>
      </div>

      <div className="card">
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <h2 className="title" style={{ marginBottom: 0 }}>Recent Transactions</h2>
          <Link to="/transactions" className="btn btn-secondary">View All</Link>
        </div>
        
        <div className="table-container">
          {transactions.length === 0 ? (
            <p className="mt-2" style={{ color: 'var(--text-secondary)' }}>No recent transactions.</p>
          ) : (
            <table>
              <thead>
                <tr>
                  <th>Date</th>
                  <th>Description</th>
                  <th>Counterparty</th>
                  <th>Amount</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {transactions.map(tx => (
                  <tr key={tx.id}>
                    <td>{new Date(tx.createdAt).toLocaleDateString()}</td>
                    <td>{tx.description}</td>
                    <td>{tx.counterpartyName || 'N/A'}</td>
                    <td className={tx.direction === 'IN' ? 'type-credit' : 'type-debit'}>
                      {tx.direction === 'IN' ? '+' : '-'}${tx.amount.toFixed(2)}
                    </td>
                    <td className={`status-${tx.status.toLowerCase()}`}>{tx.status}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      </div>
    </div>
  );
};

export default Dashboard;