import React from 'react';

class ErrorBoundary extends React.Component {
  constructor(props) {
    super(props);
    this.state = { hasError: false, error: null };
  }

  static getDerivedStateFromError(error) {
    return { hasError: true, error };
  }

  componentDidCatch(error, info) {
    console.error('ErrorBoundary caught:', error, info);
  }

  render() {
    if (this.state.hasError) {
      return (
        <div style={{
          minHeight: '100vh',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          flexDirection: 'column',
          gap: '1rem',
          padding: '2rem',
          background: 'var(--bg)',
          color: 'var(--text-2)'
        }}>
          <div style={{ fontSize: '2.5rem' }}>⚠️</div>
          <h2 style={{ color: 'var(--text-1)', fontSize: '1.25rem' }}>Something went wrong</h2>
          <p style={{ fontSize: '0.875rem', color: 'var(--text-3)', textAlign: 'center', maxWidth: '400px' }}>
            An unexpected error occurred. Please refresh the page to continue.
          </p>
          <button
            className="btn btn-secondary"
            onClick={() => { this.setState({ hasError: false }); window.location.reload(); }}
          >
            Reload Page
          </button>
        </div>
      );
    }
    return this.props.children;
  }
}

export default ErrorBoundary;
