import React, { useState, useEffect } from 'react';

const BillPayment = () => {
  const [activeCategory, setActiveCategory] = useState('Electricity');
  const [billForm, setBillForm] = useState({
    category: 'Electricity',
    provider: '',
    consumerNumber: '',
    amount: ''
  });
  const [billHistory, setBillHistory] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  const billCategories = [
    { id: 'Electricity', name: 'Electricity', icon: '💡', providers: ['MSEB', 'BSES', 'TSNPDCL', 'KSEB', 'PSPCL'] },
    { id: 'Water', name: 'Water', icon: '💧', providers: ['BMC Water', 'DJB Water', 'MCGM', 'GHMC Water'] },
    { id: 'Gas', name: 'Gas', icon: '🔥', providers: ['Indane Gas', 'Bharat Gas', 'HP Gas', 'Reliance Gas'] },
    { id: 'Broadband', name: 'Broadband', icon: '📶', providers: ['Airtel Fiber', 'Jio Fiber', 'BSNL', 'Hathway', 'ACT'] },
    { id: 'DTH', name: 'DTH/Cable', icon: '📺', providers: ['Tata Sky', 'Airtel Digital TV', 'Dish TV', 'Sun Direct'] },
    { id: 'Insurance', name: 'Insurance', icon: '🛡️', providers: ['LIC', 'HDFC Life', 'ICICI Prudential', 'SBI Life'] },
    { id: 'FASTag', name: 'FASTag', icon: '🛣️', providers: ['ICICI FASTag', 'HDFC FASTag', 'Paytm FASTag', 'Airtel FASTag'] }
  ];

  useEffect(() => {
    fetchBillHistory();
  }, []);

  useEffect(() => {
    setBillForm({...billForm, category: activeCategory, provider: ''});
  }, [activeCategory]);

  const fetchBillHistory = async () => {
    try {
      const token = localStorage.getItem('authToken');
      const response = await fetch('/api/BillPayment?page=1&pageSize=20', {
        headers: { Authorization: `Bearer ${token}` }
      });
      const data = await response.json();
      
      if (data.success) {
        setBillHistory(data.data.items);
      }
    } catch (err) {
      console.error('Failed to fetch bill history');
    }
  };

  const handlePayBill = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError('');
    setSuccess('');

    try {
      const token = localStorage.getItem('authToken');
      const response = await fetch('/api/BillPayment/pay', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`
        },
        body: JSON.stringify({
          category: billForm.category,
          provider: billForm.provider,
          consumerNumber: billForm.consumerNumber,
          amount: parseFloat(billForm.amount)
        })
      });
      
      const data = await response.json();
      
      if (data.success) {
        if (data.data.status === 'Success') {
          setSuccess(`Bill payment of ₹${billForm.amount} completed successfully!`);
        } else {
          setError(`Bill payment failed. Please try again.`);
        }
        setBillForm({
          category: activeCategory,
          provider: '',
          consumerNumber: '',
          amount: ''
        });
        fetchBillHistory();
      } else {
        setError(data.message);
      }
    } catch (err) {
      setError('Failed to process bill payment');
    } finally {
      setLoading(false);
    }
  };

  const currentCategory = billCategories.find(cat => cat.id === activeCategory);

  return (
    <div className="page-container">
      <div className="page-header">
        <h1 className="page-title">Bill Payments</h1>
        <p className="page-subtitle">Pay utility bills instantly from your wallet</p>
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

      <div className="bill-payment-layout">
        {/* Categories Sidebar */}
        <div className="categories-sidebar">
          <h3>Bill Categories</h3>
          <div className="categories-list">
            {billCategories.map((category) => (
              <button
                key={category.id}
                className={`category-item ${activeCategory === category.id ? 'active' : ''}`}
                onClick={() => setActiveCategory(category.id)}
              >
                <span className="category-icon">{category.icon}</span>
                <span className="category-name">{category.name}</span>
              </button>
            ))}
          </div>
        </div>

        {/* Main Content */}
        <div className="bill-content">
          <div className="card">
            <div className="card-header">
              <h3 className="card-title">
                {currentCategory?.icon} Pay {currentCategory?.name} Bill
              </h3>
            </div>
            <form onSubmit={handlePayBill} className="card-content">
              <div className="form-grid">
                <div className="form-group">
                  <label>Service Provider *</label>
                  <select
                    value={billForm.provider}
                    onChange={(e) => setBillForm({...billForm, provider: e.target.value})}
                    required
                  >
                    <option value="">Select Provider</option>
                    {currentCategory?.providers.map((provider) => (
                      <option key={provider} value={provider}>
                        {provider}
                      </option>
                    ))}
                  </select>
                </div>
                <div className="form-group">
                  <label>Consumer/Account Number *</label>
                  <input
                    type="text"
                    value={billForm.consumerNumber}
                    onChange={(e) => setBillForm({...billForm, consumerNumber: e.target.value})}
                    placeholder="Enter your consumer number"
                    required
                  />
                </div>
                <div className="form-group">
                  <label>Amount (₹) *</label>
                  <input
                    type="number"
                    step="0.01"
                    min="1"
                    value={billForm.amount}
                    onChange={(e) => setBillForm({...billForm, amount: e.target.value})}
                    placeholder="Enter bill amount"
                    required
                  />
                </div>
              </div>
              <div className="form-actions">
                <button type="submit" className="btn btn-primary" disabled={loading}>
                  {loading ? 'Processing...' : `Pay ₹${billForm.amount || '0'}`}
                </button>
              </div>
            </form>
          </div>

          {/* Quick Bill Info */}
          <div className="card">
            <div className="card-header">
              <h3 className="card-title">💡 Bill Payment Tips</h3>
            </div>
            <div className="card-content">
              <div className="tips-grid">
                <div className="tip-item">
                  <strong>Consumer Number:</strong> Find this on your bill statement or previous payment receipts.
                </div>
                <div className="tip-item">
                  <strong>Amount Verification:</strong> Double-check the amount before confirming payment.
                </div>
                <div className="tip-item">
                  <strong>Payment Time:</strong> Bills are usually processed instantly, but may take up to 24 hours to reflect.
                </div>
                <div className="tip-item">
                  <strong>Receipt:</strong> Keep the transaction reference for your records.
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Recent Bill Payments */}
      <div className="card mt-6">
        <div className="card-header">
          <h3 className="card-title">Recent Bill Payments</h3>
        </div>
        <div className="card-content">
          {billHistory.length === 0 ? (
            <div className="empty-state">
              <div className="empty-state-icon">💡</div>
              <h3>No Bill Payments Yet</h3>
              <p>Your bill payment history will appear here</p>
            </div>
          ) : (
            <div className="history-table">
              <div className="table-header">
                <div>Bill Type</div>
                <div>Provider</div>
                <div>Consumer No.</div>
                <div>Amount</div>
                <div>Status</div>
                <div>Date</div>
              </div>
              {billHistory.map((bill) => (
                <div key={bill.id} className="table-row">
                  <div className="bill-category">
                    {billCategories.find(c => c.id === bill.category)?.icon} {bill.category}
                  </div>
                  <div>{bill.provider}</div>
                  <div className="consumer-number">
                    ****{bill.consumerNumber.slice(-4)}
                  </div>
                  <div className="amount">₹{bill.amount}</div>
                  <div>
                    <span className={`status ${bill.status.toLowerCase()}`}>
                      {bill.status === 'Success' ? '✅' : 
                       bill.status === 'Failed' ? '❌' : 
                       bill.status === 'Pending' ? '⏳' : '❓'} 
                      {bill.status}
                    </span>
                  </div>
                  <div className="date">
                    {new Date(bill.createdAt).toLocaleDateString()}
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default BillPayment;