import React, { useState, useEffect } from 'react';
import { apiGet, apiPost } from '../utils/api';

const DEMO_CURRENCIES = [
  { id:'1', code:'INR', name:'Indian Rupee', symbol:'₹' },
  { id:'2', code:'USD', name:'US Dollar', symbol:'$' },
  { id:'3', code:'EUR', name:'Euro', symbol:'€' },
  { id:'4', code:'GBP', name:'British Pound', symbol:'£' },
  { id:'5', code:'JPY', name:'Japanese Yen', symbol:'¥' },
  { id:'6', code:'AUD', name:'Australian Dollar', symbol:'A$' },
  { id:'7', code:'CAD', name:'Canadian Dollar', symbol:'C$' },
  { id:'8', code:'SGD', name:'Singapore Dollar', symbol:'S$' },
];

const DEMO_RATES = [
  { fromCurrency:'INR', toCurrency:'USD', rate:0.012 },
  { fromCurrency:'USD', toCurrency:'INR', rate:83.25 },
  { fromCurrency:'INR', toCurrency:'EUR', rate:0.011 },
  { fromCurrency:'EUR', toCurrency:'INR', rate:90.15 },
  { fromCurrency:'INR', toCurrency:'GBP', rate:0.0095 },
  { fromCurrency:'GBP', toCurrency:'INR', rate:105.20 },
  { fromCurrency:'USD', toCurrency:'EUR', rate:0.92 },
  { fromCurrency:'EUR', toCurrency:'USD', rate:1.09 },
];

