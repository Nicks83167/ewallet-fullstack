import React, { useContext } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, AuthContext } from './context/AuthContext';
import ProtectedRoute from './components/ProtectedRoute';
import Sidebar from './components/Sidebar';
import ErrorBoundary from './components/ErrorBoundary';

// Auth
import Login          from './pages/Login';
import Register       from './pages/Register';

// Core
import Dashboard      from './pages/DashboardNew';
import AddMoney       from './pages/AddMoney';
import Withdraw       from './pages/Withdraw';
import Transfer       from './pages/Transfer';
import Transactions   from './pages/Transactions';
import Notifications  from './pages/Notifications';
import Profile        from './pages/Profile';

// Money
import Currency       from './pages/Currency';

// Payments
import QRPayment      from './pages/QRPayment';
import BillPayment    from './pages/BillPayment';
import Recharge       from './pages/Recharge';
import Merchants      from './pages/Merchants';

// Manage
import LinkedAccounts   from './pages/LinkedAccounts';
import Beneficiaries    from './pages/Beneficiaries';
import ScheduledPayments from './pages/ScheduledPayments';
import Reports          from './pages/Reports';

// Account
import Security       from './pages/Security';

const AppShell = ({ children }) => {
  const { isAuthenticated } = useContext(AuthContext);
  if (!isAuthenticated) return children;
  return (
    <div className="app-shell">
      <Sidebar />
      <div className="main-content">
        {children}
      </div>
    </div>
  );
};

const PR = ({ children }) => <ProtectedRoute>{children}</ProtectedRoute>;

function App() {
  return (
    <ErrorBoundary>
      <AuthProvider>
        <Router>
          <AppShell>
            <Routes>
              <Route path="/"                    element={<Navigate to="/dashboard" replace />} />
              <Route path="/login"               element={<Login />} />
              <Route path="/register"            element={<Register />} />

              {/* Core */}
              <Route path="/dashboard"           element={<PR><Dashboard /></PR>} />
              <Route path="/add-money"           element={<PR><AddMoney /></PR>} />
              <Route path="/withdraw"            element={<PR><Withdraw /></PR>} />
              <Route path="/transfer"            element={<PR><Transfer /></PR>} />
              <Route path="/transactions"        element={<PR><Transactions /></PR>} />
              <Route path="/notifications"       element={<PR><Notifications /></PR>} />
              <Route path="/profile"             element={<PR><Profile /></PR>} />

              {/* Money */}
              <Route path="/currency"            element={<PR><Currency /></PR>} />

              {/* Payments */}
              <Route path="/qr-payment"          element={<PR><QRPayment /></PR>} />
              <Route path="/bill-payment"        element={<PR><BillPayment /></PR>} />
              <Route path="/recharge"            element={<PR><Recharge /></PR>} />
              <Route path="/merchants"           element={<PR><Merchants /></PR>} />

              {/* Manage */}
              <Route path="/linked-accounts"     element={<PR><LinkedAccounts /></PR>} />
              <Route path="/beneficiaries"       element={<PR><Beneficiaries /></PR>} />
              <Route path="/scheduled-payments"  element={<PR><ScheduledPayments /></PR>} />
              <Route path="/reports"             element={<PR><Reports /></PR>} />

              {/* Account */}
              <Route path="/security"            element={<PR><Security /></PR>} />

              {/* Catch-all */}
              <Route path="*"                    element={<Navigate to="/dashboard" replace />} />
            </Routes>
          </AppShell>
        </Router>
      </AuthProvider>
    </ErrorBoundary>
  );
}

export default App;
