import React, { useState, useEffect } from 'react';
import { apiGet, apiPost } from '../utils/api';

const FREQ_OPTIONS = ['Daily', 'Weekly', 'Monthly', 'Yearly'];

const ScheduledPayments = () => {
  const [payments, setPayments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [form, setForm] = useState({ title: '', paymentType: 'Transfer', amount: '', frequency: 'Monthly', recipientEmail: '', description: '' });
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  useEffect(() => { fetchPayments(); }, []);

  const fetchPayments = async () => {
    setLoading(true);
    const data = await apiGet('/api/ScheduledPayment');
    if (data.success) setPayments(data.data ?? []);
    else setError(data.message || 'Failed to fetch scheduled payments');
    setLoading(false);
  };

  const handleCreate = async (e) => {
    e.preventDefault(); setSubmitting(true); setError('');
    const data = await apiPost('/api/ScheduledPayment', { ...form, amount: parseFloat(form.amount) });
    if (data.success) { setSuccess('Scheduled payment created!'); setShowForm(false); setForm({ title:'', paymentType:'Transfer', amount:'', frequency:'Monthly', recipientEmail:'', description:'' }); fetchPayments(); }
    else setError(data.message || 'Failed to create');
    setSubmitting(false);
  };

  const handleAction = async (id, action) => {
    const data = await apiPost(`/api/ScheduledPayment/${id}/${action}`, {});
    if (data.success) { setSuccess(`Payment ${action}d!`); fetchPayments(); }
    else setError(data.message || `Failed to ${action}`);
  };

  const statusColor = { Active: 'badge-success', Paused: 'badge-warning', Cancelled: 'badge-danger', Completed: 'badge-info' };
  const freqIcon = { Daily: '📅', Weekly: '🗓️', Monthly: '📆', Yearly: '🗒️' };

  return (
    <div className="page-container">
      <div className="page-header">
        <div>
          <h1 className="page-title">Scheduled Payments</h1>
          <p className="page-subtitle">Set up recurring payments and auto-transfers</p>
        </div>
        <button className="btn btn-primary" onClick={() => setShowForm(true)}>+ Schedule Payment</button>
      </div>

      {error && <div className="alert alert-error">❌ {error}</div>}
      {success && <div className="alert alert-success">✅ {success}</div>}

      {showForm && (
        <div className="card mb-6">
          <div className="card-header">
            <h3 className="card-title">New Scheduled Payment</h3>
            <button className="btn-ghost" onClick={() => setShowForm(false)}>✕</button>
          </div>
          <form onSubmit={handleCreate} className="card-content">
            <div className="form-grid">
              <div className="form-group">
                <label>Title *</label>
                <input type="text" value={form.title} onChange={e => setForm({...form,title:e.target.value})} placeholder="e.g., Monthly Rent" required />
              </div>
              <div className="form-group">
                <label>Payment Type</label>
                <select value={form.paymentType} onChange={e => setForm({...form,paymentType:e.target.value})}>
                  <option value="Transfer">Wallet Transfer</option>
                  <option value="BillPayment">Bill Payment</option>
                  <option value="Recharge">Mobile Recharge</option>
                </select>
              </div>
              <div className="form-group">
                <label>Amount (₹) *</label>
                <input type="number" min="1" value={form.amount} onChange={e => setForm({...form,amount:e.target.value})} placeholder="Enter amount" required />
              </div>
              <div className="form-group">
                <label>Frequency</label>
                <select value={form.frequency} onChange={e => setForm({...form,frequency:e.target.value})}>
                  {FREQ_OPTIONS.map(f => <option key={f} value={f}>{f}</option>)}
                </select>
              </div>
              {form.paymentType === 'Transfer' && (
                <div className="form-group">
                  <label>Recipient Email *</label>
                  <input type="email" value={form.recipientEmail} onChange={e => setForm({...form,recipientEmail:e.target.value})} placeholder="recipient@example.com" required={form.paymentType==='Transfer'} />
                </div>
              )}
              <div className="form-group">
                <label>Description</label>
                <input type="text" value={form.description} onChange={e => setForm({...form,description:e.target.value})} placeholder="Optional note" />
              </div>
            </div>
            <div className="form-actions">
              <button type="button" className="btn btn-secondary" onClick={() => setShowForm(false)}>Cancel</button>
              <button type="submit" className="btn btn-primary" disabled={submitting}>{submitting ? 'Creating…' : 'Schedule Payment'}</button>
            </div>
          </form>
        </div>
      )}

      {loading ? (
        <div className="page-loading"><div className="spinner" /><span>Loading…</span></div>
      ) : payments.length === 0 ? (
        <div className="card"><div className="empty-state">
          <div className="empty-state-icon">⏰</div>
          <h3>No Scheduled Payments</h3>
          <p>Set up recurring payments to automate your finances</p>
          <button className="btn btn-primary" onClick={() => setShowForm(true)}>Create First Schedule</button>
        </div></div>
      ) : (
        <div style={{display:'flex',flexDirection:'column',gap:'1rem'}}>
          {payments.map(p => (
            <div key={p.id} className="card">
              <div style={{display:'flex',alignItems:'flex-start',justifyContent:'space-between',gap:'1rem',flexWrap:'wrap'}}>
                <div style={{display:'flex',gap:'1rem',alignItems:'flex-start'}}>
                  <div style={{fontSize:'2rem'}}>{freqIcon[p.frequency] || '📆'}</div>
                  <div>
                    <div style={{fontWeight:600,color:'var(--text-1)',fontSize:'1rem'}}>{p.title}</div>
                    <div style={{fontSize:'0.82rem',color:'var(--text-3)',marginTop:'0.2rem'}}>{p.paymentType} • {p.frequency}</div>
                    {p.recipientEmail && <div style={{fontSize:'0.8rem',color:'var(--text-2)',marginTop:'0.15rem'}}>To: {p.recipientEmail}</div>}
                  </div>
                </div>
                <div style={{textAlign:'right'}}>
                  <div style={{fontWeight:700,fontSize:'1.2rem',color:'var(--text-1)'}}>₹{p.amount}</div>
                  <span className={`badge ${statusColor[p.status] || 'badge-default'}`}>{p.status}</span>
                </div>
              </div>
              <div style={{display:'flex',gap:'0.5rem',marginTop:'1rem',borderTop:'1px solid var(--border)',paddingTop:'0.875rem',flexWrap:'wrap',alignItems:'center'}}>
                <span style={{fontSize:'0.78rem',color:'var(--text-3)',flex:1}}>
                  Next: {p.nextRunAt ? new Date(p.nextRunAt).toLocaleDateString('en-IN') : '—'}
                  {p.lastRunAt && ` • Last: ${new Date(p.lastRunAt).toLocaleDateString('en-IN')}`}
                </span>
                {p.status === 'Active' && <button className="btn btn-sm btn-outline" onClick={() => handleAction(p.id,'pause')}>⏸ Pause</button>}
                {p.status === 'Paused' && <button className="btn btn-sm btn-success" onClick={() => handleAction(p.id,'resume')}>▶ Resume</button>}
                {p.status === 'Active' && <button className="btn btn-sm btn-primary" onClick={() => handleAction(p.id,'execute')}>▶ Run Now</button>}
                {p.status !== 'Cancelled' && p.status !== 'Completed' && <button className="btn btn-sm btn-danger" onClick={() => handleAction(p.id,'cancel')}>✕ Cancel</button>}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default ScheduledPayments;
