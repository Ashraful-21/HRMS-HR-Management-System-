import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { 
  Star, 
  Award, 
  Calendar, 
  Plus, 
  Check, 
  AlertCircle, 
  Sparkles
} from 'lucide-react';

interface PerformanceProps {
  token: string;
}

const API_BASE = 'http://localhost:5219/api/v1';

export default function Performance({ token }: PerformanceProps) {
  const [tab, setTab] = useState<'reviews' | 'cycles'>('reviews');
  
  const [reviews, setReviews] = useState<any[]>([]);
  const [cycles, setCycles] = useState<any[]>([]);
  const [employees, setEmployees] = useState<any[]>([]);
  
  const [error, setError] = useState<string | null>(null);
  const [successMsg, setSuccessMsg] = useState<string | null>(null);

  // Modals
  const [showCycleModal, setShowCycleModal] = useState(false);
  const [newCycle, setNewCycle] = useState({
    name: `Annual Review Cycle ${new Date().getFullYear()}`,
    year: new Date().getFullYear(),
    startDate: `${new Date().getFullYear()}-01-01`,
    endDate: `${new Date().getFullYear()}-12-31`
  });

  const [showCreateReviewModal, setShowCreateReviewModal] = useState(false);
  const [newReviewData, setNewReviewData] = useState({
    reviewCycleId: 0,
    employeeId: 0,
    reviewerId: 0
  });

  const [showSelfModal, setShowSelfModal] = useState(false);
  const [selectedReview, setSelectedReview] = useState<any>(null);
  const [selfData, setSelfData] = useState({
    selfRating: 4.5,
    selfComments: '',
    goals: [
      { goalDescription: 'Core feature architecture delivery', target: '100% on schedule', selfScore: 4.8, weight: 50 },
      { goalDescription: 'Team code review and mentoring', target: 'Zero regression', selfScore: 4.5, weight: 50 }
    ]
  });

  const [showManagerModal, setShowManagerModal] = useState(false);
  const [managerData, setManagerData] = useState({
    managerRating: 4.5,
    managerComments: '',
    finalRating: 4.5
  });

  const headers = { Authorization: `Bearer ${token}` };

  const loadData = async () => {
    setError(null);
    try {
      const [revRes, cycRes, empRes] = await Promise.all([
        axios.get(`${API_BASE}/performance/reviews?pageSize=50`, { headers }),
        axios.get(`${API_BASE}/performance/cycles`, { headers }),
        axios.get(`${API_BASE}/employees?pageSize=100`, { headers })
      ]);

      setReviews(revRes.data.data?.items || []);
      setCycles(cycRes.data.data || []);
      setEmployees(empRes.data.data?.items || []);

      if (cycRes.data.data?.length > 0 && newReviewData.reviewCycleId === 0) {
        setNewReviewData(prev => ({ ...prev, reviewCycleId: cycRes.data.data[0].id }));
      }
      if (empRes.data.data?.items?.length > 0 && newReviewData.employeeId === 0) {
        setNewReviewData(prev => ({ 
          ...prev, 
          employeeId: empRes.data.data.items[0].id, 
          reviewerId: empRes.data.data.items[0].id 
        }));
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error loading performance appraisal data.');
    }
  };

  useEffect(() => {
    loadData();
  }, [tab, token]);

  const handleCreateCycle = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const res = await axios.post(`${API_BASE}/performance/cycles`, newCycle, { headers });
      if (res.data.success) {
        setSuccessMsg('Review cycle created successfully.');
        setShowCycleModal(false);
        loadData();
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to create review cycle.');
    }
  };

  const handleCreateReview = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const res = await axios.post(`${API_BASE}/performance/reviews`, newReviewData, { headers });
      if (res.data.success) {
        setSuccessMsg('Employee performance appraisal created.');
        setShowCreateReviewModal(false);
        loadData();
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to initiate review.');
    }
  };

  const handleSelfSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedReview) return;
    try {
      const res = await axios.post(
        `${API_BASE}/performance/reviews/${selectedReview.id}/self-submit`,
        selfData,
        { headers }
      );
      if (res.data.success) {
        setSuccessMsg('Self-appraisal submitted successfully.');
        setShowSelfModal(false);
        loadData();
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to submit self-review.');
    }
  };

  const handleManagerSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedReview) return;
    try {
      const res = await axios.post(
        `${API_BASE}/performance/reviews/${selectedReview.id}/manager-submit`,
        managerData,
        { headers }
      );
      if (res.data.success) {
        setSuccessMsg('Manager appraisal finalized and scores recorded.');
        setShowManagerModal(false);
        loadData();
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to submit manager evaluation.');
    }
  };

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '1.25rem' }}>
      
      {/* Title & Actions */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: '1rem' }}>
        <div>
          <h2 style={{ fontSize: '1.4rem', fontWeight: 800, margin: 0, display: 'flex', alignItems: 'center', gap: '8px' }}>
            <Award size={22} color="#f59e0b" />
            Performance &amp; Appraisal Management
          </h2>
          <p style={{ margin: '0.25rem 0 0', color: 'var(--text-muted)', fontSize: '0.85rem' }}>
            Periodic evaluation cycles, scorecard goals, self-appraisals, and 360-degree reviews.
          </p>
        </div>

        <div style={{ display: 'flex', gap: '0.5rem' }}>
          <button type="button" className="btn-secondary" onClick={() => setShowCycleModal(true)}>
            <Plus size={16} /> New Review Cycle
          </button>
          <button type="button" className="btn-primary" onClick={() => setShowCreateReviewModal(true)}>
            <Sparkles size={16} /> Initiate Employee Appraisal
          </button>
        </div>
      </div>

      {/* Tabs */}
      <div className="subnav-tabs">
        <button 
          type="button" 
          className={`subnav-btn ${tab === 'reviews' ? 'active' : ''}`}
          onClick={() => setTab('reviews')}
        >
          <Award size={16} /> Employee Appraisals ({reviews.length})
        </button>
        <button 
          type="button" 
          className={`subnav-btn ${tab === 'cycles' ? 'active' : ''}`}
          onClick={() => setTab('cycles')}
        >
          <Calendar size={16} /> Review Cycles ({cycles.length})
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

      {/* TAB 1: REVIEWS TABLE */}
      {tab === 'reviews' && (
        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>Employee</th>
                <th>Cycle</th>
                <th>Reviewer</th>
                <th>Self Rating</th>
                <th>Manager Rating</th>
                <th>Final Rating</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {reviews.length > 0 ? (
                reviews.map((rev) => (
                  <tr key={rev.id}>
                    <td>
                      <strong>{rev.employeeName}</strong>
                      <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>
                        {rev.employeeCode} • {rev.departmentName}
                      </div>
                    </td>
                    <td>{rev.reviewCycleName}</td>
                    <td>{rev.reviewerName}</td>
                    <td>
                      {rev.selfRating ? (
                        <div style={{ display: 'flex', alignItems: 'center', gap: '4px', color: '#f59e0b', fontWeight: 700 }}>
                          <Star size={14} fill="#f59e0b" /> {rev.selfRating} / 5
                        </div>
                      ) : (
                        <span style={{ color: 'var(--text-muted)' }}>Pending</span>
                      )}
                    </td>
                    <td>
                      {rev.managerRating ? (
                        <div style={{ display: 'flex', alignItems: 'center', gap: '4px', color: '#38bdf8', fontWeight: 700 }}>
                          <Star size={14} fill="#38bdf8" /> {rev.managerRating} / 5
                        </div>
                      ) : (
                        <span style={{ color: 'var(--text-muted)' }}>Pending</span>
                      )}
                    </td>
                    <td>
                      {rev.finalRating ? (
                        <div style={{ display: 'flex', alignItems: 'center', gap: '4px', color: '#10b981', fontWeight: 800 }}>
                          <Star size={15} fill="#10b981" /> {rev.finalRating} / 5
                        </div>
                      ) : (
                        <span style={{ color: 'var(--text-muted)' }}>—</span>
                      )}
                    </td>
                    <td>
                      <span className={`badge-pill ${
                        rev.status === 'Finalized' ? 'badge-success' : 
                        rev.status === 'SelfSubmitted' ? 'badge-primary' : 'badge-warning'
                      }`}>
                        {rev.status}
                      </span>
                    </td>
                    <td>
                      <div style={{ display: 'flex', gap: '4px', flexWrap: 'wrap' }}>
                        {rev.status === 'Pending' && (
                          <button 
                            type="button" 
                            className="action-btn"
                            onClick={() => {
                              setSelectedReview(rev);
                              setShowSelfModal(true);
                            }}
                          >
                            Submit Self Review
                          </button>
                        )}
                        {(rev.status === 'SelfSubmitted' || rev.status === 'Pending') && (
                          <button 
                            type="button" 
                            className="action-btn"
                            onClick={() => {
                              setSelectedReview(rev);
                              setManagerData({
                                managerRating: rev.selfRating || 4.0,
                                managerComments: '',
                                finalRating: rev.selfRating || 4.0
                              });
                              setShowManagerModal(true);
                            }}
                          >
                            Manager Evaluation
                          </button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={8} style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-muted)' }}>
                    No employee appraisals started yet. Click <strong>Initiate Employee Appraisal</strong> to begin.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      )}

      {/* TAB 2: CYCLES TABLE */}
      {tab === 'cycles' && (
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(320px, 1fr))', gap: '1rem' }}>
          {cycles.length > 0 ? (
            cycles.map((cyc) => (
              <div key={cyc.id} className="glass-card-item">
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '0.75rem' }}>
                  <div>
                    <span className="badge-pill badge-primary" style={{ marginBottom: '0.35rem', display: 'inline-block' }}>
                      Year {cyc.year}
                    </span>
                    <h3 style={{ fontSize: '1.1rem', fontWeight: 700, margin: 0 }}>{cyc.name}</h3>
                  </div>
                  <span className={`badge-pill ${cyc.status === 'Active' ? 'badge-success' : 'badge-neutral'}`}>
                    {cyc.status}
                  </span>
                </div>

                <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.85rem', color: 'var(--text-muted)', margin: '1rem 0' }}>
                  <div>
                    <span>Start:</span> <strong>{cyc.startDate}</strong>
                  </div>
                  <div>
                    <span>End:</span> <strong>{cyc.endDate}</strong>
                  </div>
                </div>

                <div style={{ paddingTop: '0.75rem', borderTop: '1px solid var(--border-subtle)', fontSize: '0.8rem', color: 'var(--text-muted)' }}>
                  Total Appraisals in Cycle: <strong>{cyc.totalReviews}</strong>
                </div>
              </div>
            ))
          ) : (
            <div style={{ gridColumn: '1 / -1', textAlign: 'center', padding: '3rem', color: 'var(--text-muted)' }}>
              No review cycles found.
            </div>
          )}
        </div>
      )}

      {/* MODAL: CREATE CYCLE */}
      {showCycleModal && (
        <div className="modal-backdrop">
          <div className="modal-content">
            <h3 style={{ margin: '0 0 1rem', fontSize: '1.2rem', fontWeight: 700 }}>Create New Review Cycle</h3>
            <form onSubmit={handleCreateCycle} style={{ display: 'flex', flexDirection: 'column', gap: '0.85rem' }}>
              <div className="form-group">
                <label className="form-label">Cycle Title</label>
                <input 
                  type="text" 
                  className="input-field" 
                  required 
                  value={newCycle.name}
                  onChange={e => setNewCycle({ ...newCycle, name: e.target.value })}
                />
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: '0.75rem' }}>
                <div className="form-group">
                  <label className="form-label">Review Year</label>
                  <input 
                    type="number" 
                    className="input-field" 
                    required 
                    value={newCycle.year}
                    onChange={e => setNewCycle({ ...newCycle, year: parseInt(e.target.value) || new Date().getFullYear() })}
                  />
                </div>
                <div className="form-group">
                  <label className="form-label">Start Date</label>
                  <input 
                    type="date" 
                    className="input-field" 
                    required 
                    value={newCycle.startDate}
                    onChange={e => setNewCycle({ ...newCycle, startDate: e.target.value })}
                  />
                </div>
                <div className="form-group">
                  <label className="form-label">End Date</label>
                  <input 
                    type="date" 
                    className="input-field" 
                    required 
                    value={newCycle.endDate}
                    onChange={e => setNewCycle({ ...newCycle, endDate: e.target.value })}
                  />
                </div>
              </div>

              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '0.5rem', marginTop: '1rem' }}>
                <button type="button" className="btn-secondary" onClick={() => setShowCycleModal(false)}>Cancel</button>
                <button type="submit" className="btn-primary">Create Cycle</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* MODAL: INITIATE APPRAISAL */}
      {showCreateReviewModal && (
        <div className="modal-backdrop">
          <div className="modal-content">
            <h3 style={{ margin: '0 0 1rem', fontSize: '1.2rem', fontWeight: 700 }}>Initiate Employee Appraisal</h3>
            <form onSubmit={handleCreateReview} style={{ display: 'flex', flexDirection: 'column', gap: '0.85rem' }}>
              <div className="form-group">
                <label className="form-label">Review Cycle</label>
                <select 
                  className="input-field" 
                  required
                  value={newReviewData.reviewCycleId}
                  onChange={e => setNewReviewData({ ...newReviewData, reviewCycleId: parseInt(e.target.value) })}
                >
                  <option value="">-- Choose Review Cycle --</option>
                  {cycles.map(c => (
                    <option key={c.id} value={c.id}>{c.name} ({c.year})</option>
                  ))}
                </select>
              </div>

              <div className="form-group">
                <label className="form-label">Employee to Review</label>
                <select 
                  className="input-field" 
                  required
                  value={newReviewData.employeeId}
                  onChange={e => setNewReviewData({ ...newReviewData, employeeId: parseInt(e.target.value) })}
                >
                  <option value="">-- Choose Employee --</option>
                  {employees.map(e => (
                    <option key={e.id} value={e.id}>{e.fullName || `${e.firstName} ${e.lastName}`} ({e.employeeCode})</option>
                  ))}
                </select>
              </div>

              <div className="form-group">
                <label className="form-label">Assigned Reviewer / Manager</label>
                <select 
                  className="input-field" 
                  required
                  value={newReviewData.reviewerId}
                  onChange={e => setNewReviewData({ ...newReviewData, reviewerId: parseInt(e.target.value) })}
                >
                  <option value="">-- Choose Reviewer --</option>
                  {employees.map(e => (
                    <option key={e.id} value={e.id}>{e.fullName || `${e.firstName} ${e.lastName}`} ({e.departmentName || 'Staff'})</option>
                  ))}
                </select>
              </div>

              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '0.5rem', marginTop: '1rem' }}>
                <button type="button" className="btn-secondary" onClick={() => setShowCreateReviewModal(false)}>Cancel</button>
                <button type="submit" className="btn-primary">Initiate Appraisal</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* MODAL: SUBMIT SELF REVIEW */}
      {showSelfModal && selectedReview && (
        <div className="modal-backdrop">
          <div className="modal-content">
            <h3 style={{ margin: '0 0 1rem', fontSize: '1.2rem', fontWeight: 700 }}>
              Self-Appraisal: {selectedReview.employeeName}
            </h3>
            <form onSubmit={handleSelfSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '0.85rem' }}>
              <div className="form-group">
                <label className="form-label">Overall Self Rating (1.0 to 5.0)</label>
                <input 
                  type="number" 
                  step="0.1" 
                  min={1} 
                  max={5} 
                  className="input-field" 
                  required
                  value={selfData.selfRating}
                  onChange={e => setSelfData({ ...selfData, selfRating: parseFloat(e.target.value) || 4 })}
                />
              </div>

              <div className="form-group">
                <label className="form-label">Self Reflections &amp; Major Achievements</label>
                <textarea 
                  className="input-field" 
                  rows={4} 
                  required
                  value={selfData.selfComments}
                  onChange={e => setSelfData({ ...selfData, selfComments: e.target.value })}
                  placeholder="Detail your milestones, completed deliverables, leadership, and areas for improvement..."
                />
              </div>

              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '0.5rem', marginTop: '1rem' }}>
                <button type="button" className="btn-secondary" onClick={() => setShowSelfModal(false)}>Cancel</button>
                <button type="submit" className="btn-primary">Submit Self-Appraisal</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* MODAL: MANAGER EVALUATION */}
      {showManagerModal && selectedReview && (
        <div className="modal-backdrop">
          <div className="modal-content">
            <h3 style={{ margin: '0 0 1rem', fontSize: '1.2rem', fontWeight: 700 }}>
              Manager Evaluation: {selectedReview.employeeName}
            </h3>
            <form onSubmit={handleManagerSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '0.85rem' }}>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0.75rem' }}>
                <div className="form-group">
                  <label className="form-label">Manager Rating (1.0 - 5.0)</label>
                  <input 
                    type="number" 
                    step="0.1" 
                    min={1} 
                    max={5} 
                    className="input-field" 
                    required
                    value={managerData.managerRating}
                    onChange={e => {
                      const val = parseFloat(e.target.value) || 4;
                      setManagerData({ ...managerData, managerRating: val, finalRating: val });
                    }}
                  />
                </div>
                <div className="form-group">
                  <label className="form-label">Final Agreed Rating (1.0 - 5.0)</label>
                  <input 
                    type="number" 
                    step="0.1" 
                    min={1} 
                    max={5} 
                    className="input-field" 
                    required
                    value={managerData.finalRating}
                    onChange={e => setManagerData({ ...managerData, finalRating: parseFloat(e.target.value) || 4 })}
                  />
                </div>
              </div>

              <div className="form-group">
                <label className="form-label">Manager Review Remarks &amp; Growth Plan</label>
                <textarea 
                  className="input-field" 
                  rows={4} 
                  required
                  value={managerData.managerComments}
                  onChange={e => setManagerData({ ...managerData, managerComments: e.target.value })}
                  placeholder="Manager observations, strengths, mentorship feedback, compensation recommendation..."
                />
              </div>

              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '0.5rem', marginTop: '1rem' }}>
                <button type="button" className="btn-secondary" onClick={() => setShowManagerModal(false)}>Cancel</button>
                <button type="submit" className="btn-primary">Finalize Appraisal</button>
              </div>
            </form>
          </div>
        </div>
      )}

    </div>
  );
}
