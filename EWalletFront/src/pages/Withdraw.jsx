import React, { useState, useEffect } from 'react';
import api from '../api/axios';
import { formatCurrency } from '../utils/format';
import ConfirmModal from '../components/ConfirmModal';

const Withdraw = () => {
  const [amount, setAmount] = useState('');
  const [description, setDescription] = useState('');
  const [balance, setBalance] = useState(null);
  const [status, setStatus] = useState(null);
  const [loading, setLoading] = useState(false);
  const [confirm, setConfirm] = useState(false);

  useEffect(() => {
    api.get('/wallet/balance')
      .then(res => { if (res.data.success) setBalance(res.data.data.balance); })
      .catch(() => {});
  }, []);

  const parsed = parseFloat(amount) || 0;
  const insufficient = balance !== null && parsed > balance;

  const handleSubmit = (e) => {
    e.preventDefault();
    if (!parsed || parsed <= 0 || insufficient) return;
    setConfirm(true);
  };

  const doWithdraw = async () => {
    setConfirm(false);
    setStatus(null);
    setLoading(true);
    try {
      const res = await api.post('/wallet/withdraw', { amount: parsed, description: description || undefined });
      if (!res.data.success) throw new Error(res.data.errors?.[0] ?? res.data.message);
      setBalance(res.data.data.newBalance);
      setStatus({ type: 'success', message: res.data.data.message, ref: res.data.data.transactionId });
      setAmount('');
      setDescription('');
    } catch (err) {
      setStatus({ type: 'error', message: err.response?.data?.message ?? err.message ?? 'Withdrawal failed.' });
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="page-body">
      <div className="topbar" style={{ marginLeft: '-2.5rem', marginRight: '-2.5rem', marginTop: '-2rem', marginBottom: '2rem', paddingLeft: '2.5rem' }}>
        <span className="topbar-title">Withdraw</span>
      </div>

      <div style={{ maxWidth: '520px' }}>
        <div className="card mb-3" style={{ background: 'linear-gradient(135deg, var(--surface-2), var(--surface-3))', borderColor: 'var(--border-light)' }}>
          <div className="text-xs text-muted" style={{ marginBottom: '0.35rem', textTransform: 'uppercase', letterSpacing: '0.08em', fontWeight: 600 }}>Available Balance</div>
          <div style={{ fontSize: '2.25rem', fontWeight: 700, color: 'var(--text-1)', letterSpacing: '-0.02em' }}>
            {balance !== null ? formatCurrency(balance) : '—'}
          </div>
        </div>

        <div className="card">
          <div className="section-header">
            <div className="section-title">Withdraw Funds</div>
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
            <div className="form-group">
              <label className="form-label">Amount</label>
              <div className="input-prefix">
                <span className="prefix-symbol">₹</span>
                <input
                  type="number"
                  step="0.01"
                  min="1"
                  max={balance ?? 100000}
                  className="form-input"
                  placeholder="0.00"
                  value={amount}
                  onChange={e => setAmount(e.target.value)}
                  required
                />
              </div>
              {insufficient && (
                <span className="form-hint text-danger">Insufficient balance. Available: {formatCurrency(balance)}</span>
              )}
            </div>

            <div className="form-group">
              <label className="form-label">Description <span className="text-muted">(optional)</span></label>
              <input
                type="text"
                className="form-input"
                placeholder="e.g. UPI withdrawal, ATM"
                value={description}
                onChange={e => setDescription(e.target.value)}
                maxLength={200}
              />
            </div>

            <button
              type="submit"
              className="btn btn-full"
              style={{ background: 'var(--danger)' }}
              disabled={loading || !parsed || insufficient}
            >
              {loading ? <><span className="spinner" /> Processing…</> : `Withdraw ${parsed ? formatCurrency(parsed) : 'Funds'}`}
            </button>
          </form>
        </div>
      </div>

      {confirm && (
        <ConfirmModal
          title="Confirm Withdrawal"
          onConfirm={doWithdraw}
          onCancel={() => setConfirm(false)}
          confirmLabel="Yes, Withdraw"
          confirmClass="btn btn-danger"
        >
          <p>You are about to withdraw:</p>
          <div className="modal-highlight">
            <div className="modal-highlight-row">
              <span>Amount</span>
              <strong className="amount-debit">{formatCurrency(parsed)}</strong>
            </div>
            <div className="modal-highlight-row">
              <span>Balance after</span>
              <strong>{formatCurrency((balance ?? 0) - parsed)}</strong>
            </div>
          </div>
          <p>This action cannot be undone.</p>
        </ConfirmModal>
      )}
    </div>
  );
};

export default Withdraw;
