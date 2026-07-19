import React, { useState, useEffect } from 'react';
import { apiGet, apiPost } from '../utils/api';

const QRPayment = () => {
  const [activeTab, setActiveTab] = useState('generate');
  const [genForm, setGenForm] = useState({ amount: '', description: '' });
  const [scanForm, setScanForm] = useState({ qrCode: '', amount: '' });
  const [history, setHistory] = useState([]);
  const [generatedQR, setGeneratedQR] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  useEffect(() => {
    if (activeTab === 'history') fetchHistory();
  }, [activeTab]);

  const fetchHistory = async () => {
    const data = await apiGet('/api/QRPayment/history?page=1&pageSize=20');
    if (data.success) setHistory(data.data?.items ?? []);
  };

  const handleGenerate = async (e) => {
    e.preventDefault(); setLoading(true); setError(''); setSuccess('');
    const data = await apiPost('/api/QRPayment/generate', {
      amount: parseFloat(genForm.amount),
      description: genForm.description || undefined
    });
    if (data.success) {
      setGeneratedQR(data.data);
      setSuccess('QR code generated!');
      setGenForm({ amount: '', description: '' });
    } else setError(data.message || 'Failed to generate QR');
    setLoading(false);
  };

  const handleScanPay = async (e) => {
    e.preventDefault(); setLoading(true); setError(''); setSuccess('');
    const data = await apiPost('/api/QRPayment/scan-and-pay', {
      qrCode: scanForm.qrCode,
      amount: parseFloat(scanForm.amount)
    });
    if (data.success) {
      setSuccess('Payment completed successfully!');
      setScanForm({ qrCode: '', amount: '' });
      setActiveTab('history');
    } else setError(data.message || 'Payment failed');
    setLoading(false);
  };

  const copyCode = () => {
    if (generatedQR) { navigator.clipboard.writeText(generatedQR.qrCode); setSuccess('QR code copied!'); }
  };

  return (
    <div className="page-container">
      <div className="page-header">
        <div>
          <h1 className="page-title">QR Payments</h1>
          <p className="page-subtitle">Generate QR codes to receive payments or scan to pay</p>
        </div>
      </div>

      {error && <div className="alert alert-error">❌ {error}</div>}
      {success && <div className="alert alert-success">✅ {success}</div>}

      <div className="tabs-container">
        <div className="tabs-header">
          <button className={`tab${activeTab==='generate'?' active':''}`} onClick={() => setActiveTab('generate')}>📱 Generate QR</button>
          <button className={`tab${activeTab==='scan'?' active':''}`} onClick={() => setActiveTab('scan')}>📷 Scan & Pay</button>
          <button className={`tab${activeTab==='history'?' active':''}`} onClick={() => setActiveTab('history')}>📋 History</button>
        </div>

        {activeTab === 'generate' && (
          <div style={{display:'grid',gridTemplateColumns:'repeat(auto-fit,minmax(300px,1fr))',gap:'1.25rem'}}>
            <div className="card">
              <div className="card-header"><h3 className="card-title">Create Payment Request</h3></div>
              <form onSubmit={handleGenerate} className="card-content">
                <div className="form-group">
                  <label>Amount (₹) *</label>
                  <input type="number" step="0.01" min="1" value={genForm.amount} onChange={e => setGenForm({...genForm, amount: e.target.value})} placeholder="Enter amount to request" required />
                </div>
                <div className="form-group">
                  <label>Description</label>
                  <input type="text" value={genForm.description} onChange={e => setGenForm({...genForm, description: e.target.value})} placeholder="e.g., Dinner payment" />
                </div>
                <button type="submit" className="btn btn-primary btn-full" disabled={loading}>{loading ? 'Generating…' : 'Generate QR Code'}</button>
              </form>
            </div>

            {generatedQR && (
              <div className="card">
                <div className="card-header"><h3 className="card-title">Your QR Code</h3></div>
                <div className="card-content" style={{textAlign:'center'}}>
                  <div style={{width:160,height:160,margin:'0 auto 1rem',background:'#fff',borderRadius:8,display:'flex',flexDirection:'column',alignItems:'center',justifyContent:'center',padding:'1rem'}}>
                    <div style={{display:'grid',gridTemplateColumns:'repeat(5,1fr)',gap:3,width:'100%'}}>
                      {Array.from({length:25}).map((_,i) => (
                        <div key={i} style={{background:Math.random()>0.4?'#000':'#fff',aspectRatio:'1',borderRadius:1}} />
                      ))}
                    </div>
                  </div>
                  <div style={{fontWeight:700,fontSize:'1.4rem',color:'var(--text-1)'}}>₹{generatedQR.amount}</div>
                  {generatedQR.description && <div style={{color:'var(--text-2)',fontSize:'0.875rem',marginTop:'0.25rem'}}>{generatedQR.description}</div>}
                  <div style={{color:'var(--text-3)',fontSize:'0.8rem',marginTop:'0.25rem'}}>To: {generatedQR.merchantName}</div>
                  <div style={{display:'flex',gap:'0.5rem',justifyContent:'center',marginTop:'1rem'}}>
                    <button className="btn btn-outline btn-sm" onClick={copyCode}>📋 Copy Code</button>
                  </div>
                  <div style={{fontSize:'0.65rem',color:'var(--text-3)',marginTop:'0.75rem',wordBreak:'break-all'}}>
                    {generatedQR.qrCode.substring(0, 60)}…
                  </div>
                </div>
              </div>
            )}
          </div>
        )}

        {activeTab === 'scan' && (
          <div className="card" style={{maxWidth:480}}>
            <div className="card-header"><h3 className="card-title">Scan QR Code to Pay</h3></div>
            <form onSubmit={handleScanPay} className="card-content">
              <div className="form-group">
                <label>QR Code Data *</label>
                <textarea value={scanForm.qrCode} onChange={e => setScanForm({...scanForm, qrCode: e.target.value})} placeholder="Paste QR code data here" rows={4} required style={{width:'100%',padding:'0.7rem 0.9rem',background:'var(--surface-2)',border:'1px solid var(--border)',color:'var(--text-1)',borderRadius:'var(--radius-sm)',fontFamily:'inherit',fontSize:'0.85rem',resize:'vertical'}} />
              </div>
              <div className="form-group">
                <label>Amount (₹) *</label>
                <input type="number" step="0.01" min="1" value={scanForm.amount} onChange={e => setScanForm({...scanForm, amount: e.target.value})} placeholder="Enter payment amount" required />
              </div>
              <button type="submit" className="btn btn-primary btn-full" disabled={loading}>{loading ? 'Processing…' : 'Scan & Pay'}</button>
            </form>
            <div style={{padding:'1rem 1.5rem',textAlign:'center',borderTop:'1px solid var(--border)',color:'var(--text-3)',fontSize:'0.82rem'}}>
              📷 Camera scanner — paste QR data manually above
            </div>
          </div>
        )}

        {activeTab === 'history' && (
          <div className="card">
            <div className="card-header"><h3 className="card-title">QR Payment History</h3></div>
            <div className="card-content">
              {history.length === 0 ? (
                <div className="empty-state"><div className="empty-state-icon">📱</div><h3>No QR Payments Yet</h3><p>History appears here after you use QR payments</p></div>
              ) : (
                <div className="table-wrap">
                  <table>
                    <thead><tr><th>Type</th><th>Description</th><th>Amount</th><th>Status</th><th>Date</th></tr></thead>
                    <tbody>
                      {history.map(p => (
                        <tr key={p.id}>
                          <td><span style={{fontSize:'1.2rem'}}>{p.paymentType==='Pay'?'📤':'📥'}</span> {p.paymentType}</td>
                          <td>{p.description || '—'}</td>
                          <td><strong>₹{p.amount}</strong></td>
                          <td><span className={`badge ${p.status==='Success'?'badge-success':p.status==='Failed'?'badge-danger':'badge-warning'}`}>{p.status}</span></td>
                          <td className="text-sm">{new Date(p.createdAt).toLocaleDateString('en-IN')}</td>
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

      <div className="card" style={{marginTop:'1.25rem'}}>
        <div className="card-header"><h3 className="card-title">How QR Payments Work</h3></div>
        <div className="card-content" style={{display:'grid',gridTemplateColumns:'repeat(auto-fit,minmax(180px,1fr))',gap:'1rem'}}>
          {[['📱','Generate QR','Create a code with the amount you want to receive'],['📷','Scan & Pay','Scan someone\'s QR code or paste data to pay'],['⚡','Instant','Money transfers instantly between wallets'],['🔒','Secure','All payments are encrypted and validated']].map(([icon,title,desc]) => (
            <div key={title} style={{textAlign:'center',padding:'1rem',background:'var(--surface-2)',borderRadius:'var(--radius-sm)'}}>
              <div style={{fontSize:'1.75rem',marginBottom:'0.5rem'}}>{icon}</div>
              <div style={{fontWeight:600,color:'var(--text-1)',marginBottom:'0.25rem'}}>{title}</div>
              <div style={{fontSize:'0.78rem',color:'var(--text-3)'}}>{desc}</div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
};

export default QRPayment;
