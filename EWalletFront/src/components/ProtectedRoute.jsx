import React, { useContext, useEffect } from 'react';
import { Navigate } from 'react-router-dom';
import { AuthContext } from '../context/AuthContext';

// Decode JWT payload without a library
const getTokenExpiry = (token) => {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    return payload.exp ? payload.exp * 1000 : null;
  } catch {
    return null;
  }
};

const ProtectedRoute = ({ children }) => {
  const { isAuthenticated, logout } = useContext(AuthContext);
  const token = localStorage.getItem('token');

  useEffect(() => {
    if (token) {
      const expiry = getTokenExpiry(token);
      if (expiry && Date.now() > expiry) {
        logout();
      }
    }
  }, [token, logout]);

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return children;
};

export default ProtectedRoute;
