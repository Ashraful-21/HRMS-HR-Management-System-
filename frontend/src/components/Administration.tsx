import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { 
  Shield, 
  Users, 
  Megaphone, 
  Plus, 
  Check, 
  AlertCircle, 
  ShieldCheck, 
  Sliders,
  Trash2,
  Pin,
  Search,
  ChevronLeft,
  ChevronRight,
  X,
  RotateCcw,
  Loader2
} from 'lucide-react';

interface AdminProps {
  token: string;
}

const API_BASE = 'http://localhost:5219/api/v1';

export default function Administration({ token }: AdminProps) {
  const [tab, setTab] = useState<'users' | 'roles' | 'settings' | 'announcements'>('users');
  
  const [users, setUsers] = useState<any[]>([]);
  const [roles, setRoles] = useState<any[]>([]);
  const [settings, setSettings] = useState<any[]>([]);
  const [announcements, setAnnouncements] = useState<any[]>([]);
  const [employees, setEmployees] = useState<any[]>([]);
  
  const [error, setError] = useState<string | null>(null);
  const [successMsg, setSuccessMsg] = useState<string | null>(null);

  // Server-side Dynamic Search & Pagination for Users
  const [userSearch, setUserSearch] = useState('');
  const [activeUserSearch, setActiveUserSearch] = useState('');
  const [userPage, setUserPage] = useState(1);
  const [userPageSize, setUserPageSize] = useState(5);
  const [userTotalCount, setUserTotalCount] = useState(0);
  const [userTotalPages, setUserTotalPages] = useState(1);
  const [loadingUsers, setLoadingUsers] = useState(false);

  // Modals
  const [showUserModal, setShowUserModal] = useState(false);
  const [newUser, setNewUser] = useState({
    username: '',
    email: '',
    password: '',
    employeeId: null as number | null,
    roles: ['Employee']
  });

  const [showResetModal, setShowResetModal] = useState(false);
  const [resetTargetUser, setResetTargetUser] = useState<any>(null);
  const [newPassword, setNewPassword] = useState('');

  const [showSettingModal, setShowSettingModal] = useState(false);
  const [targetSetting, setTargetSetting] = useState({ key: '', value: '' });

  const [showAnnModal, setShowAnnModal] = useState(false);
  const [newAnn, setNewAnn] = useState({
    title: '',
    content: '',
    isPinned: false
  });

  const headers = { Authorization: `Bearer ${token}` };

  const fetchUsers = async (page = userPage, size = userPageSize, search = userSearch) => {
    setLoadingUsers(true);
    setError(null);
    try {
      const params = new URLSearchParams({
        page: page.toString(),
        pageSize: size.toString()
      });
      if (search && search.trim()) {
        params.append('search', search.trim());
      }
      const res = await axios.get(`${API_BASE}/users?${params.toString()}`, { headers });
      if (res.data.success && res.data.data) {
        setUsers(res.data.data.items || []);
        const total = res.data.data.totalCount || 0;
        setUserTotalCount(total);
        setUserTotalPages(Math.max(1, Math.ceil(total / size)));
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error loading users.');
    } finally {
      setLoadingUsers(false);
    }
  };

  // Real-time Dynamic Live Search: updates table automatically as user types
  useEffect(() => {
    if (tab !== 'users') return;
    const timer = setTimeout(() => {
      setUserPage(1);
      setActiveUserSearch(userSearch);
      fetchUsers(1, userPageSize, userSearch);
    }, 200);

    return () => clearTimeout(timer);
  }, [userSearch, userPageSize, tab]);

  const loadData = async () => {
    setError(null);
    try {
      if (tab === 'users') {
        const eRes = await axios.get(`${API_BASE}/employees?pageSize=100`, { headers });
        setEmployees(eRes.data.data?.items || []);
      } else if (tab === 'roles') {
        const rRes = await axios.get(`${API_BASE}/roles`, { headers });
        setRoles(rRes.data.data || []);
      } else if (tab === 'settings') {
        const res = await axios.get(`${API_BASE}/settings`, { headers });
        setSettings(res.data.data || []);
      } else if (tab === 'announcements') {
        const res = await axios.get(`${API_BASE}/announcements?includeUnpublished=true`, { headers });
        setAnnouncements(res.data.data || []);
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error loading administration data.');
    }
  };

  useEffect(() => {
    loadData();
  }, [tab, token]);

  const handleUserSearchSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setUserPage(1);
    setActiveUserSearch(userSearch);
    fetchUsers(1, userPageSize, userSearch);
  };

  const handleClearUserSearch = () => {
    setUserSearch('');
    setActiveUserSearch('');
    setUserPage(1);
  };

  const handlePageChange = (newPage: number) => {
    if (newPage < 1 || newPage > userTotalPages || newPage === userPage) return;
    setUserPage(newPage);
    fetchUsers(newPage, userPageSize, userSearch);
  };

  const handlePageSizeChange = (newSize: number) => {
    setUserPageSize(newSize);
    setUserPage(1);
  };

  const handleCreateUser = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const res = await axios.post(`${API_BASE}/users`, newUser, { headers });
      if (res.data.success) {
        setSuccessMsg('User account created successfully.');
        setShowUserModal(false);
        setNewUser({ username: '', email: '', password: '', employeeId: null, roles: ['Employee'] });
        fetchUsers(1, userPageSize, activeUserSearch);
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to create user.');
    }
  };

  const handleToggleStatus = async (userId: number) => {
    try {
      const res = await axios.post(`${API_BASE}/users/${userId}/toggle-status`, {}, { headers });
      if (res.data.success) {
        setSuccessMsg(res.data.message);
        fetchUsers(userPage, userPageSize, activeUserSearch);
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to toggle status.');
    }
  };

  const handleResetPassword = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!resetTargetUser) return;
    try {
      const res = await axios.post(
        `${API_BASE}/users/${resetTargetUser.id}/reset-password`,
        { newPassword },
        { headers }
      );
      if (res.data.success) {
        setSuccessMsg('User password reset successfully.');
        setShowResetModal(false);
        setNewPassword('');
        fetchUsers(userPage, userPageSize, activeUserSearch);
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to reset password.');
    }
  };

  const handleUpdateSetting = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const res = await axios.put(`${API_BASE}/settings`, targetSetting, { headers });
      if (res.data.success) {
        setSuccessMsg('Setting updated successfully.');
        setShowSettingModal(false);
        loadData();
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to update setting.');
    }
  };

  const handleCreateAnnouncement = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const res = await axios.post(`${API_BASE}/announcements`, newAnn, { headers });
      if (res.data.success) {
        setSuccessMsg('Company announcement broadcasted.');
        setShowAnnModal(false);
        loadData();
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to post announcement.');
    }
  };

  const handleDeleteAnnouncement = async (annId: number) => {
    if (!window.confirm('Delete this announcement?')) return;
    try {
      const res = await axios.delete(`${API_BASE}/announcements/${annId}`, { headers });
      if (res.data.success) {
        setSuccessMsg('Announcement removed.');
        loadData();
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to delete announcement.');
    }
  };

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '1.25rem' }}>
      
      {/* Title */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: '1rem' }}>
        <div>
          <h2 style={{ fontSize: '1.4rem', fontWeight: 800, margin: 0, display: 'flex', alignItems: 'center', gap: '8px' }}>
            <Shield size={22} color="#818cf8" />
            Administration &amp; System Governance
          </h2>
          <p style={{ margin: '0.25rem 0 0', color: 'var(--text-muted)', fontSize: '0.85rem' }}>
            User credentials, role permission control matrix, company parameters, and broadcast messages.
          </p>
        </div>

        <div style={{ display: 'flex', gap: '0.5rem' }}>
          {tab === 'users' && (
            <button type="button" className="btn-primary" onClick={() => setShowUserModal(true)}>
              <Plus size={16} /> Create User Account
            </button>
          )}
          {tab === 'announcements' && (
            <button type="button" className="btn-primary" onClick={() => setShowAnnModal(true)}>
              <Megaphone size={16} /> Broadcast Announcement
            </button>
          )}
        </div>
      </div>

      {/* Tabs */}
      <div className="subnav-tabs">
        <button 
          type="button" 
          className={`subnav-btn ${tab === 'users' ? 'active' : ''}`}
          onClick={() => setTab('users')}
        >
          <Users size={16} /> User Accounts ({userTotalCount})
        </button>
        <button 
          type="button" 
          className={`subnav-btn ${tab === 'roles' ? 'active' : ''}`}
          onClick={() => setTab('roles')}
        >
          <ShieldCheck size={16} /> Roles &amp; Permissions ({roles.length})
        </button>
        <button 
          type="button" 
          className={`subnav-btn ${tab === 'settings' ? 'active' : ''}`}
          onClick={() => setTab('settings')}
        >
          <Sliders size={16} /> System Settings ({settings.length})
        </button>
        <button 
          type="button" 
          className={`subnav-btn ${tab === 'announcements' ? 'active' : ''}`}
          onClick={() => setTab('announcements')}
        >
          <Megaphone size={16} /> Broadcast Board ({announcements.length})
        </button>
      </div>

      {/* Alerts */}
      {error && (
        <div className="alert-error">
          <AlertCircle size={18} />
          <span>{error}</span>
        </div>
      )}
      {successMsg && (
        <div className="alert-success">
          <Check size={18} />
          <span>{successMsg}</span>
        </div>
      )}

      {/* TAB 1: USERS TABLE */}
      {tab === 'users' && (
        <div>
          {/* Server-Side Search and Page Size Filter Bar */}
          <div style={{
            background: 'var(--bg-surface)',
            border: '1px solid var(--border-subtle)',
            borderRadius: 'var(--radius-lg)',
            padding: '0.85rem 1rem',
            marginBottom: '1rem',
            display: 'flex',
            gap: '1rem',
            flexWrap: 'wrap',
            alignItems: 'center',
            justifyContent: 'space-between'
          }}>
            <form onSubmit={handleUserSearchSubmit} style={{ flex: 1, minWidth: '280px', display: 'flex', gap: '0.5rem', alignItems: 'center' }}>
              <div className="input-wrapper" style={{ flex: 1 }}>
                <Search size={18} className="input-icon" />
                <input
                  className="input-field"
                  placeholder="Type to search dynamically (username, email, employee, role, code)..."
                  value={userSearch}
                  onChange={e => setUserSearch(e.target.value)}
                  style={{ paddingRight: userSearch || loadingUsers ? '4.5rem' : '1rem' }}
                />
                <div style={{ position: 'absolute', right: '10px', display: 'flex', alignItems: 'center', gap: '6px' }}>
                  {loadingUsers && (
                    <Loader2 size={16} className="animate-spin" color="#818cf8" style={{ animation: 'spin 1s linear infinite' }} />
                  )}
                  {userSearch && (
                    <button
                      type="button"
                      onClick={handleClearUserSearch}
                      style={{
                        background: 'none',
                        border: 'none',
                        color: 'var(--text-muted)',
                        cursor: 'pointer',
                        padding: 2,
                        display: 'flex',
                        alignItems: 'center'
                      }}
                      title="Clear search"
                    >
                      <X size={16} />
                    </button>
                  )}
                </div>
              </div>
              <button 
                type="submit" 
                className="btn-secondary" 
                disabled={loadingUsers}
                style={{ padding: '0.7rem 1.25rem', display: 'flex', alignItems: 'center', gap: '6px', whiteSpace: 'nowrap' }}
              >
                {loadingUsers ? <Loader2 size={15} style={{ animation: 'spin 1s linear infinite' }} /> : <Search size={15} />} Search
              </button>
              {activeUserSearch && (
                <button 
                  type="button" 
                  onClick={handleClearUserSearch} 
                  className="btn-secondary" 
                  style={{ padding: '0.7rem 1rem', display: 'flex', alignItems: 'center', gap: '4px', whiteSpace: 'nowrap' }} 
                  title="Reset Filter"
                >
                  <RotateCcw size={15} /> Reset
                </button>
              )}
            </form>

            <div style={{ display: 'flex', alignItems: 'center', gap: '0.85rem', flexWrap: 'wrap' }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: '6px', fontSize: '0.85rem', color: 'var(--text-muted)' }}>
                <span>Per Page:</span>
                <select
                  className="input-field"
                  style={{ padding: '0.5rem 0.75rem', width: 'auto', fontSize: '0.85rem' }}
                  value={userPageSize}
                  onChange={e => handlePageSizeChange(Number(e.target.value))}
                >
                  <option value={5}>5 per page</option>
                  <option value={10}>10 per page</option>
                  <option value={20}>20 per page</option>
                  <option value={50}>50 per page</option>
                </select>
              </div>

              {activeUserSearch && (
                <span style={{
                  fontSize: '0.8rem',
                  color: '#a5b4fc',
                  background: 'rgba(99, 102, 241, 0.15)',
                  padding: '0.35rem 0.75rem',
                  borderRadius: 9999,
                  border: '1px solid rgba(99, 102, 241, 0.3)'
                }}>
                  Results for: <strong>"{activeUserSearch}"</strong> ({userTotalCount})
                </span>
              )}
            </div>
          </div>

          {/* Table Container */}
          <div className="table-container" style={{ position: 'relative' }}>
            {loadingUsers && (
              <div style={{
                position: 'absolute',
                top: 0,
                left: 0,
                right: 0,
                bottom: 0,
                background: 'rgba(10, 15, 29, 0.65)',
                backdropFilter: 'blur(2px)',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                zIndex: 5,
                color: '#818cf8',
                fontWeight: 600,
                fontSize: '0.9rem'
              }}>
                Loading user data...
              </div>
            )}
            <table className="data-table">
              <thead>
                <tr>
                  <th>Username</th>
                  <th>Email</th>
                  <th>Employee Link</th>
                  <th>Roles</th>
                  <th>Status</th>
                  <th>Last Login</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {users.length > 0 ? (
                  users.map((u) => (
                    <tr key={u.id}>
                      <td><strong>{u.username}</strong></td>
                      <td>{u.email}</td>
                      <td>
                        {u.employeeName ? (
                          <div>
                            <strong>{u.employeeName}</strong>
                            <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>{u.employeeCode}</div>
                          </div>
                        ) : (
                          <span style={{ color: 'var(--text-muted)', fontSize: '0.8rem' }}>System Admin</span>
                        )}
                      </td>
                      <td>
                        {u.roles.map((r: string) => (
                          <span key={r} className="badge-pill badge-primary" style={{ marginRight: 4 }}>
                            {r}
                          </span>
                        ))}
                      </td>
                      <td>
                        <span className={`badge-pill ${u.isActive ? 'badge-success' : 'badge-danger'}`}>
                          {u.isActive ? 'Active' : 'Locked'}
                        </span>
                      </td>
                      <td>
                        {u.lastLoginAt ? new Date(u.lastLoginAt).toLocaleString() : 'Never logged in'}
                      </td>
                      <td>
                        <div style={{ display: 'flex', gap: '4px' }}>
                          <button 
                            type="button" 
                            className="action-btn"
                            onClick={() => {
                              setResetTargetUser(u);
                              setShowResetModal(true);
                            }}
                          >
                            Reset Pwd
                          </button>
                          <button 
                            type="button" 
                            className="action-btn"
                            onClick={() => handleToggleStatus(u.id)}
                          >
                            {u.isActive ? 'Deactivate' : 'Activate'}
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))
                ) : (
                  <tr>
                    <td colSpan={7} style={{ textAlign: 'center', padding: '2.5rem', color: 'var(--text-muted)' }}>
                      {activeUserSearch ? (
                        <div>
                          <div style={{ fontSize: '1rem', fontWeight: 600, color: 'var(--text-main)', marginBottom: '0.35rem' }}>
                            No users matched "{activeUserSearch}"
                          </div>
                          <div style={{ fontSize: '0.85rem' }}>Try searching by different username, email, full name, code, or role.</div>
                          <button 
                            type="button" 
                            className="btn-secondary" 
                            onClick={handleClearUserSearch}
                            style={{ marginTop: '0.75rem', fontSize: '0.8rem', padding: '0.4rem 0.8rem' }}
                          >
                            Clear Search
                          </button>
                        </div>
                      ) : (
                        'No user accounts recorded.'
                      )}
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>

          {/* Server-Side Pagination Footer */}
          <div style={{
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
            marginTop: '1rem',
            padding: '0.75rem 1rem',
            background: 'rgba(17, 23, 38, 0.6)',
            border: '1px solid var(--border-subtle)',
            borderRadius: 'var(--radius-md)',
            flexWrap: 'wrap',
            gap: '1rem'
          }}>
            <div style={{ fontSize: '0.85rem', color: 'var(--text-muted)' }}>
              {userTotalCount === 0 ? (
                'Showing 0 users'
              ) : (
                <>
                  Showing <strong>{(userPage - 1) * userPageSize + 1}</strong> to{' '}
                  <strong>{Math.min(userPage * userPageSize, userTotalCount)}</strong> of{' '}
                  <strong>{userTotalCount}</strong> users
                </>
              )}
            </div>

            <div style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
              <button
                type="button"
                className="btn-secondary"
                disabled={userPage <= 1 || loadingUsers}
                onClick={() => handlePageChange(userPage - 1)}
                style={{
                  padding: '0.45rem 0.8rem',
                  fontSize: '0.8rem',
                  display: 'flex',
                  alignItems: 'center',
                  gap: '4px',
                  opacity: userPage <= 1 ? 0.4 : 1,
                  cursor: userPage <= 1 ? 'not-allowed' : 'pointer'
                }}
              >
                <ChevronLeft size={16} /> Prev
              </button>

              {Array.from({ length: userTotalPages }, (_, i) => i + 1).map((p) => {
                if (
                  p === 1 || 
                  p === userTotalPages || 
                  (p >= userPage - 2 && p <= userPage + 2)
                ) {
                  return (
                    <button
                      key={p}
                      type="button"
                      disabled={loadingUsers}
                      onClick={() => handlePageChange(p)}
                      style={{
                        padding: '0.45rem 0.75rem',
                        fontSize: '0.8rem',
                        fontWeight: p === userPage ? 700 : 500,
                        background: p === userPage ? '#6366f1' : 'rgba(255, 255, 255, 0.05)',
                        color: p === userPage ? '#ffffff' : 'var(--text-muted)',
                        border: p === userPage ? '1px solid #818cf8' : '1px solid var(--border-subtle)',
                        borderRadius: 'var(--radius-md)',
                        cursor: 'pointer',
                        transition: 'all 0.15s ease'
                      }}
                    >
                      {p}
                    </button>
                  );
                } else if (p === userPage - 3 || p === userPage + 3) {
                  return <span key={p} style={{ color: 'var(--text-muted)', padding: '0 4px' }}>...</span>;
                }
                return null;
              })}

              <button
                type="button"
                className="btn-secondary"
                disabled={userPage >= userTotalPages || loadingUsers}
                onClick={() => handlePageChange(userPage + 1)}
                style={{
                  padding: '0.45rem 0.8rem',
                  fontSize: '0.8rem',
                  display: 'flex',
                  alignItems: 'center',
                  gap: '4px',
                  opacity: userPage >= userTotalPages ? 0.4 : 1,
                  cursor: userPage >= userTotalPages ? 'not-allowed' : 'pointer'
                }}
              >
                Next <ChevronRight size={16} />
              </button>
            </div>
          </div>
        </div>
      )}

      {/* TAB 2: ROLES & PERMISSIONS */}
      {tab === 'roles' && (
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(320px, 1fr))', gap: '1rem' }}>
          {roles.map((role) => (
            <div key={role.id} className="glass-card-item">
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '0.75rem' }}>
                <div>
                  <h3 style={{ margin: 0, fontSize: '1.15rem', fontWeight: 700 }}>{role.name}</h3>
                  <p style={{ margin: '0.25rem 0 0', fontSize: '0.8rem', color: 'var(--text-muted)' }}>
                    {role.description || 'System access level'}
                  </p>
                </div>
                <span className="badge-pill badge-primary">{role.usersCount} Assigned</span>
              </div>

              <div style={{ margin: '1rem 0' }}>
                <span style={{ fontSize: '0.8rem', fontWeight: 600, color: 'var(--text-muted)' }}>
                  Granted Capabilities ({role.permissions?.length || 0}):
                </span>
                <div style={{ display: 'flex', flexWrap: 'wrap', gap: '4px', marginTop: '0.5rem', maxHeight: '180px', overflowY: 'auto' }}>
                  {role.permissions?.map((p: string) => (
                    <span key={p} className="chip-badge">
                      {p}
                    </span>
                  ))}
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* TAB 3: SYSTEM SETTINGS */}
      {tab === 'settings' && (
        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>Setting Key</th>
                <th>Category</th>
                <th>Current Value</th>
                <th>Description</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {settings.map((s) => (
                <tr key={s.id}>
                  <td><code>{s.key}</code></td>
                  <td><span className="badge-pill badge-primary">{s.category}</span></td>
                  <td><strong>{s.value || '(empty)'}</strong></td>
                  <td style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>{s.description}</td>
                  <td>
                    <button 
                      type="button" 
                      className="action-btn"
                      onClick={() => {
                        setTargetSetting({ key: s.key, value: s.value || '' });
                        setShowSettingModal(true);
                      }}
                    >
                      Edit Value
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* TAB 4: BROADCAST ANNOUNCEMENTS */}
      {tab === 'announcements' && (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
          {announcements.map((ann) => (
            <div 
              key={ann.id} 
              className="glass-card-item"
              style={{ 
                borderLeft: ann.isPinned ? '4px solid #f59e0b' : '1px solid var(--border-subtle)',
                display: 'flex', 
                justifyContent: 'space-between', 
                alignItems: 'flex-start',
                gap: '1rem'
              }}
            >
              <div>
                <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '0.35rem' }}>
                  {ann.isPinned && <Pin size={16} color="#f59e0b" />}
                  <h4 style={{ margin: 0, fontSize: '1.05rem', fontWeight: 700 }}>{ann.title}</h4>
                  <span className={`badge-pill ${ann.isPublished ? 'badge-success' : 'badge-neutral'}`}>
                    {ann.isPublished ? 'Published' : 'Draft'}
                  </span>
                </div>
                <p style={{ margin: '0 0 0.5rem', color: 'var(--text-muted)', fontSize: '0.85rem', lineHeight: 1.5 }}>
                  {ann.content}
                </p>
                <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>
                  Posted on {new Date(ann.createdAt).toLocaleString()}
                </div>
              </div>

              <button 
                type="button" 
                className="action-btn text-danger"
                onClick={() => handleDeleteAnnouncement(ann.id)}
              >
                <Trash2 size={15} />
              </button>
            </div>
          ))}
        </div>
      )}

      {/* MODAL: CREATE USER */}
      {showUserModal && (
        <div className="modal-backdrop">
          <div className="modal-content">
            <h3 style={{ margin: '0 0 1rem', fontSize: '1.2rem', fontWeight: 700 }}>Create New User Account</h3>
            <form onSubmit={handleCreateUser} style={{ display: 'flex', flexDirection: 'column', gap: '0.85rem' }}>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0.75rem' }}>
                <div className="form-group">
                  <label className="form-label">Username</label>
                  <input 
                    type="text" 
                    className="input-field" 
                    required 
                    value={newUser.username}
                    onChange={e => setNewUser({ ...newUser, username: e.target.value })}
                    placeholder="e.g. jdoe"
                  />
                </div>
                <div className="form-group">
                  <label className="form-label">Email</label>
                  <input 
                    type="email" 
                    className="input-field" 
                    required 
                    value={newUser.email}
                    onChange={e => setNewUser({ ...newUser, email: e.target.value })}
                    placeholder="jdoe@company.com"
                  />
                </div>
              </div>

              <div className="form-group">
                <label className="form-label">Initial Password</label>
                <input 
                  type="password" 
                  className="input-field" 
                  required 
                  value={newUser.password}
                  onChange={e => setNewUser({ ...newUser, password: e.target.value })}
                  placeholder="At least 8 chars with mixed case & symbol"
                />
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0.75rem' }}>
                <div className="form-group">
                  <label className="form-label">Link to Employee</label>
                  <select 
                    className="input-field" 
                    value={newUser.employeeId || ''}
                    onChange={e => setNewUser({ ...newUser, employeeId: e.target.value ? parseInt(e.target.value) : null })}
                  >
                    <option value="">-- None (Standalone Admin) --</option>
                    {employees.map(e => (
                      <option key={e.id} value={e.id}>{e.fullName || `${e.firstName} ${e.lastName}`} ({e.employeeCode})</option>
                    ))}
                  </select>
                </div>
                <div className="form-group">
                  <label className="form-label">Assigned Role</label>
                  <select 
                    className="input-field" 
                    value={newUser.roles[0]}
                    onChange={e => setNewUser({ ...newUser, roles: [e.target.value] })}
                  >
                    <option value="Employee">Employee (Self-Service)</option>
                    <option value="Manager">Manager</option>
                    <option value="HRManager">HR Manager</option>
                    <option value="SuperAdmin">SuperAdmin (Full Access)</option>
                  </select>
                </div>
              </div>

              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '0.5rem', marginTop: '1rem' }}>
                <button type="button" className="btn-secondary" onClick={() => setShowUserModal(false)}>Cancel</button>
                <button type="submit" className="btn-primary">Create User</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* MODAL: RESET PASSWORD */}
      {showResetModal && resetTargetUser && (
        <div className="modal-backdrop">
          <div className="modal-content">
            <h3 style={{ margin: '0 0 1rem', fontSize: '1.2rem', fontWeight: 700 }}>
              Reset Password for: {resetTargetUser.username}
            </h3>
            <form onSubmit={handleResetPassword} style={{ display: 'flex', flexDirection: 'column', gap: '0.85rem' }}>
              <div className="form-group">
                <label className="form-label">New Password</label>
                <input 
                  type="password" 
                  className="input-field" 
                  required 
                  value={newPassword}
                  onChange={e => setNewPassword(e.target.value)}
                  placeholder="Enter new password"
                />
              </div>

              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '0.5rem', marginTop: '1rem' }}>
                <button type="button" className="btn-secondary" onClick={() => setShowResetModal(false)}>Cancel</button>
                <button type="submit" className="btn-primary">Update Password</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* MODAL: EDIT SETTING */}
      {showSettingModal && (
        <div className="modal-backdrop">
          <div className="modal-content">
            <h3 style={{ margin: '0 0 1rem', fontSize: '1.2rem', fontWeight: 700 }}>
              Edit Setting: {targetSetting.key}
            </h3>
            <form onSubmit={handleUpdateSetting} style={{ display: 'flex', flexDirection: 'column', gap: '0.85rem' }}>
              <div className="form-group">
                <label className="form-label">Configuration Value</label>
                <input 
                  type="text" 
                  className="input-field" 
                  required 
                  value={targetSetting.value}
                  onChange={e => setTargetSetting({ ...targetSetting, value: e.target.value })}
                />
              </div>

              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '0.5rem', marginTop: '1rem' }}>
                <button type="button" className="btn-secondary" onClick={() => setShowSettingModal(false)}>Cancel</button>
                <button type="submit" className="btn-primary">Save Setting</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* MODAL: BROADCAST ANNOUNCEMENT */}
      {showAnnModal && (
        <div className="modal-backdrop">
          <div className="modal-content">
            <h3 style={{ margin: '0 0 1rem', fontSize: '1.2rem', fontWeight: 700 }}>Broadcast Company Announcement</h3>
            <form onSubmit={handleCreateAnnouncement} style={{ display: 'flex', flexDirection: 'column', gap: '0.85rem' }}>
              <div className="form-group">
                <label className="form-label">Headline Title</label>
                <input 
                  type="text" 
                  className="input-field" 
                  required 
                  value={newAnn.title}
                  onChange={e => setNewAnn({ ...newAnn, title: e.target.value })}
                  placeholder="e.g. Office Holiday Notice"
                />
              </div>

              <div className="form-group">
                <label className="form-label">Announcement Content</label>
                <textarea 
                  className="input-field" 
                  rows={4} 
                  required 
                  value={newAnn.content}
                  onChange={e => setNewAnn({ ...newAnn, content: e.target.value })}
                  placeholder="Full message text broadcast to all employees..."
                />
              </div>

              <div className="form-group" style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                <input 
                  type="checkbox" 
                  id="pinCheckbox"
                  checked={newAnn.isPinned}
                  onChange={e => setNewAnn({ ...newAnn, isPinned: e.target.checked })}
                />
                <label htmlFor="pinCheckbox" style={{ fontSize: '0.85rem', cursor: 'pointer' }}>Pin to top of employee dashboards</label>
              </div>

              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '0.5rem', marginTop: '1rem' }}>
                <button type="button" className="btn-secondary" onClick={() => setShowAnnModal(false)}>Cancel</button>
                <button type="submit" className="btn-primary">Broadcast Now</button>
              </div>
            </form>
          </div>
        </div>
      )}

    </div>
  );
}
