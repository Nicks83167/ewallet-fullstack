import React, { useContext } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, AuthContext } from './context/AuthContext';
import ProtectedRoute from './components/ProtectedRoute';
import Sidebar from './components/Sidebar';
import ErrorBoundary from './components/ErrorBoundary';

import Login from './pages/Login';
import Register from './pages/Register';
import Dashboard from './pages/Dashboard';
import AddMoney from './pages/AddMoney';
import Withdraw from './pages/Withdraw';
import Transfer from './pages/Transfer';
import Transactions from './pages/Transactions';
import Profile from './pages/Profile';

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

function App() {
  return (
    <ErrorBoundary>
      <AuthProvider>
        <Router>
          <AppShell>
            <Routes>
              <Route path="/" element={<Navigate to="/dashboard" replace />} />
              <Route path="/login" element={<Login />} />
              <Route path="/register" element={<Register />} />

              <Route path="/dashboard" element={
                <ProtectedRoute><Dashboard /></ProtectedRoute>
              } />
              <Route path="/add-money" element={
                <ProtectedRoute><AddMoney /></ProtectedRoute>
              } />
              <Route path="/withdraw" element={
                <ProtectedRoute><Withdraw /></ProtectedRoute>
              } />
              <Route path="/transfer" element={
                <ProtectedRoute><Transfer /></ProtectedRoute>
              } />
              <Route path="/transactions" element={
                <ProtectedRoute><Transactions /></ProtectedRoute>
              } />
              <Route path="/profile" element={
                <ProtectedRoute><Profile /></ProtectedRoute>
              } />
            </Routes>
          </AppShell>
        </Router>
      </AuthProvider>
    </ErrorBoundary>
  );
}

export default App;
