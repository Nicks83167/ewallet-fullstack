import React, { useContext } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { AuthContext } from '../context/AuthContext';

const Navbar = () => {
  const { isAuthenticated, logout, user } = useContext(AuthContext);
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  if (!isAuthenticated) return null;

  return (
    <nav className="navbar">
      <Link to="/dashboard" className="nav-brand">E-Wallet</Link>
      <div className="nav-links">
        <Link to="/dashboard" className="nav-link">Dashboard</Link>
        <Link to="/add-money" className="nav-link">Add Money</Link>
        <Link to="/transfer" className="nav-link">Transfer</Link>
        <Link to="/transactions" className="nav-link">Transactions</Link>
        <span style={{ color: 'var(--text-secondary)', marginLeft: '1rem' }}>
          {user?.fullName || user?.email}
        </span>
        <button onClick={handleLogout} className="btn btn-secondary" style={{ padding: '0.4rem 1rem' }}>
          Logout
        </button>
      </div>
    </nav>
  );
};

export default Navbar;