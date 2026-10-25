import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { 
  CalendarDays, 
  Plus, 
  CheckCircle2, 
  XCircle, 
  Clock, 
  FileText, 
  ShieldCheck, 
  AlertCircle, 
  Send, 
  X,
  Check,
  Paperclip
} from 'lucide-react';

interface LeaveProps {
  token: string;
  user?: any;
}

const API_BASE = 'http://localhost:5219/api/v1';

export default function Leave({ token, user }: LeaveProps) {
  const isSuperAdmin = user?.roles?.some((r: string) => r.toLowerCase() === 'superadmin') || false;
  const isHRManager = !isSuperAdmin && (user?.roles?.some((r: string) => r.toLowerCase() === 'hrmanager' || r.toLowerCase() === 'admin') || false);
  const isEmployee = !isSuperAdmin && !isHRManager;

  const [activeTab, setActiveTab] = useState<'balances' | 'applications' | 'approvals' | 'policies'>('balances');
  const [balances, setBalances] = useState<any[]>([]);
  const [applications, setApplications] = useState<any[]>([]);
  const [leaveTypes, setLeaveTypes] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);
  const [actionLoading, setActionLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMsg, setSuccessMsg] = useState<string | null>(null);

  // Apply Modal
  const [showApplyModal, setShowApplyModal] = useState(false);
  const [applyForm, setApplyForm] = useState({
    leaveTypeId: 1,
    startDate: new Date().toISOString().split('T')[0],
    endDate: new Date().toISOString().split('T')[0],
    isHalfDay: false,
    halfDayType: 'FirstHalf',
    reason: '',
    attachmentUrl: ''
  });

  // Review Modal
  const [reviewItem, setReviewItem] = useState<any | null>(null);
  const [reviewRemarks, setReviewRemarks] = useState('');

  const headers = { Authorization: `Bearer ${token}` };

  const fetchLeaveTypes = async () => {
    try {
      const res = await axios.get(`${API_BASE}/leave/types`, { headers });
      setLeaveTypes(res.data.data || []);
      if (res.data.data?.length > 0) {
        setApplyForm(prev => ({ ...prev, leaveTypeId: res.data.data[0].id }));
      }
    } catch (err) {
      console.error(err);
    }
  };

  const fetchBalances = async () => {
    setLoading(true);
    try {
      const res = await axios.get(`${API_BASE}/leave/balances`, { headers });
      setBalances(res.data.data || []);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error fetching balances.');
    } finally {
      setLoading(false);
    }
  };

  const fetchApplications = async () => {
    setLoading(true);
    try {
      const res = await axios.get(`${API_BASE}/leave/applications`, {
        headers,
        params: { page: 1, pageSize: 30 }
      });
      setApplications(res.data.data.items || []);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error fetching applications.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchLeaveTypes();
    if (activeTab === 'balances') fetchBalances();
    if (activeTab === 'applications' || activeTab === 'approvals') fetchApplications();
  }, [activeTab]);

  const handleApply = async (e: React.FormEvent) => {
    e.preventDefault();
    setActionLoading(true);
    setError(null);
    try {
      await axios.post(
        `${API_BASE}/leave/applications`,
        {
          leaveTypeId: Number(applyForm.leaveTypeId),
          startDate: applyForm.startDate,
          endDate: applyForm.endDate,
          isHalfDay: applyForm.isHalfDay,
          halfDayType: applyForm.isHalfDay ? applyForm.halfDayType : null,
          reason: applyForm.reason,
          attachmentUrl: applyForm.attachmentUrl || null
        },
        { headers }
      );

      setSuccessMsg('Leave application submitted successfully!');
      setShowApplyModal(false);
      setApplyForm({
        leaveTypeId: leaveTypes[0]?.id || 1,
        startDate: new Date().toISOString().split('T')[0],
        endDate: new Date().toISOString().split('T')[0],
        isHalfDay: false,
        halfDayType: 'FirstHalf',
        reason: '',
        attachmentUrl: ''
      });
      fetchBalances();
      fetchApplications();
      setTimeout(() => setSuccessMsg(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to submit leave application.');
    } finally {
      setActionLoading(false);
    }
  };

  const handleReview = async (status: 'Approved' | 'Rejected') => {
    if (!reviewItem) return;
    setActionLoading(true);
    setError(null);
    try {
      await axios.post(
        `${API_BASE}/leave/applications/${reviewItem.id}/review`,
        { status, remarks: reviewRemarks },
        { headers }
      );

      setSuccessMsg(`Leave application ${status.toLowerCase()}!`);
      setReviewItem(null);
      setReviewRemarks('');
      fetchApplications();
      fetchBalances();
      setTimeout(() => setSuccessMsg(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to review leave application.');
    } finally {
      setActionLoading(false);
    }
  };

  const handleCancelApplication = async (id: number) => {
    if (!confirm('Are you sure you want to cancel this leave application?')) return;
    try {
      await axios.post(`${API_BASE}/leave/applications/${id}/cancel`, {}, { headers });
      setSuccessMsg('Leave application cancelled.');
      fetchApplications();
      fetchBalances();
      setTimeout(() => setSuccessMsg(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to cancel application.');
    }
  };

  return (
    <div style={{ marginTop: '2rem' }}>
      {/* Header */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem', flexWrap: 'wrap', gap: '1rem' }}>
        <div>
          <h2 style={{ fontSize: '1.5rem', fontWeight: 800, letterSpacing: '-0.02em', display: 'flex', alignItems: 'center', gap: '8px' }}>
            <CalendarDays size={24} color="#6366f1" /> Leave Management
          </h2>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.875rem' }}>
            Allocations, balance calculation, multi-level approvals, and policies
          </p>
        </div>

        <button 
          onClick={() => setShowApplyModal(true)}
          className="btn-primary" 
          style={{ width: 'auto', padding: '0.65rem 1.25rem', fontSize: '0.85rem' }}
        >
          <Plus size={16} /> Apply for Leave
        </button>
      </div>

      {/* Tabs */}
      <div style={{ 
        display: 'flex', 
        gap: '0.5rem', 
        borderBottom: '1px solid var(--border-subtle)', 
        paddingBottom: '0.75rem',
        marginBottom: '1.5rem'
      }}>
        {[
          { key: 'balances', label: 'My Balances', icon: ShieldCheck },
          { key: 'applications', label: 'My Applications', icon: FileText },
          ...(!isEmployee ? [
            { key: 'approvals', label: 'Team Approvals', icon: Clock },
            { key: 'policies', label: 'Leave Policies', icon: CalendarDays }
          ] : [])
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
                gap: '6px'
              }}
            >
              <Icon size={16} /> {tab.label}
            </button>
          );
        })}
      </div>

      {/* Feedback Messages */}
      {successMsg && (
        <div style={{ background: 'rgba(16, 185, 129, 0.15)', border: '1px solid #10b981', color: '#34d399', padding: '0.75rem 1rem', borderRadius: 'var(--radius-md)', marginBottom: '1.5rem', display: 'flex', alignItems: 'center', gap: '8px' }}>
          <Check size={18} /> {successMsg}
        </div>
      )}
      {error && (
        <div className="alert-error" style={{ marginBottom: '1.5rem' }}>
          <AlertCircle size={18} /> {error}
        </div>
      )}

      {/* TAB 1: LEAVE BALANCES */}
      {activeTab === 'balances' && (
        <div>
          {loading ? (
            <div style={{ padding: '3rem', textAlign: 'center', color: 'var(--text-muted)' }}>Loading balances...</div>
          ) : (
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(240px, 1fr))', gap: '1.25rem' }}>
              {balances.map((b) => (
                <div
                  key={b.id}
                  style={{
                    background: 'var(--bg-surface)',
                    border: '1px solid var(--border-subtle)',
                    borderRadius: 'var(--radius-lg)',
                    padding: '1.5rem',
                    position: 'relative',
                    overflow: 'hidden'
                  }}
                >
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '1rem' }}>
                    <div>
                      <span style={{ fontSize: '0.75rem', fontWeight: 800, color: '#818cf8', background: 'rgba(99, 102, 241, 0.15)', padding: '0.2rem 0.5rem', borderRadius: 9999 }}>
                        {b.leaveTypeCode}
                      </span>
                      <h4 style={{ fontSize: '1.1rem', fontWeight: 700, marginTop: '0.4rem' }}>{b.leaveTypeName}</h4>
                    </div>
                    <div style={{ fontSize: '2rem', fontWeight: 800, color: b.balance > 0 ? '#34d399' : '#94a3b8' }}>
                      {b.balance}
                    </div>
                  </div>

                  <div style={{ fontSize: '0.8rem', color: 'var(--text-muted)', display: 'flex', justifyContent: 'space-between', borderTop: '1px solid rgba(255,255,255,0.05)', paddingTop: '0.75rem' }}>
                    <span>Total Allocated: <strong>{b.totalDays}d</strong></span>
                    <span>Used: <strong style={{ color: '#fb7185' }}>{b.usedDays}d</strong></span>
                    {b.carryForwardDays > 0 && <span>Carry: <strong>+{b.carryForwardDays}d</strong></span>}
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      )}

      {/* TAB 2: APPLICATIONS LIST */}
      {activeTab === 'applications' && (
        <div style={{ background: 'var(--bg-surface)', border: '1px solid var(--border-subtle)', borderRadius: 'var(--radius-lg)', overflow: 'hidden' }}>
          {loading ? (
            <div style={{ padding: '3rem', textAlign: 'center', color: 'var(--text-muted)' }}>Loading applications...</div>
          ) : applications.length === 0 ? (
            <div style={{ padding: '3rem', textAlign: 'center', color: 'var(--text-muted)' }}>No leave applications found.</div>
          ) : (
            <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', fontSize: '0.875rem' }}>
              <thead>
                <tr style={{ background: 'rgba(255, 255, 255, 0.02)', borderBottom: '1px solid var(--border-subtle)' }}>
                  <th style={{ padding: '0.85rem 1rem', color: 'var(--text-muted)' }}>Dates</th>
                  <th style={{ padding: '0.85rem 1rem', color: 'var(--text-muted)' }}>Leave Type</th>
                  <th style={{ padding: '0.85rem 1rem', color: 'var(--text-muted)' }}>Duration</th>
                  <th style={{ padding: '0.85rem 1rem', color: 'var(--text-muted)' }}>Reason</th>
                  <th style={{ padding: '0.85rem 1rem', color: 'var(--text-muted)' }}>Status</th>
                  <th style={{ padding: '0.85rem 1rem', color: 'var(--text-muted)' }}>Reviewer Notes</th>
                  <th style={{ padding: '0.85rem 1rem', color: 'var(--text-muted)' }}>Action</th>
                </tr>
              </thead>
              <tbody>
                {applications.map((app) => (
                  <tr key={app.id} style={{ borderBottom: '1px solid rgba(255, 255, 255, 0.04)' }}>
                    <td style={{ padding: '0.85rem 1rem', fontWeight: 600 }}>
                      <div>{app.startDate} {app.startDate !== app.endDate ? `to ${app.endDate}` : ''}</div>
                      {app.isHalfDay && <span style={{ fontSize: '0.75rem', color: '#818cf8' }}>({app.halfDayType})</span>}
                    </td>
                    <td style={{ padding: '0.85rem 1rem' }}>
                      <span style={{ fontWeight: 600 }}>{app.leaveTypeName}</span>
                    </td>
                    <td style={{ padding: '0.85rem 1rem', fontWeight: 700 }}>
                      {app.totalDays} day(s)
                    </td>
                    <td style={{ padding: '0.85rem 1rem', maxWidth: 240 }}>
                      <span style={{ fontSize: '0.8125rem' }}>{app.reason}</span>
                    </td>
                    <td style={{ padding: '0.85rem 1rem' }}>
                      <span style={{
                        padding: '0.2rem 0.55rem',
                        borderRadius: 9999,
                        fontSize: '0.75rem',
                        fontWeight: 600,
                        background: app.status === 'Approved' ? 'rgba(16, 185, 129, 0.15)' :
                                    app.status === 'Pending' ? 'rgba(245, 158, 11, 0.15)' :
                                    'rgba(244, 63, 94, 0.15)',
                        color: app.status === 'Approved' ? '#34d399' :
                               app.status === 'Pending' ? '#fbbf24' :
                               '#fb7185'
                      }}>
                        {app.status}
                      </span>
                    </td>
                    <td style={{ padding: '0.85rem 1rem', fontSize: '0.8rem', color: 'var(--text-muted)' }}>
                      {app.approverName && <div>By {app.approverName}</div>}
                      {app.approvalRemarks && <div style={{ fontStyle: 'italic' }}>"{app.approvalRemarks}"</div>}
                    </td>
                    <td style={{ padding: '0.85rem 1rem' }}>
                      {app.status === 'Pending' && (
                        <button
                          onClick={() => handleCancelApplication(app.id)}
                          style={{ background: 'none', border: 'none', color: '#fb7185', cursor: 'pointer', fontSize: '0.75rem' }}
                        >
                          Cancel
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      )}

      {/* TAB 3: TEAM APPROVALS */}
      {activeTab === 'approvals' && (
        <div style={{ background: 'var(--bg-surface)', border: '1px solid var(--border-subtle)', borderRadius: 'var(--radius-lg)', overflow: 'hidden' }}>
          {applications.filter(a => a.status === 'Pending').length === 0 ? (
            <div style={{ padding: '3rem', textAlign: 'center', color: 'var(--text-muted)' }}>No pending leave applications requiring approval.</div>
          ) : (
            <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', fontSize: '0.875rem' }}>
              <thead>
                <tr style={{ background: 'rgba(255, 255, 255, 0.02)', borderBottom: '1px solid var(--border-subtle)' }}>
                  <th style={{ padding: '0.85rem 1rem', color: 'var(--text-muted)' }}>Employee</th>
                  <th style={{ padding: '0.85rem 1rem', color: 'var(--text-muted)' }}>Dates</th>
                  <th style={{ padding: '0.85rem 1rem', color: 'var(--text-muted)' }}>Leave Type</th>
                  <th style={{ padding: '0.85rem 1rem', color: 'var(--text-muted)' }}>Duration</th>
                  <th style={{ padding: '0.85rem 1rem', color: 'var(--text-muted)' }}>Reason</th>
                  <th style={{ padding: '0.85rem 1rem', color: 'var(--text-muted)' }}>Actions</th>
                </tr>
              </thead>
              <tbody>
                {applications.filter(a => a.status === 'Pending').map((app) => (
                  <tr key={app.id} style={{ borderBottom: '1px solid rgba(255, 255, 255, 0.04)' }}>
                    <td style={{ padding: '0.85rem 1rem' }}>
                      <div style={{ fontWeight: 600 }}>{app.employeeName}</div>
                      <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>{app.employeeCode} • {app.departmentName}</div>
                    </td>
                    <td style={{ padding: '0.85rem 1rem', fontWeight: 600 }}>
                      {app.startDate} to {app.endDate}
                    </td>
                    <td style={{ padding: '0.85rem 1rem' }}>
                      <span style={{ fontWeight: 600 }}>{app.leaveTypeName}</span>
                    </td>
                    <td style={{ padding: '0.85rem 1rem', fontWeight: 700 }}>
                      {app.totalDays} day(s)
                    </td>
                    <td style={{ padding: '0.85rem 1rem', maxWidth: 220 }}>
                      <span style={{ fontSize: '0.8125rem' }}>{app.reason}</span>
                    </td>
                    <td style={{ padding: '0.85rem 1rem' }}>
                      <button
                        onClick={() => setReviewItem(app)}
                        className="btn-secondary"
                        style={{ padding: '0.4rem 0.85rem', fontSize: '0.75rem' }}
                      >
                        Review
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      )}

      {/* TAB 4: LEAVE POLICIES */}
      {activeTab === 'policies' && (
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(260px, 1fr))', gap: '1.25rem' }}>
          {leaveTypes.map((t) => (
            <div
              key={t.id}
              style={{
                background: 'var(--bg-surface)',
                border: '1px solid var(--border-subtle)',
                borderRadius: 'var(--radius-lg)',
                padding: '1.5rem'
              }}
            >
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.75rem' }}>
                <span style={{ fontSize: '0.75rem', fontWeight: 800, color: '#818cf8', background: 'rgba(99, 102, 241, 0.15)', padding: '0.2rem 0.5rem', borderRadius: 9999 }}>
                  {t.code}
                </span>
                <span style={{ fontSize: '0.8rem', color: t.isPaidLeave ? '#34d399' : '#fb7185', fontWeight: 600 }}>
                  {t.isPaidLeave ? 'Paid' : 'Unpaid'}
                </span>
              </div>

              <h4 style={{ fontSize: '1.1rem', fontWeight: 700, marginBottom: '0.5rem' }}>{t.name}</h4>
              <div style={{ fontSize: '2rem', fontWeight: 800, color: '#f8fafc', marginBottom: '1rem' }}>
                {t.daysAllowed} <span style={{ fontSize: '1rem', color: 'var(--text-muted)' }}>days/yr</span>
              </div>

              <div style={{ fontSize: '0.8rem', color: 'var(--text-muted)', display: 'flex', flexDirection: 'column', gap: '0.4rem', borderTop: '1px solid rgba(255,255,255,0.05)', paddingTop: '0.75rem' }}>
                <div>Carry Forward: <strong>{t.isCarryForwardAllowed ? `Yes (Max ${t.maxCarryForwardDays}d)` : 'No'}</strong></div>
                <div>Half-Day: <strong>{t.isHalfDayAllowed ? 'Allowed' : 'Not Allowed'}</strong></div>
                <div>Attachment: <strong>{t.isAttachmentRequired ? `Required (> ${t.attachmentRequiredAfterDays}d)` : 'Not Required'}</strong></div>
                {t.genderRestriction && <div>Gender: <strong style={{ color: '#ec4899' }}>{t.genderRestriction} Only</strong></div>}
              </div>
            </div>
          ))}
        </div>
      )}

      {/* MODAL 1: APPLY LEAVE */}
      {showApplyModal && (
        <div style={{ position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.7)', backdropFilter: 'blur(4px)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 50, padding: '1rem' }}>
          <div style={{ background: 'var(--bg-surface)', border: '1px solid var(--border-subtle)', borderRadius: 'var(--radius-lg)', padding: '1.5rem', width: '100%', maxWidth: 480 }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.25rem' }}>
              <h3 style={{ fontSize: '1.1rem', fontWeight: 700, display: 'flex', alignItems: 'center', gap: '8px' }}>
                <CalendarDays size={18} color="#6366f1" /> Submit Leave Application
              </h3>
              <button onClick={() => setShowApplyModal(false)} style={{ background: 'none', border: 'none', color: 'var(--text-muted)', cursor: 'pointer' }}>
                <X size={18} />
              </button>
            </div>

            <form onSubmit={handleApply}>
              <div className="form-group">
                <label className="form-label">Leave Type</label>
                <select
                  className="input-field"
                  value={applyForm.leaveTypeId}
                  onChange={e => setApplyForm({ ...applyForm, leaveTypeId: Number(e.target.value) })}
                >
                  {leaveTypes.map(t => (
                    <option key={t.id} value={t.id}>{t.name} ({t.code}) - {t.daysAllowed} days</option>
                  ))}
                </select>
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
                <div className="form-group">
                  <label className="form-label">Start Date</label>
                  <input
                    type="date"
                    className="input-field"
                    required
                    value={applyForm.startDate}
                    onChange={e => setApplyForm({ ...applyForm, startDate: e.target.value })}
                  />
                </div>
                <div className="form-group">
                  <label className="form-label">End Date</label>
                  <input
                    type="date"
                    className="input-field"
                    required
                    disabled={applyForm.isHalfDay}
                    value={applyForm.isHalfDay ? applyForm.startDate : applyForm.endDate}
                    onChange={e => setApplyForm({ ...applyForm, endDate: e.target.value })}
                  />
                </div>
              </div>

              {/* Half-day toggle */}
              <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', marginBottom: '1rem' }}>
                <label style={{ display: 'flex', alignItems: 'center', gap: '6px', fontSize: '0.85rem', cursor: 'pointer' }}>
                  <input
                    type="checkbox"
                    checked={applyForm.isHalfDay}
                    onChange={e => setApplyForm({ ...applyForm, isHalfDay: e.target.checked })}
                  />
                  <span>Apply for Half-Day (0.5 day)</span>
                </label>

                {applyForm.isHalfDay && (
                  <select
                    className="input-field"
                    style={{ width: 'auto', padding: '0.4rem 0.75rem', fontSize: '0.8rem' }}
                    value={applyForm.halfDayType}
                    onChange={e => setApplyForm({ ...applyForm, halfDayType: e.target.value })}
                  >
                    <option value="FirstHalf">First Half</option>
                    <option value="SecondHalf">Second Half</option>
                  </select>
                )}
              </div>

              <div className="form-group">
                <label className="form-label">Reason / Purpose</label>
                <textarea
                  className="input-field"
                  rows={3}
                  required
                  placeholder="Detail your reason for leave..."
                  value={applyForm.reason}
                  onChange={e => setApplyForm({ ...applyForm, reason: e.target.value })}
                />
              </div>

              <div className="form-group">
                <label className="form-label">Supporting Document URL (Optional)</label>
                <div className="input-wrapper">
                  <Paperclip size={18} className="input-icon" />
                  <input
                    className="input-field"
                    placeholder="https://... medical certificate or ticket"
                    value={applyForm.attachmentUrl}
                    onChange={e => setApplyForm({ ...applyForm, attachmentUrl: e.target.value })}
                  />
                </div>
              </div>

              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '0.75rem', marginTop: '1.5rem' }}>
                <button type="button" onClick={() => setShowApplyModal(false)} className="btn-secondary">Cancel</button>
                <button type="submit" disabled={actionLoading} className="btn-primary" style={{ width: 'auto', display: 'flex', alignItems: 'center', gap: '6px' }}>
                  <Send size={16} /> Submit Application
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* MODAL 2: REVIEW LEAVE */}
      {reviewItem && (
        <div style={{ position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.7)', backdropFilter: 'blur(4px)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 50, padding: '1rem' }}>
          <div style={{ background: 'var(--bg-surface)', border: '1px solid var(--border-subtle)', borderRadius: 'var(--radius-lg)', padding: '1.5rem', width: '100%', maxWidth: 480 }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.25rem' }}>
              <h3 style={{ fontSize: '1.1rem', fontWeight: 700 }}>Review Leave Application</h3>
              <button onClick={() => setReviewItem(null)} style={{ background: 'none', border: 'none', color: 'var(--text-muted)', cursor: 'pointer' }}>
                <X size={18} />
              </button>
            </div>

            <div style={{ background: 'var(--bg-surface-elevated)', padding: '1rem', borderRadius: 'var(--radius-md)', marginBottom: '1.25rem', fontSize: '0.85rem' }}>
              <div>Employee: <strong>{reviewItem.employeeName} ({reviewItem.employeeCode})</strong></div>
              <div>Type: <strong>{reviewItem.leaveTypeName}</strong> ({reviewItem.totalDays} day(s))</div>
              <div>Duration: <strong>{reviewItem.startDate} to {reviewItem.endDate}</strong></div>
              <div style={{ marginTop: '0.5rem', color: 'var(--text-muted)' }}>Reason: "{reviewItem.reason}"</div>
            </div>

            <div className="form-group">
              <label className="form-label">Review Remarks</label>
              <textarea
                className="input-field"
                rows={2}
                placeholder="Optional approval/rejection comments..."
                value={reviewRemarks}
                onChange={e => setReviewRemarks(e.target.value)}
              />
            </div>

            <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '0.75rem', marginTop: '1.5rem' }}>
              <button
                type="button"
                onClick={() => handleReview('Rejected')}
                disabled={actionLoading}
                className="btn-secondary"
                style={{ color: '#fb7185', borderColor: '#fb718533', display: 'flex', alignItems: 'center', gap: '6px' }}
              >
                <XCircle size={16} /> Reject
              </button>
              <button
                type="button"
                onClick={() => handleReview('Approved')}
                disabled={actionLoading}
                className="btn-primary"
                style={{ width: 'auto', background: 'linear-gradient(135deg, #10b981, #059669)', display: 'flex', alignItems: 'center', gap: '6px' }}
              >
                <CheckCircle2 size={16} /> Approve &amp; Deduct Balance
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
