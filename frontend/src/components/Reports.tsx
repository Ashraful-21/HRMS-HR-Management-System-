import { useState, useEffect } from 'react';
import axios from 'axios';
import { 
  FileSpreadsheet, 
  Calendar, 
  Clock, 
  Banknote, 
  Printer, 
  AlertCircle 
} from 'lucide-react';

interface ReportsProps {
  token: string;
}

const API_BASE = 'http://localhost:5219/api/v1';

export default function Reports({ token }: ReportsProps) {
  const [reportType, setReportType] = useState<'attendance' | 'leaves' | 'payroll'>('attendance');
  const [month, setMonth] = useState<number>(new Date().getMonth() + 1);
  const [year, setYear] = useState<number>(new Date().getFullYear());
  
  const [data, setData] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const headers = { Authorization: `Bearer ${token}` };

  const fetchReport = async () => {
    setLoading(true);
    setError(null);
    try {
      let url = '';
      if (reportType === 'attendance') {
        url = `${API_BASE}/reports/attendance?month=${month}&year=${year}`;
      } else if (reportType === 'leaves') {
        url = `${API_BASE}/reports/leaves?year=${year}`;
      } else if (reportType === 'payroll') {
        url = `${API_BASE}/reports/payroll?month=${month}&year=${year}`;
      }

      const res = await axios.get(url, { headers });
      if (res.data.success) {
        setData(res.data.data || []);
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to fetch report data.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchReport();
  }, [reportType, month, year, token]);

  const handlePrint = () => {
    window.print();
  };

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '1.25rem' }}>
      
      {/* Title & Controls */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: '1rem' }}>
        <div>
          <h2 style={{ fontSize: '1.4rem', fontWeight: 800, margin: 0, display: 'flex', alignItems: 'center', gap: '8px' }}>
            <FileSpreadsheet size={22} color="#10b981" />
            Executive Reports &amp; Analytics Ledger
          </h2>
          <p style={{ margin: '0.25rem 0 0', color: 'var(--text-muted)', fontSize: '0.85rem' }}>
            Auditable attendance logs, leave balance tracking, and payroll disbursement statements.
          </p>
        </div>

        <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', flexWrap: 'wrap' }}>
          {reportType !== 'leaves' && (
            <select 
              className="input-field" 
              style={{ width: 'auto' }}
              value={month} 
              onChange={e => setMonth(parseInt(e.target.value))}
            >
              {[1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12].map(m => (
                <option key={m} value={m}>
                  {new Date(2026, m - 1).toLocaleString('default', { month: 'long' })}
                </option>
              ))}
            </select>
          )}

          <select 
            className="input-field" 
            style={{ width: 'auto' }}
            value={year} 
            onChange={e => setYear(parseInt(e.target.value))}
          >
            {[2024, 2025, 2026, 2027].map(y => (
              <option key={y} value={y}>{y}</option>
            ))}
          </select>

          <button type="button" className="btn-secondary" onClick={handlePrint} style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
            <Printer size={15} /> Print / Export PDF
          </button>
        </div>
      </div>

      {/* Report Switcher Tabs */}
      <div className="subnav-tabs">
        <button 
          type="button" 
          className={`subnav-btn ${reportType === 'attendance' ? 'active' : ''}`}
          onClick={() => setReportType('attendance')}
        >
          <Clock size={16} /> Monthly Attendance Ledger
        </button>
        <button 
          type="button" 
          className={`subnav-btn ${reportType === 'leaves' ? 'active' : ''}`}
          onClick={() => setReportType('leaves')}
        >
          <Calendar size={16} /> Annual Leave Utilization
        </button>
        <button 
          type="button" 
          className={`subnav-btn ${reportType === 'payroll' ? 'active' : ''}`}
          onClick={() => setReportType('payroll')}
        >
          <Banknote size={16} /> Monthly Payroll Disbursement
        </button>
      </div>

      {error && (
        <div className="alert-error">
          <AlertCircle size={18} />
          <span>{error}</span>
        </div>
      )}

      {loading && (
        <div style={{ textAlign: 'center', padding: '1.5rem', color: 'var(--text-muted)' }}>
          Generating report data...
        </div>
      )}

      {/* REPORT 1: ATTENDANCE */}
      {reportType === 'attendance' && (
        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>Employee</th>
                <th>Department</th>
                <th>Calendar Days</th>
                <th>Present Days</th>
                <th>Late Marks</th>
                <th>Absent Days</th>
                <th>Overtime (Hrs)</th>
                <th>Attendance %</th>
              </tr>
            </thead>
            <tbody>
              {data.length > 0 ? (
                data.map((item, idx) => (
                  <tr key={item.employeeId || idx}>
                    <td>
                      <strong>{item.fullName}</strong>
                      <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>{item.employeeCode}</div>
                    </td>
                    <td>{item.departmentName}</td>
                    <td>{item.totalDays}</td>
                    <td><strong style={{ color: '#10b981' }}>{item.presentDays}</strong></td>
                    <td><span style={{ color: '#f59e0b' }}>{item.lateDays}</span></td>
                    <td><span style={{ color: '#f43f5e' }}>{item.absentDays}</span></td>
                    <td>{item.totalOvertimeHours} hrs</td>
                    <td>
                      <span className={`badge-pill ${item.attendancePercentage >= 90 ? 'badge-success' : item.attendancePercentage >= 75 ? 'badge-primary' : 'badge-danger'}`}>
                        {item.attendancePercentage}%
                      </span>
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={8} style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-muted)' }}>
                    No attendance records for the selected period.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      )}

      {/* REPORT 2: LEAVE */}
      {reportType === 'leaves' && (
        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>Employee</th>
                <th>Department</th>
                <th>Leave Policy Category</th>
                <th>Annual Entitlement</th>
                <th>Utilized Days</th>
                <th>Remaining Balance</th>
              </tr>
            </thead>
            <tbody>
              {data.length > 0 ? (
                data.map((item, idx) => (
                  <tr key={idx}>
                    <td>
                      <strong>{item.fullName}</strong>
                      <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>{item.employeeCode}</div>
                    </td>
                    <td>{item.departmentName}</td>
                    <td><span className="badge-pill badge-primary">{item.leaveType}</span></td>
                    <td>{item.totalAllowed} days</td>
                    <td><span style={{ color: '#f59e0b', fontWeight: 600 }}>{item.usedDays} days</span></td>
                    <td><strong style={{ color: '#10b981' }}>{item.balance} days</strong></td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={6} style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-muted)' }}>
                    No leave balances recorded for {year}.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      )}

      {/* REPORT 3: PAYROLL */}
      {reportType === 'payroll' && (
        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>Employee</th>
                <th>Department &amp; Title</th>
                <th>Gross Earnings</th>
                <th>Total Deductions</th>
                <th>Net Disbursement</th>
                <th>Period</th>
              </tr>
            </thead>
            <tbody>
              {data.length > 0 ? (
                data.map((item, idx) => (
                  <tr key={idx}>
                    <td>
                      <strong>{item.fullName}</strong>
                      <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>{item.employeeCode}</div>
                    </td>
                    <td>
                      <div>{item.designationTitle}</div>
                      <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>{item.departmentName}</div>
                    </td>
                    <td>${item.grossEarnings?.toLocaleString()}</td>
                    <td style={{ color: '#f43f5e' }}>-${item.totalDeductions?.toLocaleString()}</td>
                    <td><strong style={{ color: '#10b981', fontSize: '1rem' }}>${item.netPay?.toLocaleString()}</strong></td>
                    <td>{item.month}/{item.year}</td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={6} style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-muted)' }}>
                    No payroll execution records for {month}/{year}.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      )}

    </div>
  );
}
