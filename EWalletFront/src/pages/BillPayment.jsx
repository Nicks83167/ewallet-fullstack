import React, { useState, useEffect } from 'react';
import { apiGet, apiPost } from '../utils/api';

const CATEGORIES = [
  { id: 'Electricity', name: 'Electricity', icon: '💡', providers: ['MSEB', 'BSES', 'TSNPDCL', 'KSEB', 'PSPCL', 'UPPCL', 'WBSEDCL'] },
  { id: 'Water',       name: 'Water',       icon: '💧', providers: ['BMC Water', 'DJB Water', 'MCGM', 'GHMC Water', 'BWSSB'] },
  { id: 'Gas',         name: 'Gas',         icon: '🔥', providers: ['Indane Gas', 'Bharat Gas', 'HP Gas', 'Adani Gas'] },
  { id: 'Broadband',   name: 'Broadband',   icon: '📶', providers: ['Airtel Fiber', 'Jio Fiber', 'BSNL Broadband', 'Hathway', 'ACT Fibernet'] },
  { id: 'DTH',         name: 'DTH/Cable',   icon: '📺', providers: ['Tata Sky', 'Airtel Digital TV', 'Dish TV', 'Sun Direct', 'D2H'] },
  { id: 'Insurance',   name: 'Insurance',   icon: '🛡️', providers: ['LIC', 'HDFC Life', 'ICICI Prudential', 'SBI Life', 'Bajaj Allianz'] },
  { id: 'FASTag',      name: 'FASTag',      icon: '🛣️', providers: ['ICICI FASTag', 'HDFC FASTag', 'Paytm FASTag', 'Airtel FASTag', 'SBI FASTag'] },
];

const BillPayment = () => {
  const [activeCat, setActiveCat] = useState('Electricity');
  const [form, setForm] = useState({ provider: '', consumerNumber: '', amount: '' });
  const [history, setHistory] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  useEffect(() => { fetchHistory(); }, []);
  useEffect(() => { setForm({ provider: '', consumerNumber: '', amount: '' }); }, [activeCat]);

  const fetchHistory = async () => {
    const data = await apiGet('/api/BillPayment?page=1&pageSize=20');
    if (data.success) setHistory(data.data?.items ?? []);
  };

  const handlePay = async (e) => {
    e.preventDefault(); setLoading(true); setError(''); setSuccess('');
    const data = await apiPost('/api/BillPayment/pay', {
      category: activeCat,
      provider: form.provider,
      consumerNumber: form.consumerNumber,
      amount: parseFloat(form.amount)
    });
    if (data.success) {
      const status = data.data?.status;
      if (status === 'Success') {
        setSuccess(`✅ Bill payment of ₹${form.amount} to ${form.provider} completed!`);
        setForm({ provider: '', consumerNumber: '', amount: '' });
        fetchHistory();
      } else {
        setError('Bill payment failed. Please try again.');
      }
    } else setError(data.message || 'Payment failed');
    setLoading(false);
  };

  const currentCat = CATEGORIES.find(c => c.id === activeCat);

  return (
    <div className="page-container">
      <div className="page-header">
        <div>
          <h1 className="page-title">Bill Payments</h1>
          <p className="page-subtitle">Pay utility bills instantly from your wallet</p>
        </div>
      </div>

      {error && <div className="alert alert-error">❌ {error}</div>}
      {success && <div className="alert alert-success">{success}</div>}

      <div className="bill-payment-layout">
        <div className="categories-sidebar">
          <h3>Bill Categories</h3>
          <div className="categories-list">
            {CATEGORIES.map(cat => (
              <button key={cat.id} className={`category-item${activeCat===cat.id?' active':''}`} onClick={() => setActiveCat(cat.id)}>
                <span>{cat.icon}</span>
                <span>{cat.name}</span>
              </button>
            ))}
          </div>
        </div>

        <div className="bill-content">
          <div className="card">
            <div className="card-header">
              <h3 className="card-title">{currentCat?.icon} Pay {currentCat?.name} Bill</h3>
            </div>
            <form onSubmit={handlePay} className="card-content">
              <div className="form-grid">
                <div className="form-group">
                  <label>Service Provider *</label>
                  <select value={form.provider} onChange={e => setForm({...form, provider: e.target.value})} required>
                    <option value="">Select Provider</option>
                    {currentCat?.providers.map(p => <option key={p} value={p}>{p}</option>)}
                  </select>
                </div>
                <div className="form-group">
                  <label>Consumer / Account Number *</label>
                  <input type="text" value={form.consumerNumber} onChange={e => setForm({...form, consumerNumber: e.target.value})} placeholder="Enter your consumer number" required />
                </div>
                <div className="form-group">
                  <label>Amount (₹) *</label>
                  <input type="number" step="0.01" min="1" value={form.amount} onChange={e => setForm({...form, amount: e.target.value})} placeholder="Enter bill amount" required />
                </div>
              </div>
              <div className="form-actions">
                <button type="submit" className="btn btn-primary" disabled={loading}>
                  {loading ? 'Processing…' : `Pay ₹${form.amount || '0'}`}
                </button>
              </div>
            </form>
          </div>

          <div className="card">
            <div className="card-header"><h3 className="card-title">💡 Tips</h3></div>
            <div className="card-content">
              <div className="tips-grid">
                {[['Consumer Number','Find this on your bill statement or at the service provider\'s website.'],['Verify Amount','Double-check the amount before confirming.'],['Processing Time','Bills are usually updated within 24 hours.'],['Keep Reference','Save the transaction reference number.']].map(([title,desc]) => (
                  <div key={title} className="tip-item"><strong style={{color:'var(--text-1)'}}>{title}:</strong> <span style={{color:'var(--text-2)'}}>{desc}</span></div>
                ))}
              </div>
            </div>
          </div>
        </div>
      </div>

      <div className="card" style={{marginTop:'1.5rem'}}>
        <div className="card-header"><h3 className="card-title">Recent Bill Payments</h3></div>
        <div className="card-content">
          {history.length === 0 ? (
            <div className="empty-state"><div className="empty-state-icon">💡</div><h3>No Bill Payments Yet</h3><p>Your payment history will appear here</p></div>
          ) : (
            <div className="table-wrap">
              <table>
                <thead><tr><th>Category</th><th>Provider</th><th>Consumer No.</th><th>Amount</th><th>Status</th><th>Date</th></tr></thead>
                <tbody>
                  {history.map(bill => (
                    <tr key={bill.id}>
                      <td>{CATEGORIES.find(c=>c.id===bill.category)?.icon} {bill.category}</td>
                      <td>{bill.provider}</td>
                      <td className="mono">****{bill.consumerNumber?.slice(-4)}</td>
                      <td><strong>₹{bill.amount}</strong></td>
                      <td><span className={`badge ${bill.status==='Success'?'badge-success':bill.status==='Failed'?'badge-danger':'badge-warning'}`}>{bill.status}</span></td>
                      <td className="text-sm">{new Date(bill.createdAt).toLocaleDateString('en-IN')}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default BillPayment;
