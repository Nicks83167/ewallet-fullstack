import React, { useContext } from 'react';
import { NavLink, useNavigate } from 'react-router-dom';
import { AuthContext } from '../context/AuthContext';

const NAV_SECTIONS = [
  {
    label: 'Main',
    items: [
      { to: '/dashboard',          icon: '⊞',  label: 'Dashboard'       },
      { to: '/transactions',       icon: '≡',   label: 'Transactions'    },
      { to: '/notifications',      icon: '🔔',  label: 'Notifications'   },
    ]
  },
  {
    label: 'Money',
    items: [
      { to: '/add-money',          icon: '＋',  label: 'Add Money'       },
      { to: '/withdraw',           icon: '↑',   label: 'Withdraw'        },
      { to: '/transfer',           icon: '→',   label: 'Transfer'        },
      { to: '/currency',           icon: '💱',  label: 'Currency'        },
    ]
  },
  {
    label: 'Payments',
    items: [
      { to: '/qr-payment',         icon: '📱',  label: 'QR Payment'      },
      { to: '/bill-payment',       icon: '💡',  label: 'Bill Payment'    },
      { to: '/recharge',           icon: '📶',  label: 'Recharge'        },
      { to: '/merchants',          icon: '🏪',  label: 'Merchants'       },
    ]
  },
  {
    label: 'Manage',
    items: [
      { to: '/linked-accounts',    icon: '🏦',  label: 'Linked Accounts' },
      { to: '/beneficiaries',      icon: '👥',  label: 'Beneficiaries'   },
      { to: '/scheduled-payments', icon: '⏰',  label: 'Scheduled'       },
      { to: '/reports',            icon: '📊',  label: 'Reports'         },
    ]
  },
  {
    label: 'Account',
    items: [
      { to: '/profile',            icon: '👤',  label: 'Profile'         },
      { to: '/security',           icon: '🔒',  label: 'Security'        },
    ]
  },
];

const Sidebar = () => {
  const { user, logout } = useContext(AuthContext);
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const initials = user?.fullName
    ? user.fullName.split(' ').map(n => n[0]).slice(0, 2).join('').toUpperCase()
    : user?.email?.[0]?.toUpperCase() ?? '?';

  return (
    <aside className="sidebar">
      {/* Logo */}
      <div className="sidebar-logo">
        <span className="sidebar-logo-text">
          <span className="sidebar-logo-icon">💳</span>
          EWallet
        </span>
      </div>

      {/* Navigation — scrollable */}
      <nav className="sidebar-nav">
        {NAV_SECTIONS.map((section) => (
          <div key={section.label} className="nav-section">
            <span className="sidebar-section-label">{section.label}</span>
            {section.items.map(({ to, icon, label }) => (
              <NavLink
                key={to}
                to={to}
                className={({ isActive }) => `nav-item${isActive ? ' active' : ''}`}
              >
                <span className="nav-item-icon">{icon}</span>
                {label}
              </NavLink>
            ))}
          </div>
        ))}
      </nav>

      {/* Footer / User */}
      <div className="sidebar-footer">
        <div className="sidebar-user">
          <div className="avatar">{initials}</div>
          <div className="sidebar-user-info">
            <div className="sidebar-user-name">{user?.fullName || 'User'}</div>
            <div className="sidebar-user-email">{user?.email}</div>
          </div>
        </div>
        <button
          onClick={handleLogout}
          className="nav-item"
          style={{ marginTop: '4px', color: 'var(--danger)', width: '100%' }}
        >
          <span className="nav-item-icon">⏻</span>
          Log Out
        </button>
      </div>
    </aside>
  );
};

export default Sidebar;
