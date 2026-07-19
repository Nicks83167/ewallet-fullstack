import React, { useState, useEffect } from 'react';
import { apiGet, apiPost, apiDelete } from '../utils/api';

const LinkedAccounts = () => {
  const [accounts, setAccounts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [showAddForm, setShowAddForm] = useState(false);
  const [formData, setFormData] = useState({ accountType: 'BankAccount', name: '', accountNumber: '', bankName: '' });
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  useEffect(() => { fetchAccounts(); }, []);

  const fetchAccounts = async () => {
    setLoading(true);
    const data = await apiGet('/api/LinkedAccount');
    if (data.success) setAccounts(data.data ?? []);
    else setError(data.message || 'Failed to fetch linked accounts');
    setLoading(false);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSubmitting(true); setError(''); setSuccess('');
    const data = await apiPost('/api/LinkedAccount', formData);
    if (data.success) {
      setSuccess('Account linked successfully!');
      setShowAddForm(false);
      setFormData({ accountType: 'BankAccount', name: '', accountNumber: '', bankName: '' });
      fetchAccounts();
    } else setError(data.message || 'Failed to link account');
    setSubmitting(false);
  };

  const handleDelete = async (id) => {
    if (!window.confirm('Remove this account?')) return;
    const data = await apiDelete(`/api/LinkedAccount/${id}`);
    if (data.success) { setSuccess('Account removed.'); fetchAccounts(); }
    else setError(data.message || 'Failed to remove account');
  };

  const handleSetDefault = async (id) => {
    const data = await apiPost(`/api/LinkedAccount/${id}/set-default`, {});
    if (data.success) { setSuccess('Default account updated!'); fetchAccounts(); }
    else setError(data.message || 'Failed to update default');
  };

  const typeIcons = { BankAccount: '🏦', DebitCard: '💳', CreditCard: '💎', UpiId: '📱', WalletAccount: '👛' };

  return (
    <div className="page-container">
      <div className="page-header">
        <div>
          <h1 className="page-title">Linked Accounts</h1>
          <p className="page-subtitle">Manage your bank accounts, cards and UPI IDs</p>
        </div>
        <button className="btn btn-primary" onClick={() => setShowAddForm(true)}>+ Add Account</button>
      </div>

      {error && <div className="alert alert-error">❌ {error}</div>}
      {success && <div className="alert alert-success">✅ {success}</div>}

      {showAddForm && (
        <div className="card mb-6">
          <div className="card-header">
            <h3 className="card-title">Add New Account</h3>
            <button className="btn-ghost" onClick={() => setShowAddForm(false)}>✕</button>
          </div>
          <form onSubmit={handleSubmit} className="card-content">
            <div className="form-grid">
              <div className="form-group">
                <label>Account Type</label>
                <select value={formData.accountType} onChange={e => setFormData({...formData, accountType: e.target.value})}>
                  <option value="BankAccount">Bank Account</option>
                  <option value="DebitCard">Debit Card</option>
                  <option value="CreditCard">Credit Card</option>
                  <option value="UpiId">UPI ID</option>
                  <option value="WalletAccount">Wallet Account</option>
                </select>
              </div>
              <div className="form-group">
                <label>Account Name *</label>
                <input type="text" value={formData.name} onChange={e => setFormData({...formData, name: e.target.value})} placeholder="e.g., Main Savings Account" required />
              </div>
              <div className="form-group">
                <label>{formData.accountType === 'UpiId' ? 'UPI ID' : 'Account Number'} *</label>
                <input type="text" value={formData.accountNumber} onChange={e => setFormData({...formData, accountNumber: e.target.value})} placeholder={formData.accountType === 'UpiId' ? 'rahul@paytm' : '****1234'} required />
              </div>
              <div className="form-group">
                <label>Bank / Provider Name</label>
                <input type="text" value={formData.bankName} onChange={e => setFormData({...formData, bankName: e.target.value})} placeholder="e.g., State Bank of India" />
              </div>
            </div>
            <div className="form-actions">
              <button type="button" className="btn btn-secondary" onClick={() => setShowAddForm(false)}>Cancel</button>
              <button type="submit" className="btn btn-primary" disabled={submitting}>{submitting ? 'Adding…' : 'Add Account'}</button>
            </div>
          </form>
        </div>
      )}

      {loading ? (
        <div className="page-loading"><div className="spinner" /><span>Loading accounts…</span></div>
      ) : accounts.length === 0 ? (
        <div className="card"><div className="empty-state">
          <div className="empty-state-icon">🏦</div>
          <h3>No Linked Accounts</h3>
          <p>Add your bank accounts, cards, or UPI IDs for easy payments</p>
          <button className="btn btn-primary" onClick={() => setShowAddForm(true)}>Add Your First Account</button>
        </div></div>
      ) : (
        <div className="accounts-grid">
          {accounts.map(account => (
            <div key={account.id} className={`account-card${account.isDefault ? ' default' : ''}`}>
              <div className="account-header">
                <div className="account-icon">{typeIcons[account.accountType] || '💳'}</div>
                <div className="account-info">
                  <div className="account-name">{account.name}</div>
                  <div className="account-type">{account.accountType}</div>
                </div>
                {account.isDefault && <span className="default-badge">Default</span>}
              </div>
              <div className="account-details">
                <div className="account-number"><span className="label">Number:</span><span className="value">{account.maskedNumber}</span></div>
                {account.bankName && <div className="bank-name"><span className="label">Bank:</span><span className="value">{account.bankName}</span></div>}
                <div className="account-status"><span className="label">Status:</span>
                  <span className={`status ${account.isVerified ? 'verified' : 'pending'}`}>{account.isVerified ? '✅ Verified' : '⏳ Pending'}</span>
                </div>
              </div>
              <div className="account-actions">
                {!account.isDefault && <button className="btn btn-sm btn-outline" onClick={() => handleSetDefault(account.id)}>Set as Default</button>}
                <button className="btn btn-sm btn-danger" onClick={() => handleDelete(account.id)}>Remove</button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default LinkedAccounts;