const Currency = () => {
  const [currencies, setCurrencies] = useState(DEMO_CURRENCIES);
  const [rates, setRates] = useState(DEMO_RATES);
  const [loading, setLoading] = useState(true);
  const [fromCur, setFromCur] = useState('INR');
  const [toCur, setToCur] = useState('USD');
  const [amount, setAmount] = useState('');
  const [result, setResult] = useState(null);
  const [converting, setConverting] = useState(false);
  const [walletCurrency, setWalletCurrency] = useState('INR');
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  useEffect(() => {
    const load = async () => {
      const [cRes, rRes] = await Promise.all([apiGet('/api/Currency'), apiGet('/api/Currency/exchange-rates')]);
      if (cRes.success && cRes.data?.length) setCurrencies(cRes.data);
      if (rRes.success && rRes.data?.length) setRates(rRes.data);
      setLoading(false);
    };
    load();
  }, []);

  const handleConvert = async (e) => {
    e.preventDefault(); setConverting(true); setError(''); setResult(null);
    if (!amount || parseFloat(amount) <= 0) { setError('Enter a valid amount'); setConverting(false); return; }
    const data = await apiPost('/api/Currency/convert', { fromCurrency: fromCur, toCurrency: toCur, amount: parseFloat(amount) });
    if (data.success) setResult(data.data);
    else {
      // Fallback: calculate client-side
      const rate = rates.find(r => r.fromCurrency === fromCur && r.toCurrency === toCur);
      if (rate) {
        const converted = parseFloat(amount) * rate.rate;
        const fromSym = currencies.find(c => c.code === fromCur)?.symbol || fromCur;
        const toSym = currencies.find(c => c.code === toCur)?.symbol || toCur;
        setResult({ originalAmount: parseFloat(amount), convertedAmount: converted.toFixed(2), exchangeRate: rate.rate, fromCurrency: fromCur, toCurrency: toCur, formattedOriginal: `${fromSym}${parseFloat(amount).toFixed(2)}`, formattedConverted: `${toSym}${converted.toFixed(2)}` });
      } else setError('Exchange rate not available for this pair');
    }
    setConverting(false);
  };

  const handleSwitchCurrency = async (code) => {
    if (!window.confirm(`Switch wallet to ${code}? Your balance will be converted.`)) return;
    const data = await apiPost('/api/Currency/switch-wallet-currency', { currencyCode: code });
    if (data.success) { setSuccess(`Wallet currency switched to ${code}!`); setWalletCurrency(code); }
    else setError(data.message || 'Failed to switch currency');
  };

  return (
    <div className="page-container">
      <div className="page-header">
        <div>
          <h1 className="page-title">Multi-Currency</h1>
          <p className="page-subtitle">Convert currencies and manage your wallet currency</p>
        </div>
      </div>

      {error && <div className="alert alert-error">❌ {error}</div>}
      {success && <div className="alert alert-success">✅ {success}</div>}

      <div className="currency-grid">
        {/* Converter */}
        <div className="card">
          <div className="card-header"><h3 className="card-title">💱 Currency Converter</h3></div>
          <div className="card-content">
            <form onSubmit={handleConvert} className="converter-form">
              <div className="converter-row">
                <div className="form-group" style={{margin:0}}>
                  <label>From</label>
                  <select value={fromCur} onChange={e => setFromCur(e.target.value)}>
                    {currencies.map(c => <option key={c.code} value={c.code}>{c.symbol} {c.code} – {c.name}</option>)}
                  </select>
                </div>
                <div className="converter-swap">
                  <button type="button" className="btn btn-ghost btn-sm" onClick={() => { const t=fromCur; setFromCur(toCur); setToCur(t); setResult(null); }}>⇄</button>
                </div>
                <div className="form-group" style={{margin:0}}>
                  <label>To</label>
                  <select value={toCur} onChange={e => setToCur(e.target.value)}>
                    {currencies.map(c => <option key={c.code} value={c.code}>{c.symbol} {c.code} – {c.name}</option>)}
                  </select>
                </div>
              </div>
              <div className="form-group">
                <label>Amount</label>
                <input type="number" step="0.01" min="0.01" value={amount} onChange={e => setAmount(e.target.value)} placeholder="Enter amount" required />
              </div>
              <button type="submit" className="btn btn-primary btn-full" disabled={converting}>{converting ? 'Converting…' : 'Convert'}</button>
            </form>

            {result && (
              <div className="conversion-result">
                <h4>Conversion Result</h4>
                <div className="result-amount">{result.formattedOriginal} = <strong>{result.formattedConverted}</strong></div>
                <div className="result-rate">1 {result.fromCurrency} = {result.exchangeRate} {result.toCurrency}</div>
              </div>
            )}
          </div>
        </div>

        {/* Exchange Rates */}
        <div className="card">
          <div className="card-header"><h3 className="card-title">📈 Live Exchange Rates</h3></div>
          <div className="card-content">
            <div className="exchange-rates-grid">
              {rates.slice(0, 8).map(r => (
                <div key={`${r.fromCurrency}${r.toCurrency}`} className="exchange-rate-item">
                  <div className="currency-pair">{r.fromCurrency}/{r.toCurrency}</div>
                  <div className="rate-value">{typeof r.rate === 'number' ? r.rate.toFixed(4) : r.rate}</div>
                  <div className="rate-updated">Live Rate</div>
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* Supported Currencies */}
        <div className="card">
          <div className="card-header"><h3 className="card-title">🌍 Supported Currencies</h3></div>
          <div className="card-content">
            <div className="currencies-grid">
              {currencies.map(c => (
                <div key={c.id || c.code} className={`currency-item${walletCurrency === c.code ? ' active' : ''}`}>
                  <div className="currency-info">
                    <div className="currency-symbol">{c.symbol}</div>
                    <div className="currency-details">
                      <div className="currency-code">{c.code}</div>
                      <div className="currency-name">{c.name}</div>
                    </div>
                  </div>
                  {walletCurrency === c.code
                    ? <span className="wallet-badge">Current</span>
                    : <button className="btn btn-sm btn-outline" onClick={() => handleSwitchCurrency(c.code)}>Switch</button>}
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* Tips */}
        <div className="card">
          <div className="card-header"><h3 className="card-title">💡 Tips</h3></div>
          <div className="card-content">
            <div className="tips-list">
              {[['🔄','Real-time Rates','Rates are updated regularly for accurate conversions.'],['💰','Wallet Currency','Store balance in any currency, switch anytime.'],['🏦','Multi-Currency','Send & receive in different currencies with auto-conversion.'],['📊','Rate History','Track trends and plan international transactions.']].map(([icon,title,desc]) => (
                <div key={title} className="tip-item" style={{display:'flex',gap:'0.75rem',alignItems:'flex-start'}}>
                  <span style={{fontSize:'1.25rem'}}>{icon}</span>
                  <div><strong style={{color:'var(--text-1)'}}>{title}:</strong><span style={{color:'var(--text-2)',marginLeft:'0.35rem'}}>{desc}</span></div>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Currency;
