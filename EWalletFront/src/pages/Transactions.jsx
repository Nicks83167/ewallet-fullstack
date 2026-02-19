import React, { useState, useEffect } from 'react';
import api from '../api/axios';

const Transactions = () => {
  const [transactions, setTransactions] = useState([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [error, setError] = useState(null);
  const [loading, setLoading] = useState(false);
  const pageSize = 10;

  useEffect(() => {
    const fetchTransactions = async () => {
      setLoading(true);
      setError(null);
      try {
        const response = await api.get(`/transactions?page=${page}&pageSize=${pageSize}`);
        
        if (!response.data.success) {
          throw new Error(response.data.errors[0]);
        }

        const data = response.data.data;
        setTransactions(data.transactions);
        setTotalPages(data.totalPages);
      } catch (err) {
        if (err.response?.data?.errors?.length > 0) {
          setError(err.response.data.errors[0]);
        } else {
          setError("Network error fetching transactions.");
        }
      } finally {
        setLoading(false);
      }
    };

    fetchTransactions();
  }, [page]);

  return (
    <div className="container mt-2">
      <div className="card">
        <h2 className="title">Transaction History</h2>
        
        {error && <div className="alert alert-error">{error}</div>}
        
        {loading ? (
          <p>Loading transactions...</p>
        ) : (
          <>
            <div className="table-container">
              {transactions.length === 0 ? (
                <p style={{ color: 'var(--text-secondary)' }}>No transactions found.</p>
              ) : (
                <table>
                  <thead>
                    <tr>
                      <th>Ref Code</th>
                      <th>Date</th>
                      <th>Type</th>
                      <th>Description</th>
                      <th>Counterparty</th>
                      <th>Amount</th>
                      <th>Status</th>
                    </tr>
                  </thead>
                  <tbody>
                    {transactions.map(tx => (
                      <tr key={tx.id}>
                        <td style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>{tx.referenceCode}</td>
                        <td>{new Date(tx.createdAt).toLocaleDateString()} {new Date(tx.createdAt).toLocaleTimeString()}</td>
                        <td>{tx.type}</td>
                        <td>{tx.description}</td>
                        <td>{tx.counterpartyName || '-'}</td>
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

            {totalPages > 1 && (
              <div className="pagination">
                <button 
                  className="btn btn-secondary" 
                  disabled={page === 1} 
                  onClick={() => setPage(p => p - 1)}
                >
                  Previous
                </button>
                <span>Page {page} of {totalPages}</span>
                <button 
                  className="btn btn-secondary" 
                  disabled={page === totalPages} 
                  onClick={() => setPage(p => p + 1)}
                >
                  Next
                </button>
              </div>
            )}
          </>
        )}
      </div>
    </div>
  );
};

export default Transactions;