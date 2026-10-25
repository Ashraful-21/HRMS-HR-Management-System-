import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { 
  Clock, 
  LogIn, 
  LogOut, 
  Calendar, 
  AlertTriangle, 
  CheckCircle2, 
  XCircle, 
  FileText, 
  Plus, 
  Check, 
  X, 
  Send
} from 'lucide-react';

interface AttendanceProps {
  token: string;
  user?: any;
}

const API_BASE = 'http://localhost:5219/api/v1';

export default function Attendance({ token, user }: AttendanceProps) {
  const isSuperAdmin = user?.roles?.some((r: string) => r.toLowerCase() === 'superadmin') || false;
  const isHRManager = !isSuperAdmin && (user?.roles?.some((r: string) => r.toLowerCase() === 'hrmanager' || r.toLowerCase() === 'admin') || false);
  const isEmployee = !isSuperAdmin && !isHRManager;
  const canReview = isSuperAdmin || isHRManager;

  const [activeTab, setActiveTab] = useState<'clock' | 'history' | 'regularization'>('clock');
  const [todayStatus, setTodayStatus] = useState<any | null>(null);
  const [monthlySummary, setMonthlySummary] = useState<any | null>(null);
  const [logs, setLogs] = useState<any[]>([]);
  const [regularizations, setRegularizations] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);
  const [actionLoading, setActionLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMsg, setSuccessMsg] = useState<string | null>(null);

  // Live Clock
  const [currentTime, setCurrentTime] = useState(new Date());

  // Regularization Modal
  const [showRegModal, setShowRegModal] = useState(false);
  const [regForm, setRegForm] = useState({
    logDate: new Date().toISOString().split('T')[0],
    requestedCheckIn: '09:00',
    requestedCheckOut: '18:00',
    reason: ''
  });

  // Review Modal
  const [reviewItem, setReviewItem] = useState<any | null>(null);
  const [reviewRemarks, setReviewRemarks] = useState('');

  const headers = { Authorization: `Bearer ${token}` };

  // Live timer tick
  useEffect(() => {
    const timer = setInterval(() => setCurrentTime(new Date()), 1000);
    return () => clearInterval(timer);
  }, []);

  const fetchTodayStatus = async () => {
    try {
      const res = await axios.get(`${API_BASE}/attendance/today`, { headers });
      setTodayStatus(res.data.data);
      if (res.data.data?.employeeId) {
        fetchMonthlySummary(res.data.data.employeeId);
      }
    } catch (err: any) {
      console.error(err);
    }
  };

  const fetchMonthlySummary = async (empId: number) => {
    try {
      const now = new Date();
      const res = await axios.get(`${API_BASE}/attendance/summary`, {
        headers,
        params: { employeeId: empId, year: now.getFullYear(), month: now.getMonth() + 1 }
      });
      setMonthlySummary(res.data.data);
    } catch (err) {
      console.error(err);
    }
  };

  const fetchLogs = async () => {
    setLoading(true);
    try {
      const params: any = { page: 1, pageSize: 30 };
      if (isEmployee && user?.employeeId) {
        params.employeeId = user.employeeId;
      }
      const res = await axios.get(`${API_BASE}/attendance`, {
        headers,
        params
      });
      setLogs(res.data.data.items || []);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error fetching attendance logs.');
    } finally {
      setLoading(false);
    }
  };

  const fetchRegularizations = async () => {
    setLoading(true);
    try {
      const params: any = { page: 1, pageSize: 30 };
      if (isEmployee && user?.employeeId) {
        params.employeeId = user.employeeId;
      }
      const res = await axios.get(`${API_BASE}/attendance/regularize`, {
        headers,
        params
      });
      setRegularizations(res.data.data.items || []);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error fetching regularizations.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchTodayStatus();
    if (activeTab === 'history') fetchLogs();
    if (activeTab === 'regularization') fetchRegularizations();
  }, [activeTab]);

  const handleCheckIn = async () => {
    setActionLoading(true);
    setError(null);
    try {
      let lat: number | undefined;
      let lng: number | undefined;

      if (navigator.geolocation) {
        try {
          const pos: any = await new Promise((res, rej) =>
            navigator.geolocation.getCurrentPosition(res, rej, { timeout: 3000 })
          );
          lat = pos.coords.latitude;
          lng = pos.coords.longitude;
        } catch {
          // Geolocation optional fallback
        }
      }

      await axios.post(
        `${API_BASE}/attendance/check-in`,
        { latitude: lat, longitude: lng, remarks: 'Web portal check-in' },
        { headers }
      );

      setSuccessMsg('Check-in recorded successfully!');
      fetchTodayStatus();
      setTimeout(() => setSuccessMsg(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to check in.');
    } finally {
      setActionLoading(false);
    }
  };

  const handleCheckOut = async () => {
    setActionLoading(true);
    setError(null);
    try {
      await axios.post(
        `${API_BASE}/attendance/check-out`,
        { remarks: 'Web portal check-out' },
        { headers }
      );

      setSuccessMsg('Check-out recorded successfully!');
      fetchTodayStatus();
      setTimeout(() => setSuccessMsg(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to check out.');
    } finally {
      setActionLoading(false);
    }
  };

  const handleRequestRegularization = async (e: React.FormEvent) => {
    e.preventDefault();
    setActionLoading(true);
    setError(null);
    try {
      const checkInIso = `${regForm.logDate}T${regForm.requestedCheckIn}:00Z`;
      const checkOutIso = `${regForm.logDate}T${regForm.requestedCheckOut}:00Z`;

      await axios.post(
        `${API_BASE}/attendance/regularize`,
        {
          logDate: regForm.logDate,
          requestedCheckIn: checkInIso,
          requestedCheckOut: checkOutIso,
          reason: regForm.reason
        },
        { headers }
      );

      setSuccessMsg('Regularization request submitted for review!');
      setShowRegModal(false);
      setRegForm({
        logDate: new Date().toISOString().split('T')[0],
        requestedCheckIn: '09:00',
        requestedCheckOut: '18:00',
        reason: ''
      });
      fetchRegularizations();
      setTimeout(() => setSuccessMsg(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to submit regularization.');
    } finally {
      setActionLoading(false);
    }
  };

  const handleReviewRegularization = async (status: 'Approved' | 'Rejected') => {
    if (!reviewItem) return;
    setActionLoading(true);
    setError(null);
    try {
      await axios.post(
        `${API_BASE}/attendance/regularize/${reviewItem.id}/review`,
        { status, remarks: reviewRemarks },
        { headers }
      );

      setSuccessMsg(`Regularization request ${status.toLowerCase()}!`);
      setReviewItem(null);
      setReviewRemarks('');
      fetchRegularizations();
      fetchTodayStatus();
      setTimeout(() => setSuccessMsg(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to review request.');
    } finally {
      setActionLoading(false);
    }
  };

  return (
    <div style={{ marginTop: '2rem' }}>
      {/* Header */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem', flexWrap: 'wrap', gap: '1rem' }}>
        <div>
          <h2 style={{ fontSize: '1.5rem', fontWeight: 800, letterSpacing: '-0.02em', display: 'flex', alignItems: 'center', gap: '8px' }}>
            <Clock size={24} color="#6366f1" /> Attendance & Shifts
          </h2>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.875rem' }}>
            Digital punch clock, shift adherence, overtime tracking, and regularizations
          </p>
        </div>

        <div style={{ display: 'flex', gap: '0.75rem' }}>
          <button 
            onClick={() => setShowRegModal(true)}
            className="btn-primary" 
            style={{ width: 'auto', padding: '0.65rem 1.25rem', fontSize: '0.85rem' }}
          >
            <Plus size={16} /> Request Regularization
          </button>
        </div>
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
          { key: 'clock', label: isEmployee ? 'My Punch Clock' : 'Live Punch Clock', icon: Clock },
          { key: 'history', label: isEmployee ? 'My Attendance History' : 'Attendance Records', icon: Calendar },
          { key: 'regularization', label: isEmployee ? 'My Regularizations' : 'Regularization Workflow', icon: FileText }
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

      {/* Messages */}
      {successMsg && (
        <div style={{ background: 'rgba(16, 185, 129, 0.15)', border: '1px solid #10b981', color: '#34d399', padding: '0.75rem 1rem', borderRadius: 'var(--radius-md)', marginBottom: '1.5rem', display: 'flex', alignItems: 'center', gap: '8px' }}>
          <Check size={18} /> {successMsg}
        </div>
      )}
      {error && (
        <div className="alert-error" style={{ marginBottom: '1.5rem' }}>
          <AlertTriangle size={18} /> {error}
        </div>
      )}

      {/* TAB 1: LIVE PUNCH CLOCK */}
      {activeTab === 'clock' && (
        <div>
          {/* Punch Widget Card */}
          <div style={{
            background: 'var(--bg-surface)',
            border: '1px solid var(--border-subtle)',
            borderRadius: 'var(--radius-lg)',
            padding: '2rem',
            marginBottom: '2rem',
            display: 'grid',
            gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))',
            gap: '2rem',
            alignItems: 'center'
          }}>
            {/* Clock display */}
            <div>
              <span style={{ fontSize: '0.8rem', textTransform: 'uppercase', letterSpacing: '0.05em', color: 'var(--text-muted)', fontWeight: 700 }}>
                System Live Clock (UTC)
              </span>
              <div style={{ fontSize: '3rem', fontWeight: 800, fontFamily: 'var(--font-mono)', letterSpacing: '-0.03em', color: '#f8fafc', margin: '0.5rem 0' }}>
                {currentTime.toLocaleTimeString()}
              </div>
              <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem' }}>
                {currentTime.toLocaleDateString(undefined, { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })}
              </p>

              {/* Shift info badge */}
              <div style={{ marginTop: '1.25rem', display: 'flex', gap: '0.75rem', flexWrap: 'wrap' }}>
                <div style={{ background: 'rgba(99, 102, 241, 0.15)', padding: '0.4rem 0.8rem', borderRadius: 9999, fontSize: '0.8rem', color: '#a5b4fc', border: '1px solid rgba(99, 102, 241, 0.3)' }}>
                  Shift: <strong>{todayStatus?.shiftName || 'Standard Office'}</strong> ({todayStatus?.shiftStartTime || '09:00'} - {todayStatus?.shiftEndTime || '18:00'})
                </div>
                <div style={{ background: 'rgba(255, 255, 255, 0.05)', padding: '0.4rem 0.8rem', borderRadius: 9999, fontSize: '0.8rem', color: '#94a3b8' }}>
                  Grace Period: <strong>{todayStatus?.shiftGraceMinutes || 15} mins</strong>
                </div>
              </div>
            </div>

            {/* Action buttons */}
            <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem', justifyContent: 'center' }}>
              <div style={{ display: 'flex', gap: '1rem' }}>
                <button
                  onClick={handleCheckIn}
                  disabled={todayStatus?.hasCheckedIn || actionLoading}
                  className="btn-primary"
                  style={{
                    padding: '1.1rem 1.75rem',
                    fontSize: '1rem',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    gap: '10px',
                    opacity: todayStatus?.hasCheckedIn ? 0.5 : 1,
                    cursor: todayStatus?.hasCheckedIn ? 'not-allowed' : 'pointer',
                    background: todayStatus?.hasCheckedIn ? 'rgba(255,255,255,0.08)' : 'linear-gradient(135deg, #10b981, #059669)'
                  }}
                >
                  <LogIn size={20} />
                  <span>{todayStatus?.hasCheckedIn ? 'Already Checked In' : 'Web Check-In'}</span>
                </button>

                <button
                  onClick={handleCheckOut}
                  disabled={!todayStatus?.hasCheckedIn || todayStatus?.hasCheckedOut || actionLoading}
                  className="btn-primary"
                  style={{
                    padding: '1.1rem 1.75rem',
                    fontSize: '1rem',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    gap: '10px',
                    opacity: (!todayStatus?.hasCheckedIn || todayStatus?.hasCheckedOut) ? 0.5 : 1,
                    cursor: (!todayStatus?.hasCheckedIn || todayStatus?.hasCheckedOut) ? 'not-allowed' : 'pointer',
                    background: (!todayStatus?.hasCheckedIn || todayStatus?.hasCheckedOut) ? 'rgba(255,255,255,0.08)' : 'linear-gradient(135deg, #f43f5e, #e11d48)'
                  }}
                >
                  <LogOut size={20} />
                  <span>{todayStatus?.hasCheckedOut ? 'Already Checked Out' : 'Web Check-Out'}</span>
                </button>
              </div>

              {/* Current status pill */}
              <div style={{ background: 'var(--bg-surface-elevated)', padding: '0.85rem 1.25rem', borderRadius: 'var(--radius-md)', border: '1px solid var(--border-subtle)', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <span style={{ fontSize: '0.85rem', color: 'var(--text-muted)' }}>Status Today:</span>
                <span style={{
                  padding: '0.25rem 0.75rem',
                  borderRadius: 9999,
                  fontSize: '0.85rem',
                  fontWeight: 700,
                  background: todayStatus?.hasCheckedIn ? 'rgba(16, 185, 129, 0.2)' : 'rgba(244, 63, 94, 0.2)',
                  color: todayStatus?.hasCheckedIn ? '#34d399' : '#fb7185'
                }}>
                  {todayStatus?.status || 'Not Checked In'}
                </span>
              </div>

              {todayStatus?.hasCheckedIn && (
                <div style={{ fontSize: '0.8rem', color: 'var(--text-muted)', display: 'flex', justifyContent: 'space-between' }}>
                  <span>In: <strong>{new Date(todayStatus.checkInTime).toLocaleTimeString()}</strong></span>
                  {todayStatus.hasCheckedOut && (
                    <span>Out: <strong>{new Date(todayStatus.checkOutTime).toLocaleTimeString()}</strong></span>
                  )}
                  <span>Working: <strong>{todayStatus.workingMinutes} mins</strong></span>
                </div>
              )}
            </div>
          </div>

          {/* Monthly Statistics Overview */}
          <h3 style={{ fontSize: '1.1rem', fontWeight: 700, marginBottom: '1rem', display: 'flex', alignItems: 'center', gap: '8px' }}>
            <Calendar size={18} color="#6366f1" /> Monthly Overview ({new Date().toLocaleString('default', { month: 'long', year: 'numeric' })})
          </h3>

          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(160px, 1fr))', gap: '1rem' }}>
            {[
              { label: 'Present Days', value: monthlySummary?.presentDays || 0, color: '#34d399', bg: 'rgba(16, 185, 129, 0.12)' },
              { label: 'Late Arrivals', value: monthlySummary?.lateDays || 0, color: '#fbbf24', bg: 'rgba(245, 158, 11, 0.12)' },
              { label: 'Early Leaves', value: monthlySummary?.earlyLeaveDays || 0, color: '#f43f5e', bg: 'rgba(244, 63, 94, 0.12)' },
              { label: 'Half Days', value: monthlySummary?.halfDays || 0, color: '#c084fc', bg: 'rgba(192, 132, 252, 0.12)' },
              { label: 'Absent Days', value: monthlySummary?.absentDays || 0, color: '#fb7185', bg: 'rgba(251, 113, 133, 0.12)' },
              { label: 'Total Hours', value: `${((monthlySummary?.totalWorkingMinutes || 0) / 60).toFixed(1)}h`, color: '#60a5fa', bg: 'rgba(96, 165, 250, 0.12)' }
            ].map((stat, idx) => (
              <div key={idx} style={{ background: stat.bg, border: `1px solid ${stat.color}33`, borderRadius: 'var(--radius-md)', padding: '1.25rem', textAlign: 'center' }}>
                <div style={{ fontSize: '1.75rem', fontWeight: 800, color: stat.color }}>{stat.value}</div>
                <div style={{ fontSize: '0.8rem', color: 'var(--text-muted)', marginTop: '0.25rem' }}>{stat.label}</div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* TAB 2: ATTENDANCE HISTORY LOGS */}
      {activeTab === 'history' && (
        <div style={{ background: 'var(--bg-surface)', border: '1px solid var(--border-subtle)', borderRadius: 'var(--radius-lg)', overflow: 'hidden' }}>
          {loading ? (
            <div style={{ padding: '3rem', textAlign: 'center', color: 'var(--text-muted)' }}>Loading attendance history...</div>
          ) : logs.length === 0 ? (
            <div style={{ padding: '3rem', textAlign: 'center', color: 'var(--text-muted)' }}>No attendance records found.</div>
          ) : (
            <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', fontSize: '0.875rem' }}>
              <thead>
                <tr style={{ background: 'rgba(255, 255, 255, 0.02)', borderBottom: '1px solid var(--border-subtle)' }}>
                  <th style={{ padding: '0.85rem 1rem', color: 'var(--text-muted)' }}>Date</th>
                  <th style={{ padding: '0.85rem 1rem', color: 'var(--text-muted)' }}>Employee</th>
                  <th style={{ padding: '0.85rem 1rem', color: 'var(--text-muted)' }}>Shift</th>
                  <th style={{ padding: '0.85rem 1rem', color: 'var(--text-muted)' }}>Check-In</th>
                  <th style={{ padding: '0.85rem 1rem', color: 'var(--text-muted)' }}>Check-Out</th>
                  <th style={{ padding: '0.85rem 1rem', color: 'var(--text-muted)' }}>Status</th>
                  <th style={{ padding: '0.85rem 1rem', color: 'var(--text-muted)' }}>Late / Early</th>
                  <th style={{ padding: '0.85rem 1rem', color: 'var(--text-muted)' }}>Working Time</th>
                </tr>
              </thead>
              <tbody>
                {logs.map((log) => (
                  <tr key={log.id} style={{ borderBottom: '1px solid rgba(255, 255, 255, 0.04)' }}>
                    <td style={{ padding: '0.85rem 1rem', fontWeight: 600 }}>{log.logDate}</td>
                    <td style={{ padding: '0.85rem 1rem' }}>
                      <div style={{ fontWeight: 600 }}>{log.employeeName}</div>
                      <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>{log.employeeCode}</div>
                    </td>
                    <td style={{ padding: '0.85rem 1rem', color: 'var(--text-muted)' }}>{log.shiftName || 'Standard'}</td>
                    <td style={{ padding: '0.85rem 1rem' }}>
                      {log.checkInTime ? new Date(log.checkInTime).toLocaleTimeString() : '—'}
                    </td>
                    <td style={{ padding: '0.85rem 1rem' }}>
                      {log.checkOutTime ? new Date(log.checkOutTime).toLocaleTimeString() : '—'}
                    </td>
                    <td style={{ padding: '0.85rem 1rem' }}>
                      <span style={{
                        padding: '0.2rem 0.55rem',
                        borderRadius: 9999,
                        fontSize: '0.75rem',
                        fontWeight: 600,
                        background: log.status === 'Present' ? 'rgba(16, 185, 129, 0.15)' :
                                    log.status === 'Late' ? 'rgba(245, 158, 11, 0.15)' :
                                    log.status === 'HalfDay' ? 'rgba(192, 132, 252, 0.15)' :
                                    'rgba(244, 63, 94, 0.15)',
                        color: log.status === 'Present' ? '#34d399' :
                               log.status === 'Late' ? '#fbbf24' :
                               log.status === 'HalfDay' ? '#c084fc' :
                               '#fb7185'
                      }}>
                        {log.status}
                      </span>
                    </td>
                    <td style={{ padding: '0.85rem 1rem', fontSize: '0.8rem' }}>
                      {log.isLate && <span style={{ color: '#fbbf24', display: 'block' }}>+{log.lateMinutes}m Late</span>}
                      {log.isEarlyLeave && <span style={{ color: '#fb7185', display: 'block' }}>-{log.earlyLeaveMinutes}m Early</span>}
                      {!log.isLate && !log.isEarlyLeave && <span style={{ color: 'var(--text-muted)' }}>On time</span>}
                    </td>
                    <td style={{ padding: '0.85rem 1rem', fontWeight: 600 }}>
                      {log.workingMinutes ? `${(log.workingMinutes / 60).toFixed(1)}h` : '—'}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      )}

      {/* TAB 3: REGULARIZATION WORKFLOW */}
      {activeTab === 'regularization' && (
        <div style={{ background: 'var(--bg-surface)', border: '1px solid var(--border-subtle)', borderRadius: 'var(--radius-lg)', overflow: 'hidden' }}>
          {loading ? (
            <div style={{ padding: '3rem', textAlign: 'center', color: 'var(--text-muted)' }}>Loading regularization requests...</div>
          ) : regularizations.length === 0 ? (
            <div style={{ padding: '3rem', textAlign: 'center', color: 'var(--text-muted)' }}>No regularization requests.</div>
          ) : (
            <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', fontSize: '0.875rem' }}>
              <thead>
                <tr style={{ background: 'rgba(255, 255, 255, 0.02)', borderBottom: '1px solid var(--border-subtle)' }}>
                  <th style={{ padding: '0.85rem 1rem', color: 'var(--text-muted)' }}>Requested Date</th>
                  <th style={{ padding: '0.85rem 1rem', color: 'var(--text-muted)' }}>Employee</th>
                  <th style={{ padding: '0.85rem 1rem', color: 'var(--text-muted)' }}>Requested Punch Times</th>
                  <th style={{ padding: '0.85rem 1rem', color: 'var(--text-muted)' }}>Reason</th>
                  <th style={{ padding: '0.85rem 1rem', color: 'var(--text-muted)' }}>Status</th>
                  <th style={{ padding: '0.85rem 1rem', color: 'var(--text-muted)' }}>Reviewer</th>
                  {canReview && <th style={{ padding: '0.85rem 1rem', color: 'var(--text-muted)' }}>Actions</th>}
                </tr>
              </thead>
              <tbody>
                {regularizations.map((reg) => (
                  <tr key={reg.id} style={{ borderBottom: '1px solid rgba(255, 255, 255, 0.04)' }}>
                    <td style={{ padding: '0.85rem 1rem', fontWeight: 600 }}>{reg.logDate}</td>
                    <td style={{ padding: '0.85rem 1rem' }}>
                      <div style={{ fontWeight: 600 }}>{reg.employeeName}</div>
                      <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>{reg.employeeCode}</div>
                    </td>
                    <td style={{ padding: '0.85rem 1rem', fontSize: '0.8rem' }}>
                      <div>In: <strong>{reg.requestedCheckIn ? new Date(reg.requestedCheckIn).toLocaleTimeString() : '—'}</strong></div>
                      <div>Out: <strong>{reg.requestedCheckOut ? new Date(reg.requestedCheckOut).toLocaleTimeString() : '—'}</strong></div>
                    </td>
                    <td style={{ padding: '0.85rem 1rem', maxWidth: 220 }}>
                      <span style={{ fontSize: '0.8125rem' }}>{reg.reason}</span>
                    </td>
                    <td style={{ padding: '0.85rem 1rem' }}>
                      <span style={{
                        padding: '0.2rem 0.55rem',
                        borderRadius: 9999,
                        fontSize: '0.75rem',
                        fontWeight: 600,
                        background: reg.status === 'Approved' ? 'rgba(16, 185, 129, 0.15)' :
                                    reg.status === 'Pending' ? 'rgba(245, 158, 11, 0.15)' :
                                    'rgba(244, 63, 94, 0.15)',
                        color: reg.status === 'Approved' ? '#34d399' :
                               reg.status === 'Pending' ? '#fbbf24' :
                               '#fb7185'
                      }}>
                        {reg.status}
                      </span>
                    </td>
                    <td style={{ padding: '0.85rem 1rem', fontSize: '0.8rem', color: 'var(--text-muted)' }}>
                      {reg.reviewerName ? (
                        <div>
                          <div>{reg.reviewerName}</div>
                          {reg.reviewRemarks && <div style={{ fontStyle: 'italic', fontSize: '0.75rem' }}>"{reg.reviewRemarks}"</div>}
                        </div>
                      ) : '—'}
                    </td>
                    {canReview ? (
                      <td style={{ padding: '0.85rem 1rem' }}>
                        {reg.status === 'Pending' ? (
                          <button
                            onClick={() => setReviewItem(reg)}
                            className="btn-secondary"
                            style={{ padding: '0.4rem 0.75rem', fontSize: '0.75rem' }}
                          >
                            Review
                          </button>
                        ) : (
                          <span style={{ color: 'var(--text-muted)', fontSize: '0.75rem' }}>Reviewed</span>
                        )}
                      </td>
                    ) : (
                      <td style={{ padding: '0.85rem 1rem' }}>
                        <span style={{ color: 'var(--text-muted)', fontSize: '0.75rem' }}>
                          {reg.status === 'Pending' ? 'In Queue' : 'Completed'}
                        </span>
                      </td>
                    )}
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      )}

      {/* MODAL 1: REQUEST REGULARIZATION */}
      {showRegModal && (
        <div style={{ position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.7)', backdropFilter: 'blur(4px)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 50, padding: '1rem' }}>
          <div style={{ background: 'var(--bg-surface)', border: '1px solid var(--border-subtle)', borderRadius: 'var(--radius-lg)', padding: '1.5rem', width: '100%', maxWidth: 480 }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.25rem' }}>
              <h3 style={{ fontSize: '1.1rem', fontWeight: 700, display: 'flex', alignItems: 'center', gap: '8px' }}>
                <Clock size={18} color="#6366f1" /> Attendance Regularization
              </h3>
              <button onClick={() => setShowRegModal(false)} style={{ background: 'none', border: 'none', color: 'var(--text-muted)', cursor: 'pointer' }}>
                <X size={18} />
              </button>
            </div>

            <form onSubmit={handleRequestRegularization}>
              <div className="form-group">
                <label className="form-label">Attendance Date</label>
                <input
                  type="date"
                  className="input-field"
                  required
                  value={regForm.logDate}
                  onChange={e => setRegForm({ ...regForm, logDate: e.target.value })}
                />
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
                <div className="form-group">
                  <label className="form-label">Requested Check-In</label>
                  <input
                    type="time"
                    className="input-field"
                    required
                    value={regForm.requestedCheckIn}
                    onChange={e => setRegForm({ ...regForm, requestedCheckIn: e.target.value })}
                  />
                </div>
                <div className="form-group">
                  <label className="form-label">Requested Check-Out</label>
                  <input
                    type="time"
                    className="input-field"
                    required
                    value={regForm.requestedCheckOut}
                    onChange={e => setRegForm({ ...regForm, requestedCheckOut: e.target.value })}
                  />
                </div>
              </div>

              <div className="form-group">
                <label className="form-label">Reason for Missed / Incorrect Log</label>
                <textarea
                  className="input-field"
                  rows={3}
                  required
                  placeholder="e.g. Biometric gate offline, worked client on-site..."
                  value={regForm.reason}
                  onChange={e => setRegForm({ ...regForm, reason: e.target.value })}
                />
              </div>

              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '0.75rem', marginTop: '1.5rem' }}>
                <button type="button" onClick={() => setShowRegModal(false)} className="btn-secondary">Cancel</button>
                <button type="submit" disabled={actionLoading} className="btn-primary" style={{ width: 'auto', display: 'flex', alignItems: 'center', gap: '6px' }}>
                  <Send size={16} /> Submit Request
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* MODAL 2: REVIEW REGULARIZATION */}
      {reviewItem && (
        <div style={{ position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.7)', backdropFilter: 'blur(4px)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 50, padding: '1rem' }}>
          <div style={{ background: 'var(--bg-surface)', border: '1px solid var(--border-subtle)', borderRadius: 'var(--radius-lg)', padding: '1.5rem', width: '100%', maxWidth: 480 }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.25rem' }}>
              <h3 style={{ fontSize: '1.1rem', fontWeight: 700 }}>Review Regularization Request</h3>
              <button onClick={() => setReviewItem(null)} style={{ background: 'none', border: 'none', color: 'var(--text-muted)', cursor: 'pointer' }}>
                <X size={18} />
              </button>
            </div>

            <div style={{ background: 'var(--bg-surface-elevated)', padding: '1rem', borderRadius: 'var(--radius-md)', marginBottom: '1.25rem', fontSize: '0.85rem' }}>
              <div>Employee: <strong>{reviewItem.employeeName} ({reviewItem.employeeCode})</strong></div>
              <div>Date: <strong>{reviewItem.logDate}</strong></div>
              <div style={{ marginTop: '0.5rem', color: 'var(--text-muted)' }}>Reason: "{reviewItem.reason}"</div>
            </div>

            <div className="form-group">
              <label className="form-label">Review Remarks / Justification</label>
              <textarea
                className="input-field"
                rows={2}
                placeholder="Optional review notes..."
                value={reviewRemarks}
                onChange={e => setReviewRemarks(e.target.value)}
              />
            </div>

            <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '0.75rem', marginTop: '1.5rem' }}>
              <button
                type="button"
                onClick={() => handleReviewRegularization('Rejected')}
                disabled={actionLoading}
                className="btn-secondary"
                style={{ color: '#fb7185', borderColor: '#fb718533', display: 'flex', alignItems: 'center', gap: '6px' }}
              >
                <XCircle size={16} /> Reject
              </button>
              <button
                type="button"
                onClick={() => handleReviewRegularization('Approved')}
                disabled={actionLoading}
                className="btn-primary"
                style={{ width: 'auto', background: 'linear-gradient(135deg, #10b981, #059669)', display: 'flex', alignItems: 'center', gap: '6px' }}
              >
                <CheckCircle2 size={16} /> Approve & Update Log
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
