export const formatCurrency = (amount) =>
  new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR', maximumFractionDigits: 2 }).format(amount ?? 0);

export const formatDate = (dateStr) => {
  const d = new Date(dateStr);
  return d.toLocaleDateString('en-IN', { month: 'short', day: 'numeric', year: 'numeric' });
};

export const formatDateTime = (dateStr) => {
  const d = new Date(dateStr);
  return d.toLocaleDateString('en-IN', { month: 'short', day: 'numeric', year: 'numeric' })
    + ' · '
    + d.toLocaleTimeString('en-IN', { hour: '2-digit', minute: '2-digit' });
};

export const txBadge = (status) => {
  const map = {
    completed: 'badge-success',
    pending:   'badge-warning',
    failed:    'badge-danger',
    reversed:  'badge-default',
  };
  return map[status?.toLowerCase()] ?? 'badge-default';
};

export const txAmountClass = (direction) =>
  direction === 'IN' ? 'amount-credit' : 'amount-debit';
