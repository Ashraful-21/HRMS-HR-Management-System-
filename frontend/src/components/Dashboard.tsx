import { useState, useEffect } from 'react';
import axios from 'axios';
import { 
  Users, 
  Clock, 
  CalendarDays, 
  Banknote, 
  Briefcase, 
  Laptop, 
  TrendingUp, 
  AlertCircle, 
  CheckCircle2, 
  Megaphone, 
  History, 
  ArrowUpRight, 
  Sparkles,
  Shield,
  LogIn,
  LogOut,
  Zap,
  Database,
  Download,
  PlusCircle
} from 'lucide-react';

export interface UserInfo {
  id: number;
  username: string;
  email: string;
  employeeId?: number;
  fullName?: string;
  employeeCode?: string;
  designation?: string;
  department?: string;
  profilePhotoUrl?: string;
  roles: string[];
  permissions: string[];
}

interface DashboardProps {
  token: string;
  user: UserInfo | null;
  onNavigate: (module: any) => void;
}

const API_BASE = 'http://localhost:5219/api/v1';

export default function Dashboard({ token, user, onNavigate }: DashboardProps) {
  const isSuperAdmin = user?.roles?.some(r => r.toLowerCase() === 'superadmin') || false;
  const isHRManager = !isSuperAdmin && (user?.roles?.some(r => r.toLowerCase() === 'hrmanager' || r.toLowerCase() === 'admin') || false);
  const isEmployee = !isSuperAdmin && !isHRManager;

  // -------------------------------------------------------------
  // STATE FOR SUPERADMIN & HR DASHBOARD
  // -------------------------------------------------------------
  const [adminData, setAdminData] = useState<any>(null);
  const [announcements, setAnnouncements] = useState<any[]>([]);
  const [adminLoading, setAdminLoading] = useState(true);
  const [adminError, setAdminError] = useState<string | null>(null);

  // -------------------------------------------------------------
  // STATE FOR EMPLOYEE (ESS) DASHBOARD
  // -------------------------------------------------------------
  const [todayAttendance, setTodayAttendance] = useState<any>(null);
  const [leaveBalances, setLeaveBalances] = useState<any[]>([]);
  const [assignedAssets, setAssignedAssets] = useState<any[]>([]);
  const [recentPayslip, setRecentPayslip] = useState<any>(null);
  const [empLoading, setEmpLoading] = useState(true);
  const [empActionLoading, setEmpActionLoading] = useState(false);
  const [empMessage, setEmpMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);
  const [liveTime, setLiveTime] = useState(new Date());

  const headers = { Authorization: `Bearer ${token}` };

  // Timer for employee live clock
  useEffect(() => {
    if (isEmployee) {
      const timer = setInterval(() => setLiveTime(new Date()), 1000);
      return () => clearInterval(timer);
    }
  }, [isEmployee]);

  // Load SuperAdmin / HR Data
  const loadAdminDashboard = async () => {
    setAdminLoading(true);
    setAdminError(null);
    try {
      const [sumRes, annRes] = await Promise.all([
        axios.get(`${API_BASE}/dashboard/summary`, { headers }),
        axios.get(`${API_BASE}/announcements?includeUnpublished=false`, { headers })
      ]);
      if (sumRes.data.success) setAdminData(sumRes.data.data);
      if (annRes.data.success) setAnnouncements(annRes.data.data || []);
    } catch (err: any) {
      setAdminError(err.response?.data?.message || 'Failed to load executive summary.');
    } finally {
      setAdminLoading(false);
    }
  };

  // Load Employee Data
  const loadEmployeeDashboard = async () => {
    setEmpLoading(true);
    try {
      const empId = user?.employeeId || 1;
      const [attRes, balRes, astRes, payRes, annRes] = await Promise.all([
        axios.get(`${API_BASE}/attendance/today`, { headers }).catch(() => ({ data: { data: null } })),
        axios.get(`${API_BASE}/leave/balances`, { headers }).catch(() => ({ data: { data: [] } })),
        axios.get(`${API_BASE}/assets/assignments?employeeId=${empId}`, { headers }).catch(() => ({ data: { data: [] } })),
        axios.get(`${API_BASE}/payroll/my-payslips`, { headers }).catch(() => ({ data: { data: [] } })),
        axios.get(`${API_BASE}/announcements?includeUnpublished=false`, { headers }).catch(() => ({ data: { data: [] } }))
      ]);

      if (attRes.data?.data) setTodayAttendance(attRes.data.data);
      if (balRes.data?.data) setLeaveBalances(balRes.data.data);
      if (astRes.data?.data) setAssignedAssets(astRes.data.data);
      if (payRes.data?.data && payRes.data.data.length > 0) setRecentPayslip(payRes.data.data[0]);
      if (annRes.data?.data) setAnnouncements(annRes.data.data);
    } catch (err: any) {
      console.error('Error loading employee portal data:', err);
    } finally {
      setEmpLoading(false);
    }
  };

  useEffect(() => {
    if (isSuperAdmin || isHRManager) {
      loadAdminDashboard();
    } else {
      loadEmployeeDashboard();
    }
  }, [token, user]);

  // Employee Punch Actions
  const handleClockIn = async () => {
    setEmpActionLoading(true);
    setEmpMessage(null);
    try {
      const res = await axios.post(`${API_BASE}/attendance/check-in`, { notes: 'Web Portal Punch In' }, { headers });
      if (res.data.success) {
        setEmpMessage({ type: 'success', text: 'Checked in successfully! Have a productive day.' });
        loadEmployeeDashboard();
      }
    } catch (err: any) {
      setEmpMessage({ type: 'error', text: err.response?.data?.message || 'Check-in failed.' });
    } finally {
      setEmpActionLoading(false);
    }
  };

  const handleClockOut = async () => {
    setEmpActionLoading(true);
    setEmpMessage(null);
    try {
      const res = await axios.post(`${API_BASE}/attendance/check-out`, { notes: 'Web Portal Punch Out' }, { headers });
      if (res.data.success) {
        setEmpMessage({ type: 'success', text: 'Checked out successfully! Good rest.' });
        loadEmployeeDashboard();
      }
    } catch (err: any) {
      setEmpMessage({ type: 'error', text: err.response?.data?.message || 'Check-out failed.' });
    } finally {
      setEmpActionLoading(false);
    }
  };

  // =========================================================================
  // VIEW 1: REGULAR EMPLOYEE SELF-SERVICE (ESS) DASHBOARD
  // =========================================================================
  if (isEmployee) {
    if (empLoading) {
      return (
        <div style={{ textAlign: 'center', padding: '3.5rem', color: 'var(--text-muted)' }}>
          <div className="spinner" style={{ margin: '0 auto 1rem' }} />
          <p>Loading your personal workspace...</p>
        </div>
      );
    }

    const hasCheckedIn = todayAttendance?.hasCheckedIn || false;
    const hasCheckedOut = todayAttendance?.hasCheckedOut || false;
    const checkInTimeFormatted = todayAttendance?.checkInTime 
      ? new Date(todayAttendance.checkInTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
      : null;

    return (
      <div className="dashboard-view" style={{ display: 'flex', flexDirection: 'column', gap: '1.5rem' }}>
        
        {/* Personal Employee Banner */}
        <div style={{
          background: 'linear-gradient(135deg, rgba(99, 102, 241, 0.16) 0%, rgba(6, 182, 212, 0.12) 100%)',
          border: '1px solid rgba(99, 102, 241, 0.3)',
          borderRadius: 'var(--radius-lg)',
          padding: '1.5rem 1.75rem',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          flexWrap: 'wrap',
          gap: '1.25rem'
        }}>
          <div>
            <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '0.4rem' }}>
              <span className="badge-pill badge-primary" style={{ textTransform: 'uppercase', letterSpacing: '0.05em', fontSize: '0.75rem' }}>
                Employee Self-Service (ESS)
              </span>
              <span style={{ color: 'var(--text-muted)', fontSize: '0.8rem' }}>• {user?.employeeCode || 'EMP-ID'}</span>
            </div>
            <h2 style={{ fontSize: '1.65rem', fontWeight: 800, margin: 0, color: '#ffffff' }}>
              Welcome back, {user?.fullName || user?.username}!
            </h2>
            <div style={{ display: 'flex', gap: '12px', marginTop: '0.5rem', flexWrap: 'wrap', color: 'var(--text-muted)', fontSize: '0.875rem' }}>
              <span>🏢 {user?.department || 'Engineering'}</span>
              <span>•</span>
              <span>💼 {user?.designation || 'Staff Software Engineer'}</span>
              <span>•</span>
              <span>✉️ {user?.email}</span>
            </div>
          </div>

          <div style={{ display: 'flex', gap: '0.75rem' }}>
            <button 
              type="button" 
              className="btn-primary"
              onClick={() => onNavigate('leave')}
              style={{ display: 'flex', alignItems: 'center', gap: '6px', fontSize: '0.85rem' }}
            >
              <CalendarDays size={16} /> Apply for Leave
            </button>
            <button 
              type="button" 
              className="btn-secondary"
              onClick={() => onNavigate('attendance')}
              style={{ display: 'flex', alignItems: 'center', gap: '6px', fontSize: '0.85rem' }}
            >
              <Clock size={16} /> My Punch History
            </button>
          </div>
        </div>

        {/* Action / Error Notification */}
        {empMessage && (
          <div className={empMessage.type === 'success' ? 'alert-success' : 'alert-error'} style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
            {empMessage.type === 'success' ? <CheckCircle2 size={18} /> : <AlertCircle size={18} />}
            <span>{empMessage.text}</span>
          </div>
        )}

        {/* Top ESS Grid: Punch Clock + Leave Balances */}
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(340px, 1fr))', gap: '1.25rem' }}>
          
          {/* Card 1: Interactive Live Punch Clock */}
          <div className="section-card" style={{ display: 'flex', flexDirection: 'column', justifyContent: 'space-between' }}>
            <div>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                  <Clock size={20} color="#6366f1" />
                  <h3 style={{ fontSize: '1.1rem', fontWeight: 700, margin: 0 }}>Attendance Clock</h3>
                </div>
                <span className={`badge-pill ${hasCheckedIn && !hasCheckedOut ? 'badge-success' : 'badge-neutral'}`}>
                  {hasCheckedIn && !hasCheckedOut ? '🟢 Checked In' : hasCheckedOut ? '⚪ Shift Completed' : '⚪ Not Checked In'}
                </span>
              </div>

              {/* Digital Clock Display */}
              <div style={{
                background: 'rgba(0, 0, 0, 0.3)',
                border: '1px solid var(--border-subtle)',
                borderRadius: 'var(--radius-md)',
                padding: '1.25rem',
                textAlign: 'center',
                marginBottom: '1rem'
              }}>
                <div style={{ fontSize: '2.25rem', fontWeight: 800, fontFamily: 'var(--font-mono)', letterSpacing: '0.05em', color: '#67e8f9' }}>
                  {liveTime.toLocaleTimeString()}
                </div>
                <p style={{ margin: '0.25rem 0 0', color: 'var(--text-muted)', fontSize: '0.85rem' }}>
                  {liveTime.toLocaleDateString(undefined, { weekday: 'long', month: 'short', day: 'numeric', year: 'numeric' })}
                </p>
              </div>

              {/* Attendance Details */}
              <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.85rem', color: 'var(--text-muted)', marginBottom: '0.5rem' }}>
                <span>Today's Status:</span>
                <strong style={{ color: hasCheckedIn ? '#10b981' : '#f59e0b' }}>
                  {todayAttendance?.status || 'Not Marked Yet'}
                </strong>
              </div>
              <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.85rem', color: 'var(--text-muted)' }}>
                <span>First Clock-In:</span>
                <span>{checkInTimeFormatted || '—'}</span>
              </div>
              <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.85rem', color: 'var(--text-muted)', marginTop: '0.35rem' }}>
                <span>Scheduled Shift:</span>
                <span>{todayAttendance?.shiftName || 'Standard Day Shift (09:00 - 18:00)'}</span>
              </div>
            </div>

            {/* Punch In / Out Button */}
            <div style={{ marginTop: '1.25rem', paddingTop: '1rem', borderTop: '1px solid var(--border-subtle)' }}>
              {!hasCheckedIn ? (
                <button
                  type="button"
                  className="btn-primary"
                  onClick={handleClockIn}
                  disabled={empActionLoading}
                  style={{ width: '100%', padding: '0.85rem', display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '8px', fontSize: '0.95rem' }}
                >
                  <LogIn size={18} /> {empActionLoading ? 'Recording Clock-In...' : 'Clock In Now'}
                </button>
              ) : !hasCheckedOut ? (
                <button
                  type="button"
                  className="btn-secondary"
                  onClick={handleClockOut}
                  disabled={empActionLoading}
                  style={{ width: '100%', padding: '0.85rem', display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '8px', fontSize: '0.95rem', borderColor: '#f43f5e', color: '#f43f5e' }}
                >
                  <LogOut size={18} /> {empActionLoading ? 'Recording Clock-Out...' : 'Clock Out (End Shift)'}
                </button>
              ) : (
                <div style={{ textAlign: 'center', padding: '0.5rem', background: 'rgba(16, 185, 129, 0.1)', border: '1px solid rgba(16, 185, 129, 0.25)', borderRadius: 'var(--radius-sm)', color: '#10b981', fontSize: '0.85rem', fontWeight: 600 }}>
                  ✓ Shift recorded for today
                </div>
              )}
            </div>
          </div>

          {/* Card 2: My Leave Balances */}
          <div className="section-card" style={{ display: 'flex', flexDirection: 'column', justifyContent: 'space-between' }}>
            <div>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                  <CalendarDays size={20} color="#06b6d4" />
                  <h3 style={{ fontSize: '1.1rem', fontWeight: 700, margin: 0 }}>My Leave Balances</h3>
                </div>
                <button 
                  type="button" 
                  className="text-btn" 
                  onClick={() => onNavigate('leave')}
                  style={{ fontSize: '0.8rem', display: 'flex', alignItems: 'center', gap: '4px' }}
                >
                  Leave Portal <ArrowUpRight size={14} />
                </button>
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: 'repeat(2, 1fr)', gap: '0.75rem' }}>
                {leaveBalances.length > 0 ? (
                  leaveBalances.slice(0, 4).map((b: any) => {
                    const balance = b.balance ?? (b.totalDays - b.usedDays);
                    return (
                      <div 
                        key={b.leaveTypeId} 
                        style={{ 
                          padding: '0.85rem', 
                          background: 'rgba(255, 255, 255, 0.02)', 
                          border: '1px solid var(--border-subtle)', 
                          borderRadius: 'var(--radius-md)' 
                        }}
                      >
                        <p style={{ margin: '0 0 0.25rem', fontSize: '0.785rem', fontWeight: 600, color: 'var(--text-muted)' }}>
                          {b.leaveTypeName}
                        </p>
                        <h4 style={{ fontSize: '1.5rem', fontWeight: 800, margin: '0 0 0.25rem', color: '#ffffff' }}>
                          {balance} <span style={{ fontSize: '0.8rem', color: 'var(--text-muted)', fontWeight: 500 }}>days</span>
                        </h4>
                        <div style={{ fontSize: '0.725rem', color: 'var(--text-dim)' }}>
                          Used: {b.usedDays} of {b.totalDays}
                        </div>
                      </div>
                    );
                  })
                ) : (
                  <p style={{ gridColumn: 'span 2', color: 'var(--text-muted)', fontSize: '0.85rem' }}>No leave balances assigned yet.</p>
                )}
              </div>
            </div>

            <div style={{ marginTop: '1.25rem', paddingTop: '1rem', borderTop: '1px solid var(--border-subtle)' }}>
              <button
                type="button"
                className="btn-primary"
                onClick={() => onNavigate('leave')}
                style={{ width: '100%', padding: '0.75rem', display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '6px', fontSize: '0.875rem' }}
              >
                <PlusCircle size={16} /> Request Time Off
              </button>
            </div>
          </div>

        </div>

        {/* Bottom ESS Grid: My Equipment (Assets) + Payslip Summary + Announcements */}
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(340px, 1fr))', gap: '1.25rem' }}>
          
          {/* Card 3: My Equipment & Custody */}
          <div className="section-card">
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                <Laptop size={18} color="#a855f7" />
                <h3 style={{ fontSize: '1.05rem', fontWeight: 700, margin: 0 }}>My Company Equipment</h3>
              </div>
              <button 
                type="button" 
                className="text-btn" 
                onClick={() => onNavigate('assets')}
                style={{ fontSize: '0.8rem', display: 'flex', alignItems: 'center', gap: '4px' }}
              >
                View Equipment <ArrowUpRight size={14} />
              </button>
            </div>

            <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
              {assignedAssets.length > 0 ? (
                assignedAssets.map((ast: any) => (
                  <div 
                    key={ast.id || ast.assetCode} 
                    style={{
                      padding: '0.85rem 1rem',
                      background: 'rgba(168, 85, 247, 0.05)',
                      border: '1px solid rgba(168, 85, 247, 0.2)',
                      borderRadius: 'var(--radius-md)'
                    }}
                  >
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.25rem' }}>
                      <strong style={{ fontSize: '0.95rem', color: '#ffffff' }}>{ast.assetName || ast.name}</strong>
                      <span className="badge-pill badge-primary">In Your Custody</span>
                    </div>
                    <p style={{ margin: '0 0 0.25rem', fontSize: '0.8rem', color: 'var(--text-muted)' }}>
                      Asset Code: <code style={{ color: '#c084fc' }}>{ast.assetCode}</code> • Serial: {ast.serialNumber || 'N/A'}
                    </p>
                    <p style={{ margin: 0, fontSize: '0.75rem', color: 'var(--text-dim)' }}>
                      Issued on: {ast.assignedDate ? new Date(ast.assignedDate).toLocaleDateString() : 'Active'} • Condition: Good
                    </p>
                  </div>
                ))
              ) : (
                <div style={{ textAlign: 'center', padding: '1.5rem', background: 'rgba(255,255,255,0.02)', borderRadius: 'var(--radius-md)', border: '1px solid var(--border-subtle)' }}>
                  <Laptop size={32} color="var(--text-dim)" style={{ margin: '0 auto 0.5rem' }} />
                  <p style={{ margin: 0, fontSize: '0.85rem', color: 'var(--text-muted)' }}>No hardware devices currently in your custody.</p>
                </div>
              )}
            </div>
          </div>

          {/* Card 4: My Latest Payslip Preview */}
          <div className="section-card">
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                <Banknote size={18} color="#10b981" />
                <h3 style={{ fontSize: '1.05rem', fontWeight: 700, margin: 0 }}>Latest Payslip</h3>
              </div>
              <button 
                type="button" 
                className="text-btn" 
                onClick={() => onNavigate('payroll')}
                style={{ fontSize: '0.8rem', display: 'flex', alignItems: 'center', gap: '4px' }}
              >
                All Payslips <ArrowUpRight size={14} />
              </button>
            </div>

            {recentPayslip ? (
              <div style={{ padding: '1rem', background: 'rgba(16, 185, 129, 0.05)', border: '1px solid rgba(16, 185, 129, 0.2)', borderRadius: 'var(--radius-md)' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.75rem' }}>
                  <span style={{ fontSize: '0.85rem', color: 'var(--text-muted)' }}>Disbursement Period:</span>
                  <span style={{ fontWeight: 700, color: '#ffffff' }}>Month {recentPayslip.payrollRunId || 1} / 2026</span>
                </div>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.75rem' }}>
                  <span style={{ fontSize: '0.85rem', color: 'var(--text-muted)' }}>Net Pay:</span>
                  <span style={{ fontSize: '1.4rem', fontWeight: 800, color: '#10b981' }}>${(recentPayslip.netPay || 0).toLocaleString()}</span>
                </div>
                <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.8rem', color: 'var(--text-dim)', marginBottom: '1rem' }}>
                  <span>Gross: ${(recentPayslip.grossEarnings || 0).toLocaleString()}</span>
                  <span>Deductions: ${(recentPayslip.totalDeductions || 0).toLocaleString()}</span>
                </div>
                <button
                  type="button"
                  className="btn-secondary"
                  onClick={() => onNavigate('payroll')}
                  style={{ width: '100%', fontSize: '0.825rem', display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '6px' }}
                >
                  <Download size={14} /> View Detailed Breakdown
                </button>
              </div>
            ) : (
              <div style={{ textAlign: 'center', padding: '1.5rem', background: 'rgba(255,255,255,0.02)', borderRadius: 'var(--radius-md)', border: '1px solid var(--border-subtle)' }}>
                <Banknote size={32} color="var(--text-dim)" style={{ margin: '0 auto 0.5rem' }} />
                <p style={{ margin: 0, fontSize: '0.85rem', color: 'var(--text-muted)' }}>No payslip statements generated yet.</p>
              </div>
            )}
          </div>

          {/* Card 5: Company Announcements */}
          <div className="section-card">
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                <Megaphone size={18} color="#f59e0b" />
                <h3 style={{ fontSize: '1.05rem', fontWeight: 700, margin: 0 }}>Company Notices</h3>
              </div>
              <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>{announcements.length} posted</span>
            </div>

            <div style={{ display: 'flex', flexDirection: 'column', gap: '0.65rem' }}>
              {announcements.length > 0 ? (
                announcements.slice(0, 3).map((ann: any) => (
                  <div 
                    key={ann.id}
                    style={{ 
                      padding: '0.75rem 0.85rem', 
                      background: ann.isPinned ? 'rgba(245, 158, 11, 0.08)' : 'rgba(255, 255, 255, 0.02)', 
                      border: ann.isPinned ? '1px solid rgba(245, 158, 11, 0.25)' : '1px solid var(--border-subtle)',
                      borderRadius: 'var(--radius-sm)'
                    }}
                  >
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.25rem' }}>
                      <strong style={{ fontSize: '0.875rem', color: ann.isPinned ? '#fbbf24' : '#ffffff' }}>
                        {ann.isPinned && '📌 '} {ann.title}
                      </strong>
                      <span style={{ fontSize: '0.7rem', color: 'var(--text-muted)' }}>
                        {new Date(ann.createdAt).toLocaleDateString()}
                      </span>
                    </div>
                    <p style={{ margin: 0, fontSize: '0.785rem', color: 'var(--text-muted)', lineHeight: 1.35 }}>
                      {ann.content}
                    </p>
                  </div>
                ))
              ) : (
                <p style={{ color: 'var(--text-muted)', fontSize: '0.85rem' }}>No company notices currently posted.</p>
              )}
            </div>
          </div>

        </div>

      </div>
    );
  }

  // =========================================================================
  // VIEW 2 & 3: SUPERADMIN OR HR MANAGER DASHBOARD
  // =========================================================================
  if (adminLoading) {
    return (
      <div style={{ textAlign: 'center', padding: '3.5rem', color: 'var(--text-muted)' }}>
        <div className="spinner" style={{ margin: '0 auto 1rem' }} />
        <p>Loading enterprise intelligence &amp; system state...</p>
      </div>
    );
  }

  if (adminError) {
    return (
      <div className="alert-error" style={{ margin: '1.5rem 0' }}>
        <AlertCircle size={18} />
        <span>{adminError}</span>
      </div>
    );
  }

  return (
    <div className="dashboard-view" style={{ display: 'flex', flexDirection: 'column', gap: '1.5rem' }}>
      
      {/* Top Banner (Tailored for SuperAdmin vs HRManager) */}
      <div style={{
        background: isSuperAdmin 
          ? 'linear-gradient(135deg, rgba(99, 102, 241, 0.18) 0%, rgba(168, 85, 247, 0.14) 100%)'
          : 'linear-gradient(135deg, rgba(16, 185, 129, 0.15) 0%, rgba(6, 182, 212, 0.12) 100%)',
        border: isSuperAdmin ? '1px solid rgba(99, 102, 241, 0.35)' : '1px solid rgba(16, 185, 129, 0.35)',
        borderRadius: 'var(--radius-lg)',
        padding: '1.5rem 1.75rem',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        flexWrap: 'wrap',
        gap: '1rem'
      }}>
        <div>
          <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '0.35rem' }}>
            <Sparkles size={18} color={isSuperAdmin ? '#c084fc' : '#34d399'} />
            <span style={{ fontSize: '0.85rem', fontWeight: 600, color: isSuperAdmin ? '#c084fc' : '#34d399', textTransform: 'uppercase', letterSpacing: '0.05em' }}>
              {isSuperAdmin ? 'SuperAdmin Executive Command' : 'People Operations & HR Command'}
            </span>
          </div>
          <h2 style={{ fontSize: '1.65rem', fontWeight: 800, margin: 0, color: '#ffffff' }}>
            {isSuperAdmin ? 'Enterprise Master Overview' : 'HR & Workforce Operations'}
          </h2>
          <p style={{ margin: '0.25rem 0 0', color: 'var(--text-muted)', fontSize: '0.9rem' }}>
            {isSuperAdmin 
              ? 'Multi-branch workforce KPIs, payroll expenditure, live distributed cache & system security.'
              : 'Daily employee presence, leave approvals, recruitment pipeline and staff records.'}
          </p>
        </div>

        <div style={{ display: 'flex', gap: '0.75rem', flexWrap: 'wrap' }}>
          <button 
            type="button" 
            className="btn-primary"
            onClick={() => onNavigate('employees')}
            style={{ display: 'flex', alignItems: 'center', gap: '6px', fontSize: '0.85rem' }}
          >
            <Users size={15} /> Workforce Directory
          </button>
          {isSuperAdmin && (
            <button 
              type="button" 
              className="btn-secondary"
              onClick={() => onNavigate('admin')}
              style={{ display: 'flex', alignItems: 'center', gap: '6px', fontSize: '0.85rem' }}
            >
              <Shield size={15} /> System Admin
            </button>
          )}
          <button 
            type="button" 
            className="btn-secondary"
            onClick={() => onNavigate('payroll')}
            style={{ display: 'flex', alignItems: 'center', gap: '6px', fontSize: '0.85rem' }}
          >
            <Banknote size={15} /> Run Payroll
          </button>
        </div>
      </div>

      {/* SUPERADMIN SYSTEM HEALTH STATUS BAR (Only shown to SuperAdmin) */}
      {isSuperAdmin && (
        <div style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))',
          gap: '0.75rem',
          padding: '0.85rem 1rem',
          background: 'rgba(255, 255, 255, 0.02)',
          border: '1px solid var(--border-subtle)',
          borderRadius: 'var(--radius-md)'
        }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
            <Zap size={18} color="#eab308" />
            <div>
              <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>Distributed Caching</div>
              <strong style={{ fontSize: '0.85rem', color: '#10b981' }}>Active (Hybrid In-Memory/Redis)</strong>
            </div>
          </div>
          <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
            <Database size={18} color="#38bdf8" />
            <div>
              <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>SQL Server Database</div>
              <strong style={{ fontSize: '0.85rem', color: '#10b981' }}>Connected (HRMS DB)</strong>
            </div>
          </div>
          <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
            <Clock size={18} color="#a855f7" />
            <div>
              <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>Hangfire Background Jobs</div>
              <strong style={{ fontSize: '0.85rem', color: '#10b981' }}>20 Workers Active</strong>
            </div>
          </div>
          <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
            <Shield size={18} color="#6366f1" />
            <div>
              <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>RBAC Policy Engine</div>
              <strong style={{ fontSize: '0.85rem', color: '#818cf8' }}>4 Roles • 28 Permissions</strong>
            </div>
          </div>
        </div>
      )}

      {/* Main KPI Stats Grid */}
      <div style={{
        display: 'grid',
        gridTemplateColumns: 'repeat(auto-fit, minmax(240px, 1fr))',
        gap: '1rem'
      }}>
        {/* Total Workforce */}
        <div className="stat-card" onClick={() => onNavigate('employees')} style={{ cursor: 'pointer' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
            <div>
              <p style={{ color: 'var(--text-muted)', fontSize: '0.825rem', fontWeight: 600, margin: '0 0 0.5rem' }}>
                TOTAL WORKFORCE
              </p>
              <h3 style={{ fontSize: '2rem', fontWeight: 800, margin: 0, color: '#ffffff' }}>
                {adminData?.totalEmployees || 0}
              </h3>
            </div>
            <div style={{ width: 44, height: 44, borderRadius: 10, background: 'rgba(99, 102, 241, 0.15)', color: '#818cf8', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
              <Users size={22} />
            </div>
          </div>
          <div style={{ marginTop: '0.75rem', fontSize: '0.8rem', color: '#10b981', display: 'flex', alignItems: 'center', gap: '4px' }}>
            <TrendingUp size={14} />
            <span>{adminData?.activeEmployees || 0} Active • {adminData?.onProbationEmployees || 0} On Probation</span>
          </div>
        </div>

        {/* Today's Attendance */}
        <div className="stat-card" onClick={() => onNavigate('attendance')} style={{ cursor: 'pointer' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
            <div>
              <p style={{ color: 'var(--text-muted)', fontSize: '0.825rem', fontWeight: 600, margin: '0 0 0.5rem' }}>
                TODAY ATTENDANCE
              </p>
              <h3 style={{ fontSize: '2rem', fontWeight: 800, margin: 0, color: '#ffffff' }}>
                {adminData?.attendanceRatePercent || 0}%
              </h3>
            </div>
            <div style={{ width: 44, height: 44, borderRadius: 10, background: 'rgba(16, 185, 129, 0.15)', color: '#10b981', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
              <Clock size={22} />
            </div>
          </div>
          <div style={{ marginTop: '0.75rem', fontSize: '0.8rem', color: 'var(--text-muted)', display: 'flex', alignItems: 'center', gap: '6px' }}>
            <span style={{ color: '#10b981', fontWeight: 600 }}>{adminData?.attendancePresent || 0} Present</span>
            <span>•</span>
            <span style={{ color: '#f59e0b' }}>{adminData?.attendanceLate || 0} Late</span>
            <span>•</span>
            <span style={{ color: '#f43f5e' }}>{adminData?.attendanceAbsent || 0} Absent</span>
          </div>
        </div>

        {/* Leaves Pending */}
        <div className="stat-card" onClick={() => onNavigate('leave')} style={{ cursor: 'pointer' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
            <div>
              <p style={{ color: 'var(--text-muted)', fontSize: '0.825rem', fontWeight: 600, margin: '0 0 0.5rem' }}>
                PENDING LEAVE REQUESTS
              </p>
              <h3 style={{ fontSize: '2rem', fontWeight: 800, margin: 0, color: '#ffffff' }}>
                {adminData?.pendingLeaveApplications || 0}
              </h3>
            </div>
            <div style={{ width: 44, height: 44, borderRadius: 10, background: 'rgba(245, 158, 11, 0.15)', color: '#f59e0b', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
              <CalendarDays size={22} />
            </div>
          </div>
          <div style={{ marginTop: '0.75rem', fontSize: '0.8rem', color: 'var(--text-muted)' }}>
            <span>{adminData?.onLeaveToday || 0} Employee(s) on approved leave today</span>
          </div>
        </div>

        {/* Monthly Payroll */}
        <div className="stat-card" onClick={() => onNavigate('payroll')} style={{ cursor: 'pointer' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
            <div>
              <p style={{ color: 'var(--text-muted)', fontSize: '0.825rem', fontWeight: 600, margin: '0 0 0.5rem' }}>
                MONTHLY NET PAYROLL
              </p>
              <h3 style={{ fontSize: '1.7rem', fontWeight: 800, margin: 0, color: '#ffffff' }}>
                ${(adminData?.lastPayrollNetTotal || 0).toLocaleString()}
              </h3>
            </div>
            <div style={{ width: 44, height: 44, borderRadius: 10, background: 'rgba(14, 165, 233, 0.15)', color: '#38bdf8', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
              <Banknote size={22} />
            </div>
          </div>
          <div style={{ marginTop: '0.75rem', fontSize: '0.8rem', color: 'var(--text-muted)' }}>
            <span>Period: <strong>{adminData?.lastPayrollPeriod}</strong> ({adminData?.lastPayrollStatus})</span>
          </div>
        </div>
      </div>

      {/* Middle Section: Department Distribution & Operations Hub */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(350px, 1fr))', gap: '1.5rem' }}>
        
        {/* Department Workforce Breakdown */}
        <div className="section-card">
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.25rem' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
              <Users size={18} color="#6366f1" />
              <h3 style={{ fontSize: '1.05rem', fontWeight: 700, margin: 0 }}>Department Distribution</h3>
            </div>
            <button 
              type="button" 
              className="text-btn" 
              onClick={() => onNavigate('organization')}
              style={{ fontSize: '0.8rem', display: 'flex', alignItems: 'center', gap: '4px' }}
            >
              Org Chart <ArrowUpRight size={14} />
            </button>
          </div>

          <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
            {adminData?.departmentDistribution && adminData.departmentDistribution.length > 0 ? (
              adminData.departmentDistribution.map((dept: any, idx: number) => (
                <div key={dept.departmentName || idx}>
                  <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.85rem', marginBottom: '0.35rem' }}>
                    <span style={{ fontWeight: 600 }}>{dept.departmentName}</span>
                    <span style={{ color: 'var(--text-muted)' }}>{dept.employeeCount} staff ({dept.percentage}%)</span>
                  </div>
                  <div style={{ width: '100%', height: '8px', background: 'rgba(255,255,255,0.08)', borderRadius: '999px', overflow: 'hidden' }}>
                    <div 
                      style={{ 
                        width: `${Math.min(100, Math.max(10, dept.percentage))}%`, 
                        height: '100%', 
                        background: idx % 3 === 0 ? '#6366f1' : idx % 3 === 1 ? '#a855f7' : '#06b6d4',
                        borderRadius: '999px'
                      }} 
                    />
                  </div>
                </div>
              ))
            ) : (
              <p style={{ color: 'var(--text-muted)', fontSize: '0.875rem' }}>No department statistics recorded yet.</p>
            )}
          </div>
        </div>

        {/* Quick Operations & Recruitment / Assets Stats */}
        <div className="section-card">
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.25rem' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
              <Briefcase size={18} color="#a855f7" />
              <h3 style={{ fontSize: '1.05rem', fontWeight: 700, margin: 0 }}>Recruitment &amp; Asset Status</h3>
            </div>
            <span className="badge-pill badge-primary">Active</span>
          </div>

          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
            <div 
              style={{ padding: '1rem', background: 'rgba(255,255,255,0.02)', border: '1px solid var(--border-subtle)', borderRadius: 'var(--radius-md)', cursor: 'pointer' }}
              onClick={() => onNavigate('recruitment')}
            >
              <div style={{ display: 'flex', alignItems: 'center', gap: '6px', color: '#c084fc', marginBottom: '0.5rem' }}>
                <Briefcase size={16} />
                <span style={{ fontSize: '0.8rem', fontWeight: 600 }}>JOB OPENINGS</span>
              </div>
              <h4 style={{ fontSize: '1.5rem', fontWeight: 800, margin: '0 0 0.25rem' }}>{adminData?.openJobPositions || 0}</h4>
              <p style={{ margin: 0, fontSize: '0.75rem', color: 'var(--text-muted)' }}>{adminData?.totalCandidatesInPipeline || 0} Candidates in pipeline</p>
            </div>

            <div 
              style={{ padding: '1rem', background: 'rgba(255,255,255,0.02)', border: '1px solid var(--border-subtle)', borderRadius: 'var(--radius-md)', cursor: 'pointer' }}
              onClick={() => onNavigate('assets')}
            >
              <div style={{ display: 'flex', alignItems: 'center', gap: '6px', color: '#38bdf8', marginBottom: '0.5rem' }}>
                <Laptop size={16} />
                <span style={{ fontSize: '0.8rem', fontWeight: 600 }}>ASSETS ASSIGNED</span>
              </div>
              <h4 style={{ fontSize: '1.5rem', fontWeight: 800, margin: '0 0 0.25rem' }}>{adminData?.totalAssetsAssigned || 0}</h4>
              <p style={{ margin: 0, fontSize: '0.75rem', color: 'var(--text-muted)' }}>{adminData?.totalAssetsAvailable || 0} Available in stock</p>
            </div>
          </div>

          {/* Quick Pending Actions */}
          <div style={{ marginTop: '1.25rem', paddingTop: '1rem', borderTop: '1px solid var(--border-subtle)' }}>
            <p style={{ fontSize: '0.8rem', fontWeight: 600, color: 'var(--text-muted)', marginBottom: '0.75rem' }}>QUICK ACTIONS</p>
            <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap' }}>
              <button 
                type="button" 
                className="chip-btn"
                onClick={() => onNavigate('employees')}
              >
                + Add Employee
              </button>
              <button 
                type="button" 
                className="chip-btn"
                onClick={() => onNavigate('leave')}
              >
                Review Leaves ({adminData?.pendingLeaveApplications || 0})
              </button>
              <button 
                type="button" 
                className="chip-btn"
                onClick={() => onNavigate('attendance')}
              >
                View Attendance
              </button>
              {isSuperAdmin && (
                <button 
                  type="button" 
                  className="chip-btn"
                  onClick={() => onNavigate('admin')}
                  style={{ color: '#818cf8', borderColor: 'rgba(99,102,241,0.3)' }}
                >
                  RBAC &amp; User Accounts
                </button>
              )}
            </div>
          </div>
        </div>

      </div>

      {/* Bottom Section: Company Announcements & Audit Stream */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(350px, 1fr))', gap: '1.5rem' }}>
        
        {/* Company Announcements Board */}
        <div className="section-card">
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
              <Megaphone size={18} color="#f59e0b" />
              <h3 style={{ fontSize: '1.05rem', fontWeight: 700, margin: 0 }}>Company Announcements</h3>
            </div>
            <span style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>{announcements.length} posted</span>
          </div>

          <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
            {announcements.length > 0 ? (
              announcements.slice(0, 3).map((ann: any) => (
                <div 
                  key={ann.id} 
                  style={{ 
                    padding: '0.85rem 1rem', 
                    background: ann.isPinned ? 'rgba(245, 158, 11, 0.08)' : 'rgba(255,255,255,0.02)', 
                    border: ann.isPinned ? '1px solid rgba(245, 158, 11, 0.25)' : '1px solid var(--border-subtle)',
                    borderRadius: 'var(--radius-md)'
                  }}
                >
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.35rem' }}>
                    <h5 style={{ margin: 0, fontSize: '0.9rem', fontWeight: 700, color: ann.isPinned ? '#fbbf24' : '#ffffff' }}>
                      {ann.isPinned && '📌 '} {ann.title}
                    </h5>
                    <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>
                      {new Date(ann.createdAt).toLocaleDateString()}
                    </span>
                  </div>
                  <p style={{ margin: 0, fontSize: '0.825rem', color: 'var(--text-muted)', lineHeight: 1.4 }}>
                    {ann.content}
                  </p>
                </div>
              ))
            ) : (
              <p style={{ color: 'var(--text-muted)', fontSize: '0.85rem' }}>No active company announcements.</p>
            )}
          </div>
        </div>

        {/* Live Audit Log Stream (Visible to SuperAdmin & HR) */}
        <div className="section-card">
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
              <History size={18} color="#10b981" />
              <h3 style={{ fontSize: '1.05rem', fontWeight: 700, margin: 0 }}>Recent Enterprise Activity</h3>
            </div>
            <span className="badge-pill badge-success">Live Auditing</span>
          </div>

          <div style={{ display: 'flex', flexDirection: 'column', gap: '0.65rem' }}>
            {adminData?.recentActivities && adminData.recentActivities.length > 0 ? (
              adminData.recentActivities.map((act: any) => (
                <div 
                  key={act.id} 
                  style={{ 
                    display: 'flex', 
                    alignItems: 'center', 
                    justifyContent: 'space-between',
                    padding: '0.65rem 0.85rem',
                    background: 'rgba(255,255,255,0.02)',
                    border: '1px solid var(--border-subtle)',
                    borderRadius: 'var(--radius-sm)',
                    fontSize: '0.825rem'
                  }}
                >
                  <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                    <CheckCircle2 size={15} color="#10b981" />
                    <span><strong>{act.username}</strong> performed <code style={{ color: '#818cf8', background: 'rgba(99,102,241,0.1)', padding: '2px 6px', borderRadius: 4 }}>{act.action}</code> on {act.entityType}</span>
                  </div>
                  <span style={{ color: 'var(--text-muted)', fontSize: '0.75rem' }}>
                    {new Date(act.timestamp).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                  </span>
                </div>
              ))
            ) : (
              <p style={{ color: 'var(--text-muted)', fontSize: '0.85rem' }}>No recent activities recorded.</p>
            )}
          </div>
        </div>

      </div>

    </div>
  );
}
