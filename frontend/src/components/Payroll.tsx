import { useState, useEffect } from 'react';
import axios from 'axios';
import {
  Banknote,
  Calendar,
  CheckCircle2,
  Download,
  FileText,
  Filter,
  Layers,
  PlusCircle,
  RefreshCw,
  Search,
  Sliders,
  TrendingUp,
  UserCheck,
  AlertCircle,
  DollarSign
} from 'lucide-react';

interface PayrollProps {
  token: string;
  user?: any;
}

interface SalaryComponent {
  id: number;
  name: string;
  code: string;
  type: string;
  isFixed: boolean;
  isTaxable: boolean;
}

interface PayrollRunDetail {
  id: number;
  payrollRunId: number;
  employeeId: number;
  employeeName: string;
  employeeCode: string;
  departmentName?: string;
  designationTitle?: string;
  workingDays: number;
  presentDays: number;
  paidLeaveDays: number;
  unpaidLeaveDays: number;
  overtimeMinutes: number;
  grossEarnings: number;
  totalDeductions: number;
  netPay: number;
  lossOfPay: number;
  components: {
    componentId: number;
    componentName: string;
    componentCode: string;
    componentType: string;
    amount: number;
  }[];
}

interface PayrollRunSummary {
  id: number;
  month: number;
  year: number;
  branchId?: number;
  branchName?: string;
  status: 'Draft' | 'Approved' | 'Paid';
  workingDays: number;
  totalEmployees: number;
  totalGross: number;
  totalDeductions: number;
  totalNetPay: number;
  processedAt?: string;
  approvedAt?: string;
  paidAt?: string;
  remarks?: string;
  details: PayrollRunDetail[];
}

interface SalaryStructureComponent {
  id: number;
  componentId: number;
  componentName: string;
  componentCode: string;
  componentType: string;
  calculationType: string;
  amount: number;
  percentageBase?: string;
  calculatedMonthlyAmount: number;
}

interface SalaryStructure {
  id: number;
  employeeId: number;
  employeeName: string;
  employeeCode: string;
  grossSalary: number;
  effectiveFrom: string;
  isCurrent: boolean;
  remarks?: string;
  components: SalaryStructureComponent[];
}

interface TaxCalculationResult {
  monthlyGross: number;
  annualGross: number;
  taxExemptAmount: number;
  taxableIncome: number;
  estimatedAnnualTax: number;
  monthlyTds: number;
  effectiveTaxRatePercentage: number;
}

const API_BASE = 'http://localhost:5219/api/v1';

