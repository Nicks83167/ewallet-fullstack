import React, { useState, useEffect, useCallback } from 'react';
import { apiGet, apiPost, apiPut, apiDelete } from '../utils/api';

const emptyForm = { name: '', email: '', phone: '', upiId: '', bankName: '', isFavourite: false };

const Beneficiaries = () => {
  const [beneficiaries, setBeneficiaries] = useState([]);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [showForm, setShowForm] = useState(false);
  const [editId, setEditId] = useState(null);
  const [search, setSearch] = useState('');
  const [formData, setFormData] = useState(emptyForm);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  const fetchBeneficiaries = useCallback(async () => {
    const url = search ? `/api/Beneficiary?search=${encodeURIComponent(search)}` : '/api/Beneficiary';
    const data = await apiGet(url);
    if (data.success) setBeneficiaries(data.data ?? []);
    else setError(data.message || 'Failed to fetch beneficiaries');
    setLoading(false);
  }, [search]);

  useEffect(() => { fetchBeneficiaries(); }, [fetchBeneficiaries]);

  const openAdd = () => { setEditId(null); setFormData(emptyForm); setShowForm(true); };
  const openEdit = (b) => { setEditId(b.id); setFormData({ name: b.name, email: b.email, phone: b.phone||'', upiId: b.upiId||'', bankName: b.bankName||'', isFavourite: b.isFavourite }); setShowForm(true); };
  const closeForm = () => { setShowForm(false); setEditId(null); setFormData(emptyForm); };

  const handleSubmit = async (e) => {
    e.preventDefault(); setSubmitting(true); setError(''); setSuccess('');
    const data = editId ? await apiPut(`/api/Beneficiary/${editId}`, formData) : await apiPost('/api/Beneficiary', formData);
    if (data.success) { setSuccess(editId ? 'Updated!' : 'Beneficiary added!'); closeForm(); fetchBeneficiaries(); }
    else setError(data.message || 'Failed to save beneficiary');
    setSubmitting(false);
  };

  const handleDelete = async (id) => {
    if (!window.confirm('Remove this beneficiary?')) return;
    const data = await apiDelete(`/api/Beneficiary/${id}`);
    if (data.success) { setSuccess('Removed!'); fetchBeneficiaries(); }
    else setError(data.message || 'Failed to remove');
  };

  const handleFavourite = async (id) => {
    await apiPost(`/api/Beneficiary/${id}/toggle-favourite`, {});
    fetchBeneficiaries();
  };

  return (
    <div className="page-container">
      <div className="page-header">
        <div>
          <h1 className="page-title">Beneficiaries</h1>
          <p className="page-subtitle">Saved contacts for quick money transfers</p>
        </div>
        <button className="btn btn-primary" onClick={openAdd}>+ Add Beneficiary</button>
      </div>

      {error && <div className="alert alert-error">❌ {error}</div>}
      {success && <div className="alert alert-success">✅ {success}</div>}

      <div className="search-section">
        <div className="search-box">
          <span className="search-icon">🔍</span>
          <input type="text" placeholder="Search by name, email or phone…" value={search} onChange={e => setSearch(e.target.value)} />
        </div>
      </div>

      {showForm && (
        <div className="card mb-6">
          <div className="card-header">
            <h3 className="card-title">{editId ? 'Edit Beneficiary' : 'Add New Beneficiary'}</h3>
            <button className="btn-ghost" onClick={closeForm}>✕</button>
          </div>
          <form onSubmit={handleSubmit} className="card-content">
            <div className="form-grid">
              <div className="form-group">
                <label>Full Name *</label>
                <input type="text" value={formData.name} onChange={e => setFormData({...formData, name: e.target.value})} placeholder="Rahul Sharma" required />
              </div>
              <div className="form-group">
                <label>Email *</label>
                <input type="email" value={formData.email} onChange={e => setFormData({...formData, email: e.target.value})} placeholder="rahul@example.com" required />
              </div>
              <div className="form-group">
                <label>Phone</label>
                <input type="tel" value={formData.phone} onChange={e => setFormData({...formData, phone: e.target.value})} placeholder="+91 98765 43210" />
              </div>
              <div className="form-group">
                <label>UPI ID</label>
                <input type="text" value={formData.upiId} onChange={e => setFormData({...formData, upiId: e.target.value})} placeholder="rahul@paytm" />
              </div>
              <div className="form-group">
                <label>Bank Name</label>
                <input type="text" value={formData.bankName} onChange={e => setFormData({...formData, bankName: e.target.value})} placeholder="State Bank of India" />
              </div>
              <div className="form-group" style={{display:'flex',alignItems:'center',paddingTop:'1.5rem'}}>
                <label className="checkbox-label">
                  <input type="checkbox" checked={formData.isFavourite} onChange={e => setFormData({...formData, isFavourite: e.target.checked})} />
                  Mark as Favourite
                </label>
              </div>
            </div>
            <div className="form-actions">
              <button type="button" className="btn btn-secondary" onClick={closeForm}>Cancel</button>
              <button type="submit" className="btn btn-primary" disabled={submitting}>{submitting ? 'Saving…' : (editId ? 'Update' : 'Add Beneficiary')}</button>
            </div>
          </form>
        </div>
      )}

      {loading ? (
        <div className="page-loading"><div className="spinner" /><span>Loading…</span></div>
      ) : beneficiaries.length === 0 ? (
        <div className="card"><div className="empty-state">
          <div className="empty-state-icon">👥</div>
          <h3>No Beneficiaries</h3>
          <p>Add people you send money to regularly</p>
          <button className="btn btn-primary" onClick={openAdd}>Add Your First Beneficiary</button>
        </div></div>
      ) : (
        <div className="beneficiaries-grid">
          {beneficiaries.map(b => (
            <div key={b.id} className="beneficiary-card">
              <div className="beneficiary-header">
                <div className="beneficiary-avatar">{b.name.charAt(0).toUpperCase()}</div>
                <div className="beneficiary-info">
                  <div className="beneficiary-name">{b.name}{b.isFavourite && <span className="favourite-star">⭐</span>}</div>
                  <div className="beneficiary-email">{b.email}</div>
                </div>
                <button className="favourite-btn" onClick={() => handleFavourite(b.id)} title="Toggle favourite">{b.isFavourite ? '💗' : '🤍'}</button>
              </div>
              <div className="beneficiary-details">
                {b.phone && <div className="detail-item"><span className="label">📱 Phone:</span><span className="value">{b.phone}</span></div>}
                {b.upiId && <div className="detail-item"><span className="label">💳 UPI:</span><span className="value">{b.upiId}</span></div>}
                {b.bankName && <div className="detail-item"><span className="label">🏦 Bank:</span><span className="value">{b.bankName}</span></div>}
              </div>
              <div className="beneficiary-actions">
                <button className="btn btn-sm btn-primary" onClick={() => window.location.href=`/transfer?email=${encodeURIComponent(b.email)}`}>Send Money</button>
                <button className="btn btn-sm btn-outline" onClick={() => openEdit(b)}>Edit</button>
                <button className="btn btn-sm btn-danger" onClick={() => handleDelete(b.id)}>Remove</button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default Beneficiaries;
