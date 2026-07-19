import React, { useState, useEffect } from 'react';

const QRPayment = () => {
  const [activeTab, setActiveTab] = useState('generate');
  const [generateForm, setGenerateForm] = useState({
    amount: '',
    description: ''
  });
  const [scanForm, setScanForm] = useState({
    qrCode: '',
    amount: ''
  });
  const [qrHistory, setQrHistory] = useState([]);
  const [generatedQR, setGeneratedQR] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  useEffect(() => {
    if (activeTab === 'history') {
      fetchQRHistory();
    }
  }, [activeTab]);

  const fetchQRHistory = async () => {
    try {
      const token = localStorage.getItem('authToken');
      const response = await fetch('/api/QRPayment/history?page=1&pageSize=20', {
        headers: { Authorization: `Bearer ${token}` }
      });
      const data = await response.json();
      
      if (data.success) {
        setQrHistory(data.data.items);
      } else {
        setError(data.message);
      }
    } catch (err) {
      setError('Failed to fetch QR history');
    }
  };

  const handleGenerateQR = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError('');
    setSuccess('');

    try {
      const token = localStorage.getItem('authToken');
      const response = await fetch('/api/QRPayment/generate', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`
        },
        body: JSON.stringify({
          amount: parseFloat(generateForm.amount),
          description: generateForm.description
        })
      });
      
      const data = await response.json();
      
      if (data.success) {
        setGeneratedQR(data.data);
        setSuccess('QR code generated successfully!');
        setGenerateForm({ amount: '', description: '' });
      } else {
        setError(data.message);
      }
    } catch (err) {
      setError('Failed to generate QR code');
    } finally {
      setLoading(false);
    }
  };

  const handleScanAndPay = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError('');
    setSuccess('');

    try {
      const token = localStorage.getItem('authToken');
      const response = await fetch('/api/QRPayment/scan-and-pay', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`
        },
        body: JSON.stringify({
          qrCode: scanForm.qrCode,
          amount: parseFloat(scanForm.amount)
        })
      });
      
      const data = await response.json();
      
      if (data.success) {
        setSuccess('Payment completed successfully!');
        setScanForm({ qrCode: '', amount: '' });
        if (activeTab === 'history') {
          fetchQRHistory();
        }
      } else {
        setError(data.message);
      }
    } catch (err) {
      setError('Failed to process payment');
    } finally {
      setLoading(false);
    }
  };

  const copyQRCode = () => {
    if (generatedQR) {
      navigator.clipboard.writeText(generatedQR.qrCode);
      setSuccess('QR code copied to clipboard!');
    }
  };

  const shareQRCode = () => {
    if (generatedQR && navigator.share) {
      navigator.share({
        title: 'Payment Request',
        text: `Pay ₹${generatedQR.amount} - ${generatedQR.description}`,
        url: `data:text/plain,${generatedQR.qrCode}`
      });
    }
  };

  return (
    <div className="page-container">
      <div className="page-header">
        <h1 className="page-title">QR Payments</h1>
        <p className="page-subtitle">Generate QR codes to receive payments or scan to pay</p>
      </div>

      {error && (
        <div className="alert alert-error">
          <span>❌</span>
          <span>{error}</span>
        </div>
      )}

      {success && (
        <div className="alert alert-success">
          <span>✅</span>
          <span>{success}</span>
        </div>
      )}

      <div className="tabs-container">
        <div className="tabs-header">
          <button 
            className={`tab ${activeTab === 'generate' ? 'active' : ''}`}
            onClick={() => setActiveTab('generate')}
          >
            📱 Generate QR
          </button>
          <button 
            className={`tab ${activeTab === 'scan' ? 'active' : ''}`}
            onClick={() => setActiveTab('scan')}
          >
            📷 Scan & Pay
          </button>
          <button 
            className={`tab ${activeTab === 'history' ? 'active' : ''}`}
            onClick={() => setActiveTab('history')}
          >
            📋 History
          </button>
        </div>

        <div className="tabs-content">
          {activeTab === 'generate' && (
            <div className="tab-content">
              <div className="qr-generate-grid">
                <div className="card">
                  <div className="card-header">
                    <h3 className="card-title">Create Payment Request</h3>
                  </div>
                  <form onSubmit={handleGenerateQR} className="card-content">
                    <div className="form-group">
                      <label>Amount (₹) *</label>
                      <input
                        type="number"
                        step="0.01"
                        min="1"
                        value={generateForm.amount}
                        onChange={(e) => setGenerateForm({...generateForm, amount: e.target.value})}
                        placeholder="Enter amount to request"
                        required
                      />
                    </div>
                    <div className="form-group">
                      <label>Description</label>
                      <input
                        type="text"
                        value={generateForm.description}
                        onChange={(e) => setGenerateForm({...generateForm, description: e.target.value})}
                        placeholder="e.g., Dinner payment, Shopping"
                      />
                    </div>
                    <button type="submit" className="btn btn-primary btn-full" disabled={loading}>
                      {loading ? 'Generating...' : 'Generate QR Code'}
                    </button>
                  </form>
                </div>

                {generatedQR && (
                  <div className="card">
                    <div className="card-header">
                      <h3 className="card-title">Your QR Code</h3>
                    </div>
                    <div className="card-content text-center">
                      <div className="qr-code-display">
                        <div className="qr-code-placeholder">
                          <div className="qr-pattern">
                            <div></div><div></div><div></div>
                            <div></div><div></div><div></div>
                            <div></div><div></div><div></div>
                          </div>
                          <div className="qr-code-text">QR CODE</div>
                        </div>
                      </div>
                      <div className="qr-details">
                        <h4>₹{generatedQR.amount}</h4>
                        <p>{generatedQR.description}</p>
                        <p className="qr-merchant">To: {generatedQR.merchantName}</p>
                      </div>
                      <div className="qr-actions">
                        <button className="btn btn-outline" onClick={copyQRCode}>
                          📋 Copy Code
                        </button>
                        {navigator.share && (
                          <button className="btn btn-outline" onClick={shareQRCode}>
                            📤 Share
                          </button>
                        )}
                      </div>
                      <div className="qr-code-data">
                        <small>QR Data: {generatedQR.qrCode.substring(0, 50)}...</small>
                      </div>
                    </div>
                  </div>
                )}
              </div>
            </div>
          )}

          {activeTab === 'scan' && (
            <div className="tab-content">
              <div className="card max-w-md mx-auto">
                <div className="card-header">
                  <h3 className="card-title">Scan QR Code to Pay</h3>
                </div>
                <form onSubmit={handleScanAndPay} className="card-content">
                  <div className="form-group">
                    <label>QR Code Data *</label>
                    <textarea
                      value={scanForm.qrCode}
                      onChange={(e) => setScanForm({...scanForm, qrCode: e.target.value})}
                      placeholder="Paste QR code data here or use camera to scan"
                      rows={4}
                      required
                    />
                  </div>
                  <div className="form-group">
                    <label>Amount (₹) *</label>
                    <input
                      type="number"
                      step="0.01"
                      min="1"
                      value={scanForm.amount}
                      onChange={(e) => setScanForm({...scanForm, amount: e.target.value})}
                      placeholder="Enter payment amount"
                      required
                    />
                  </div>
                  <button type="submit" className="btn btn-primary btn-full" disabled={loading}>
                    {loading ? 'Processing...' : 'Scan & Pay'}
                  </button>
                </form>

                <div className="card-content">
                  <div className="camera-placeholder">
                    <div className="camera-icon">📷</div>
                    <p>Camera scanner coming soon</p>
                    <small>Currently paste QR code data manually</small>
                  </div>
                </div>
              </div>
            </div>
          )}

          {activeTab === 'history' && (
            <div className="tab-content">
              <div className="qr-history">
                {qrHistory.length === 0 ? (
                  <div className="empty-state">
                    <div className="empty-state-icon">📱</div>
                    <h3>No QR Payments Yet</h3>
                    <p>Your QR payment history will appear here</p>
                  </div>
                ) : (
                  <div className="history-grid">
                    {qrHistory.map((payment) => (
                      <div key={payment.id} className="history-card">
                        <div className="payment-header">
                          <div className="payment-type">
                            <span className={`type-badge ${payment.paymentType.toLowerCase()}`}>
                              {payment.paymentType === 'Pay' ? '📤' : '📥'} {payment.paymentType}
                            </span>
                          </div>
                          <div className="payment-amount">
                            ₹{payment.amount}
                          </div>
                        </div>
                        <div className="payment-details">
                          <div className="payment-description">
                            {payment.description}
                          </div>
                          {payment.merchantName && payment.merchantName !== 'QR Payment' && (
                            <div className="payment-merchant">
                              {payment.paymentType === 'Pay' ? 'To' : 'From'}: {payment.merchantName}
                            </div>
                          )}
                          <div className="payment-date">
                            {new Date(payment.createdAt).toLocaleString()}
                          </div>
                        </div>
                        <div className="payment-status">
                          <span className={`status ${payment.status.toLowerCase()}`}>
                            {payment.status === 'Success' ? '✅' : 
                             payment.status === 'Failed' ? '❌' : 
                             payment.status === 'Pending' ? '⏳' : '❓'} 
                            {payment.status}
                          </span>
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </div>
          )}
        </div>
      </div>

      <div className="qr-info-section">
        <div className="card">
          <div className="card-header">
            <h3 className="card-title">How QR Payments Work</h3>
          </div>
          <div className="card-content">
            <div className="info-grid">
              <div className="info-item">
                <div className="info-icon">📱</div>
                <div className="info-content">
                  <h4>Generate QR</h4>
                  <p>Create a QR code with the amount you want to receive. Share it with the payer.</p>
                </div>
              </div>
              <div className="info-item">
                <div className="info-icon">📷</div>
                <div className="info-content">
                  <h4>Scan & Pay</h4>
                  <p>Scan someone's QR code or paste the QR data to make instant payments.</p>
                </div>
              </div>
              <div className="info-item">
                <div className="info-icon">⚡</div>
                <div className="info-content">
                  <h4>Instant Transfer</h4>
                  <p>Money is transferred instantly from wallet to wallet with notifications.</p>
                </div>
              </div>
              <div className="info-item">
                <div className="info-icon">🔒</div>
                <div className="info-content">
                  <h4>Secure</h4>
                  <p>All QR payments are encrypted and validated for security.</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default QRPayment;