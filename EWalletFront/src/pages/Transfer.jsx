import React, { useState, useEffect } from 'react';
import api from '../api/axios';

const Transfer = () => {
  const [receiverEmail, setReceiverEmail] = useState('');
  const [amount, setAmount] = useState('');
  const [description, setDescription] = useState('');
  
  const [currentBalance, setCurrentBalance] = useState(null);
  const [status, setStatus] = useState({ type: null, message: null });
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    // Fetch initial balance
    api.get('/wallet/balance').then(res => {
      if (res.data.success) setCurrentBalance(res.data.data.balance);
    }).catch(err => console.error("Could not fetch balance", err));
  }, []);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setStatus({ type: null, message: null });
    setLoading(true);

    try {
      const response = await api.post('/wallet/transfer', {
        receiverEmail,
        amount: parseFloat(amount),
        description
      });

      if (!response.data.success) {
        throw new Error(response.data.errors[0]);
      }

      const newBalance = response.data.data.newBalance;
      setCurrentBalance(newBalance);
      setStatus({ type: 'success', message: 'Transfer successful!' });
      
      // Reset form
      setReceiverEmail('');
      setAmount('');
      setDescription('');
    } catch (err) {
      if (err.response?.data?.errors?.length > 0) {
        setStatus({ type: 'error', message: err.response.data.errors[0] });
      } else {
        setStatus({ type: 'error', message: err.message || "Network error. Please try again." });
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="container mt-2" style={{ maxWidth: '600px' }}>
      <div className="card">
        <h2 className="title">Transfer Money</h2>
        {currentBalance !== null && (
          <p style={{ marginBottom: '1.5rem', color: 'var(--text-secondary)' }}>
            Available Balance: <strong style={{ color: 'var(--text-primary)' }}>${currentBalance.toFixed(2)}</strong>
          </p>
        )}

        {status.message && (
          <div className={`alert alert-${status.type}`}>
            {status.message}
          </div>
        )}

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label className="form-label">Receiver Email</label>
            <input 
              type="email" 
              className="form-input" 
              value={receiverEmail} 
              onChange={(e) => setReceiverEmail(e.target.value)} 
              required 
            />
          </div>
          <div className="form-group">
            <label className="form-label">Amount</label>
            <input 
              type="number" 
              step="0.01"
              min="0.01"
              className="form-input" 
              value={amount} 
              onChange={(e) => setAmount(e.target.value)} 
              required 
            />
          </div>
          <div className="form-group">
            <label className="form-label">Description</label>
            <input 
              type="text" 
              className="form-input" 
              value={description} 
              onChange={(e) => setDescription(e.target.value)} 
              required 
            />
          </div>
          <button type="submit" className="btn" style={{ width: '100%' }} disabled={loading}>
            {loading ? 'Processing...' : 'Send Money'}
          </button>
        </form>
      </div>
    </div>
  );
};

export default Transfer;