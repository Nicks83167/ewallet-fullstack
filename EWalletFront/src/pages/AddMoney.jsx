import React, { useState, useEffect } from 'react';
import api from '../api/axios';
import { formatCurrency } from '../utils/format';

const PRESETS = [100, 500, 1000, 2000, 5000, 10000];

const AddMoney = () => {
  const [amount, setAmount] = useState('');
  const [description, setDescription] = useState('');
  const [balance, setBalance] = useState(null);
  const [status, setStatus] = useState(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    api.get('/wallet/balance')
      .then(res => { if (res.data.success) setBalance(res.data.data.balance); })
      .catch(() => {});
  }, []);

  const handleSubmit = async (e) => {
    e.preventDefault();
    const parsed = parseFloat(amount);
    if (!parsed || parsed <= 0) return;
    setStatus(null);
    setLoading(true);
    try {
      const res = await api.post('/wallet/add-money', { amount: parsed, description: description || undefined });
      if (!res.data.success) throw new Error(res.data.errors?.[0] ?? res.data.message);
      setBalance(res.data.data.newBalance);
      setStatus({ type: 'success', message: res.data.data.message, ref: res.data.data.transactionId });
      setAmount('');
      setDescription('');
    } catch (err) {
      setStatus({ type: 'error', message: err.response?.data?.message ?? err.message ?? 'Something went wrong.' });
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="page-body">
      <div className="topbar" style={{ marginLeft: '-2.5rem', marginRight: '-2.5rem', marginTop: '-2rem', marginBottom: '2rem', paddingLeft: '2.5rem' }}>
        <span className="topbar-title">Add Money</span>
      </div>

      <div style={{ maxWidth: '520px' }}>
        {/* Balance card */}
        <div className="card mb-3" style={{ background: 'linear-gradient(135deg, var(--surface-2), var(--surface-3))', borderColor: 'var(--border-light)' }}>
          <div className="text-xs text-muted" style={{ marginBottom: '0.35rem', textTransform: 'uppercase', letterSpacing: '0.08em', fontWeight: 600 }}>Current Balance</div>
          <div style={{ fontSize: '2.25rem', fontWeight: 700, color: 'var(--text-1)', letterSpacing: '-0.02em' }}>
            {balance !== null ? formatCurrency(balance) : '—'}
          </div>
        </div>

        <div className="card">
          <div className="section-header">
            <div className="section-title">Deposit Funds</div>
          </div>

          {status && (
            <div className={`alert alert-${status.type}`}>
              <span>{status.type === 'success' ? '✓' : '⚠'}</span>
              <div>
                {status.message}
                {status.ref && <div className="text-xs mono" style={{ marginTop: '0.25rem', opacity: 0.7 }}>Ref: {status.ref}</div>}
              </div>
            </div>
          )}

          <form onSubmit={handleSubmit}>
            {/* Quick presets */}
            <div className="form-group">
              <label className="form-label">Quick select</label>
              <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap' }}>
                {PRESETS.map(p => (
                  <button
                    key={p}
                    type="button"
                    className={`btn btn-secondary btn-sm ${parseFloat(amount) === p ? 'active' : ''}`}
                    style={parseFloat(amount) === p ? { borderColor: 'var(--primary)', color: 'var(--primary)', background: 'var(--primary-dim)' } : {}}
                    onClick={() => setAmount(String(p))}
                  >
                    ₹{p.toLocaleString('en-IN')}
                  </button>
                ))}
              </div>
            </div>

            <div className="form-group">
              <label className="form-label">Amount</label>
              <div className="input-prefix">
                <span className="prefix-symbol">₹</span>
                <input
                  type="number"
                  step="0.01"
                  min="1"
                  max="100000"
                  className="form-input"
                  placeholder="0.00"
                  value={amount}
                  onChange={e => setAmount(e.target.value)}
                  required
                />
              </div>
              <span className="form-hint">Min ₹1 · Max ₹1,00,000</span>
            </div>

            <div className="form-group">
              <label className="form-label">Description <span className="text-muted">(optional)</span></label>
              <input
                type="text"
                className="form-input"
                placeholder="e.g. Salary credit, UPI top-up"
                value={description}
                onChange={e => setDescription(e.target.value)}
                maxLength={200}
              />
            </div>

            <button type="submit" className="btn btn-full" disabled={loading || !amount}>
              {loading ? <><span className="spinner" /> Processing…</> : `Deposit ${amount ? formatCurrency(parseFloat(amount)) : 'Funds'}`}
            </button>
          </form>
        </div>
      </div>
    </div>
  );
};

export default AddMoney;
