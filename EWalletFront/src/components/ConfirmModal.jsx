import React from 'react';

const ConfirmModal = ({ title, children, onConfirm, onCancel, confirmLabel = 'Confirm', confirmClass = 'btn' }) => (
  <div className="modal-overlay" onClick={onCancel}>
    <div className="modal" onClick={e => e.stopPropagation()}>
      <h3 className="modal-title">{title}</h3>
      <div className="modal-body">{children}</div>
      <div className="modal-actions">
        <button className="btn btn-secondary" onClick={onCancel}>Cancel</button>
        <button className={confirmClass} onClick={onConfirm}>{confirmLabel}</button>
      </div>
    </div>
  </div>
);

export default ConfirmModal;