export default function Payroll({ token, user }: PayrollProps) {
  const isSuperAdmin = user?.roles?.some((r: string) => r.toLowerCase() === 'superadmin') || false;
  const isHRManager = !isSuperAdmin && (user?.roles?.some((r: string) => r.toLowerCase() === 'hrmanager' || r.toLowerCase() === 'admin') || false);
  const isEmployee = !isSuperAdmin && !isHRManager;

  const [activeTab, setActiveTab] = useState<'runs' | 'structures' | 'payslips' | 'tax-calc'>(isEmployee ? 'payslips' : 'runs');
  const [loading, setLoading] = useState(false);
  const [successMsg, setSuccessMsg] = useState<string | null>(null);
  const [errorMsg, setErrorMsg] = useState<string | null>(null);

  // Payroll Runs state
  const [runs, setRuns] = useState<PayrollRunSummary[]>([]);
  const [selectedRun, setSelectedRun] = useState<PayrollRunSummary | null>(null);
  const [selectedDetail, setSelectedDetail] = useState<PayrollRunDetail | null>(null);

  // New Run Dialog state
  const [showProcessModal, setShowProcessModal] = useState(false);
  const [processMonth, setProcessMonth] = useState(new Date().getMonth() + 1);
  const [processYear, setProcessYear] = useState(new Date().getFullYear());
  const [workingDays, setWorkingDays] = useState(22);
  const [processRemarks, setProcessRemarks] = useState('Monthly Payroll Disbursement');

  // Salary Components & Structures state
  const [components, setComponents] = useState<SalaryComponent[]>([]);
  const [structureEmployeeId, setStructureEmployeeId] = useState<number>(1);
  const [salaryStructure, setSalaryStructure] = useState<SalaryStructure | null>(null);
  const [structureGross, setStructureGross] = useState<number>(80000);
  const [showAssignModal, setShowAssignModal] = useState(false);

  // Tax Calculator state
  const [taxInputGross, setTaxInputGross] = useState<number>(85000);
  const [taxEstimate, setTaxEstimate] = useState<TaxCalculationResult | null>(null);

  // Payslip Filter
  const [payslipSearch, setPayslipSearch] = useState('');

  const axiosHeaders = { headers: { Authorization: `Bearer ${token}` } };

  const showNotification = (msg: string, isError = false) => {
    if (isError) {
      setErrorMsg(msg);
      setTimeout(() => setErrorMsg(null), 5000);
    } else {
      setSuccessMsg(msg);
      setTimeout(() => setSuccessMsg(null), 4000);
    }
  };

  // Initial Data Load
  const fetchPayrollRuns = async () => {
    setLoading(true);
    try {
      const res = await axios.get<{ success: boolean; data: PayrollRunSummary[] }>(
        `${API_BASE}/payroll/runs`,
        axiosHeaders
      );
      if (res.data.success) {
        setRuns(res.data.data);
        if (res.data.data.length > 0) {
          // Default select the latest run
          setSelectedRun(res.data.data[0]);
        }
      }
    } catch (err: any) {
      showNotification(err.response?.data?.message || 'Failed to fetch payroll runs', true);
    } finally {
      setLoading(false);
    }
  };

  const fetchComponents = async () => {
    try {
      const res = await axios.get<{ success: boolean; data: SalaryComponent[] }>(
        `${API_BASE}/payroll/components`,
        axiosHeaders
      );
      if (res.data.success) {
        setComponents(res.data.data);
      }
    } catch (err) {
      console.error('Failed to load salary components', err);
    }
  };

  const fetchSalaryStructure = async (empId: number) => {
    try {
      const res = await axios.get<{ success: boolean; data: SalaryStructure }>(
        `${API_BASE}/payroll/structures/${empId}`,
        axiosHeaders
      );
      if (res.data.success) {
        setSalaryStructure(res.data.data);
        setStructureGross(res.data.data.grossSalary);
      }
    } catch (err: any) {
      console.error(err);
    }
  };

  const fetchTaxEstimate = async (gross: number) => {
    try {
      const res = await axios.get<{ success: boolean; data: TaxCalculationResult }>(
        `${API_BASE}/payroll/tax-estimate?monthlyGross=${gross}`,
        axiosHeaders
      );
      if (res.data.success) {
        setTaxEstimate(res.data.data);
      }
    } catch (err: any) {
      console.error(err);
    }
  };

  useEffect(() => {
    fetchPayrollRuns();
    fetchComponents();
    fetchSalaryStructure(1);
    fetchTaxEstimate(85000);
  }, [token]);

  // Handle Process Payroll Batch
  const handleProcessPayroll = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      const res = await axios.post<{ success: boolean; data: PayrollRunSummary }>(
        `${API_BASE}/payroll/runs`,
        {
          month: Number(processMonth),
          year: Number(processYear),
          workingDays: Number(workingDays),
          remarks: processRemarks
        },
        axiosHeaders
      );

      if (res.data.success) {
        showNotification(`Payroll processed successfully for ${processMonth}/${processYear}! Total Net: ৳${res.data.data.totalNetPay.toLocaleString()}`);
        setShowProcessModal(false);
        await fetchPayrollRuns();
        setSelectedRun(res.data.data);
      }
    } catch (err: any) {
      showNotification(err.response?.data?.message || 'Payroll processing failed.', true);
    } finally {
      setLoading(false);
    }
  };

  // Handle Approve Run
  const handleApproveRun = async (runId: number) => {
    try {
      const res = await axios.post<{ success: boolean; data: PayrollRunSummary }>(
        `${API_BASE}/payroll/runs/${runId}/approve`,
        {},
        axiosHeaders
      );
      if (res.data.success) {
        showNotification('Payroll run approved successfully!');
        await fetchPayrollRuns();
        if (selectedRun?.id === runId) setSelectedRun(res.data.data);
      }
    } catch (err: any) {
      showNotification(err.response?.data?.message || 'Approval failed.', true);
    }
  };

  // Handle Mark Paid
  const handleMarkPaid = async (runId: number) => {
    try {
      const res = await axios.post<{ success: boolean; data: PayrollRunSummary }>(
        `${API_BASE}/payroll/runs/${runId}/pay`,
        {},
        axiosHeaders
      );
      if (res.data.success) {
        showNotification('Payroll marked as disbursed and Paid!');
        await fetchPayrollRuns();
        if (selectedRun?.id === runId) setSelectedRun(res.data.data);
      }
    } catch (err: any) {
      showNotification(err.response?.data?.message || 'Payment mark failed.', true);
    }
  };

  // Handle Download Payslip PDF
  const handleDownloadPdf = async (detailId: number, empName: string, month: number, year: number) => {
    try {
      const response = await axios.get(
        `${API_BASE}/payroll/payslips/${detailId}/pdf`,
        {
          headers: { Authorization: `Bearer ${token}` },
          responseType: 'blob'
        }
      );

      const blob = new Blob([response.data], { type: 'application/pdf' });
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `Payslip_${empName.replace(/\s+/g, '_')}_${month}_${year}.pdf`);
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(url);
      showNotification(`Payslip PDF downloaded for ${empName}`);
    } catch (err: any) {
      showNotification('Failed to download PDF payslip.', true);
    }
  };

  // Handle Assign Salary Structure
  const handleAssignStructure = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      const payload = {
        employeeId: structureEmployeeId,
        grossSalary: Number(structureGross),
        effectiveFrom: new Date().toISOString().split('T')[0],
        remarks: 'Updated Compensation Package',
        components: [
          { componentId: 1, calculationType: 'Percentage', amount: 50, percentageBase: 'Gross' },
          { componentId: 2, calculationType: 'Percentage', amount: 50, percentageBase: 'Basic' },
          { componentId: 3, calculationType: 'Percentage', amount: 20, percentageBase: 'Basic' },
          { componentId: 4, calculationType: 'Percentage', amount: 10, percentageBase: 'Basic' },
          { componentId: 6, calculationType: 'Percentage', amount: 10, percentageBase: 'Basic' },
          { componentId: 7, calculationType: 'Percentage', amount: 5, percentageBase: 'Basic' }
        ]
      };

      const res = await axios.post<{ success: boolean; data: SalaryStructure }>(
        `${API_BASE}/payroll/structures`,
        payload,
        axiosHeaders
      );

      if (res.data.success) {
        showNotification('Salary structure updated successfully!');
        setShowAssignModal(false);
        setSalaryStructure(res.data.data);
      }
    } catch (err: any) {
      showNotification(err.response?.data?.message || 'Failed to update salary structure.', true);
    } finally {
      setLoading(false);
    }
  };

  // Filtered payslips
  const filteredDetails = selectedRun?.details.filter(d => 
    d.employeeName.toLowerCase().includes(payslipSearch.toLowerCase()) ||
    d.employeeCode.toLowerCase().includes(payslipSearch.toLowerCase()) ||
    (d.departmentName && d.departmentName.toLowerCase().includes(payslipSearch.toLowerCase()))
  ) || [];

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Paid': return { bg: 'rgba(16, 185, 129, 0.15)', text: '#34d399', border: 'rgba(16, 185, 129, 0.3)' };
      case 'Approved': return { bg: 'rgba(59, 130, 246, 0.15)', text: '#60a5fa', border: 'rgba(59, 130, 246, 0.3)' };
      default: return { bg: 'rgba(245, 158, 11, 0.15)', text: '#fbbf24', border: 'rgba(245, 158, 11, 0.3)' };
    }
  };

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '1.5rem', width: '100%' }}>
      {/* Notifications */}
      {successMsg && (
        <div style={{
          padding: '0.875rem 1.25rem',
          borderRadius: 'var(--radius-md)',
          background: 'rgba(16, 185, 129, 0.15)',
          border: '1px solid rgba(16, 185, 129, 0.3)',
          color: '#34d399',
          display: 'flex',
          alignItems: 'center',
          gap: '10px'
        }}>
          <CheckCircle2 size={18} />
          <span style={{ fontWeight: 500 }}>{successMsg}</span>
        </div>
      )}
      {errorMsg && (
        <div style={{
          padding: '0.875rem 1.25rem',
          borderRadius: 'var(--radius-md)',
          background: 'rgba(239, 68, 68, 0.15)',
          border: '1px solid rgba(239, 68, 68, 0.3)',
          color: '#f87171',
          display: 'flex',
          alignItems: 'center',
          gap: '10px'
        }}>
          <AlertCircle size={18} />
          <span style={{ fontWeight: 500 }}>{errorMsg}</span>
        </div>
      )}

      {/* Top Banner & KPI Metrics */}
      {!isEmployee ? (
      <div style={{
        display: 'grid',
        gridTemplateColumns: 'repeat(auto-fit, minmax(240px, 1fr))',
        gap: '1rem'
      }}>
        <div style={{
          background: 'linear-gradient(135deg, rgba(99, 102, 241, 0.12) 0%, rgba(99, 102, 241, 0.03) 100%)',
          padding: '1.25rem',
          borderRadius: 'var(--radius-md)',
          border: '1px solid rgba(99, 102, 241, 0.25)',
          display: 'flex',
          alignItems: 'center',
          gap: '1rem'
        }}>
          <div style={{
            width: 44,
            height: 44,
            borderRadius: 10,
            background: 'rgba(99, 102, 241, 0.2)',
            color: '#818cf8',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center'
          }}>
            <Banknote size={22} />
          </div>
          <div>
            <div style={{ fontSize: '0.8125rem', color: 'var(--text-muted)', fontWeight: 500 }}>Latest Net Pay</div>
            <div style={{ fontSize: '1.5rem', fontWeight: 700, color: '#ffffff' }}>
              ৳{runs.length > 0 ? runs[0].totalNetPay.toLocaleString(undefined, { minimumFractionDigits: 2 }) : '0.00'}
            </div>
          </div>
        </div>

        <div style={{
          background: 'linear-gradient(135deg, rgba(16, 185, 129, 0.12) 0%, rgba(16, 185, 129, 0.03) 100%)',
          padding: '1.25rem',
          borderRadius: 'var(--radius-md)',
          border: '1px solid rgba(16, 185, 129, 0.25)',
          display: 'flex',
          alignItems: 'center',
          gap: '1rem'
        }}>
          <div style={{
            width: 44,
            height: 44,
            borderRadius: 10,
            background: 'rgba(16, 185, 129, 0.2)',
            color: '#34d399',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center'
          }}>
            <TrendingUp size={22} />
          </div>
          <div>
            <div style={{ fontSize: '0.8125rem', color: 'var(--text-muted)', fontWeight: 500 }}>Total Batches Run</div>
            <div style={{ fontSize: '1.5rem', fontWeight: 700, color: '#ffffff' }}>
              {runs.length} Runs
            </div>
          </div>
        </div>

        <div style={{
          background: 'linear-gradient(135deg, rgba(245, 158, 11, 0.12) 0%, rgba(245, 158, 11, 0.03) 100%)',
          padding: '1.25rem',
          borderRadius: 'var(--radius-md)',
          border: '1px solid rgba(245, 158, 11, 0.25)',
          display: 'flex',
          alignItems: 'center',
          gap: '1rem'
        }}>
          <div style={{
            width: 44,
            height: 44,
            borderRadius: 10,
            background: 'rgba(245, 158, 11, 0.2)',
            color: '#fbbf24',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center'
          }}>
            <Layers size={22} />
          </div>
          <div>
            <div style={{ fontSize: '0.8125rem', color: 'var(--text-muted)', fontWeight: 500 }}>Salary Components</div>
            <div style={{ fontSize: '1.5rem', fontWeight: 700, color: '#ffffff' }}>
              {components.length} Active
            </div>
          </div>
        </div>

        <div style={{
          background: 'linear-gradient(135deg, rgba(236, 72, 153, 0.12) 0%, rgba(236, 72, 153, 0.03) 100%)',
          padding: '1.25rem',
          borderRadius: 'var(--radius-md)',
          border: '1px solid rgba(236, 72, 153, 0.25)',
          display: 'flex',
          alignItems: 'center',
          gap: '1rem'
        }}>
          <div style={{
            width: 44,
            height: 44,
            borderRadius: 10,
            background: 'rgba(236, 72, 153, 0.2)',
            color: '#f472b6',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center'
          }}>
            <FileText size={22} />
          </div>
          <div>
            <div style={{ fontSize: '0.8125rem', color: 'var(--text-muted)', fontWeight: 500 }}>Payslip Engine</div>
            <div style={{ fontSize: '1.5rem', fontWeight: 700, color: '#ffffff' }}>
              QuestPDF 2026
            </div>
          </div>
        </div>
      </div>
      ) : (
        /* Employee Personal Compensation Banner */
        <div style={{
          background: 'linear-gradient(135deg, rgba(16, 185, 129, 0.12) 0%, rgba(6, 182, 212, 0.08) 100%)',
          padding: '1.25rem 1.5rem',
          borderRadius: 'var(--radius-md)',
          border: '1px solid rgba(16, 185, 129, 0.25)',
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          flexWrap: 'wrap',
          gap: '1rem'
        }}>
          <div>
            <h3 style={{ fontSize: '1.25rem', fontWeight: 700, margin: '0 0 0.25rem', color: '#ffffff' }}>
              My Compensation &amp; Payslips
            </h3>
            <p style={{ margin: 0, color: 'var(--text-muted)', fontSize: '0.85rem' }}>
              Access, inspect, and download official QuestPDF salary payslips for your employment.
            </p>
          </div>
          <div style={{ display: 'flex', gap: '8px' }}>
            <span className="badge-pill badge-success">Direct Deposit Active</span>
          </div>
        </div>
      )}

      {/* Sub Tabs Navigation */}
      <div style={{
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
        borderBottom: '1px solid var(--border-subtle)',
        paddingBottom: '0.75rem'
      }}>
        <div style={{ display: 'flex', gap: '0.5rem' }}>
          {!isEmployee && (
            <>
              <button
                type="button"
                onClick={() => setActiveTab('runs')}
                style={{
                  display: 'flex',
                  alignItems: 'center',
                  gap: '8px',
                  padding: '0.5rem 1rem',
                  borderRadius: 'var(--radius-sm)',
                  fontSize: '0.875rem',
                  fontWeight: 600,
                  cursor: 'pointer',
                  border: activeTab === 'runs' ? '1px solid #6366f1' : '1px solid transparent',
                  background: activeTab === 'runs' ? 'rgba(99, 102, 241, 0.15)' : 'transparent',
                  color: activeTab === 'runs' ? '#818cf8' : 'var(--text-muted)'
                }}
              >
                <Banknote size={16} />
                <span>Payroll Batches</span>
              </button>

              <button
                type="button"
                onClick={() => setActiveTab('structures')}
                style={{
                  display: 'flex',
                  alignItems: 'center',
                  gap: '8px',
                  padding: '0.5rem 1rem',
                  borderRadius: 'var(--radius-sm)',
                  fontSize: '0.875rem',
                  fontWeight: 600,
                  cursor: 'pointer',
                  border: activeTab === 'structures' ? '1px solid #6366f1' : '1px solid transparent',
                  background: activeTab === 'structures' ? 'rgba(99, 102, 241, 0.15)' : 'transparent',
                  color: activeTab === 'structures' ? '#818cf8' : 'var(--text-muted)'
                }}
              >
                <Sliders size={16} />
                <span>Salary Structures</span>
              </button>
            </>
          )}

          <button
            type="button"
            onClick={() => setActiveTab('payslips')}
            style={{
              display: 'flex',
              alignItems: 'center',
              gap: '8px',
              padding: '0.5rem 1rem',
              borderRadius: 'var(--radius-sm)',
              fontSize: '0.875rem',
              fontWeight: 600,
              cursor: 'pointer',
              border: activeTab === 'payslips' ? '1px solid #6366f1' : '1px solid transparent',
              background: activeTab === 'payslips' ? 'rgba(99, 102, 241, 0.15)' : 'transparent',
              color: activeTab === 'payslips' ? '#818cf8' : 'var(--text-muted)'
            }}
          >
            <FileText size={16} />
            <span>Payslip Explorer</span>
          </button>

          <button
            type="button"
            onClick={() => setActiveTab('tax-calc')}
            style={{
              display: 'flex',
              alignItems: 'center',
              gap: '8px',
              padding: '0.5rem 1rem',
              borderRadius: 'var(--radius-sm)',
              fontSize: '0.875rem',
              fontWeight: 600,
              cursor: 'pointer',
              border: activeTab === 'tax-calc' ? '1px solid #6366f1' : '1px solid transparent',
              background: activeTab === 'tax-calc' ? 'rgba(99, 102, 241, 0.15)' : 'transparent',
              color: activeTab === 'tax-calc' ? '#818cf8' : 'var(--text-muted)'
            }}
          >
            <DollarSign size={16} />
            <span>Tax Bracket Estimator</span>
          </button>
        </div>

        {activeTab === 'runs' && (
          <button
            type="button"
            onClick={() => setShowProcessModal(true)}
            style={{
              display: 'flex',
              alignItems: 'center',
              gap: '8px',
              padding: '0.55rem 1.15rem',
              borderRadius: 'var(--radius-sm)',
              background: 'linear-gradient(135deg, #6366f1 0%, #4f46e5 100%)',
              color: '#ffffff',
              border: 'none',
              fontSize: '0.875rem',
              fontWeight: 600,
              cursor: 'pointer',
              boxShadow: '0 4px 12px rgba(99, 102, 241, 0.25)'
            }}
          >
            <PlusCircle size={16} />
            <span>Process New Payroll</span>
          </button>
        )}
      </div>

      {/* TAB 1: PAYROLL RUNS */}
      {activeTab === 'runs' && (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '1.25rem' }}>
          <div style={{
            background: 'var(--card-bg)',
            borderRadius: 'var(--radius-md)',
            border: '1px solid var(--border-subtle)',
            overflow: 'hidden'
          }}>
            <div style={{
              padding: '1rem 1.25rem',
              borderBottom: '1px solid var(--border-subtle)',
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'center'
            }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                <Banknote size={18} color="#818cf8" />
                <h3 style={{ fontSize: '1rem', fontWeight: 600, margin: 0 }}>Payroll Batches Overview</h3>
              </div>
              <button
                type="button"
                onClick={fetchPayrollRuns}
                style={{
                  background: 'transparent',
                  border: '1px solid var(--border-subtle)',
                  borderRadius: 'var(--radius-sm)',
                  color: 'var(--text-muted)',
                  padding: '0.4rem 0.75rem',
                  display: 'flex',
                  alignItems: 'center',
                  gap: '6px',
                  cursor: 'pointer',
                  fontSize: '0.8125rem'
                }}
              >
                <RefreshCw size={14} className={loading ? 'spin' : ''} />
                <span>Refresh</span>
              </button>
            </div>

            <div style={{ overflowX: 'auto' }}>
              <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', fontSize: '0.875rem' }}>
                <thead>
                  <tr style={{ background: 'rgba(255, 255, 255, 0.02)', borderBottom: '1px solid var(--border-subtle)' }}>
                    <th style={{ padding: '0.875rem 1.25rem', color: 'var(--text-muted)', fontWeight: 600 }}>Period</th>
                    <th style={{ padding: '0.875rem 1.25rem', color: 'var(--text-muted)', fontWeight: 600 }}>Employees</th>
                    <th style={{ padding: '0.875rem 1.25rem', color: 'var(--text-muted)', fontWeight: 600 }}>Working Days</th>
                    <th style={{ padding: '0.875rem 1.25rem', color: 'var(--text-muted)', fontWeight: 600 }}>Total Gross</th>
                    <th style={{ padding: '0.875rem 1.25rem', color: 'var(--text-muted)', fontWeight: 600 }}>Deductions</th>
                    <th style={{ padding: '0.875rem 1.25rem', color: 'var(--text-muted)', fontWeight: 600 }}>Total Net Pay</th>
                    <th style={{ padding: '0.875rem 1.25rem', color: 'var(--text-muted)', fontWeight: 600 }}>Status</th>
                    <th style={{ padding: '0.875rem 1.25rem', color: 'var(--text-muted)', fontWeight: 600, textAlign: 'right' }}>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {runs.length === 0 ? (
                    <tr>
                      <td colSpan={8} style={{ padding: '2.5rem', textAlign: 'center', color: 'var(--text-muted)' }}>
                        No payroll batches processed yet. Click "Process New Payroll" to run a monthly batch.
                      </td>
                    </tr>
                  ) : (
                    runs.map((r) => {
                      const statusStyle = getStatusColor(r.status);
                      const isSelected = selectedRun?.id === r.id;
                      return (
                        <tr
                          key={r.id}
                          onClick={() => setSelectedRun(r)}
                          style={{
                            borderBottom: '1px solid var(--border-subtle)',
                            background: isSelected ? 'rgba(99, 102, 241, 0.08)' : 'transparent',
                            cursor: 'pointer',
                            transition: 'background 0.15s'
                          }}
                        >
                          <td style={{ padding: '1rem 1.25rem', fontWeight: 600, color: '#ffffff' }}>
                            <div style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
                              <Calendar size={15} color="#818cf8" />
                              <span>{new Date(r.year, r.month - 1).toLocaleString('default', { month: 'long', year: 'numeric' })}</span>
                            </div>
                            <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>ID #{r.id}</span>
                          </td>
                          <td style={{ padding: '1rem 1.25rem' }}>
                            <span style={{
                              background: 'rgba(255, 255, 255, 0.05)',
                              padding: '0.2rem 0.6rem',
                              borderRadius: '12px',
                              fontSize: '0.8125rem'
                            }}>
                              {r.totalEmployees} Staff
                            </span>
                          </td>
                          <td style={{ padding: '1rem 1.25rem', color: 'var(--text-muted)' }}>{r.workingDays} Days</td>
                          <td style={{ padding: '1rem 1.25rem', color: '#ffffff', fontWeight: 500 }}>
                            ৳{r.totalGross.toLocaleString(undefined, { minimumFractionDigits: 2 })}
                          </td>
                          <td style={{ padding: '1rem 1.25rem', color: '#f87171' }}>
                            ৳{r.totalDeductions.toLocaleString(undefined, { minimumFractionDigits: 2 })}
                          </td>
                          <td style={{ padding: '1rem 1.25rem', color: '#34d399', fontWeight: 700 }}>
                            ৳{r.totalNetPay.toLocaleString(undefined, { minimumFractionDigits: 2 })}
                          </td>
                          <td style={{ padding: '1rem 1.25rem' }}>
                            <span style={{
                              background: statusStyle.bg,
                              color: statusStyle.text,
                              border: `1px solid ${statusStyle.border}`,
                              padding: '0.25rem 0.65rem',
                              borderRadius: '20px',
                              fontSize: '0.75rem',
                              fontWeight: 600,
                              textTransform: 'uppercase'
                            }}>
                              {r.status}
                            </span>
                          </td>
                          <td style={{ padding: '1rem 1.25rem', textAlign: 'right' }}>
                            <div style={{ display: 'flex', gap: '6px', justifyContent: 'flex-end' }}>
                              {r.status === 'Draft' && (
                                <button
                                  type="button"
                                  onClick={(e) => { e.stopPropagation(); handleApproveRun(r.id); }}
                                  style={{
                                    padding: '0.35rem 0.75rem',
                                    borderRadius: 'var(--radius-sm)',
                                    background: 'rgba(59, 130, 246, 0.15)',
                                    border: '1px solid rgba(59, 130, 246, 0.3)',
                                    color: '#60a5fa',
                                    fontSize: '0.75rem',
                                    fontWeight: 600,
                                    cursor: 'pointer'
                                  }}
                                >
                                  Approve
                                </button>
                              )}
                              {r.status === 'Approved' && (
                                <button
                                  type="button"
                                  onClick={(e) => { e.stopPropagation(); handleMarkPaid(r.id); }}
                                  style={{
                                    padding: '0.35rem 0.75rem',
                                    borderRadius: 'var(--radius-sm)',
                                    background: 'rgba(16, 185, 129, 0.15)',
                                    border: '1px solid rgba(16, 185, 129, 0.3)',
                                    color: '#34d399',
                                    fontSize: '0.75rem',
                                    fontWeight: 600,
                                    cursor: 'pointer'
                                  }}
                                >
                                  Mark Paid
                                </button>
                              )}
                              <button
                                type="button"
                                onClick={(e) => {
                                  e.stopPropagation();
                                  setSelectedRun(r);
                                  setActiveTab('payslips');
                                }}
                                style={{
                                  padding: '0.35rem 0.75rem',
                                  borderRadius: 'var(--radius-sm)',
                                  background: 'rgba(255, 255, 255, 0.05)',
                                  border: '1px solid var(--border-subtle)',
                                  color: 'var(--text-main)',
                                  fontSize: '0.75rem',
                                  cursor: 'pointer'
                                }}
                              >
                                View Payslips
                              </button>
                            </div>
                          </td>
                        </tr>
                      );
                    })
                  )}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}

      {/* TAB 2: SALARY STRUCTURES */}
      {activeTab === 'structures' && (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '1.25rem' }}>
          <div style={{
            background: 'var(--card-bg)',
            borderRadius: 'var(--radius-md)',
            border: '1px solid var(--border-subtle)',
            padding: '1.25rem'
          }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.25rem' }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
                <UserCheck size={20} color="#818cf8" />
                <h3 style={{ margin: 0, fontSize: '1.1rem' }}>Employee Salary Structure Configuration</h3>
              </div>
              <div style={{ display: 'flex', gap: '10px', alignItems: 'center' }}>
                <label style={{ fontSize: '0.875rem', color: 'var(--text-muted)' }}>Select Employee ID:</label>
                <select
                  value={structureEmployeeId}
                  onChange={(e) => {
                    const id = Number(e.target.value);
                    setStructureEmployeeId(id);
                    fetchSalaryStructure(id);
                  }}
                  style={{
                    background: 'rgba(255, 255, 255, 0.05)',
                    border: '1px solid var(--border-subtle)',
                    borderRadius: 'var(--radius-sm)',
                    color: '#ffffff',
                    padding: '0.4rem 0.8rem',
                    fontSize: '0.875rem'
                  }}
                >
                  <option value={1} style={{ background: '#1e293b' }}>Employee #1 (Tariqul Islam)</option>
                  <option value={2} style={{ background: '#1e293b' }}>Employee #2 (Jane Doe)</option>
                </select>
                <button
                  type="button"
                  onClick={() => setShowAssignModal(true)}
                  style={{
                    padding: '0.45rem 1rem',
                    borderRadius: 'var(--radius-sm)',
                    background: 'linear-gradient(135deg, #6366f1 0%, #4f46e5 100%)',
                    color: '#ffffff',
                    border: 'none',
                    fontSize: '0.8125rem',
                    fontWeight: 600,
                    cursor: 'pointer'
                  }}
                >
                  Adjust Structure
                </button>
              </div>
            </div>

            {salaryStructure ? (
              <div style={{ display: 'flex', flexDirection: 'column', gap: '1.25rem' }}>
                <div style={{
                  display: 'grid',
                  gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))',
                  gap: '1rem',
                  padding: '1rem',
                  borderRadius: 'var(--radius-md)',
                  background: 'rgba(255, 255, 255, 0.02)',
                  border: '1px solid var(--border-subtle)'
                }}>
                  <div>
                    <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>Employee</span>
                    <div style={{ fontSize: '1rem', fontWeight: 600, color: '#ffffff' }}>
                      {salaryStructure.employeeName} ({salaryStructure.employeeCode})
                    </div>
                  </div>
                  <div>
                    <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>Gross Monthly Salary</span>
                    <div style={{ fontSize: '1.25rem', fontWeight: 700, color: '#34d399' }}>
                      ৳{salaryStructure.grossSalary.toLocaleString(undefined, { minimumFractionDigits: 2 })}
                    </div>
                  </div>
                  <div>
                    <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>Effective From</span>
                    <div style={{ fontSize: '0.9375rem', color: '#ffffff' }}>{salaryStructure.effectiveFrom}</div>
                  </div>
                  <div>
                    <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>Status</span>
                    <div style={{ color: '#34d399', fontWeight: 600 }}>Active Structure</div>
                  </div>
                </div>

                <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', fontSize: '0.875rem' }}>
                  <thead>
                    <tr style={{ background: 'rgba(255, 255, 255, 0.02)', borderBottom: '1px solid var(--border-subtle)' }}>
                      <th style={{ padding: '0.75rem 1rem', color: 'var(--text-muted)' }}>Component</th>
                      <th style={{ padding: '0.75rem 1rem', color: 'var(--text-muted)' }}>Code</th>
                      <th style={{ padding: '0.75rem 1rem', color: 'var(--text-muted)' }}>Type</th>
                      <th style={{ padding: '0.75rem 1rem', color: 'var(--text-muted)' }}>Calculation Rule</th>
                      <th style={{ padding: '0.75rem 1rem', color: 'var(--text-muted)', textAlign: 'right' }}>Monthly Amount</th>
                    </tr>
                  </thead>
                  <tbody>
                    {salaryStructure.components.map((c) => (
                      <tr key={c.id} style={{ borderBottom: '1px solid var(--border-subtle)' }}>
                        <td style={{ padding: '0.875rem 1rem', fontWeight: 600, color: '#ffffff' }}>{c.componentName}</td>
                        <td style={{ padding: '0.875rem 1rem', color: '#818cf8', fontFamily: 'monospace' }}>{c.componentCode}</td>
                        <td style={{ padding: '0.875rem 1rem' }}>
                          <span style={{
                            padding: '0.2rem 0.5rem',
                            borderRadius: '4px',
                            fontSize: '0.75rem',
                            fontWeight: 600,
                            background: c.componentType === 'Earning' ? 'rgba(16, 185, 129, 0.15)' : 'rgba(239, 68, 68, 0.15)',
                            color: c.componentType === 'Earning' ? '#34d399' : '#f87171'
                          }}>
                            {c.componentType}
                          </span>
                        </td>
                        <td style={{ padding: '0.875rem 1rem', color: 'var(--text-muted)' }}>
                          {c.calculationType === 'Percentage' ? `${c.amount}% of ${c.percentageBase}` : `Fixed ৳${c.amount.toLocaleString()}`}
                        </td>
                        <td style={{
                          padding: '0.875rem 1rem',
                          textAlign: 'right',
                          fontWeight: 600,
                          color: c.componentType === 'Earning' ? '#34d399' : '#f87171'
                        }}>
                          {c.componentType === 'Earning' ? '+' : '-'}৳{c.calculatedMonthlyAmount.toLocaleString(undefined, { minimumFractionDigits: 2 })}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            ) : (
              <div style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-muted)' }}>
                Loading salary structure...
              </div>
            )}
          </div>
        </div>
      )}

      {/* TAB 3: PAYSLIP EXPLORER */}
      {activeTab === 'payslips' && (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '1.25rem' }}>
          <div style={{
            background: 'var(--card-bg)',
            borderRadius: 'var(--radius-md)',
            border: '1px solid var(--border-subtle)',
            padding: '1.25rem'
          }}>
            <div style={{
              display: 'flex',
              flexWrap: 'wrap',
              justifyContent: 'space-between',
              alignItems: 'center',
              gap: '1rem',
              marginBottom: '1.25rem'
            }}>
              <div>
                <h3 style={{ margin: 0, fontSize: '1.1rem' }}>
                  Payslips for {selectedRun ? `${new Date(selectedRun.year, selectedRun.month - 1).toLocaleString('default', { month: 'long', year: 'numeric' })} (Batch #${selectedRun.id})` : 'Selected Batch'}
                </h3>
                <p style={{ margin: '4px 0 0', fontSize: '0.8125rem', color: 'var(--text-muted)' }}>
                  Generated using QuestPDF enterprise PDF layout with audit timestamps and serial breakdown
                </p>
              </div>

              <div style={{ display: 'flex', gap: '10px', alignItems: 'center' }}>
                <div style={{
                  display: 'flex',
                  alignItems: 'center',
                  gap: '8px',
                  background: 'rgba(255, 255, 255, 0.05)',
                  border: '1px solid var(--border-subtle)',
                  borderRadius: 'var(--radius-sm)',
                  padding: '0.35rem 0.75rem'
                }}>
                  <Search size={16} color="var(--text-muted)" />
                  <input
                    type="text"
                    placeholder="Search employee or code..."
                    value={payslipSearch}
                    onChange={(e) => setPayslipSearch(e.target.value)}
                    style={{
                      background: 'transparent',
                      border: 'none',
                      outline: 'none',
                      color: '#ffffff',
                      fontSize: '0.8125rem',
                      width: '180px'
                    }}
                  />
                </div>

                <div style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
                  <Filter size={16} color="var(--text-muted)" />
                  <select
                    value={selectedRun?.id || ''}
                    onChange={(e) => {
                      const run = runs.find(r => r.id === Number(e.target.value));
                      if (run) setSelectedRun(run);
                    }}
                    style={{
                      background: 'rgba(255, 255, 255, 0.05)',
                      border: '1px solid var(--border-subtle)',
                      borderRadius: 'var(--radius-sm)',
                      color: '#ffffff',
                      padding: '0.4rem 0.75rem',
                      fontSize: '0.8125rem'
                    }}
                  >
                    {runs.map(r => (
                      <option key={r.id} value={r.id} style={{ background: '#1e293b' }}>
                        Batch #{r.id} ({r.month}/{r.year})
                      </option>
                    ))}
                  </select>
                </div>
              </div>
            </div>

            <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', fontSize: '0.875rem' }}>
              <thead>
                <tr style={{ background: 'rgba(255, 255, 255, 0.02)', borderBottom: '1px solid var(--border-subtle)' }}>
                  <th style={{ padding: '0.75rem 1rem', color: 'var(--text-muted)' }}>Employee</th>
                  <th style={{ padding: '0.75rem 1rem', color: 'var(--text-muted)' }}>Attendance</th>
                  <th style={{ padding: '0.75rem 1rem', color: 'var(--text-muted)' }}>Loss of Pay</th>
                  <th style={{ padding: '0.75rem 1rem', color: 'var(--text-muted)' }}>Gross Earnings</th>
                  <th style={{ padding: '0.75rem 1rem', color: 'var(--text-muted)' }}>Deductions</th>
                  <th style={{ padding: '0.75rem 1rem', color: 'var(--text-muted)' }}>Net Pay</th>
                  <th style={{ padding: '0.75rem 1rem', color: 'var(--text-muted)', textAlign: 'right' }}>Actions</th>
                </tr>
              </thead>
              <tbody>
                {filteredDetails.length === 0 ? (
                  <tr>
                    <td colSpan={7} style={{ textAlign: 'center', padding: '2.5rem', color: 'var(--text-muted)' }}>
                      No payslips found for this batch.
                    </td>
                  </tr>
                ) : (
                  filteredDetails.map((d) => (
                    <tr key={d.id} style={{ borderBottom: '1px solid var(--border-subtle)' }}>
                      <td style={{ padding: '0.875rem 1rem' }}>
                        <div style={{ fontWeight: 600, color: '#ffffff' }}>{d.employeeName}</div>
                        <div style={{ fontSize: '0.75rem', color: '#818cf8' }}>{d.employeeCode} • {d.designationTitle || 'Staff'}</div>
                      </td>
                      <td style={{ padding: '0.875rem 1rem', fontSize: '0.8125rem' }}>
                        <div>Present: <strong>{d.presentDays}</strong> / {d.workingDays} d</div>
                        {d.unpaidLeaveDays > 0 && (
                          <div style={{ color: '#f87171' }}>Unpaid: {d.unpaidLeaveDays} d</div>
                        )}
                      </td>
                      <td style={{ padding: '0.875rem 1rem', color: d.lossOfPay > 0 ? '#f87171' : 'var(--text-muted)' }}>
                        {d.lossOfPay > 0 ? `-৳${d.lossOfPay.toLocaleString(undefined, { minimumFractionDigits: 2 })}` : '৳0.00'}
                      </td>
                      <td style={{ padding: '0.875rem 1rem', color: '#ffffff', fontWeight: 500 }}>
                        ৳{d.grossEarnings.toLocaleString(undefined, { minimumFractionDigits: 2 })}
                      </td>
                      <td style={{ padding: '0.875rem 1rem', color: '#f87171' }}>
                        ৳{d.totalDeductions.toLocaleString(undefined, { minimumFractionDigits: 2 })}
                      </td>
                      <td style={{ padding: '0.875rem 1rem', color: '#34d399', fontWeight: 700 }}>
                        ৳{d.netPay.toLocaleString(undefined, { minimumFractionDigits: 2 })}
                      </td>
                      <td style={{ padding: '0.875rem 1rem', textAlign: 'right' }}>
                        <div style={{ display: 'flex', gap: '6px', justifyContent: 'flex-end' }}>
                          <button
                            type="button"
                            onClick={() => setSelectedDetail(d)}
                            style={{
                              padding: '0.35rem 0.75rem',
                              borderRadius: 'var(--radius-sm)',
                              background: 'rgba(255, 255, 255, 0.05)',
                              border: '1px solid var(--border-subtle)',
                              color: 'var(--text-main)',
                              fontSize: '0.75rem',
                              cursor: 'pointer'
                            }}
                          >
                            Details
                          </button>
                          <button
                            type="button"
                            onClick={() => handleDownloadPdf(d.id, d.employeeName, selectedRun!.month, selectedRun!.year)}
                            style={{
                              display: 'flex',
                              alignItems: 'center',
                              gap: '6px',
                              padding: '0.35rem 0.75rem',
                              borderRadius: 'var(--radius-sm)',
                              background: 'rgba(99, 102, 241, 0.15)',
                              border: '1px solid rgba(99, 102, 241, 0.3)',
                              color: '#818cf8',
                              fontSize: '0.75rem',
                              fontWeight: 600,
                              cursor: 'pointer'
                            }}
                          >
                            <Download size={13} />
                            <span>PDF</span>
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* TAB 4: TAX ESTIMATE CALCULATOR */}
      {activeTab === 'tax-calc' && (
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(320px, 1fr))', gap: '1.25rem' }}>
          {/* Input Panel */}
          <div style={{
            background: 'var(--card-bg)',
            borderRadius: 'var(--radius-md)',
            border: '1px solid var(--border-subtle)',
            padding: '1.5rem',
            display: 'flex',
            flexDirection: 'column',
            gap: '1.25rem'
          }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
              <DollarSign size={20} color="#818cf8" />
              <h3 style={{ margin: 0, fontSize: '1.1rem' }}>Progressive Tax Bracket Estimator</h3>
            </div>
            <p style={{ margin: 0, fontSize: '0.8125rem', color: 'var(--text-muted)' }}>
              Computes annual taxable income, medical/conveyance standard exemptions, and progressive progressive tax brackets (0% - 25%).
            </p>

            <div>
              <label style={{ display: 'block', fontSize: '0.8125rem', color: 'var(--text-muted)', marginBottom: '6px' }}>
                Proposed Monthly Gross Salary (BDT):
              </label>
              <input
                type="number"
                value={taxInputGross}
                onChange={(e) => {
                  const val = Number(e.target.value);
                  setTaxInputGross(val);
                  fetchTaxEstimate(val);
                }}
                style={{
                  width: '100%',
                  background: 'rgba(255, 255, 255, 0.05)',
                  border: '1px solid var(--border-subtle)',
                  borderRadius: 'var(--radius-sm)',
                  color: '#ffffff',
                  padding: '0.6rem 0.8rem',
                  fontSize: '1rem',
                  fontWeight: 600
                }}
              />
            </div>

            <div style={{
              background: 'rgba(99, 102, 241, 0.05)',
              padding: '1rem',
              borderRadius: 'var(--radius-sm)',
              border: '1px solid rgba(99, 102, 241, 0.2)',
              fontSize: '0.8125rem',
              color: 'var(--text-muted)',
              lineHeight: 1.6
            }}>
              <strong style={{ color: '#818cf8' }}>Slab Distribution Standard:</strong>
              <div>• First ৳350,000 : <strong>0% (Tax-Free Threshold)</strong></div>
              <div>• Next ৳100,000 : <strong>5%</strong></div>
              <div>• Next ৳300,000 : <strong>10%</strong></div>
              <div>• Next ৳400,000 : <strong>15%</strong></div>
              <div>• Next ৳500,000 : <strong>20%</strong></div>
              <div>• Remaining Balance : <strong>25%</strong></div>
            </div>
          </div>

          {/* Results Panel */}
          {taxEstimate && (
            <div style={{
              background: 'var(--card-bg)',
              borderRadius: 'var(--radius-md)',
              border: '1px solid var(--border-subtle)',
              padding: '1.5rem',
              display: 'flex',
              flexDirection: 'column',
              gap: '1rem'
            }}>
              <h3 style={{ margin: 0, fontSize: '1.1rem', color: '#ffffff' }}>Estimated Tax Assessment</h3>

              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0.75rem' }}>
                <div style={{ padding: '0.75rem', background: 'rgba(255, 255, 255, 0.02)', borderRadius: 'var(--radius-sm)' }}>
                  <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>Annual Gross</span>
                  <div style={{ fontSize: '1.125rem', fontWeight: 700, color: '#ffffff' }}>
                    ৳{taxEstimate.annualGross.toLocaleString(undefined, { minimumFractionDigits: 2 })}
                  </div>
                </div>

                <div style={{ padding: '0.75rem', background: 'rgba(255, 255, 255, 0.02)', borderRadius: 'var(--radius-sm)' }}>
                  <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>Statutory Exemptions</span>
                  <div style={{ fontSize: '1.125rem', fontWeight: 700, color: '#34d399' }}>
                    ৳{taxEstimate.taxExemptAmount.toLocaleString(undefined, { minimumFractionDigits: 2 })}
                  </div>
                </div>

                <div style={{ padding: '0.75rem', background: 'rgba(255, 255, 255, 0.02)', borderRadius: 'var(--radius-sm)' }}>
                  <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>Net Taxable Income</span>
                  <div style={{ fontSize: '1.125rem', fontWeight: 700, color: '#fbbf24' }}>
                    ৳{taxEstimate.taxableIncome.toLocaleString(undefined, { minimumFractionDigits: 2 })}
                  </div>
                </div>

                <div style={{ padding: '0.75rem', background: 'rgba(255, 255, 255, 0.02)', borderRadius: 'var(--radius-sm)' }}>
                  <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>Effective Tax Rate</span>
                  <div style={{ fontSize: '1.125rem', fontWeight: 700, color: '#818cf8' }}>
                    {taxEstimate.effectiveTaxRatePercentage.toFixed(2)}%
                  </div>
                </div>
              </div>

              <div style={{
                marginTop: '0.5rem',
                padding: '1.25rem',
                borderRadius: 'var(--radius-sm)',
                background: 'linear-gradient(135deg, rgba(239, 68, 68, 0.15) 0%, rgba(239, 68, 68, 0.05) 100%)',
                border: '1px solid rgba(239, 68, 68, 0.3)',
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center'
              }}>
                <div>
                  <div style={{ fontSize: '0.8125rem', color: '#fca5a5' }}>Estimated Monthly TDS Deduction</div>
                  <div style={{ fontSize: '1.5rem', fontWeight: 800, color: '#f87171' }}>
                    ৳{taxEstimate.monthlyTds.toLocaleString(undefined, { minimumFractionDigits: 2 })}
                  </div>
                </div>
                <div style={{ textAlign: 'right' }}>
                  <div style={{ fontSize: '0.75rem', color: '#fca5a5' }}>Annual Projected Tax</div>
                  <div style={{ fontSize: '1.125rem', fontWeight: 700, color: '#ffffff' }}>
                    ৳{taxEstimate.estimatedAnnualTax.toLocaleString(undefined, { minimumFractionDigits: 2 })}
                  </div>
                </div>
              </div>
            </div>
          )}
        </div>
      )}

      {/* PROCESS PAYROLL MODAL */}
      {showProcessModal && (
        <div style={{
          position: 'fixed',
          top: 0,
          left: 0,
          right: 0,
          bottom: 0,
          background: 'rgba(0, 0, 0, 0.75)',
          backdropFilter: 'blur(4px)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          zIndex: 9999,
          padding: '1rem'
        }}>
          <div style={{
            background: '#0f172a',
            border: '1px solid var(--border-subtle)',
            borderRadius: 'var(--radius-lg)',
            width: '100%',
            maxWidth: '480px',
            padding: '1.5rem',
            boxShadow: '0 20px 40px rgba(0,0,0,0.5)'
          }}>
            <h3 style={{ margin: '0 0 0.5rem 0', fontSize: '1.25rem', color: '#ffffff' }}>
              Process Monthly Payroll Batch
            </h3>
            <p style={{ margin: '0 0 1.25rem 0', fontSize: '0.8125rem', color: 'var(--text-muted)' }}>
              Automatically integrates attendance logs, approved leaves, Loss of Pay (LOP) deductions, and progressive income tax.
            </p>

            <form onSubmit={handleProcessPayroll} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
                <div>
                  <label style={{ display: 'block', fontSize: '0.8125rem', color: 'var(--text-muted)', marginBottom: '4px' }}>
                    Month (1-12)
                  </label>
                  <input
                    type="number"
                    min={1}
                    max={12}
                    value={processMonth}
                    onChange={(e) => setProcessMonth(Number(e.target.value))}
                    required
                    style={{
                      width: '100%',
                      padding: '0.55rem',
                      background: 'rgba(255, 255, 255, 0.05)',
                      border: '1px solid var(--border-subtle)',
                      borderRadius: 'var(--radius-sm)',
                      color: '#ffffff'
                    }}
                  />
                </div>
                <div>
                  <label style={{ display: 'block', fontSize: '0.8125rem', color: 'var(--text-muted)', marginBottom: '4px' }}>
                    Year
                  </label>
                  <input
                    type="number"
                    min={2020}
                    max={2035}
                    value={processYear}
                    onChange={(e) => setProcessYear(Number(e.target.value))}
                    required
                    style={{
                      width: '100%',
                      padding: '0.55rem',
                      background: 'rgba(255, 255, 255, 0.05)',
                      border: '1px solid var(--border-subtle)',
                      borderRadius: 'var(--radius-sm)',
                      color: '#ffffff'
                    }}
                  />
                </div>
              </div>

              <div>
                <label style={{ display: 'block', fontSize: '0.8125rem', color: 'var(--text-muted)', marginBottom: '4px' }}>
                  Working Days in Month
                </label>
                <input
                  type="number"
                  min={10}
                  max={31}
                  value={workingDays}
                  onChange={(e) => setWorkingDays(Number(e.target.value))}
                  required
                  style={{
                    width: '100%',
                    padding: '0.55rem',
                    background: 'rgba(255, 255, 255, 0.05)',
                    border: '1px solid var(--border-subtle)',
                    borderRadius: 'var(--radius-sm)',
                    color: '#ffffff'
                  }}
                />
              </div>

              <div>
                <label style={{ display: 'block', fontSize: '0.8125rem', color: 'var(--text-muted)', marginBottom: '4px' }}>
                  Remarks / Batch Notes
                </label>
                <input
                  type="text"
                  value={processRemarks}
                  onChange={(e) => setProcessRemarks(e.target.value)}
                  style={{
                    width: '100%',
                    padding: '0.55rem',
                    background: 'rgba(255, 255, 255, 0.05)',
                    border: '1px solid var(--border-subtle)',
                    borderRadius: 'var(--radius-sm)',
                    color: '#ffffff'
                  }}
                />
              </div>

              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '10px', marginTop: '0.5rem' }}>
                <button
                  type="button"
                  onClick={() => setShowProcessModal(false)}
                  style={{
                    padding: '0.5rem 1rem',
                    borderRadius: 'var(--radius-sm)',
                    background: 'transparent',
                    border: '1px solid var(--border-subtle)',
                    color: 'var(--text-muted)',
                    cursor: 'pointer'
                  }}
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={loading}
                  style={{
                    padding: '0.5rem 1.25rem',
                    borderRadius: 'var(--radius-sm)',
                    background: 'linear-gradient(135deg, #6366f1 0%, #4f46e5 100%)',
                    color: '#ffffff',
                    border: 'none',
                    fontWeight: 600,
                    cursor: 'pointer'
                  }}
                >
                  {loading ? 'Processing...' : 'Run Payroll'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* ADJUST STRUCTURE MODAL */}
      {showAssignModal && (
        <div style={{
          position: 'fixed',
          top: 0,
          left: 0,
          right: 0,
          bottom: 0,
          background: 'rgba(0, 0, 0, 0.75)',
          backdropFilter: 'blur(4px)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          zIndex: 9999,
          padding: '1rem'
        }}>
          <div style={{
            background: '#0f172a',
            border: '1px solid var(--border-subtle)',
            borderRadius: 'var(--radius-lg)',
            width: '100%',
            maxWidth: '460px',
            padding: '1.5rem',
            boxShadow: '0 20px 40px rgba(0,0,0,0.5)'
          }}>
            <h3 style={{ margin: '0 0 0.5rem 0', fontSize: '1.25rem', color: '#ffffff' }}>
              Adjust Gross Compensation Package
            </h3>
            <p style={{ margin: '0 0 1.25rem 0', fontSize: '0.8125rem', color: 'var(--text-muted)' }}>
              Set new monthly gross salary for Employee #{structureEmployeeId}. Component percentages will automatically scale.
            </p>

            <form onSubmit={handleAssignStructure} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
              <div>
                <label style={{ display: 'block', fontSize: '0.8125rem', color: 'var(--text-muted)', marginBottom: '4px' }}>
                  Monthly Gross Salary (BDT)
                </label>
                <input
                  type="number"
                  step="500"
                  value={structureGross}
                  onChange={(e) => setStructureGross(Number(e.target.value))}
                  required
                  style={{
                    width: '100%',
                    padding: '0.6rem',
                    background: 'rgba(255, 255, 255, 0.05)',
                    border: '1px solid var(--border-subtle)',
                    borderRadius: 'var(--radius-sm)',
                    color: '#ffffff',
                    fontSize: '1rem',
                    fontWeight: 600
                  }}
                />
              </div>

              <div style={{
                background: 'rgba(255, 255, 255, 0.02)',
                padding: '0.75rem',
                borderRadius: 'var(--radius-sm)',
                fontSize: '0.8125rem',
                color: 'var(--text-muted)'
              }}>
                <div>• Basic: 50% of Gross = <strong>৳{(structureGross * 0.5).toLocaleString()}</strong></div>
                <div>• HRA: 50% of Basic = <strong>৳{(structureGross * 0.25).toLocaleString()}</strong></div>
                <div>• Medical: 20% of Basic = <strong>৳{(structureGross * 0.1).toLocaleString()}</strong></div>
                <div>• Conveyance: 10% of Basic = <strong>৳{(structureGross * 0.05).toLocaleString()}</strong></div>
                <div>• Provident Fund: 10% of Basic = <strong>৳{(structureGross * 0.05).toLocaleString()}</strong></div>
              </div>

              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '10px', marginTop: '0.5rem' }}>
                <button
                  type="button"
                  onClick={() => setShowAssignModal(false)}
                  style={{
                    padding: '0.5rem 1rem',
                    borderRadius: 'var(--radius-sm)',
                    background: 'transparent',
                    border: '1px solid var(--border-subtle)',
                    color: 'var(--text-muted)',
                    cursor: 'pointer'
                  }}
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={loading}
                  style={{
                    padding: '0.5rem 1.25rem',
                    borderRadius: 'var(--radius-sm)',
                    background: 'linear-gradient(135deg, #6366f1 0%, #4f46e5 100%)',
                    color: '#ffffff',
                    border: 'none',
                    fontWeight: 600,
                    cursor: 'pointer'
                  }}
                >
                  {loading ? 'Saving...' : 'Apply Structure'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* PAYSLIP DETAIL MODAL */}
      {selectedDetail && (
        <div style={{
          position: 'fixed',
          top: 0,
          left: 0,
          right: 0,
          bottom: 0,
          background: 'rgba(0, 0, 0, 0.75)',
          backdropFilter: 'blur(4px)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          zIndex: 9999,
          padding: '1rem'
        }}>
          <div style={{
            background: '#0f172a',
            border: '1px solid var(--border-subtle)',
            borderRadius: 'var(--radius-lg)',
            width: '100%',
            maxWidth: '560px',
            padding: '1.5rem',
            boxShadow: '0 20px 40px rgba(0,0,0,0.5)'
          }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
              <div>
                <h3 style={{ margin: 0, fontSize: '1.2rem', color: '#ffffff' }}>{selectedDetail.employeeName}</h3>
                <span style={{ fontSize: '0.8125rem', color: '#818cf8' }}>
                  {selectedDetail.employeeCode} • {selectedDetail.designationTitle || 'Staff'} • {selectedDetail.departmentName || 'Core'}
                </span>
              </div>
              <button
                type="button"
                onClick={() => setSelectedDetail(null)}
                style={{
                  background: 'transparent',
                  border: 'none',
                  color: 'var(--text-muted)',
                  fontSize: '1.25rem',
                  cursor: 'pointer'
                }}
              >
                ✕
              </button>
            </div>

            <div style={{
              display: 'grid',
              gridTemplateColumns: '1fr 1fr',
              gap: '0.75rem',
              marginBottom: '1rem',
              background: 'rgba(255, 255, 255, 0.02)',
              padding: '0.75rem',
              borderRadius: 'var(--radius-sm)'
            }}>
              <div>Working Days: <strong>{selectedDetail.workingDays}</strong></div>
              <div>Present Days: <strong style={{ color: '#34d399' }}>{selectedDetail.presentDays}</strong></div>
              <div>Paid Leave: <strong>{selectedDetail.paidLeaveDays}</strong></div>
              <div>Loss of Pay: <strong style={{ color: '#f87171' }}>{selectedDetail.lossOfPay > 0 ? `৳${selectedDetail.lossOfPay.toLocaleString()}` : '৳0'}</strong></div>
            </div>

            <h4 style={{ margin: '0 0 0.5rem 0', fontSize: '0.875rem', color: 'var(--text-muted)' }}>Salary Components Breakdown</h4>
            <div style={{ maxHeight: '200px', overflowY: 'auto', marginBottom: '1rem' }}>
              <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.8125rem' }}>
                <tbody>
                  {selectedDetail.components.map((c, i) => (
                    <tr key={i} style={{ borderBottom: '1px solid var(--border-subtle)' }}>
                      <td style={{ padding: '0.4rem 0', color: '#ffffff' }}>{c.componentName}</td>
                      <td style={{ padding: '0.4rem 0', color: 'var(--text-muted)', textAlign: 'center' }}>{c.componentType}</td>
                      <td style={{
                        padding: '0.4rem 0',
                        textAlign: 'right',
                        fontWeight: 600,
                        color: c.componentType === 'Earning' ? '#34d399' : '#f87171'
                      }}>
                        {c.componentType === 'Earning' ? '+' : '-'}৳{c.amount.toLocaleString(undefined, { minimumFractionDigits: 2 })}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            <div style={{
              padding: '0.875rem',
              borderRadius: 'var(--radius-sm)',
              background: 'rgba(16, 185, 129, 0.1)',
              border: '1px solid rgba(16, 185, 129, 0.25)',
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'center'
            }}>
              <div>
                <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>Net Disbursable Amount</span>
                <div style={{ fontSize: '1.25rem', fontWeight: 800, color: '#34d399' }}>
                  ৳{selectedDetail.netPay.toLocaleString(undefined, { minimumFractionDigits: 2 })}
                </div>
              </div>
              <button
                type="button"
                onClick={() => {
                  handleDownloadPdf(selectedDetail.id, selectedDetail.employeeName, selectedRun!.month, selectedRun!.year);
                  setSelectedDetail(null);
                }}
                style={{
                  display: 'flex',
                  alignItems: 'center',
                  gap: '6px',
                  padding: '0.5rem 1rem',
                  borderRadius: 'var(--radius-sm)',
                  background: 'linear-gradient(135deg, #6366f1 0%, #4f46e5 100%)',
                  color: '#ffffff',
                  border: 'none',
                  fontSize: '0.8125rem',
                  fontWeight: 600,
                  cursor: 'pointer'
                }}
              >
                <Download size={14} />
                <span>Download PDF</span>
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
