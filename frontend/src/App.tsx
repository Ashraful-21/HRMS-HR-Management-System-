import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { 
  Lock, 
  User as UserIcon, 
  Eye, 
  EyeOff, 
  AlertCircle, 
  LogOut, 
  Building2,
  KeyRound,
  Users,
  Clock,
  CalendarDays,
  Banknote,
  Briefcase,
  Award,
  Laptop,
  FileSpreadsheet,
  Shield,
  LayoutDashboard,
  Bell,
  X,
  Mail,
  ArrowLeft,
  RefreshCw
} from 'lucide-react';
import Dashboard from './components/Dashboard';
import Employees from './components/Employees';
import Attendance from './components/Attendance';
import Leave from './components/Leave';
import Payroll from './components/Payroll';
import Recruitment from './components/Recruitment';
import Performance from './components/Performance';
import Assets from './components/Assets';
import Organization from './components/Organization';
import Reports from './components/Reports';
import Administration from './components/Administration';

interface UserInfo {
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

interface LoginResponseData {
  requiresTwoFactor?: boolean;
  twoFactorChallenge?: string;
  maskedEmail?: string;
  devOtpHint?: string;
  message?: string;
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAt: string;
  user: UserInfo;
}

const API_BASE = 'http://localhost:5219/api/v1';

export default function App() {
  const [username, setUsername] = useState('superadmin');
  const [password, setPassword] = useState('Admin@123456');
  const [showPassword, setShowPassword] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [infoMsg, setInfoMsg] = useState<string | null>(null);
  const [user, setUser] = useState<UserInfo | null>(null);
  const [accessToken, setAccessToken] = useState<string | null>(null);
  const [apiOnline, setApiOnline] = useState<boolean>(true);

  // 2FA Email OTP State
  const [otpStep, setOtpStep] = useState(false);
  const [challengeToken, setChallengeToken] = useState<string>('');
  const [maskedEmail, setMaskedEmail] = useState<string>('');
  const [devOtpHint, setDevOtpHint] = useState<string | null>(null);
  const [otpCode, setOtpCode] = useState('');
  
  // Navigation State
  const [mainModule, setMainModule] = useState<
    'dashboard' | 'employees' | 'attendance' | 'leave' | 'payroll' | 
    'recruitment' | 'performance' | 'assets' | 'organization' | 'reports' | 'admin'
  >('dashboard');

  // Notifications State
  const [notifications, setNotifications] = useState<any[]>([]);
  const [unreadCount, setUnreadCount] = useState<number>(0);
  const [showNotifDropdown, setShowNotifDropdown] = useState(false);

  // Check backend health
  useEffect(() => {
    axios.get('http://localhost:5219/health')
      .then(() => setApiOnline(true))
      .catch(() => setApiOnline(false));
  }, []);

  // Fetch notifications periodically when logged in
  const fetchNotifications = async () => {
    if (!accessToken) return;
    try {
      const headers = { Authorization: `Bearer ${accessToken}` };
      const [nRes, cRes] = await Promise.all([
        axios.get(`${API_BASE}/notifications`, { headers }),
        axios.get(`${API_BASE}/notifications/unread-count`, { headers })
      ]);
      if (nRes.data.success) setNotifications(nRes.data.data || []);
      if (cRes.data.success) setUnreadCount(cRes.data.data || 0);
    } catch (err) {
      console.error(err);
    }
  };

  useEffect(() => {
    if (accessToken) {
      fetchNotifications();
      const timer = setInterval(fetchNotifications, 20000);
      return () => clearInterval(timer);
    }
  }, [accessToken]);

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setInfoMsg(null);
    setLoading(true);

    try {
      const response = await axios.post<{ success: boolean; data: LoginResponseData; message: string }>(
        `${API_BASE}/auth/login`,
        { username, password }
      );

      if (response.data.success) {
        if (response.data.data.requiresTwoFactor) {
          setOtpStep(true);
          setChallengeToken(response.data.data.twoFactorChallenge || '');
          setMaskedEmail(response.data.data.maskedEmail || '');
          setDevOtpHint(response.data.data.devOtpHint || null);
          setInfoMsg(response.data.message || 'A 6-digit verification code has been dispatched to your email.');
        } else {
          setUser(response.data.data.user);
          setAccessToken(response.data.data.accessToken);
          localStorage.setItem('accessToken', response.data.data.accessToken);
          localStorage.setItem('refreshToken', response.data.data.refreshToken);
        }
      } else {
        setError(response.data.message || 'Login failed.');
      }
    } catch (err: any) {
      if (err.response?.data?.message) {
        setError(err.response.data.message);
      } else {
        setError('Network error connecting to API server at localhost:5219.');
      }
    } finally {
      setLoading(false);
    }
  };

