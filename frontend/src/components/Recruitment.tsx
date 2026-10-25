import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { 
  Briefcase, 
  UserCheck, 
  Calendar, 
  FileText, 
  Plus, 
  AlertCircle, 
  Check, 
  ExternalLink
} from 'lucide-react';

interface RecruitmentProps {
  token: string;
}

const API_BASE = 'http://localhost:5219/api/v1';

export default function Recruitment({ token }: RecruitmentProps) {
  const [tab, setTab] = useState<'jobs' | 'pipeline' | 'interviews' | 'offers'>('jobs');
  
  // Data states
  const [jobs, setJobs] = useState<any[]>([]);
  const [applications, setApplications] = useState<any[]>([]);
  const [candidates, setCandidates] = useState<any[]>([]);
  const [interviews, setInterviews] = useState<any[]>([]);
  const [offers, setOffers] = useState<any[]>([]);
  const [departments, setDepartments] = useState<any[]>([]);
  const [branches, setBranches] = useState<any[]>([]);
  
  const [error, setError] = useState<string | null>(null);
  const [successMsg, setSuccessMsg] = useState<string | null>(null);

  // Modals
  const [showJobModal, setShowJobModal] = useState(false);
  const [newJob, setNewJob] = useState({
    title: '',
    departmentId: 1,
    branchId: 1,
    vacancies: 1,
    employmentType: 'Full-Time',
    experienceMinYrs: 2,
    experienceMaxYrs: 5,
    salaryMin: 50000,
    salaryMax: 90000,
    description: '',
    requirements: ''
  });

  const [showCandidateModal, setShowCandidateModal] = useState(false);
  const [newCandidate, setNewCandidate] = useState({
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    linkedInUrl: '',
    source: 'LinkedIn'
  });

  const [showApplyModal, setShowApplyModal] = useState(false);
  const [applyData, setApplyData] = useState({ jobOpeningId: 0, candidateId: 0, notes: '' });

  const [showInterviewModal, setShowInterviewModal] = useState(false);
  const [interviewData, setInterviewData] = useState({
    applicationId: 0,
    interviewerId: 1,
    scheduledAt: new Date(Date.now() + 86400000 * 2).toISOString().slice(0, 16),
    mode: 'Online',
    location: '',
    meetingLink: 'https://meet.google.com/interview-room'
  });

  const [showFeedbackModal, setShowFeedbackModal] = useState(false);
  const [feedbackData, setFeedbackData] = useState({
    interviewId: 0,
    feedback: '',
    score: 4.5,
    result: 'Passed'
  });

  const [showOfferModal, setShowOfferModal] = useState(false);
  const [offerData, setOfferData] = useState({
    applicationId: 0,
    offeredSalary: 85000,
    joiningDate: new Date(Date.now() + 86400000 * 14).toISOString().slice(0, 10),
    expiryDate: new Date(Date.now() + 86400000 * 7).toISOString().slice(0, 10)
  });

  const headers = { Authorization: `Bearer ${token}` };

  const loadData = async () => {
    setError(null);
    try {
      if (tab === 'jobs') {
        const res = await axios.get(`${API_BASE}/recruitment/jobs?pageSize=50`, { headers });
        setJobs(res.data.data?.items || []);
      } else if (tab === 'pipeline') {
        const [appRes, candRes, jobRes] = await Promise.all([
          axios.get(`${API_BASE}/recruitment/applications`, { headers }),
          axios.get(`${API_BASE}/recruitment/candidates?pageSize=50`, { headers }),
          axios.get(`${API_BASE}/recruitment/jobs?pageSize=50`, { headers })
        ]);
        setApplications(appRes.data.data || []);
        setCandidates(candRes.data.data?.items || []);
        setJobs(jobRes.data.data?.items || []);
      } else if (tab === 'interviews') {
        const res = await axios.get(`${API_BASE}/recruitment/interviews`, { headers });
        setInterviews(res.data.data || []);
      } else if (tab === 'offers') {
        const res = await axios.get(`${API_BASE}/recruitment/offers`, { headers });
        setOffers(res.data.data || []);
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error loading recruitment data.');
    }
  };

  useEffect(() => {
    loadData();
  }, [tab, token]);

  // Load departments and branches on mount
  useEffect(() => {
    Promise.all([
      axios.get(`${API_BASE}/departments`, { headers }),
      axios.get(`${API_BASE}/branches`, { headers })
    ]).then(([dRes, bRes]) => {
      setDepartments(dRes.data.data || []);
      setBranches(bRes.data.data?.items || []);
      if (dRes.data.data?.length > 0) {
        setNewJob(prev => ({ ...prev, departmentId: dRes.data.data[0].id }));
      }
      if (bRes.data.data?.items?.length > 0) {
        setNewJob(prev => ({ ...prev, branchId: bRes.data.data.items[0].id }));
      }
    }).catch(console.error);
  }, []);

  const handleCreateJob = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const res = await axios.post(`${API_BASE}/recruitment/jobs`, newJob, { headers });
      if (res.data.success) {
        setSuccessMsg('Job opening published successfully.');
        setShowJobModal(false);
        loadData();
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to create job opening.');
    }
  };

  const handleCreateCandidate = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const res = await axios.post(`${API_BASE}/recruitment/candidates`, newCandidate, { headers });
      if (res.data.success) {
        setSuccessMsg('Candidate added successfully.');
        setShowCandidateModal(false);
        loadData();
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to create candidate.');
    }
  };

  const handleApply = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const res = await axios.post(`${API_BASE}/recruitment/applications`, applyData, { headers });
      if (res.data.success) {
        setSuccessMsg('Candidate linked to position.');
        setShowApplyModal(false);
        loadData();
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to apply candidate.');
    }
  };

  const handleStageChange = async (appId: number, nextStage: string) => {
    try {
      const res = await axios.patch(
        `${API_BASE}/recruitment/applications/${appId}/stage`,
        { stage: nextStage },
        { headers }
      );
      if (res.data.success) {
        setSuccessMsg(`Application stage moved to ${nextStage}.`);
        loadData();
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to update stage.');
    }
  };

  const handleScheduleInterview = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const res = await axios.post(`${API_BASE}/recruitment/interviews`, interviewData, { headers });
      if (res.data.success) {
        setSuccessMsg('Interview scheduled successfully.');
        setShowInterviewModal(false);
        loadData();
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to schedule interview.');
    }
  };

  const handleSubmitFeedback = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const res = await axios.post(
        `${API_BASE}/recruitment/interviews/${feedbackData.interviewId}/feedback`,
        { feedback: feedbackData.feedback, score: feedbackData.score, result: feedbackData.result },
        { headers }
      );
      if (res.data.success) {
        setSuccessMsg('Interview evaluation submitted.');
        setShowFeedbackModal(false);
        loadData();
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to submit feedback.');
    }
  };

  const handleCreateOffer = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const res = await axios.post(`${API_BASE}/recruitment/offers`, offerData, { headers });
      if (res.data.success) {
        setSuccessMsg('Offer letter issued to candidate.');
        setShowOfferModal(false);
        loadData();
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to create offer.');
    }
  };

  const handleOfferStatus = async (offerId: number, status: string) => {
    try {
      const res = await axios.patch(`${API_BASE}/recruitment/offers/${offerId}/status`, { status }, { headers });
      if (res.data.success) {
        setSuccessMsg(`Offer letter marked as ${status}.`);
        loadData();
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to update offer status.');
    }
  };

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '1.25rem' }}>
      
      {/* Module Title & Sub-tabs */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: '1rem' }}>
        <div>
          <h2 style={{ fontSize: '1.4rem', fontWeight: 800, margin: 0, display: 'flex', alignItems: 'center', gap: '8px' }}>
            <Briefcase size={22} color="#a855f7" />
            Recruitment &amp; Applicant Tracking (ATS)
          </h2>
          <p style={{ margin: '0.25rem 0 0', color: 'var(--text-muted)', fontSize: '0.85rem' }}>
            Manage openings, applicant pipeline stages, interview assessments, and job offers.
          </p>
        </div>

        {/* Action Buttons based on active tab */}
        <div style={{ display: 'flex', gap: '0.5rem' }}>
          {tab === 'jobs' && (
            <button type="button" className="btn-primary" onClick={() => setShowJobModal(true)}>
              <Plus size={16} /> Post New Job Opening
            </button>
          )}
          {tab === 'pipeline' && (
            <>
              <button type="button" className="btn-secondary" onClick={() => setShowCandidateModal(true)}>
                <Plus size={16} /> Add Candidate
              </button>
              <button type="button" className="btn-primary" onClick={() => setShowApplyModal(true)}>
                <UserCheck size={16} /> Link to Job
              </button>
            </>
          )}
        </div>
      </div>

      {/* Sub-nav Tabs */}
      <div className="subnav-tabs">
        <button 
          type="button" 
          className={`subnav-btn ${tab === 'jobs' ? 'active' : ''}`}
          onClick={() => setTab('jobs')}
        >
          <Briefcase size={16} /> Job Openings ({jobs.length})
        </button>
        <button 
          type="button" 
          className={`subnav-btn ${tab === 'pipeline' ? 'active' : ''}`}
          onClick={() => setTab('pipeline')}
        >
          <UserCheck size={16} /> Candidate Pipeline ({applications.length})
        </button>
        <button 
          type="button" 
          className={`subnav-btn ${tab === 'interviews' ? 'active' : ''}`}
          onClick={() => setTab('interviews')}
        >
          <Calendar size={16} /> Interview Schedule ({interviews.length})
        </button>
        <button 
          type="button" 
          className={`subnav-btn ${tab === 'offers' ? 'active' : ''}`}
          onClick={() => setTab('offers')}
        >
          <FileText size={16} /> Offer Letters ({offers.length})
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

      {/* TAB 1: JOB OPENINGS */}
      {tab === 'jobs' && (
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(320px, 1fr))', gap: '1rem' }}>
          {jobs.length > 0 ? (
            jobs.map((job) => (
              <div key={job.id} className="glass-card-item">
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '0.75rem' }}>
                  <div>
                    <span className="badge-pill badge-primary" style={{ marginBottom: '0.4rem', display: 'inline-block' }}>
                      {job.departmentName}
                    </span>
                    <h3 style={{ fontSize: '1.1rem', fontWeight: 700, margin: 0 }}>{job.title}</h3>
                    <p style={{ margin: '0.25rem 0 0', fontSize: '0.8rem', color: 'var(--text-muted)' }}>
                      {job.branchName} • {job.employmentType}
                    </p>
                  </div>
                  <span className={`badge-pill ${job.status === 'Open' ? 'badge-success' : 'badge-neutral'}`}>
                    {job.status}
                  </span>
                </div>

                <div style={{ fontSize: '0.825rem', color: 'var(--text-muted)', marginBottom: '0.75rem', lineHeight: 1.4 }}>
                  {job.description || 'No detailed description provided.'}
                </div>

                <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.8rem', paddingTop: '0.75rem', borderTop: '1px solid var(--border-subtle)' }}>
                  <div>
                    <span style={{ color: 'var(--text-muted)' }}>Vacancies:</span> <strong>{job.vacancies}</strong>
                  </div>
                  <div>
                    <span style={{ color: 'var(--text-muted)' }}>Salary:</span> <strong>${job.salaryMin?.toLocaleString()} - ${job.salaryMax?.toLocaleString()}</strong>
                  </div>
                  <div>
                    <span style={{ color: 'var(--text-muted)' }}>Applicants:</span> <strong>{job.applicationsCount}</strong>
                  </div>
                </div>
              </div>
            ))
          ) : (
            <div style={{ gridColumn: '1 / -1', textAlign: 'center', padding: '3rem', color: 'var(--text-muted)' }}>
              No active job openings found. Click <strong>Post New Job Opening</strong> to create one.
            </div>
          )}
        </div>
      )}

      {/* TAB 2: CANDIDATE PIPELINE */}
      {tab === 'pipeline' && (
        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>Candidate</th>
                <th>Applying For</th>
                <th>Contact</th>
                <th>Pipeline Stage</th>
                <th>Interviews</th>
                <th>Applied Date</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {applications.length > 0 ? (
                applications.map((app) => (
                  <tr key={app.id}>
                    <td>
                      <strong>{app.candidateName}</strong>
                      <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>{app.candidateEmail}</div>
                    </td>
                    <td>{app.jobOpeningTitle}</td>
                    <td>{app.candidatePhone || 'N/A'}</td>
                    <td>
                      <span className={`badge-pill ${
                        app.stage === 'Hired' ? 'badge-success' : 
                        app.stage === 'Offered' ? 'badge-primary' : 
                        app.stage === 'Interview' ? 'badge-warning' : 'badge-neutral'
                      }`}>
                        {app.stage}
                      </span>
                    </td>
                    <td>{app.interviewsCount} recorded</td>
                    <td>{new Date(app.createdAt).toLocaleDateString()}</td>
                    <td>
                      <div style={{ display: 'flex', gap: '4px', flexWrap: 'wrap' }}>
                        {app.stage === 'Applied' && (
                          <button type="button" className="action-btn" onClick={() => handleStageChange(app.id, 'Shortlisted')}>
                            Shortlist
                          </button>
                        )}
                        {app.stage === 'Shortlisted' && (
                          <button 
                            type="button" 
                            className="action-btn"
                            onClick={() => {
                              setInterviewData(prev => ({ ...prev, applicationId: app.id }));
                              setShowInterviewModal(true);
                            }}
                          >
                            Schedule Interview
                          </button>
                        )}
                        {app.stage === 'Interview' && (
                          <button 
                            type="button" 
                            className="action-btn"
                            onClick={() => {
                              setOfferData(prev => ({ ...prev, applicationId: app.id }));
                              setShowOfferModal(true);
                            }}
                          >
                            Generate Offer
                          </button>
                        )}
                        {app.stage === 'Offered' && (
                          <button type="button" className="action-btn" onClick={() => handleStageChange(app.id, 'Hired')}>
                            Mark Hired
                          </button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={7} style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-muted)' }}>
                    No candidates currently in the pipeline.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      )}

      {/* TAB 3: INTERVIEW SCHEDULE */}
      {tab === 'interviews' && (
        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>Candidate</th>
                <th>Position</th>
                <th>Interviewer</th>
                <th>Scheduled At</th>
                <th>Mode &amp; Link</th>
                <th>Result</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {interviews.length > 0 ? (
                interviews.map((item) => (
                  <tr key={item.id}>
                    <td><strong>{item.candidateName}</strong></td>
                    <td>{item.jobTitle}</td>
                    <td>{item.interviewerName}</td>
                    <td>{new Date(item.scheduledAt).toLocaleString()}</td>
                    <td>
                      <span className="badge-pill badge-primary">{item.mode}</span>
                      {item.meetingLink && (
                        <a href={item.meetingLink} target="_blank" rel="noreferrer" style={{ marginLeft: 6, color: '#818cf8', display: 'inline-flex', verticalAlign: 'middle' }}>
                          <ExternalLink size={14} />
                        </a>
                      )}
                    </td>
                    <td>
                      <span className={`badge-pill ${item.result === 'Passed' ? 'badge-success' : item.result === 'Failed' ? 'badge-danger' : 'badge-neutral'}`}>
                        {item.result || 'Pending'}
                      </span>
                      {item.score && <span style={{ marginLeft: 6, fontSize: '0.8rem' }}>({item.score}/5)</span>}
                    </td>
                    <td>
                      <button 
                        type="button" 
                        className="action-btn"
                        onClick={() => {
                          setFeedbackData(prev => ({ ...prev, interviewId: item.id }));
                          setShowFeedbackModal(true);
                        }}
                      >
                        Evaluate Feedback
                      </button>
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={7} style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-muted)' }}>
                    No interviews scheduled.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      )}

      {/* TAB 4: OFFER LETTERS */}
      {tab === 'offers' && (
        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>Candidate</th>
                <th>Position</th>
                <th>Offered Salary</th>
                <th>Joining Date</th>
                <th>Expiry Date</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {offers.length > 0 ? (
                offers.map((offer) => (
                  <tr key={offer.id}>
                    <td><strong>{offer.candidateName}</strong></td>
                    <td>{offer.jobTitle}</td>
                    <td>${offer.offeredSalary?.toLocaleString()}</td>
                    <td>{offer.joiningDate}</td>
                    <td>{offer.expiryDate}</td>
                    <td>
                      <span className={`badge-pill ${
                        offer.status === 'Accepted' ? 'badge-success' : 
                        offer.status === 'Declined' ? 'badge-danger' : 'badge-warning'
                      }`}>
                        {offer.status}
                      </span>
                    </td>
                    <td>
                      {offer.status === 'Sent' && (
                        <div style={{ display: 'flex', gap: '4px' }}>
                          <button type="button" className="action-btn" onClick={() => handleOfferStatus(offer.id, 'Accepted')}>
                            Accept
                          </button>
                          <button type="button" className="action-btn text-danger" onClick={() => handleOfferStatus(offer.id, 'Declined')}>
                            Decline
                          </button>
                        </div>
                      )}
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={7} style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-muted)' }}>
                    No offer letters created.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      )}

      {/* MODAL: POST JOB OPENING */}
      {showJobModal && (
        <div className="modal-backdrop">
          <div className="modal-content">
            <h3 style={{ margin: '0 0 1rem', fontSize: '1.2rem', fontWeight: 700 }}>Post New Job Opening</h3>
            <form onSubmit={handleCreateJob} style={{ display: 'flex', flexDirection: 'column', gap: '0.85rem' }}>
              <div className="form-group">
                <label className="form-label">Position Title</label>
                <input 
                  type="text" 
                  className="input-field" 
                  required 
                  value={newJob.title}
                  onChange={e => setNewJob({ ...newJob, title: e.target.value })}
                  placeholder="e.g. Lead Cloud Architect"
                />
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0.75rem' }}>
                <div className="form-group">
                  <label className="form-label">Department</label>
                  <select 
                    className="input-field" 
                    value={newJob.departmentId}
                    onChange={e => setNewJob({ ...newJob, departmentId: parseInt(e.target.value) })}
                  >
                    {departments.map(d => (
                      <option key={d.id} value={d.id}>{d.name}</option>
                    ))}
                  </select>
                </div>
                <div className="form-group">
                  <label className="form-label">Branch</label>
                  <select 
                    className="input-field" 
                    value={newJob.branchId}
                    onChange={e => setNewJob({ ...newJob, branchId: parseInt(e.target.value) })}
                  >
                    {branches.map(b => (
                      <option key={b.id} value={b.id}>{b.name}</option>
                    ))}
                  </select>
                </div>
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: '0.75rem' }}>
                <div className="form-group">
                  <label className="form-label">Vacancies</label>
                  <input 
                    type="number" 
                    className="input-field" 
                    min={1} 
                    value={newJob.vacancies}
                    onChange={e => setNewJob({ ...newJob, vacancies: parseInt(e.target.value) || 1 })}
                  />
                </div>
                <div className="form-group">
                  <label className="form-label">Min Salary ($)</label>
                  <input 
                    type="number" 
                    className="input-field" 
                    value={newJob.salaryMin}
                    onChange={e => setNewJob({ ...newJob, salaryMin: parseFloat(e.target.value) || 0 })}
                  />
                </div>
                <div className="form-group">
                  <label className="form-label">Max Salary ($)</label>
                  <input 
                    type="number" 
                    className="input-field" 
                    value={newJob.salaryMax}
                    onChange={e => setNewJob({ ...newJob, salaryMax: parseFloat(e.target.value) || 0 })}
                  />
                </div>
              </div>

              <div className="form-group">
                <label className="form-label">Key Requirements</label>
                <textarea 
                  className="input-field" 
                  rows={3}
                  value={newJob.requirements}
                  onChange={e => setNewJob({ ...newJob, requirements: e.target.value })}
                  placeholder="Required skills, qualifications, certifications..."
                />
              </div>

              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '0.5rem', marginTop: '1rem' }}>
                <button type="button" className="btn-secondary" onClick={() => setShowJobModal(false)}>Cancel</button>
                <button type="submit" className="btn-primary">Publish Job Opening</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* MODAL: ADD CANDIDATE */}
      {showCandidateModal && (
        <div className="modal-backdrop">
          <div className="modal-content">
            <h3 style={{ margin: '0 0 1rem', fontSize: '1.2rem', fontWeight: 700 }}>Add New Candidate</h3>
            <form onSubmit={handleCreateCandidate} style={{ display: 'flex', flexDirection: 'column', gap: '0.85rem' }}>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0.75rem' }}>
                <div className="form-group">
                  <label className="form-label">First Name</label>
                  <input 
                    type="text" 
                    className="input-field" 
                    required 
                    value={newCandidate.firstName}
                    onChange={e => setNewCandidate({ ...newCandidate, firstName: e.target.value })}
                  />
                </div>
                <div className="form-group">
                  <label className="form-label">Last Name</label>
                  <input 
                    type="text" 
                    className="input-field" 
                    required 
                    value={newCandidate.lastName}
                    onChange={e => setNewCandidate({ ...newCandidate, lastName: e.target.value })}
                  />
                </div>
              </div>

              <div className="form-group">
                <label className="form-label">Email</label>
                <input 
                  type="email" 
                  className="input-field" 
                  required 
                  value={newCandidate.email}
                  onChange={e => setNewCandidate({ ...newCandidate, email: e.target.value })}
                />
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0.75rem' }}>
                <div className="form-group">
                  <label className="form-label">Phone</label>
                  <input 
                    type="text" 
                    className="input-field" 
                    value={newCandidate.phone}
                    onChange={e => setNewCandidate({ ...newCandidate, phone: e.target.value })}
                  />
                </div>
                <div className="form-group">
                  <label className="form-label">Source</label>
                  <select 
                    className="input-field" 
                    value={newCandidate.source}
                    onChange={e => setNewCandidate({ ...newCandidate, source: e.target.value })}
                  >
                    <option value="LinkedIn">LinkedIn</option>
                    <option value="Referral">Referral</option>
                    <option value="Careers Page">Careers Page</option>
                    <option value="Agency">Agency</option>
                  </select>
                </div>
              </div>

              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '0.5rem', marginTop: '1rem' }}>
                <button type="button" className="btn-secondary" onClick={() => setShowCandidateModal(false)}>Cancel</button>
                <button type="submit" className="btn-primary">Save Candidate</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* MODAL: LINK CANDIDATE TO JOB */}
      {showApplyModal && (
        <div className="modal-backdrop">
          <div className="modal-content">
            <h3 style={{ margin: '0 0 1rem', fontSize: '1.2rem', fontWeight: 700 }}>Link Candidate to Job Position</h3>
            <form onSubmit={handleApply} style={{ display: 'flex', flexDirection: 'column', gap: '0.85rem' }}>
              <div className="form-group">
                <label className="form-label">Select Job Opening</label>
                <select 
                  className="input-field" 
                  required
                  value={applyData.jobOpeningId}
                  onChange={e => setApplyData({ ...applyData, jobOpeningId: parseInt(e.target.value) })}
                >
                  <option value="">-- Choose Position --</option>
                  {jobs.map(j => (
                    <option key={j.id} value={j.id}>{j.title} ({j.departmentName})</option>
                  ))}
                </select>
              </div>

              <div className="form-group">
                <label className="form-label">Select Candidate</label>
                <select 
                  className="input-field" 
                  required
                  value={applyData.candidateId}
                  onChange={e => setApplyData({ ...applyData, candidateId: parseInt(e.target.value) })}
                >
                  <option value="">-- Choose Candidate --</option>
                  {candidates.map(c => (
                    <option key={c.id} value={c.id}>{c.fullName} ({c.email})</option>
                  ))}
                </select>
              </div>

              <div className="form-group">
                <label className="form-label">Screening Notes</label>
                <textarea 
                  className="input-field" 
                  rows={2}
                  value={applyData.notes}
                  onChange={e => setApplyData({ ...applyData, notes: e.target.value })}
                  placeholder="Initial screening remarks..."
                />
              </div>

              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '0.5rem', marginTop: '1rem' }}>
                <button type="button" className="btn-secondary" onClick={() => setShowApplyModal(false)}>Cancel</button>
                <button type="submit" className="btn-primary">Add to Pipeline</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* MODAL: SCHEDULE INTERVIEW */}
      {showInterviewModal && (
        <div className="modal-backdrop">
          <div className="modal-content">
            <h3 style={{ margin: '0 0 1rem', fontSize: '1.2rem', fontWeight: 700 }}>Schedule Candidate Interview</h3>
            <form onSubmit={handleScheduleInterview} style={{ display: 'flex', flexDirection: 'column', gap: '0.85rem' }}>
              <div className="form-group">
                <label className="form-label">Interview Date &amp; Time</label>
                <input 
                  type="datetime-local" 
                  className="input-field" 
                  required 
                  value={interviewData.scheduledAt}
                  onChange={e => setInterviewData({ ...interviewData, scheduledAt: e.target.value })}
                />
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0.75rem' }}>
                <div className="form-group">
                  <label className="form-label">Interview Mode</label>
                  <select 
                    className="input-field" 
                    value={interviewData.mode}
                    onChange={e => setInterviewData({ ...interviewData, mode: e.target.value })}
                  >
                    <option value="Online">Online Video (Google Meet / Zoom)</option>
                    <option value="InPerson">In Person (Office)</option>
                    <option value="Phone">Phone Screening</option>
                  </select>
                </div>
                <div className="form-group">
                  <label className="form-label">Meeting URL or Room</label>
                  <input 
                    type="text" 
                    className="input-field" 
                    value={interviewData.meetingLink}
                    onChange={e => setInterviewData({ ...interviewData, meetingLink: e.target.value })}
                  />
                </div>
              </div>

              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '0.5rem', marginTop: '1rem' }}>
                <button type="button" className="btn-secondary" onClick={() => setShowInterviewModal(false)}>Cancel</button>
                <button type="submit" className="btn-primary">Confirm Interview</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* MODAL: SUBMIT INTERVIEW FEEDBACK */}
      {showFeedbackModal && (
        <div className="modal-backdrop">
          <div className="modal-content">
            <h3 style={{ margin: '0 0 1rem', fontSize: '1.2rem', fontWeight: 700 }}>Interview Assessment Evaluation</h3>
            <form onSubmit={handleSubmitFeedback} style={{ display: 'flex', flexDirection: 'column', gap: '0.85rem' }}>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0.75rem' }}>
                <div className="form-group">
                  <label className="form-label">Score (1.0 - 5.0)</label>
                  <input 
                    type="number" 
                    step="0.1" 
                    min={1} 
                    max={5} 
                    className="input-field" 
                    value={feedbackData.score}
                    onChange={e => setFeedbackData({ ...feedbackData, score: parseFloat(e.target.value) || 3 })}
                  />
                </div>
                <div className="form-group">
                  <label className="form-label">Verdict Result</label>
                  <select 
                    className="input-field" 
                    value={feedbackData.result}
                    onChange={e => setFeedbackData({ ...feedbackData, result: e.target.value })}
                  >
                    <option value="Passed">Passed (Recommend Next Stage / Offer)</option>
                    <option value="Failed">Failed (Does Not Meet Bar)</option>
                    <option value="Hold">On Hold</option>
                  </select>
                </div>
              </div>

              <div className="form-group">
                <label className="form-label">Detailed Technical &amp; Cultural Feedback</label>
                <textarea 
                  className="input-field" 
                  rows={4} 
                  required
                  value={feedbackData.feedback}
                  onChange={e => setFeedbackData({ ...feedbackData, feedback: e.target.value })}
                  placeholder="Notes on communication, domain proficiency, problem solving skills..."
                />
              </div>

              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '0.5rem', marginTop: '1rem' }}>
                <button type="button" className="btn-secondary" onClick={() => setShowFeedbackModal(false)}>Cancel</button>
                <button type="submit" className="btn-primary">Submit Evaluation</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* MODAL: ISSUE OFFER LETTER */}
      {showOfferModal && (
        <div className="modal-backdrop">
          <div className="modal-content">
            <h3 style={{ margin: '0 0 1rem', fontSize: '1.2rem', fontWeight: 700 }}>Issue Formal Offer Letter</h3>
            <form onSubmit={handleCreateOffer} style={{ display: 'flex', flexDirection: 'column', gap: '0.85rem' }}>
              <div className="form-group">
                <label className="form-label">Offered Base Monthly Salary ($)</label>
                <input 
                  type="number" 
                  className="input-field" 
                  required
                  value={offerData.offeredSalary}
                  onChange={e => setOfferData({ ...offerData, offeredSalary: parseFloat(e.target.value) || 0 })}
                />
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0.75rem' }}>
                <div className="form-group">
                  <label className="form-label">Expected Joining Date</label>
                  <input 
                    type="date" 
                    className="input-field" 
                    required 
                    value={offerData.joiningDate}
                    onChange={e => setOfferData({ ...offerData, joiningDate: e.target.value })}
                  />
                </div>
                <div className="form-group">
                  <label className="form-label">Offer Expiry Date</label>
                  <input 
                    type="date" 
                    className="input-field" 
                    required 
                    value={offerData.expiryDate}
                    onChange={e => setOfferData({ ...offerData, expiryDate: e.target.value })}
                  />
                </div>
              </div>

              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '0.5rem', marginTop: '1rem' }}>
                <button type="button" className="btn-secondary" onClick={() => setShowOfferModal(false)}>Cancel</button>
                <button type="submit" className="btn-primary">Send Offer Letter</button>
              </div>
            </form>
          </div>
        </div>
      )}

    </div>
  );
}
