import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { 
  Users, 
  UserPlus, 
  Search, 
  Filter, 
  Mail, 
  Phone, 
  MapPin, 
  CreditCard, 
  History, 
  Trash2, 
  Check, 
  AlertCircle, 
  X,
  FileSpreadsheet,
  Shield,
  KeyRound
} from 'lucide-react';

interface EmployeesProps {
  token: string;
}

const API_BASE = 'http://localhost:5219/api/v1';

export default function Employees({ token }: EmployeesProps) {
  const [employees, setEmployees] = useState<any[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMsg, setSuccessMsg] = useState<string | null>(null);

  // Detail & Drawer
  const [selectedEmp, setSelectedEmp] = useState<any | null>(null);

  // Organization data for dropdowns
  const [branches, setBranches] = useState<any[]>([]);
  const [departments, setDepartments] = useState<any[]>([]);
  const [designations, setDesignations] = useState<any[]>([]);

  // Add Employee Modal
  const [showAddModal, setShowAddModal] = useState(false);
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    gender: 'Male',
    dateOfBirth: '1995-01-01',
    nationalId: '',
    bloodGroup: 'O+',
    personalEmail: '',
    workEmail: '',
    personalPhone: '',
    workPhone: '',
    branchId: 1,
    departmentId: 1,
    designationId: 1,
    joiningDate: '2026-01-01',
    employmentType: 'FullTime',
    status: 'Active',
    createPortalAccount: true,
    portalPassword: 'Password@123',
    portalRole: 'Employee'
  });

  const headers = { Authorization: `Bearer ${token}` };

  const fetchEmployees = async () => {
    setLoading(true);
    setError(null);
    try {
      const res = await axios.get(`${API_BASE}/employees`, {
        headers,
        params: {
          page,
          pageSize: 10,
          search: search || undefined,
          status: statusFilter || undefined
        }
      });
      setEmployees(res.data.data.items || []);
      setTotalCount(res.data.data.totalCount || 0);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error fetching employees.');
    } finally {
      setLoading(false);
    }
  };

  const fetchDropdowns = async () => {
    try {
      const [bRes, dRes, desRes] = await Promise.all([
        axios.get(`${API_BASE}/branches/all`, { headers }),
        axios.get(`${API_BASE}/departments`, { headers }),
        axios.get(`${API_BASE}/designations`, { headers })
      ]);
      setBranches(bRes.data.data || []);
      setDepartments(dRes.data.data || []);
      setDesignations(desRes.data.data || []);

      if (bRes.data.data?.length > 0) {
        setFormData(prev => ({ ...prev, branchId: bRes.data.data[0].id }));
      }
      if (dRes.data.data?.length > 0) {
        setFormData(prev => ({ ...prev, departmentId: dRes.data.data[0].id }));
      }
      if (desRes.data.data?.length > 0) {
        setFormData(prev => ({ ...prev, designationId: desRes.data.data[0].id }));
      }
    } catch (err) {
      console.error(err);
    }
  };

  useEffect(() => {
    fetchEmployees();
    fetchDropdowns();
  }, [page, statusFilter]);

  const handleSearchSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setPage(1);
    fetchEmployees();
  };

  const handleCreateEmployee = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    try {
      await axios.post(`${API_BASE}/employees`, formData, { headers });
      setSuccessMsg('Employee created successfully with auto-generated code!');
      setShowAddModal(false);
      fetchEmployees();
      setTimeout(() => setSuccessMsg(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to create employee.');
    }
  };

  const viewEmployeeDetail = async (id: number) => {
    try {
      const res = await axios.get(`${API_BASE}/employees/${id}`, { headers });
      setSelectedEmp(res.data.data);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load employee details.');
    }
  };

  const handleResetEmployeePassword = async (empId: number) => {
    const pwd = prompt('Enter new portal password for this employee (min 6 characters):', 'Password@123');
    if (!pwd) return;
    try {
      const res = await axios.post(`${API_BASE}/employees/${empId}/reset-password`, { newPassword: pwd }, { headers });
      if (res.data.success) {
        setSuccessMsg('Employee portal password has been reset successfully!');
        viewEmployeeDetail(empId);
        setTimeout(() => setSuccessMsg(null), 3000);
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to reset employee password.');
    }
  };

  const handleCreateEmployeePortalAccount = async (emp: any) => {
    const pwd = prompt(`Create portal login credentials for ${emp.fullName}.\nEnter initial password:`, 'Password@123');
    if (!pwd) return;
    try {
      const res = await axios.post(`${API_BASE}/employees/${emp.id}/account`, {
        username: emp.workEmail.split('@')[0],
        email: emp.workEmail,
        password: pwd,
        role: 'Employee',
        isActive: true
      }, { headers });
      if (res.data.success) {
        setSuccessMsg('Employee portal account created successfully!');
        viewEmployeeDetail(emp.id);
        setTimeout(() => setSuccessMsg(null), 3000);
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to create portal account.');
    }
  };

  const handleDeleteEmployee = async (id: number) => {
    if (!confirm('Are you sure you want to soft delete this employee?')) return;
    try {
      await axios.delete(`${API_BASE}/employees/${id}`, { headers });
      setSuccessMsg('Employee soft deleted successfully.');
      fetchEmployees();
      if (selectedEmp?.id === id) setSelectedEmp(null);
      setTimeout(() => setSuccessMsg(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to delete employee.');
    }
  };

  const handleExportExcel = async () => {
    try {
      const res = await axios.get(`${API_BASE}/employees/export`, {
        headers,
        responseType: 'blob'
      });
      const url = window.URL.createObjectURL(new Blob([res.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `Employees_${new Date().toISOString().slice(0, 10)}.xlsx`);
      document.body.appendChild(link);
      link.click();
      link.remove();
    } catch (err) {
      setError('Failed to export Excel spreadsheet.');
    }
  };

  return (
    <div style={{ marginTop: '2rem' }}>
      {/* Header & Metric Cards */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem', flexWrap: 'wrap', gap: '1rem' }}>
        <div>
          <h2 style={{ fontSize: '1.5rem', fontWeight: 800, letterSpacing: '-0.02em', display: 'flex', alignItems: 'center', gap: '8px' }}>
            <Users size={24} color="#6366f1" /> Workforce &amp; Employees
            <span style={{ fontSize: '0.8rem', background: 'rgba(99, 102, 241, 0.2)', color: '#818cf8', padding: '0.2rem 0.6rem', borderRadius: 9999 }}>
              {totalCount} total
            </span>
          </h2>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.875rem' }}>
            Complete directory, onboarding, lifecycle events, and sub-entities
          </p>
        </div>

        <div style={{ display: 'flex', gap: '0.75rem' }}>
          <button 
            onClick={handleExportExcel}
            className="btn-secondary" 
            style={{ display: 'flex', alignItems: 'center', gap: '6px', fontSize: '0.85rem' }}
          >
            <FileSpreadsheet size={16} color="#10b981" /> Export Excel
          </button>
          <button 
            onClick={() => setShowAddModal(true)}
            className="btn-primary" 
            style={{ width: 'auto', padding: '0.65rem 1.25rem', fontSize: '0.85rem' }}
          >
            <UserPlus size={16} /> Onboard Employee
          </button>
        </div>
      </div>

      {/* Filter / Search Bar */}
      <div style={{ 
        background: 'var(--bg-surface)', 
        border: '1px solid var(--border-subtle)', 
        borderRadius: 'var(--radius-lg)', 
        padding: '1rem',
        marginBottom: '1.5rem',
        display: 'flex',
        gap: '1rem',
        flexWrap: 'wrap',
        alignItems: 'center'
      }}>
        <form onSubmit={handleSearchSubmit} style={{ flex: 1, minWidth: '240px', display: 'flex', gap: '0.5rem' }}>
          <div className="input-wrapper" style={{ flex: 1 }}>
            <Search size={18} className="input-icon" />
            <input 
              className="input-field"
              placeholder="Search by code, full name, email, or phone..."
              value={search}
              onChange={e => setSearch(e.target.value)}
            />
          </div>
          <button type="submit" className="btn-secondary" style={{ padding: '0.7rem 1.25rem' }}>Search</button>
        </form>

        <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
          <Filter size={16} color="var(--text-muted)" />
          <select 
            className="input-field" 
            style={{ padding: '0.7rem 1rem', width: 'auto' }}
            value={statusFilter}
            onChange={e => setStatusFilter(e.target.value)}
          >
            <option value="">All Statuses</option>
            <option value="Active">Active</option>
            <option value="Probation">Probation</option>
            <option value="Resigned">Resigned</option>
            <option value="Terminated">Terminated</option>
          </select>
        </div>
      </div>

      {/* Feedback Messages */}
      {successMsg && (
        <div style={{ background: 'rgba(16, 185, 129, 0.12)', border: '1px solid rgba(16, 185, 129, 0.3)', color: '#6ee7b7', padding: '0.75rem 1rem', borderRadius: 'var(--radius-md)', marginBottom: '1rem', display: 'flex', alignItems: 'center', gap: '8px', fontSize: '0.875rem' }}>
          <Check size={18} /> {successMsg}
        </div>
      )}

      {error && (
        <div className="alert-error">
          <AlertCircle size={18} /> {error}
        </div>
      )}

      {/* Employee Table */}
      <div style={{ 
        background: 'var(--bg-surface)', 
        border: '1px solid var(--border-subtle)', 
        borderRadius: 'var(--radius-lg)', 
        overflow: 'hidden'
      }}>
        {loading ? (
          <div style={{ padding: '3rem', textAlign: 'center', color: 'var(--text-dim)' }}>Loading directory...</div>
        ) : employees.length === 0 ? (
          <div style={{ padding: '3rem', textAlign: 'center', color: 'var(--text-dim)' }}>No employees found.</div>
        ) : (
          <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', fontSize: '0.875rem' }}>
            <thead>
              <tr style={{ background: 'rgba(255, 255, 255, 0.02)', borderBottom: '1px solid var(--border-subtle)', color: 'var(--text-muted)' }}>
                <th style={{ padding: '0.85rem 1rem' }}>Code</th>
                <th style={{ padding: '0.85rem 1rem' }}>Employee Name</th>
                <th style={{ padding: '0.85rem 1rem' }}>Contact</th>
                <th style={{ padding: '0.85rem 1rem' }}>Branch & Dept</th>
                <th style={{ padding: '0.85rem 1rem' }}>Designation</th>
                <th style={{ padding: '0.85rem 1rem' }}>Status</th>
                <th style={{ padding: '0.85rem 1rem' }}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {employees.map((emp) => (
                <tr key={emp.id} style={{ borderBottom: '1px solid var(--border-subtle)' }}>
                  <td style={{ padding: '0.85rem 1rem', fontFamily: 'var(--font-mono)', color: '#818cf8', fontWeight: 600 }}>
                    {emp.employeeCode}
                  </td>
                  <td style={{ padding: '0.85rem 1rem' }}>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
                      <div style={{ width: 34, height: 34, borderRadius: '50%', background: '#312e81', color: '#c7d2fe', display: 'flex', alignItems: 'center', justifyContent: 'center', fontWeight: 700, fontSize: '0.8rem' }}>
                        {emp.firstName[0]}{emp.lastName[0]}
                      </div>
                      <div>
                        <div style={{ fontWeight: 600, color: '#f8fafc' }}>{emp.fullName}</div>
                        <div style={{ fontSize: '0.75rem', color: 'var(--text-dim)' }}>{emp.gender} • Joined {emp.joiningDate}</div>
                      </div>
                    </div>
                  </td>
                  <td style={{ padding: '0.85rem 1rem' }}>
                    <div style={{ fontSize: '0.8rem', color: 'var(--text-muted)', display: 'flex', alignItems: 'center', gap: '4px' }}>
                      <Mail size={12} /> {emp.workEmail}
                    </div>
                    {emp.workPhone && (
                      <div style={{ fontSize: '0.75rem', color: 'var(--text-dim)', display: 'flex', alignItems: 'center', gap: '4px' }}>
                        <Phone size={12} /> {emp.workPhone}
                      </div>
                    )}
                  </td>
                  <td style={{ padding: '0.85rem 1rem', color: 'var(--text-muted)' }}>
                    <div>{emp.branchName}</div>
                    <div style={{ fontSize: '0.75rem', color: 'var(--text-dim)' }}>{emp.departmentName}</div>
                  </td>
                  <td style={{ padding: '0.85rem 1rem', fontWeight: 500 }}>{emp.designationTitle}</td>
                  <td style={{ padding: '0.85rem 1rem' }}>
                    <span style={{ 
                      background: emp.status === 'Active' ? 'rgba(16, 185, 129, 0.15)' : 'rgba(245, 158, 11, 0.15)',
                      color: emp.status === 'Active' ? '#34d399' : '#fbbf24',
                      padding: '0.2rem 0.55rem',
                      borderRadius: 9999,
                      fontSize: '0.75rem',
                      fontWeight: 600
                    }}>
                      {emp.status}
                    </span>
                  </td>
                  <td style={{ padding: '0.85rem 1rem' }}>
                    <div style={{ display: 'flex', gap: '8px' }}>
                      <button 
                        onClick={() => viewEmployeeDetail(emp.id)}
                        className="btn-secondary" 
                        style={{ padding: '0.35rem 0.65rem', fontSize: '0.75rem' }}
                      >
                        Profile
                      </button>
                      <button 
                        onClick={() => handleDeleteEmployee(emp.id)}
                        style={{ background: 'none', border: 'none', color: '#f43f5e', cursor: 'pointer', padding: '0.35rem' }}
                      >
                        <Trash2 size={16} />
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {/* EMPLOYEE DETAIL DRAWER */}
      {selectedEmp && (
        <div style={{
          position: 'fixed',
          inset: 0,
          background: 'rgba(0, 0, 0, 0.75)',
          backdropFilter: 'blur(8px)',
          display: 'flex',
          justifyContent: 'flex-end',
          zIndex: 100
        }}>
          <div style={{
            background: 'var(--bg-surface)',
            width: '100%',
            maxWidth: 680,
            height: '100%',
            overflowY: 'auto',
            padding: '2rem',
            borderLeft: '1px solid var(--border-subtle)',
            boxShadow: '-10px 0 30px rgba(0, 0, 0, 0.5)'
          }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '1.5rem' }}>
              <div style={{ display: 'flex', gap: '1rem', alignItems: 'center' }}>
                <div style={{ width: 60, height: 60, borderRadius: '50%', background: 'linear-gradient(135deg, #6366f1, #06b6d4)', display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: '1.5rem', fontWeight: 800 }}>
                  {selectedEmp.firstName[0]}{selectedEmp.lastName[0]}
                </div>
                <div>
                  <h3 style={{ fontSize: '1.35rem', fontWeight: 800 }}>{selectedEmp.fullName}</h3>
                  <div style={{ fontFamily: 'var(--font-mono)', color: '#818cf8', fontSize: '0.85rem' }}>
                    {selectedEmp.employeeCode} • {selectedEmp.designationTitle}
                  </div>
                  <div style={{ color: 'var(--text-dim)', fontSize: '0.8rem' }}>{selectedEmp.branchName} | {selectedEmp.departmentName}</div>
                </div>
              </div>
              <button onClick={() => setSelectedEmp(null)} style={{ background: 'none', border: 'none', color: 'var(--text-dim)', cursor: 'pointer' }}>
                <X size={20} />
              </button>
            </div>

            {/* Portal Access & Login Security */}
            <div style={{ marginBottom: '1.5rem', background: 'rgba(99, 102, 241, 0.08)', border: '1px solid rgba(99, 102, 241, 0.25)', borderRadius: 'var(--radius-md)', padding: '1.25rem' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.75rem' }}>
                <h4 style={{ fontSize: '0.95rem', fontWeight: 700, color: '#f8fafc', margin: 0, display: 'flex', alignItems: 'center', gap: '8px' }}>
                  <Shield size={18} color="#818cf8" /> Portal Access &amp; Security
                </h4>
                {selectedEmp.portalAccount && (
                  <span style={{
                    padding: '0.2rem 0.6rem',
                    borderRadius: 9999,
                    fontSize: '0.75rem',
                    fontWeight: 700,
                    background: selectedEmp.portalAccount.isActive ? 'rgba(16, 185, 129, 0.15)' : 'rgba(244, 63, 94, 0.15)',
                    color: selectedEmp.portalAccount.isActive ? '#34d399' : '#fb7185'
                  }}>
                    {selectedEmp.portalAccount.isActive ? 'Active Login' : 'Disabled'}
                  </span>
                )}
              </div>

              {selectedEmp.portalAccount ? (
                <div>
                  <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0.75rem', fontSize: '0.8125rem', marginBottom: '1rem' }}>
                    <div>
                      <span style={{ color: 'var(--text-muted)' }}>Portal Username:</span>
                      <div style={{ fontWeight: 600, color: '#ffffff', fontFamily: 'var(--font-mono)' }}>{selectedEmp.portalAccount.username}</div>
                    </div>
                    <div>
                      <span style={{ color: 'var(--text-muted)' }}>Login Email:</span>
                      <div style={{ fontWeight: 600, color: '#ffffff' }}>{selectedEmp.portalAccount.email}</div>
                    </div>
                    <div>
                      <span style={{ color: 'var(--text-muted)' }}>Assigned Role(s):</span>
                      <div style={{ fontWeight: 600, color: '#a5b4fc' }}>{selectedEmp.portalAccount.roles?.join(', ') || 'Employee'}</div>
                    </div>
                    <div>
                      <span style={{ color: 'var(--text-muted)' }}>Last Login:</span>
                      <div style={{ color: 'var(--text-muted)' }}>
                        {selectedEmp.portalAccount.lastLoginAt ? new Date(selectedEmp.portalAccount.lastLoginAt).toLocaleString() : 'Never logged in'}
                      </div>
                    </div>
                  </div>

                  <div style={{ display: 'flex', gap: '0.5rem' }}>
                    <button
                      type="button"
                      onClick={() => handleResetEmployeePassword(selectedEmp.id)}
                      className="btn-secondary"
                      style={{ fontSize: '0.8rem', padding: '0.45rem 0.9rem', display: 'flex', alignItems: 'center', gap: '6px' }}
                    >
                      <KeyRound size={14} /> Set / Reset Password
                    </button>
                  </div>
                </div>
              ) : (
                <div>
                  <p style={{ color: 'var(--text-muted)', fontSize: '0.825rem', marginBottom: '0.75rem' }}>
                    This employee does not have a portal login account yet. SuperAdmin can create login credentials to grant them workspace access.
                  </p>
                  <button
                    type="button"
                    onClick={() => handleCreateEmployeePortalAccount(selectedEmp)}
                    className="btn-primary"
                    style={{ fontSize: '0.8rem', padding: '0.45rem 1rem', width: 'auto' }}
                  >
                    + Create Portal Login Account
                  </button>
                </div>
              )}
            </div>

            {/* Sub-Entity Sections */}
            <div style={{ marginBottom: '1.5rem' }}>
              <h4 style={{ fontSize: '0.95rem', fontWeight: 700, color: '#f8fafc', marginBottom: '0.75rem', display: 'flex', alignItems: 'center', gap: '6px' }}>
                <MapPin size={16} color="#06b6d4" /> Addresses ({selectedEmp.addresses?.length || 0})
              </h4>
              {selectedEmp.addresses?.map((a: any) => (
                <div key={a.id} style={{ background: 'var(--bg-surface-elevated)', padding: '0.75rem', borderRadius: 'var(--radius-sm)', marginBottom: '0.5rem', fontSize: '0.8125rem' }}>
                  <strong>{a.addressType}:</strong> {a.street}, {a.city}, {a.country}
                </div>
              ))}
            </div>

            <div style={{ marginBottom: '1.5rem' }}>
              <h4 style={{ fontSize: '0.95rem', fontWeight: 700, color: '#f8fafc', marginBottom: '0.75rem', display: 'flex', alignItems: 'center', gap: '6px' }}>
                <CreditCard size={16} color="#10b981" /> Bank Accounts ({selectedEmp.bankAccounts?.length || 0})
              </h4>
              {selectedEmp.bankAccounts?.map((b: any) => (
                <div key={b.id} style={{ background: 'var(--bg-surface-elevated)', padding: '0.75rem', borderRadius: 'var(--radius-sm)', marginBottom: '0.5rem', fontSize: '0.8125rem' }}>
                  <strong>{b.bankName}</strong> ({b.branchName}) • A/C: {b.accountNumber} {b.isPrimary && <span style={{ color: '#10b981' }}>[Primary]</span>}
                </div>
              ))}
            </div>

            <div style={{ marginBottom: '1.5rem' }}>
              <h4 style={{ fontSize: '0.95rem', fontWeight: 700, color: '#f8fafc', marginBottom: '0.75rem', display: 'flex', alignItems: 'center', gap: '6px' }}>
                <History size={16} color="#a855f7" /> Lifecycle Event History ({selectedEmp.lifecycleEvents?.length || 0})
              </h4>
              {selectedEmp.lifecycleEvents?.map((le: any) => (
                <div key={le.id} style={{ borderLeft: '2px solid #a855f7', paddingLeft: '0.75rem', marginBottom: '0.75rem', fontSize: '0.8125rem' }}>
                  <div style={{ fontWeight: 600, color: '#e2e8f0' }}>{le.eventType} — {le.eventDate}</div>
                  <div style={{ color: 'var(--text-muted)' }}>{le.newValue || le.remarks}</div>
                </div>
              ))}
            </div>
          </div>
        </div>
      )}

      {/* ONBOARD EMPLOYEE MODAL */}
      {showAddModal && (
        <div style={{
          position: 'fixed',
          inset: 0,
          background: 'rgba(0, 0, 0, 0.8)',
          backdropFilter: 'blur(8px)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          zIndex: 100,
          padding: '1rem'
        }}>
          <div className="glass-card" style={{ maxWidth: 640, maxHeight: '90vh', overflowY: 'auto' }}>
            <h3 style={{ fontSize: '1.25rem', fontWeight: 700, marginBottom: '1rem', display: 'flex', alignItems: 'center', gap: '8px' }}>
              <UserPlus size={20} color="#6366f1" /> New Employee Onboarding
            </h3>

            <form onSubmit={handleCreateEmployee}>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
                <div className="form-group">
                  <label className="form-label">First Name *</label>
                  <input className="input-field" required value={formData.firstName} onChange={e => setFormData({ ...formData, firstName: e.target.value })} placeholder="e.g. Tariqul" />
                </div>
                <div className="form-group">
                  <label className="form-label">Last Name *</label>
                  <input className="input-field" required value={formData.lastName} onChange={e => setFormData({ ...formData, lastName: e.target.value })} placeholder="e.g. Islam" />
                </div>
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
                <div className="form-group">
                  <label className="form-label">Work Corporate Email *</label>
                  <input className="input-field" type="email" required value={formData.workEmail} onChange={e => setFormData({ ...formData, workEmail: e.target.value })} placeholder="e.g. tariqul@hrms.local" />
                </div>
                <div className="form-group">
                  <label className="form-label">Work Phone</label>
                  <input className="input-field" value={formData.workPhone} onChange={e => setFormData({ ...formData, workPhone: e.target.value })} placeholder="+8801700000000" />
                </div>
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: '1rem' }}>
                <div className="form-group">
                  <label className="form-label">Gender *</label>
                  <select className="input-field" value={formData.gender} onChange={e => setFormData({ ...formData, gender: e.target.value })}>
                    <option value="Male">Male</option>
                    <option value="Female">Female</option>
                    <option value="Other">Other</option>
                  </select>
                </div>
                <div className="form-group">
                  <label className="form-label">Date of Birth *</label>
                  <input className="input-field" type="date" required value={formData.dateOfBirth} onChange={e => setFormData({ ...formData, dateOfBirth: e.target.value })} />
                </div>
                <div className="form-group">
                  <label className="form-label">Blood Group</label>
                  <input className="input-field" value={formData.bloodGroup} onChange={e => setFormData({ ...formData, bloodGroup: e.target.value })} placeholder="B+" />
                </div>
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
                <div className="form-group">
                  <label className="form-label">Branch *</label>
                  <select className="input-field" value={formData.branchId} onChange={e => setFormData({ ...formData, branchId: Number(e.target.value) })}>
                    {branches.map(b => <option key={b.id} value={b.id}>{b.name}</option>)}
                  </select>
                </div>
                <div className="form-group">
                  <label className="form-label">Department *</label>
                  <select className="input-field" value={formData.departmentId} onChange={e => setFormData({ ...formData, departmentId: Number(e.target.value) })}>
                    {departments.map(d => <option key={d.id} value={d.id}>{d.name}</option>)}
                  </select>
                </div>
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
                <div className="form-group">
                  <label className="form-label">Designation *</label>
                  <select className="input-field" value={formData.designationId} onChange={e => setFormData({ ...formData, designationId: Number(e.target.value) })}>
                    {designations.map(d => <option key={d.id} value={d.id}>{d.title}</option>)}
                  </select>
                </div>
                <div className="form-group">
                  <label className="form-label">Joining Date *</label>
                  <input className="input-field" type="date" required value={formData.joiningDate} onChange={e => setFormData({ ...formData, joiningDate: e.target.value })} />
                </div>
              </div>

              {/* Portal Login Credentials Option */}
              <div style={{ background: 'rgba(99, 102, 241, 0.08)', border: '1px solid rgba(99, 102, 241, 0.25)', borderRadius: 'var(--radius-md)', padding: '1rem', marginTop: '1rem' }}>
                <label style={{ display: 'flex', alignItems: 'center', gap: '8px', cursor: 'pointer', fontWeight: 700, fontSize: '0.85rem', color: '#f8fafc' }}>
                  <input
                    type="checkbox"
                    checked={formData.createPortalAccount}
                    onChange={e => setFormData({ ...formData, createPortalAccount: e.target.checked })}
                  />
                  <span>Create Portal Login Account for this Employee</span>
                </label>

                {formData.createPortalAccount && (
                  <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem', marginTop: '0.85rem' }}>
                    <div className="form-group" style={{ margin: 0 }}>
                      <label className="form-label">Initial Password *</label>
                      <input
                        className="input-field"
                        type="text"
                        required={formData.createPortalAccount}
                        value={formData.portalPassword}
                        onChange={e => setFormData({ ...formData, portalPassword: e.target.value })}
                        placeholder="Password@123"
                      />
                    </div>
                    <div className="form-group" style={{ margin: 0 }}>
                      <label className="form-label">Portal Role *</label>
                      <select
                        className="input-field"
                        value={formData.portalRole}
                        onChange={e => setFormData({ ...formData, portalRole: e.target.value })}
                      >
                        <option value="Employee">Employee</option>
                        <option value="HRManager">HR Manager</option>
                        <option value="Admin">Admin</option>
                        <option value="SuperAdmin">SuperAdmin</option>
                      </select>
                    </div>
                  </div>
                )}
              </div>

              <div style={{ display: 'flex', gap: '0.75rem', marginTop: '1.5rem' }}>
                <button type="submit" className="btn-primary" style={{ flex: 1 }}>Submit &amp; Auto-Generate ID</button>
                <button type="button" onClick={() => setShowAddModal(false)} className="btn-secondary" style={{ flex: 1 }}>Cancel</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