  const handleVerifyOtp = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!otpCode || otpCode.trim().length !== 6) {
      setError('Please enter the full 6-digit verification code.');
      return;
    }
    setError(null);
    setInfoMsg(null);
    setLoading(true);

    try {
      const response = await axios.post<{ success: boolean; data: LoginResponseData; message: string }>(
        `${API_BASE}/auth/verify-otp`,
        { challengeToken, otpCode: otpCode.trim() }
      );

      if (response.data.success) {
        setUser(response.data.data.user);
        setAccessToken(response.data.data.accessToken);
        localStorage.setItem('accessToken', response.data.data.accessToken);
        localStorage.setItem('refreshToken', response.data.data.refreshToken);
        setOtpStep(false);
        setOtpCode('');
      } else {
        setError(response.data.message || 'Invalid verification code.');
      }
    } catch (err: any) {
      if (err.response?.data?.message) {
        setError(err.response.data.message);
      } else {
        setError('Verification failed. Please check the OTP code.');
      }
    } finally {
      setLoading(false);
    }
  };

  const handleResendOtp = async () => {
    if (!challengeToken) return;
    setError(null);
    setLoading(true);
    try {
      const response = await axios.post<{ success: boolean; data: LoginResponseData; message: string }>(
        `${API_BASE}/auth/resend-otp`,
        { challengeToken }
      );
      if (response.data.success) {
        setDevOtpHint(response.data.data.devOtpHint || null);
        setInfoMsg(response.data.message || 'A fresh verification code has been dispatched to your email.');
      } else {
        setError(response.data.message || 'Failed to resend code.');
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error resending verification code.');
    } finally {
      setLoading(false);
    }
  };

  const handleBackToLogin = () => {
    setOtpStep(false);
    setOtpCode('');
    setError(null);
    setInfoMsg(null);
  };

  const handleLogout = async () => {
    try {
      const refreshToken = localStorage.getItem('refreshToken');
      await axios.post(
        `${API_BASE}/auth/logout`,
        { refreshToken },
        { headers: { Authorization: `Bearer ${accessToken}` } }
      );
    } catch (err) {
      console.error(err);
    } finally {
      setUser(null);
      setAccessToken(null);
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
    }
  };

  const handleMarkAllRead = async () => {
    try {
      await axios.post(`${API_BASE}/notifications/read-all`, {}, {
        headers: { Authorization: `Bearer ${accessToken}` }
      });
      setUnreadCount(0);
      fetchNotifications();
    } catch (err) {
      console.error(err);
    }
  };

  const isSuperAdmin = user?.roles?.some(r => r.toLowerCase() === 'superadmin') || false;
  const isHRManager = !isSuperAdmin && (user?.roles?.some(r => r.toLowerCase() === 'hrmanager' || r.toLowerCase() === 'admin') || false);
  const isEmployee = !isSuperAdmin && !isHRManager;

  const autofillSuperAdmin = () => {
    setUsername('superadmin');
    setPassword('Admin@123456');
    setError(null);
  };

  const autofillHRManager = () => {
    setUsername('amina');
    setPassword('Password@123');
    setError(null);
  };

  const autofillEmployee = () => {
    setUsername('tariqul');
    setPassword('Password@123');
    setError(null);
  };

  const getModules = () => {
    if (isSuperAdmin) {
      return [
        { key: 'dashboard', label: 'I. Executive Dashboard', icon: LayoutDashboard },
        { key: 'employees', label: 'II. Workforce Directory', icon: Users },
        { key: 'attendance', label: 'III. Attendance Logs', icon: Clock },
        { key: 'leave', label: 'IV. Leave Allocation', icon: CalendarDays },
        { key: 'payroll', label: 'V. Payroll & Payslips', icon: Banknote },
        { key: 'recruitment', label: 'VI. Recruitment ATS', icon: Briefcase },
        { key: 'performance', label: 'VII. Performance Cycle', icon: Award },
        { key: 'assets', label: 'VIII. Asset Inventory', icon: Laptop },
        { key: 'organization', label: 'IX. Organization Tree', icon: Building2 },
        { key: 'reports', label: 'X. Reports & Registers', icon: FileSpreadsheet },
        { key: 'admin', label: 'XI. System Governance', icon: Shield }
      ];
    }
    if (isHRManager) {
      return [
        { key: 'dashboard', label: 'I. HR Management', icon: LayoutDashboard },
        { key: 'employees', label: 'II. Workforce Directory', icon: Users },
        { key: 'attendance', label: 'III. Attendance Records', icon: Clock },
        { key: 'leave', label: 'IV. Leave Administration', icon: CalendarDays },
        { key: 'payroll', label: 'V. Payroll Engine', icon: Banknote },
        { key: 'recruitment', label: 'VI. Recruitment ATS', icon: Briefcase },
        { key: 'performance', label: 'VII. Performance Cycle', icon: Award },
        { key: 'assets', label: 'VIII. Asset Custody', icon: Laptop },
        { key: 'organization', label: 'IX. Organization Structure', icon: Building2 },
        { key: 'reports', label: 'X. Reports & Registers', icon: FileSpreadsheet }
      ];
    }
    if (isEmployee) {
      return [
        { key: 'dashboard', label: 'I. Employee Self-Service', icon: LayoutDashboard },
        { key: 'attendance', label: 'II. My Attendance', icon: Clock },
        { key: 'leave', label: 'III. My Leaves', icon: CalendarDays },
        { key: 'payroll', label: 'IV. My Payslips', icon: Banknote },
        { key: 'assets', label: 'V. My Equipment', icon: Laptop },
        { key: 'performance', label: 'VI. My Performance', icon: Award }
      ];
    }
    return [
      { key: 'dashboard', label: 'I. Overview', icon: LayoutDashboard }
    ];
  };

  const modules = getModules();

  useEffect(() => {
    if (user) {
      const allowed = getModules().map(m => m.key);
      if (!allowed.includes(mainModule)) {
        setMainModule('dashboard');
      }
    }
  }, [user]);

  return (
    <div className="app-container">
      {/* Top Architectural Navbar */}
      <header className="top-nav">
        <div className="brand" onClick={() => user && setMainModule('dashboard')} style={{ cursor: 'pointer' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
            <Building2 size={24} color="#b69355" />
            <span style={{ fontFamily: 'var(--dp-typography-serif)', fontSize: '1.45rem', fontWeight: 600, letterSpacing: '0.04em', textTransform: 'uppercase' }}>
              HRMS Enterprise
            </span>
          </div>
          <span className="brand-badge">ARCHITECTURAL EDITION</span>
        </div>

        <div style={{ display: 'flex', alignItems: 'center', gap: '1rem' }}>
          {/* Notification Bell */}
          {user && (
            <div style={{ position: 'relative' }}>
              <button 
                type="button" 
                className="notification-bell-btn"
                onClick={() => setShowNotifDropdown(!showNotifDropdown)}
                aria-label="Notifications"
              >
                <Bell size={18} color="#262321" />
                {unreadCount > 0 && (
                  <span className="notification-badge">
                    {unreadCount > 9 ? '9+' : unreadCount}
                  </span>
                )}
              </button>

              {/* Notifications Dropdown Drawer */}
              {showNotifDropdown && (
                <div className="notification-dropdown">
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', padding: '0.75rem 1rem', borderBottom: '1px solid var(--border-subtle)' }}>
                    <h4 style={{ margin: 0, fontSize: '0.9rem', fontWeight: 700 }}>Notifications ({unreadCount} new)</h4>
                    <div style={{ display: 'flex', gap: '8px' }}>
                      <button 
                        type="button" 
                        className="text-btn" 
                        onClick={handleMarkAllRead}
                        style={{ fontSize: '0.75rem' }}
                      >
                        Mark all read
                      </button>
                      <button 
                        type="button" 
                        className="icon-close-btn" 
                        onClick={() => setShowNotifDropdown(false)}
                      >
                        <X size={14} />
                      </button>
                    </div>
                  </div>

                  <div style={{ maxHeight: '320px', overflowY: 'auto' }}>
                    {notifications.length > 0 ? (
                      notifications.map(n => (
                        <div 
                          key={n.id} 
                          style={{ 
                            padding: '0.75rem 1rem', 
                            borderBottom: '1px solid rgba(255,255,255,0.04)',
                            background: n.isRead ? 'transparent' : 'rgba(99, 102, 241, 0.08)'
                          }}
                        >
                          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.2rem' }}>
                            <span style={{ fontSize: '0.85rem', fontWeight: 700, color: n.isRead ? 'var(--text-muted)' : '#ffffff' }}>
                              {n.title}
                            </span>
                            <span style={{ fontSize: '0.7rem', color: 'var(--text-muted)' }}>
                              {new Date(n.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                            </span>
                          </div>
                          <p style={{ margin: 0, fontSize: '0.8rem', color: 'var(--text-muted)', lineHeight: 1.3 }}>
                            {n.message}
                          </p>
                        </div>
                      ))
                    ) : (
                      <div style={{ padding: '2rem', textAlign: 'center', color: 'var(--text-muted)', fontSize: '0.85rem' }}>
                        No notifications to display.
                      </div>
                    )}
                  </div>
                </div>
              )}
            </div>
          )}

          {/* API Health */}
          <div className="status-indicator">
            <span
              className="status-dot"
              style={{ background: apiOnline ? '#10b981' : '#f43f5e' }}
            />
            <span>API: {apiOnline ? 'Online (5219)' : 'Offline'}</span>
          </div>
        </div>
      </header>

      {/* Main Container */}
      <main className="main-content">
        {!user ? (
          /* Login Card */
          /* Login / 2FA Card */
          <div className="glass-card">
            {otpStep ? (
              /* Step 2: 2FA Email OTP Verification */
              <div>
                <div className="card-header">
                  <div style={{ 
                    width: 52, 
                    height: 52, 
                    background: 'var(--dp-color-panel)', 
                    color: 'var(--dp-color-accent-text)', 
                    border: '1px solid var(--dp-color-accent)',
                    display: 'flex', 
                    alignItems: 'center', 
                    justifyContent: 'center',
                    margin: '0 auto 1rem'
                  }}>
                    <Shield size={26} />
                  </div>
                  <h1 className="card-title">Two-Factor Authentication</h1>
                  <p className="card-subtitle">
                    Enter the 6-digit verification code sent to your registered email
                  </p>
                </div>

                {/* Email info banner */}
                <div style={{
                  background: 'var(--dp-color-panel)',
                  border: '1px solid var(--dp-color-panel-line)',
                  padding: '0.85rem 1rem',
                  marginBottom: '1.25rem',
                  display: 'flex',
                  alignItems: 'center',
                  gap: '10px'
                }}>
                  <Mail size={20} color="#b69355" style={{ flexShrink: 0 }} />
                  <div style={{ fontSize: '0.85rem' }}>
                    <span style={{ color: 'var(--dp-color-ink-2)' }}>Security Code dispatched to: </span>
                    <strong style={{ color: 'var(--dp-color-ink)', fontFamily: 'var(--font-mono)' }}>{maskedEmail || 'registered email'}</strong>
                  </div>
                </div>

                {devOtpHint && (
                  <div 
                    onClick={() => setOtpCode(devOtpHint)}
                    style={{
                      background: '#eaf4ed',
                      border: '1px solid #4a8a61',
                      padding: '0.65rem 1rem',
                      marginBottom: '1.25rem',
                      cursor: 'pointer',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'space-between',
                      fontSize: '0.8rem',
                      color: '#1e5e3a'
                    }}
                    title="Click to auto-fill code in development mode"
                  >
                    <span>⚡ <strong>Local Demo / Dev OTP:</strong></span>
                    <code style={{ fontSize: '1rem', fontWeight: 800, letterSpacing: '2px', background: 'rgba(0,0,0,0.06)', padding: '2px 8px' }}>
                      {devOtpHint}
                    </code>
                    <span style={{ fontSize: '0.75rem', textDecoration: 'underline' }}>Click to Fill</span>
                  </div>
                )}

                {infoMsg && !devOtpHint && (
                  <div style={{
                    background: 'var(--dp-color-panel)',
                    border: '1px solid var(--dp-color-accent)',
                    color: 'var(--dp-color-ink)',
                    padding: '0.65rem 1rem',
                    marginBottom: '1.25rem',
                    fontSize: '0.8rem'
                  }}>
                    {infoMsg}
                  </div>
                )}

                {error && (
                  <div className="alert-error" style={{ marginBottom: '1.25rem' }}>
                    <AlertCircle size={18} style={{ flexShrink: 0, marginTop: 2 }} />
                    <span>{error}</span>
                  </div>
                )}

                <form onSubmit={handleVerifyOtp}>
                  <div className="form-group">
                    <label className="form-label" style={{ textAlign: 'center', display: 'block', marginBottom: '0.5rem' }}>
                      6-Digit Security Code
                    </label>
                    <input
                      type="text"
                      className="input-field"
                      style={{
                        textAlign: 'center',
                        fontSize: '2rem',
                        letterSpacing: '14px',
                        fontFamily: 'var(--font-mono)',
                        fontWeight: 700,
                        height: '60px'
                      }}
                      maxLength={6}
                      placeholder="••••••"
                      value={otpCode}
                      onChange={(e) => setOtpCode(e.target.value.replace(/\D/g, ''))}
                      autoFocus
                      required
                    />
                  </div>

                  <button
                    type="submit"
                    className="btn-primary"
                    disabled={loading || otpCode.length !== 6}
                    style={{ marginTop: '1.5rem' }}
                  >
                    {loading ? 'Verifying Code...' : 'Verify & Enter Workspace'}
                  </button>

                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginTop: '1.25rem' }}>
                    <button
                      type="button"
                      onClick={handleBackToLogin}
                      className="text-btn"
                      style={{ display: 'flex', alignItems: 'center', gap: '4px', fontSize: '0.8rem' }}
                    >
                      <ArrowLeft size={14} /> Back to Sign In
                    </button>

                    <button
                      type="button"
                      onClick={handleResendOtp}
                      disabled={loading}
                      className="text-btn"
                      style={{ display: 'flex', alignItems: 'center', gap: '4px', fontSize: '0.8rem', color: '#b69355' }}
                    >
                      <RefreshCw size={14} className={loading ? 'animate-spin' : ''} /> Resend Code
                    </button>
                  </div>
                </form>
              </div>
            ) : (
              /* Step 1: Standard Credentials Form */
              <div>
                <div className="card-header">
                  <div style={{ 
                    width: 48, 
                    height: 48, 
                    background: 'var(--dp-color-field)', 
                    color: 'var(--dp-color-accent-light)', 
                    border: '1px solid var(--dp-color-accent)',
                    display: 'flex', 
                    alignItems: 'center', 
                    justifyContent: 'center',
                    margin: '0 auto 1rem'
                  }}>
                    <Lock size={22} />
                  </div>
                  <h1 className="card-title">Welcome back</h1>
                  <p className="card-subtitle">Sign in to your Human Resource Portal</p>
                </div>

                {error && (
                  <div className="alert-error">
                    <AlertCircle size={18} style={{ flexShrink: 0, marginTop: 2 }} />
                    <span>{error}</span>
                  </div>
                )}

                <div style={{ display: 'flex', flexDirection: 'column', gap: '6px', marginBottom: '1.25rem' }}>
                  <div style={{ fontSize: '0.75rem', fontWeight: 600, color: 'var(--text-muted)', textTransform: 'uppercase', letterSpacing: '0.05em' }}>
                    ⚡ One-Click Role Demos
                  </div>
                  <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: '6px' }}>
                    <button 
                      type="button" 
                      className="demo-badge-btn"
                      onClick={autofillSuperAdmin}
                      style={{ padding: '0.5rem 0.6rem', fontSize: '0.75rem', justifyContent: 'center' }}
                      title="Full Executive & System Access"
                    >
                      👑 <strong>SuperAdmin</strong>
                    </button>
                    <button 
                      type="button" 
                      className="demo-badge-btn"
                      onClick={autofillHRManager}
                      style={{ padding: '0.5rem 0.6rem', fontSize: '0.75rem', justifyContent: 'center' }}
                      title="People Operations & HR Management"
                    >
                      💼 <strong>HR Manager</strong>
                    </button>
                    <button 
                      type="button" 
                      className="demo-badge-btn"
                      onClick={autofillEmployee}
                      style={{ padding: '0.5rem 0.6rem', fontSize: '0.75rem', justifyContent: 'center' }}
                      title="Employee Self-Service (ESS)"
                    >
                      👤 <strong>Employee</strong>
                    </button>
                  </div>
                </div>

                <form onSubmit={handleLogin}>
                  <div className="form-group">
                    <label className="form-label" htmlFor="username">Username or Corporate Email</label>
                    <div className="input-wrapper">
                      <UserIcon size={18} className="input-icon" />
                      <input
                        id="username"
                        type="text"
                        className="input-field"
                        placeholder="e.g. superadmin or admin@hrms.local"
                        value={username}
                        onChange={(e) => setUsername(e.target.value)}
                        required
                      />
                    </div>
                  </div>

                  <div className="form-group">
                    <label className="form-label" htmlFor="password">Password</label>
                    <div className="input-wrapper">
                      <KeyRound size={18} className="input-icon" />
                      <input
                        id="password"
                        type={showPassword ? 'text' : 'password'}
                        className="input-field"
                        placeholder="Enter password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        required
                      />
                      <button
                        type="button"
                        className="toggle-password"
                        onClick={() => setShowPassword(!showPassword)}
                        tabIndex={-1}
                      >
                        {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                      </button>
                    </div>
                  </div>

                  <button
                    type="submit"
                    className="btn-primary"
                    disabled={loading}
                    style={{ marginTop: '1.75rem' }}
                  >
                    {loading ? 'Authenticating...' : 'Sign In to Workspace'}
                  </button>
                </form>
              </div>
            )}
          </div>
        ) : (
          /* Authenticated State Card */
          <div className="glass-card dashboard-card">
            
            {/* User Profile Header */}
            <div className="user-badge-header">
              <div className="user-avatar-group">
                <div className="avatar">
                  {user.username.substring(0, 2).toUpperCase()}
                </div>
                <div>
                  <h2 style={{ fontSize: '1.25rem', fontWeight: 700 }}>
                    {user.fullName || user.username}
                  </h2>
                  <p style={{ color: 'var(--text-muted)', fontSize: '0.875rem' }}>{user.email}</p>
                  <span className="role-tag">Role: {user.roles.join(', ') || 'Employee'}</span>
                </div>
              </div>

              <button 
                type="button" 
                className="btn-secondary" 
                onClick={handleLogout}
                style={{ display: 'flex', alignItems: 'center', gap: '6px' }}
              >
                <LogOut size={16} /> Logout
              </button>
            </div>

            {/* Enterprise Module Switcher Tabs */}
            <nav className="module-nav-bar" aria-label="Main Navigation">
              {modules.map(mod => {
                const IconComponent = mod.icon;
                const isActive = mainModule === mod.key;
                return (
                  <button
                    key={mod.key}
                    type="button"
                    onClick={() => setMainModule(mod.key as any)}
                    className={`nav-tab-btn ${isActive ? 'active' : ''}`}
                  >
                    <IconComponent size={16} color={isActive ? '#d2b071' : 'currentColor'} />
                    <span>{mod.label}</span>
                  </button>
                );
              })}
            </nav>

            {/* Module Views */}
            <div className="module-view-container" style={{ marginTop: '1.5rem' }}>
              {accessToken && mainModule === 'dashboard' && (
                <Dashboard token={accessToken} user={user} onNavigate={setMainModule} />
              )}
              {accessToken && mainModule === 'employees' && (
                <Employees token={accessToken} />
              )}
              {accessToken && mainModule === 'attendance' && (
                <Attendance token={accessToken} user={user} />
              )}
              {accessToken && mainModule === 'leave' && (
                <Leave token={accessToken} user={user} />
              )}
              {accessToken && mainModule === 'payroll' && (
                <Payroll token={accessToken} user={user} />
              )}
              {accessToken && mainModule === 'recruitment' && (
                <Recruitment token={accessToken} />
              )}
              {accessToken && mainModule === 'performance' && (
                <Performance token={accessToken} />
              )}
              {accessToken && mainModule === 'assets' && (
                <Assets token={accessToken} user={user} />
              )}
              {accessToken && mainModule === 'organization' && (
                <Organization token={accessToken} />
              )}
              {accessToken && mainModule === 'reports' && (
                <Reports token={accessToken} />
              )}
              {accessToken && mainModule === 'admin' && (
                <Administration token={accessToken} />
              )}
            </div>

          </div>
        )}
      </main>
    </div>
  );
}
