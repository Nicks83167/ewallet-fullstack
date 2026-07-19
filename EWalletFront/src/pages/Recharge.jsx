import React, { useState, useEffect } from 'react';
import { apiGet, apiPost } from '../utils/api';

const OPERATORS = { Mobile: ['Jio', 'Airtel', 'BSNL', 'Vi (Vodafone Idea)', 'MTNL'], DTH: ['Tata Sky', 'Airtel Digital TV', 'Dish TV', 'Sun Direct', 'D2H'], DataPack: ['Jio', 'Airtel', 'BSNL', 'Vi'], OTT: ['Netflix', 'Amazon Prime', 'Disney+ Hotstar', 'SonyLIV', 'Zee5'], Electric: ['MSEB', 'BSES', 'TSNPDCL', 'KSEB'] };

const QUICK_AMOUNTS = [19, 49, 99, 199, 299, 399, 599, 999];

const Recharge = () => {
  const [tab, setTab] = useState('recharge');
  const [form, setForm] = useState({ rechargeType: 'Mobile', operator: '', mobileNumber: '', planName: '', amount: '' });
  const [history, setHistory] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  useEffect(() => { if (tab === 'history') fetchHistory(); }, [tab]);

  const fetchHistory = async () => {
    const data = await apiGet('/api/Recharge/history?page=1&pageSize=20');
    if (data.success) setHistory(data.data?.items ?? []);
  };

  const handleRecharge = async (e) => {
    e.preventDefault(); setLoading(true); setError(''); setSuccess('');
    const data = await apiPost('/api/Recharge', {
      rechargeType: form.rechargeType,
      operator: form.operator,
      mobileNumber: form.mobileNumber,
      planName: form.planName || undefined,
      amount: parseFloat(form.amount)
    });
    if (data.success && data.data?.status === 'Success') {
      setSuccess(`✅ Recharge of ₹${form.amount} successful for ${form.mobileNumber}!`);
      setForm({ rechargeType: form.rechargeType, operator: '', mobileNumber: '', planName: '', amount: '' });
      fetchHistory();
    } else setError(data.message || (data.data?.status === 'Failed' ? 'Recharge failed. Please try again.' : 'Recharge failed'));
    setLoading(false);
  };

  return (
    <div className="page-container">
      <div className="page-header">
        <div>
          <h1 className="page-title">Recharge</h1>
          <p className="page-subtitle">Mobile, DTH, data pack and OTT recharge</p>
        </div>
      </div>

      {error && <div className="alert alert-error">❌ {error}</div>}
      {success && <div className="alert alert-success">{success}</div>}

      <div className="tabs-container">
        <div className="tabs-header">
          <button className={`tab${tab==='recharge'?' active':''}`} onClick={() => setTab('recharge')}>📶 Recharge</button>
          <button className={`tab${tab==='history'?' active':''}`} onClick={() => setTab('history')}>📋 History</button>
        </div>

        {tab === 'recharge' && (
          <div className="recharge-grid" style={{display:'grid',gridTemplateColumns:'repeat(auto-fit,minmax(320px,1fr))',gap:'1.25rem'}}>
            <div className="card">
              <div className="card-header"><h3 className="card-title">New Recharge</h3></div>
              <form onSubmit={handleRecharge} className="card-content">
                <div className="form-group">
                  <label>Recharge Type</label>
                  <select value={form.rechargeType} onChange={e => setForm({...form, rechargeType: e.target.value, operator: ''})}>
                    {Object.keys(OPERATORS).map(t => <option key={t} value={t}>{t === 'DTH' ? 'DTH / Cable TV' : t === 'OTT' ? 'OTT Subscription' : t}</option>)}
                  </select>
                </div>
                <div className="form-group">
                  <label>Operator / Provider *</label>
                  <select value={form.operator} onChange={e => setForm({...form, operator: e.target.value})} required>
                    <option value="">Select Operator</option>
                    {(OPERATORS[form.rechargeType] || []).map(op => <option key={op} value={op}>{op}</option>)}
                  </select>
                </div>
                <div className="form-group">
                  <label>{form.rechargeType === 'DTH' ? 'Subscriber ID' : form.rechargeType === 'OTT' ? 'Account / Email' : 'Mobile Number'} *</label>
                  <input type="text" value={form.mobileNumber} onChange={e => setForm({...form, mobileNumber: e.target.value})} placeholder={form.rechargeType === 'Mobile' ? '10-digit mobile number' : 'Enter ID / number'} required />
                </div>
                <div className="form-group">
                  <label>Plan Name (optional)</label>
                  <input type="text" value={form.planName} onChange={e => setForm({...form, planName: e.target.value})} placeholder="e.g., 2GB/day + unlimited calls" />
                </div>
                <div className="form-group">
                  <label>Amount (₹) *</label>
                  <input type="number" min="1" value={form.amount} onChange={e => setForm({...form, amount: e.target.value})} placeholder="Enter amount" required />
                  <div style={{display:'flex',flexWrap:'wrap',gap:'0.4rem',marginTop:'0.5rem'}}>
                    {QUICK_AMOUNTS.map(a => (
                      <button type="button" key={a} className={`btn btn-sm ${form.amount==a?'btn-primary':'btn-outline'}`} onClick={() => setForm({...form, amount: String(a)})}>₹{a}</button>
                    ))}
                  </div>
                </div>
                <button type="submit" className="btn btn-primary btn-full" disabled={loading}>
                  {loading ? 'Processing…' : `Recharge ₹${form.amount||'0'}`}
                </button>
              </form>
            </div>

            <div className="card">
              <div className="card-header"><h3 className="card-title">💡 Recharge Tips</h3></div>
              <div className="card-content">
                <div className="tips-list">
                  {[['📱','Instant Processing','Most recharges are processed within seconds.'],['🔄','Auto-Retry','Failed recharges are automatically retried.'],['💰','No Extra Charges','Recharge at face value — no hidden fees.'],['📋','Keep Record','All recharges are saved in history.']].map(([icon,title,desc]) => (
                    <div key={title} className="tip-item" style={{display:'flex',gap:'0.75rem',alignItems:'flex-start'}}>
                      <span style={{fontSize:'1.2rem'}}>{icon}</span>
                      <div><strong style={{color:'var(--text-1)'}}>{title}:</strong><span style={{color:'var(--text-2)',marginLeft:'0.35rem'}}>{desc}</span></div>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </div>
        )}

        {tab === 'history' && (
          <div className="card">
            <div className="card-header"><h3 className="card-title">Recharge History</h3></div>
            <div className="card-content">
              {history.length === 0 ? (
                <div className="empty-state"><div className="empty-state-icon">📶</div><h3>No Recharges Yet</h3><p>Your recharge history will appear here</p></div>
              ) : (
                <div className="table-wrap">
                  <table>
                    <thead><tr><th>Type</th><th>Operator</th><th>Number</th><th>Plan</th><th>Amount</th><th>Status</th><th>Date</th></tr></thead>
                    <tbody>
                      {history.map(r => (
                        <tr key={r.id}>
                          <td>{r.rechargeType}</td>
                          <td>{r.operator}</td>
                          <td className="mono">{r.mobileNumber}</td>
                          <td>{r.planName || '—'}</td>
                          <td><strong>₹{r.amount}</strong></td>
                          <td><span className={`badge ${r.status==='Success'?'badge-success':r.status==='Failed'?'badge-danger':'badge-warning'}`}>{r.status}</span></td>
                          <td className="text-sm">{new Date(r.createdAt).toLocaleDateString('en-IN')}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default Recharge;
