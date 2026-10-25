import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { 
  Building2, 
  FolderTree, 
  Award, 
  BarChart3, 
  Clock, 
  CalendarDays, 
  Plus, 
  Trash2, 
  Check, 
  AlertCircle,
  ChevronRight
} from 'lucide-react';

interface OrganizationProps {
  token: string;
}

const API_BASE = 'http://localhost:5219/api/v1';

export default function Organization({ token }: OrganizationProps) {
  const [activeTab, setActiveTab] = useState<'branches' | 'departments' | 'designations' | 'grades' | 'shifts' | 'holidays'>('branches');
  const [branches, setBranches] = useState<any[]>([]);
  const [departments, setDepartments] = useState<any[]>([]);
  const [deptTree, setDeptTree] = useState<any[]>([]);
  const [designations, setDesignations] = useState<any[]>([]);
  const [grades, setGrades] = useState<any[]>([]);
  const [shifts, setShifts] = useState<any[]>([]);
  const [holidays, setHolidays] = useState<any[]>([]);
  
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMsg, setSuccessMsg] = useState<string | null>(null);

  // Modal forms
  const [showModal, setShowModal] = useState(false);
  const [newBranch, setNewBranch] = useState({ name: '', code: '', city: 'Dhaka', country: 'Bangladesh', isHeadOffice: false });
  const [newDept, setNewDept] = useState({ name: '', code: '', branchId: 1, parentId: null as number | null, description: '' });
  const [newDesig, setNewDesig] = useState({ title: '', code: '', departmentId: 1 });
  const [newGrade, setNewGrade] = useState({ name: '', code: '', minSalary: 60000, maxSalary: 100000, description: '' });
  const [newShift, setNewShift] = useState({ name: '', code: '', startTime: '09:00:00', endTime: '18:00:00', graceMinutes: 15, workingHours: 8.0 });
  const [newHoliday, setNewHoliday] = useState({ name: '', holidayDate: '2026-12-25', type: 'Public', isRecurring: true });

  const headers = { Authorization: `Bearer ${token}` };

  const fetchCurrentTab = async () => {
    setLoading(true);
    setError(null);
    try {
      if (activeTab === 'branches') {
        const res = await axios.get(`${API_BASE}/branches`, { headers });
        setBranches(res.data.data.items || []);
      } else if (activeTab === 'departments') {
        const [listRes, treeRes] = await Promise.all([
          axios.get(`${API_BASE}/departments`, { headers }),
          axios.get(`${API_BASE}/departments/tree`, { headers })
        ]);
        setDepartments(listRes.data.data || []);
        setDeptTree(treeRes.data.data || []);
      } else if (activeTab === 'designations') {
        const res = await axios.get(`${API_BASE}/designations`, { headers });
        setDesignations(res.data.data || []);
      } else if (activeTab === 'grades') {
        const res = await axios.get(`${API_BASE}/grades`, { headers });
        setGrades(res.data.data || []);
      } else if (activeTab === 'shifts') {
        const res = await axios.get(`${API_BASE}/shifts`, { headers });
        setShifts(res.data.data || []);
      } else if (activeTab === 'holidays') {
        const res = await axios.get(`${API_BASE}/holidays`, { headers });
        setHolidays(res.data.data || []);
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error loading records.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchCurrentTab();
  }, [activeTab]);

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    try {
      if (activeTab === 'branches') {
        await axios.post(`${API_BASE}/branches`, newBranch, { headers });
      } else if (activeTab === 'departments') {
        await axios.post(`${API_BASE}/departments`, newDept, { headers });
      } else if (activeTab === 'designations') {
        await axios.post(`${API_BASE}/designations`, newDesig, { headers });
      } else if (activeTab === 'grades') {
        await axios.post(`${API_BASE}/grades`, newGrade, { headers });
      } else if (activeTab === 'shifts') {
        await axios.post(`${API_BASE}/shifts`, newShift, { headers });
      } else if (activeTab === 'holidays') {
        await axios.post(`${API_BASE}/holidays`, newHoliday, { headers });
      }

      setSuccessMsg(`Record added successfully!`);
      setShowModal(false);
      fetchCurrentTab();
      setTimeout(() => setSuccessMsg(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error saving record.');
    }
  };

  const handleDelete = async (endpoint: string, id: number) => {
    if (!confirm('Are you sure you want to soft delete this item?')) return;
    try {
      await axios.delete(`${API_BASE}/${endpoint}/${id}`, { headers });
      setSuccessMsg('Record deleted successfully.');
      fetchCurrentTab();
      setTimeout(() => setSuccessMsg(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to delete record.');
    }
  };

  return (
    <div style={{ marginTop: '2rem' }}>
      {/* Header and Controls */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem', flexWrap: 'wrap', gap: '1rem' }}>
        <div>
          <h2 style={{ fontSize: '1.5rem', fontWeight: 800, letterSpacing: '-0.02em', display: 'flex', alignItems: 'center', gap: '8px' }}>
            <Building2 size={24} color="#6366f1" /> Organization Core
          </h2>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.875rem' }}>
            Manage organizational hierarchy, branches, salary bands, and schedules
          </p>
        </div>

        <button 
          onClick={() => setShowModal(true)}
          className="btn-primary" 
          style={{ width: 'auto', padding: '0.65rem 1.25rem', fontSize: '0.85rem' }}
        >
          <Plus size={16} /> Add {activeTab.slice(0, -1)}
        </button>
      </div>

      {/* Tabs */}
      <div style={{ 
        display: 'flex', 
        gap: '0.5rem', 
        borderBottom: '1px solid var(--border-subtle)', 
        paddingBottom: '0.75rem',
        overflowX: 'auto',
        marginBottom: '1.5rem'
      }}>
        {[
          { key: 'branches', label: 'Branches', icon: Building2 },
          { key: 'departments', label: 'Departments & Tree', icon: FolderTree },
          { key: 'designations', label: 'Designations', icon: Award },
          { key: 'grades', label: 'Grades & Bands', icon: BarChart3 },
          { key: 'shifts', label: 'Work Shifts', icon: Clock },
          { key: 'holidays', label: 'Holidays', icon: CalendarDays }
        ].map((tab) => {
          const Icon = tab.icon;
          const isActive = activeTab === tab.key;
          return (
            <button
              key={tab.key}
              onClick={() => setActiveTab(tab.key as any)}
              style={{
                background: isActive ? 'rgba(99, 102, 241, 0.15)' : 'transparent',
                border: isActive ? '1px solid #6366f1' : '1px solid transparent',
                color: isActive ? '#a5b4fc' : 'var(--text-muted)',
                borderRadius: 'var(--radius-sm)',
                padding: '0.5rem 0.85rem',
                fontSize: '0.8125rem',
                fontWeight: 600,
                cursor: 'pointer',
                display: 'flex',
                alignItems: 'center',
                gap: '6px',
                whiteSpace: 'nowrap',
                transition: 'all 0.15s'
              }}
            >
              <Icon size={16} />
              {tab.label}
            </button>
          );
        })}
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

      {/* Content Area */}
      <div style={{ 
        background: 'var(--bg-surface)', 
        border: '1px solid var(--border-subtle)', 
        borderRadius: 'var(--radius-lg)', 
        overflow: 'hidden'
      }}>
        {loading ? (
          <div style={{ padding: '3rem', textAlign: 'center', color: 'var(--text-dim)' }}>
            Loading organization data...
          </div>
        ) : (
          <div>
            {/* BRANCHES */}
            {activeTab === 'branches' && (
              <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', fontSize: '0.875rem' }}>
                <thead>
                  <tr style={{ background: 'rgba(255, 255, 255, 0.02)', borderBottom: '1px solid var(--border-subtle)', color: 'var(--text-muted)' }}>
                    <th style={{ padding: '0.85rem 1rem' }}>Code</th>
                    <th style={{ padding: '0.85rem 1rem' }}>Name</th>
                    <th style={{ padding: '0.85rem 1rem' }}>City / Country</th>
                    <th style={{ padding: '0.85rem 1rem' }}>Head Office</th>
                    <th style={{ padding: '0.85rem 1rem' }}>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {branches.map((b) => (
                    <tr key={b.id} style={{ borderBottom: '1px solid var(--border-subtle)' }}>
                      <td style={{ padding: '0.85rem 1rem', fontFamily: 'var(--font-mono)', color: '#818cf8' }}>{b.code}</td>
                      <td style={{ padding: '0.85rem 1rem', fontWeight: 600 }}>{b.name}</td>
                      <td style={{ padding: '0.85rem 1rem', color: 'var(--text-muted)' }}>{b.city}, {b.country}</td>
                      <td style={{ padding: '0.85rem 1rem' }}>
                        {b.isHeadOffice ? (
                          <span style={{ background: 'rgba(16, 185, 129, 0.15)', color: '#34d399', padding: '0.2rem 0.5rem', borderRadius: 9999, fontSize: '0.75rem', fontWeight: 600 }}>
                            HQ
                          </span>
                        ) : 'Branch'}
                      </td>
                      <td style={{ padding: '0.85rem 1rem' }}>
                        <button onClick={() => handleDelete('branches', b.id)} style={{ background: 'none', border: 'none', color: '#f43f5e', cursor: 'pointer' }}>
                          <Trash2 size={16} />
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}

            {/* DEPARTMENTS & TREE */}
            {activeTab === 'departments' && (
              <div style={{ padding: '1.5rem' }}>
                <h3 style={{ fontSize: '1rem', fontWeight: 700, marginBottom: '1rem', display: 'flex', alignItems: 'center', gap: '8px' }}>
                  <FolderTree size={18} color="#06b6d4" /> Department Hierarchy ({departments.length} total)
                </h3>
                <div style={{ background: 'var(--bg-surface-elevated)', borderRadius: 'var(--radius-md)', padding: '1.25rem', border: '1px solid var(--border-subtle)' }}>
                  {deptTree.map((root) => (
                    <div key={root.id} style={{ marginBottom: '1rem' }}>
                      <div style={{ display: 'flex', alignItems: 'center', gap: '8px', fontWeight: 700, color: '#f8fafc' }}>
                        <span style={{ color: '#818cf8', fontFamily: 'var(--font-mono)', fontSize: '0.8rem' }}>[{root.code}]</span>
                        <span>{root.name}</span>
                        <button onClick={() => handleDelete('departments', root.id)} style={{ background: 'none', border: 'none', color: '#f43f5e', cursor: 'pointer', marginLeft: 'auto' }}>
                          <Trash2 size={14} />
                        </button>
                      </div>
                      {root.children?.length > 0 && (
                        <div style={{ paddingLeft: '1.5rem', marginTop: '0.5rem', borderLeft: '2px dashed var(--border-subtle)' }}>
                          {root.children.map((child: any) => (
                            <div key={child.id} style={{ display: 'flex', alignItems: 'center', gap: '6px', padding: '0.35rem 0', color: 'var(--text-muted)' }}>
                              <ChevronRight size={14} />
                              <span style={{ color: '#06b6d4', fontFamily: 'var(--font-mono)', fontSize: '0.78rem' }}>[{child.code}]</span>
                              <span>{child.name}</span>
                              <button onClick={() => handleDelete('departments', child.id)} style={{ background: 'none', border: 'none', color: '#f43f5e', cursor: 'pointer', marginLeft: 'auto' }}>
                                <Trash2 size={14} />
                              </button>
                            </div>
                          ))}
                        </div>
                      )}
                    </div>
                  ))}
                </div>
              </div>
            )}

            {/* DESIGNATIONS */}
            {activeTab === 'designations' && (
              <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', fontSize: '0.875rem' }}>
                <thead>
                  <tr style={{ background: 'rgba(255, 255, 255, 0.02)', borderBottom: '1px solid var(--border-subtle)', color: 'var(--text-muted)' }}>
                    <th style={{ padding: '0.85rem 1rem' }}>Code</th>
                    <th style={{ padding: '0.85rem 1rem' }}>Title</th>
                    <th style={{ padding: '0.85rem 1rem' }}>Department</th>
                    <th style={{ padding: '0.85rem 1rem' }}>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {designations.map((d) => (
                    <tr key={d.id} style={{ borderBottom: '1px solid var(--border-subtle)' }}>
                      <td style={{ padding: '0.85rem 1rem', fontFamily: 'var(--font-mono)', color: '#818cf8' }}>{d.code}</td>
                      <td style={{ padding: '0.85rem 1rem', fontWeight: 600 }}>{d.title}</td>
                      <td style={{ padding: '0.85rem 1rem', color: 'var(--text-muted)' }}>{d.departmentName}</td>
                      <td style={{ padding: '0.85rem 1rem' }}>
                        <button onClick={() => handleDelete('designations', d.id)} style={{ background: 'none', border: 'none', color: '#f43f5e', cursor: 'pointer' }}>
                          <Trash2 size={16} />
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}

            {/* GRADES */}
            {activeTab === 'grades' && (
              <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', fontSize: '0.875rem' }}>
                <thead>
                  <tr style={{ background: 'rgba(255, 255, 255, 0.02)', borderBottom: '1px solid var(--border-subtle)', color: 'var(--text-muted)' }}>
                    <th style={{ padding: '0.85rem 1rem' }}>Code</th>
                    <th style={{ padding: '0.85rem 1rem' }}>Grade Name</th>
                    <th style={{ padding: '0.85rem 1rem' }}>Salary Band</th>
                    <th style={{ padding: '0.85rem 1rem' }}>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {grades.map((g) => (
                    <tr key={g.id} style={{ borderBottom: '1px solid var(--border-subtle)' }}>
                      <td style={{ padding: '0.85rem 1rem', fontFamily: 'var(--font-mono)', color: '#818cf8' }}>{g.code}</td>
                      <td style={{ padding: '0.85rem 1rem', fontWeight: 600 }}>{g.name}</td>
                      <td style={{ padding: '0.85rem 1rem', color: '#34d399', fontWeight: 600 }}>
                        ${g.minSalary?.toLocaleString()} - ${g.maxSalary?.toLocaleString()}
                      </td>
                      <td style={{ padding: '0.85rem 1rem' }}>
                        <button onClick={() => handleDelete('grades', g.id)} style={{ background: 'none', border: 'none', color: '#f43f5e', cursor: 'pointer' }}>
                          <Trash2 size={16} />
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}

            {/* SHIFTS */}
            {activeTab === 'shifts' && (
              <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', fontSize: '0.875rem' }}>
                <thead>
                  <tr style={{ background: 'rgba(255, 255, 255, 0.02)', borderBottom: '1px solid var(--border-subtle)', color: 'var(--text-muted)' }}>
                    <th style={{ padding: '0.85rem 1rem' }}>Code</th>
                    <th style={{ padding: '0.85rem 1rem' }}>Shift Name</th>
                    <th style={{ padding: '0.85rem 1rem' }}>Working Hours</th>
                    <th style={{ padding: '0.85rem 1rem' }}>Grace Period</th>
                    <th style={{ padding: '0.85rem 1rem' }}>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {shifts.map((s) => (
                    <tr key={s.id} style={{ borderBottom: '1px solid var(--border-subtle)' }}>
                      <td style={{ padding: '0.85rem 1rem', fontFamily: 'var(--font-mono)', color: '#818cf8' }}>{s.code}</td>
                      <td style={{ padding: '0.85rem 1rem', fontWeight: 600 }}>{s.name}</td>
                      <td style={{ padding: '0.85rem 1rem', color: 'var(--text-muted)' }}>{s.startTime} - {s.endTime} ({s.workingHours}h)</td>
                      <td style={{ padding: '0.85rem 1rem', color: '#fbbf24' }}>{s.graceMinutes} mins</td>
                      <td style={{ padding: '0.85rem 1rem' }}>
                        <button onClick={() => handleDelete('shifts', s.id)} style={{ background: 'none', border: 'none', color: '#f43f5e', cursor: 'pointer' }}>
                          <Trash2 size={16} />
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}

            {/* HOLIDAYS */}
            {activeTab === 'holidays' && (
              <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', fontSize: '0.875rem' }}>
                <thead>
                  <tr style={{ background: 'rgba(255, 255, 255, 0.02)', borderBottom: '1px solid var(--border-subtle)', color: 'var(--text-muted)' }}>
                    <th style={{ padding: '0.85rem 1rem' }}>Holiday</th>
                    <th style={{ padding: '0.85rem 1rem' }}>Date</th>
                    <th style={{ padding: '0.85rem 1rem' }}>Type</th>
                    <th style={{ padding: '0.85rem 1rem' }}>Scope</th>
                    <th style={{ padding: '0.85rem 1rem' }}>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {holidays.map((h) => (
                    <tr key={h.id} style={{ borderBottom: '1px solid var(--border-subtle)' }}>
                      <td style={{ padding: '0.85rem 1rem', fontWeight: 600 }}>{h.name}</td>
                      <td style={{ padding: '0.85rem 1rem', fontFamily: 'var(--font-mono)', color: '#38bdf8' }}>{h.holidayDate}</td>
                      <td style={{ padding: '0.85rem 1rem' }}>{h.type}</td>
                      <td style={{ padding: '0.85rem 1rem', color: 'var(--text-muted)' }}>{h.branchName || 'Company-wide'}</td>
                      <td style={{ padding: '0.85rem 1rem' }}>
                        <button onClick={() => handleDelete('holidays', h.id)} style={{ background: 'none', border: 'none', color: '#f43f5e', cursor: 'pointer' }}>
                          <Trash2 size={16} />
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>
        )}
      </div>

      {/* CREATE MODAL */}
      {showModal && (
        <div style={{
          position: 'fixed',
          inset: 0,
          background: 'rgba(0, 0, 0, 0.7)',
          backdropFilter: 'blur(8px)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          zIndex: 100,
          padding: '1rem'
        }}>
          <div className="glass-card" style={{ maxWidth: 480 }}>
            <h3 style={{ fontSize: '1.25rem', fontWeight: 700, marginBottom: '1rem' }}>
              Add New {activeTab.slice(0, -1).toUpperCase()}
            </h3>

            <form onSubmit={handleCreate}>
              {activeTab === 'branches' && (
                <>
                  <div className="form-group">
                    <label className="form-label">Branch Name</label>
                    <input className="input-field" required value={newBranch.name} onChange={e => setNewBranch({ ...newBranch, name: e.target.value })} placeholder="e.g. Chittagong Branch" />
                  </div>
                  <div className="form-group">
                    <label className="form-label">Code</label>
                    <input className="input-field" required value={newBranch.code} onChange={e => setNewBranch({ ...newBranch, code: e.target.value })} placeholder="e.g. CTG-01" />
                  </div>
                </>
              )}

              {activeTab === 'departments' && (
                <>
                  <div className="form-group">
                    <label className="form-label">Department Name</label>
                    <input className="input-field" required value={newDept.name} onChange={e => setNewDept({ ...newDept, name: e.target.value })} placeholder="e.g. Human Resources" />
                  </div>
                  <div className="form-group">
                    <label className="form-label">Code</label>
                    <input className="input-field" required value={newDept.code} onChange={e => setNewDept({ ...newDept, code: e.target.value })} placeholder="e.g. HR-01" />
                  </div>
                </>
              )}

              {activeTab === 'designations' && (
                <>
                  <div className="form-group">
                    <label className="form-label">Designation Title</label>
                    <input className="input-field" required value={newDesig.title} onChange={e => setNewDesig({ ...newDesig, title: e.target.value })} placeholder="e.g. Senior Talent Acquisition Specialist" />
                  </div>
                  <div className="form-group">
                    <label className="form-label">Code</label>
                    <input className="input-field" required value={newDesig.code} onChange={e => setNewDesig({ ...newDesig, code: e.target.value })} placeholder="e.g. TAS-02" />
                  </div>
                </>
              )}

              {activeTab === 'grades' && (
                <>
                  <div className="form-group">
                    <label className="form-label">Grade Name</label>
                    <input className="input-field" required value={newGrade.name} onChange={e => setNewGrade({ ...newGrade, name: e.target.value })} placeholder="e.g. Band 2 - Officer" />
                  </div>
                  <div className="form-group">
                    <label className="form-label">Code</label>
                    <input className="input-field" required value={newGrade.code} onChange={e => setNewGrade({ ...newGrade, code: e.target.value })} placeholder="e.g. B2-OFF" />
                  </div>
                </>
              )}

              {activeTab === 'shifts' && (
                <>
                  <div className="form-group">
                    <label className="form-label">Shift Name</label>
                    <input className="input-field" required value={newShift.name} onChange={e => setNewShift({ ...newShift, name: e.target.value })} placeholder="e.g. Morning Shift" />
                  </div>
                  <div className="form-group">
                    <label className="form-label">Code</label>
                    <input className="input-field" required value={newShift.code} onChange={e => setNewShift({ ...newShift, code: e.target.value })} placeholder="e.g. MORN-1" />
                  </div>
                </>
              )}

              {activeTab === 'holidays' && (
                <>
                  <div className="form-group">
                    <label className="form-label">Holiday Name</label>
                    <input className="input-field" required value={newHoliday.name} onChange={e => setNewHoliday({ ...newHoliday, name: e.target.value })} placeholder="e.g. Eid-ul-Fitr" />
                  </div>
                  <div className="form-group">
                    <label className="form-label">Date (YYYY-MM-DD)</label>
                    <input className="input-field" type="date" required value={newHoliday.holidayDate} onChange={e => setNewHoliday({ ...newHoliday, holidayDate: e.target.value })} />
                  </div>
                </>
              )}

              <div style={{ display: 'flex', gap: '0.75rem', marginTop: '1.5rem' }}>
                <button type="submit" className="btn-primary" style={{ flex: 1 }}>Save</button>
                <button type="button" onClick={() => setShowModal(false)} className="btn-secondary" style={{ flex: 1 }}>Cancel</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
