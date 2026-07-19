import React, { useState, useEffect } from 'react';
import { apiPost } from '../utils/api';

const DEMO_MERCHANTS = [
  { id: 'm1', name: 'Swiggy', category: 'Food Delivery', icon: '🍔', description: 'Order food from top restaurants', color: '#FF6600' },
  { id: 'm2', name: 'Zomato', category: 'Food Delivery', icon: '🍕', description: 'Food delivery & restaurant discovery', color: '#E23744' },
  { id: 'm3', name: 'Amazon', category: 'E-Commerce', icon: '📦', description: 'Online shopping & more', color: '#FF9900' },
  { id: 'm4', name: 'Flipkart', category: 'E-Commerce', icon: '🛒', description: 'India\'s largest e-commerce platform', color: '#2874F0' },
  { id: 'm5', name: 'Ola', category: 'Transport', icon: '🚖', description: 'Book cabs & auto-rickshaws', color: '#1CAB46' },
  { id: 'm6', name: 'Uber', category: 'Transport', icon: '🚗', description: 'Ride-sharing & delivery', color: '#000000' },
  { id: 'm7', name: 'BookMyShow', category: 'Entertainment', icon: '🎬', description: 'Movies, events & experiences', color: '#E03027' },
  { id: 'm8', name: 'MakeMyTrip', category: 'Travel', icon: '✈️', description: 'Flights, hotels & holiday packages', color: '#0060AF' },
  { id: 'm9', name: 'BigBasket', category: 'Grocery', icon: '🛍️', description: 'Online grocery delivery', color: '#84C225' },
  { id: 'm10', name: 'Nykaa', category: 'Beauty', icon: '💄', description: 'Beauty & wellness products', color: '#FC2779' },
  { id: 'm11', name: 'IRCTC', category: 'Travel', icon: '🚆', description: 'Train tickets & tourism', color: '#004080' },
  { id: 'm12', name: 'PharmEasy', category: 'Healthcare', icon: '💊', description: 'Online pharmacy & health products', color: '#15B4A7' },
];

const CATEGORIES = ['All', ...new Set(DEMO_MERCHANTS.map(m => m.category))];

const Merchants = () => {
  const [activeCategory, setActiveCategory] = useState('All');
  const [payModal, setPayModal] = useState(null);
  const [amount, setAmount] = useState('');
  const [desc, setDesc] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  const filtered = activeCategory === 'All' ? DEMO_MERCHANTS : DEMO_MERCHANTS.filter(m => m.category === activeCategory);

  const openPay = (merchant) => { setPayModal(merchant); setAmount(''); setDesc(''); setError(''); };
  const closePay = () => setPayModal(null);

  const handlePay = async (e) => {
    e.preventDefault(); setLoading(true); setError('');
    if (!amount || parseFloat(amount) <= 0) { setError('Enter a valid amount'); setLoading(false); return; }

    // Simulate merchant payment (demo)
    await new Promise(r => setTimeout(r, 1000));
    setSuccess(`✅ Payment of ₹${amount} to ${payModal.name} successful!`);
    closePay();
    setLoading(false);
  };

  return (
    <div className="page-container">
      <div className="page-header">
        <div>
          <h1 className="page-title">Merchant Payments</h1>
          <p className="page-subtitle">Pay your favourite brands and services instantly</p>
        </div>
      </div>

      {error && <div className="alert alert-error">❌ {error}</div>}
      {success && <div className="alert alert-success">{success}</div>}

      {/* Category filter */}
      <div style={{display:'flex',gap:'0.5rem',flexWrap:'wrap',marginBottom:'1.5rem'}}>
        {CATEGORIES.map(cat => (
          <button key={cat} className={`tab${activeCategory===cat?' active':''}`} onClick={() => setActiveCategory(cat)}>{cat}</button>
        ))}
      </div>

      <div className="merchants-grid">
        {filtered.map(merchant => (
          <div key={merchant.id} className="merchant-card">
            <div className="merchant-header">
              <div className="merchant-icon" style={{width:48,height:48,background:'var(--surface-2)',borderRadius:'var(--radius-sm)',display:'flex',alignItems:'center',justifyContent:'center',fontSize:'1.75rem',flexShrink:0}}>
                {merchant.icon}
              </div>
              <div className="merchant-info">
                <div className="merchant-name">{merchant.name}</div>
                <div className="merchant-category">{merchant.category}</div>
              </div>
            </div>
            <p style={{fontSize:'0.82rem',color:'var(--text-3)',marginBottom:'1rem',lineHeight:1.5}}>{merchant.description}</p>
            <div className="merchant-actions">
              <button className="btn btn-primary btn-sm" style={{width:'100%'}} onClick={() => openPay(merchant)}>Pay Now</button>
            </div>
          </div>
        ))}
      </div>

      {/* Payment Modal */}
      {payModal && (
        <div className="modal-overlay" onClick={closePay}>
          <div className="modal" onClick={e => e.stopPropagation()}>
            <div style={{display:'flex',alignItems:'center',gap:'0.875rem',marginBottom:'1.25rem'}}>
              <span style={{fontSize:'2rem'}}>{payModal.icon}</span>
              <div>
                <div className="modal-title">Pay {payModal.name}</div>
                <div style={{fontSize:'0.82rem',color:'var(--text-3)'}}>{payModal.category}</div>
              </div>
            </div>
            <form onSubmit={handlePay}>
              <div className="form-group">
                <label className="form-label">Amount (₹) *</label>
                <input className="form-input" type="number" min="1" value={amount} onChange={e => setAmount(e.target.value)} placeholder="Enter amount" required />
              </div>
              <div className="form-group">
                <label className="form-label">Description (optional)</label>
                <input className="form-input" type="text" value={desc} onChange={e => setDesc(e.target.value)} placeholder={`Payment to ${payModal.name}`} />
              </div>
              {error && <div className="alert alert-error" style={{marginBottom:'1rem'}}>❌ {error}</div>}
              <div className="modal-actions">
                <button type="button" className="btn btn-secondary" onClick={closePay}>Cancel</button>
                <button type="submit" className="btn btn-primary" disabled={loading}>{loading ? 'Processing…' : `Pay ₹${amount||'0'}`}</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default Merchants;
