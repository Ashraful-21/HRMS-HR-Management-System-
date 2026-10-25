import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { 
  Laptop, 
  FolderTree, 
  UserCheck, 
  Plus, 
  Check, 
  AlertCircle
} from 'lucide-react';

interface AssetsProps {
  token: string;
  user?: any;
}

const API_BASE = 'http://localhost:5219/api/v1';

export default function Assets({ token, user }: AssetsProps) {
  const isSuperAdmin = user?.roles?.some((r: string) => r.toLowerCase() === 'superadmin') || false;
  const isHRManager = !isSuperAdmin && (user?.roles?.some((r: string) => r.toLowerCase() === 'hrmanager' || r.toLowerCase() === 'admin') || false);
  const isEmployee = !isSuperAdmin && !isHRManager;

  const [tab, setTab] = useState<'inventory' | 'assignments' | 'categories'>('inventory');
  
  const [assets, setAssets] = useState<any[]>([]);
  const [assignments, setAssignments] = useState<any[]>([]);
  const [categories, setCategories] = useState<any[]>([]);
  const [employees, setEmployees] = useState<any[]>([]);
  
  const [error, setError] = useState<string | null>(null);
  const [successMsg, setSuccessMsg] = useState<string | null>(null);

  // Modals
  const [showAssetModal, setShowAssetModal] = useState(false);
  const [newAsset, setNewAsset] = useState({
    assetCode: '',
    name: '',
    categoryId: 1,
    serialNumber: '',
    purchaseDate: new Date().toISOString().slice(0, 10),
    purchaseCost: 1500,
    condition: 'Good',
    notes: ''
  });

  const [showCategoryModal, setShowCategoryModal] = useState(false);
  const [newCategory, setNewCategory] = useState({ name: '', code: '' });

  const [showAssignModal, setShowAssignModal] = useState(false);
  const [assignData, setAssignData] = useState({
    assetId: 0,
    employeeId: 0,
    assignedDate: new Date().toISOString().slice(0, 10),
    condition: 'Good',
    notes: ''
  });

  const [showReturnModal, setShowReturnModal] = useState(false);
  const [returnData, setReturnData] = useState({
    assignmentId: 0,
    returnedDate: new Date().toISOString().slice(0, 10),
    returnCondition: 'Good',
    notes: ''
  });

  const headers = { Authorization: `Bearer ${token}` };

  const loadData = async () => {
    setError(null);
    try {
      if (isEmployee) {
        const empId = user?.employeeId || 1;
        const asgRes = await axios.get(`${API_BASE}/assets/assignments?employeeId=${empId}`, { headers });
        setAssignments(asgRes.data.data || []);
        return;
      }

      if (tab === 'inventory') {
        const [astRes, catRes] = await Promise.all([
          axios.get(`${API_BASE}/assets?pageSize=100`, { headers }),
          axios.get(`${API_BASE}/assets/categories`, { headers })
        ]);
        setAssets(astRes.data.data?.items || []);
        setCategories(catRes.data.data || []);
      } else if (tab === 'assignments') {
        const [asgRes, astRes, empRes] = await Promise.all([
          axios.get(`${API_BASE}/assets/assignments`, { headers }),
          axios.get(`${API_BASE}/assets?pageSize=100`, { headers }),
          axios.get(`${API_BASE}/employees?pageSize=100`, { headers })
        ]);
        setAssignments(asgRes.data.data || []);
        setAssets(astRes.data.data?.items || []);
        setEmployees(empRes.data.data?.items || []);
      } else if (tab === 'categories') {
        const res = await axios.get(`${API_BASE}/assets/categories`, { headers });
        setCategories(res.data.data || []);
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error loading asset records.');
    }
  };

  useEffect(() => {
    loadData();
  }, [tab, token, user]);

  const handleCreateAsset = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const res = await axios.post(`${API_BASE}/assets`, newAsset, { headers });
      if (res.data.success) {
        setSuccessMsg('Asset registered successfully.');
        setShowAssetModal(false);
        loadData();
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to create asset.');
    }
  };

  const handleCreateCategory = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const res = await axios.post(`${API_BASE}/assets/categories`, newCategory, { headers });
      if (res.data.success) {
        setSuccessMsg('Category created.');
        setShowCategoryModal(false);
        loadData();
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to create category.');
    }
  };

  const handleAssignAsset = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const res = await axios.post(`${API_BASE}/assets/assignments`, assignData, { headers });
      if (res.data.success) {
        setSuccessMsg('Asset assigned to employee.');
        setShowAssignModal(false);
        loadData();
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to assign asset.');
    }
  };

  const handleReturnAsset = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const res = await axios.post(
        `${API_BASE}/assets/assignments/${returnData.assignmentId}/return`,
        { returnedDate: returnData.returnedDate, returnCondition: returnData.returnCondition, notes: returnData.notes },
        { headers }
      );
      if (res.data.success) {
        setSuccessMsg('Asset returned and placed back in inventory.');
        setShowReturnModal(false);
        loadData();
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to return asset.');
    }
  };
  // ==========================================
  // EMPLOYEE VIEW (Personal Custody Only)
  // ==========================================
  if (isEmployee) {
    return (
      <div style={{ display: 'flex', flexDirection: 'column', gap: '1.25rem' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: '1rem' }}>
          <div>
            <h2 style={{ fontSize: '1.4rem', fontWeight: 800, margin: 0, display: 'flex', alignItems: 'center', gap: '8px' }}>
              <Laptop size={22} color="#06b6d4" />
              My Equipment &amp; Asset Custody
            </h2>
            <p style={{ margin: '0.25rem 0 0', color: 'var(--text-muted)', fontSize: '0.85rem' }}>
              Hardware devices, laptops, and peripheral equipment currently issued to your custody.
            </p>
          </div>
          <div style={{ padding: '0.4rem 0.85rem', background: 'rgba(99, 102, 241, 0.1)', border: '1px solid rgba(99, 102, 241, 0.3)', borderRadius: 'var(--radius-sm)', fontSize: '0.8rem', color: '#a5b4fc', display: 'flex', alignItems: 'center', gap: '6px' }}>
            <span>👤 Custody Holder:</span>
            <strong>{user?.fullName || user?.username}</strong>
          </div>
        </div>

        {error && (
          <div className="alert-error">
            <AlertCircle size={18} />
            <span>{error}</span>
          </div>
        )}

        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>Device Code &amp; Name</th>
                <th>Serial Number</th>
                <th>Condition</th>
                <th>Issued Date</th>
                <th>Custody Status</th>
              </tr>
            </thead>
            <tbody>
              {assignments.length > 0 ? (
                assignments.map((asg) => (
                  <tr key={asg.id}>
                    <td>
                      <strong>{asg.assetName}</strong>
                      <div style={{ fontSize: '0.75rem', color: '#38bdf8' }}>{asg.assetCode}</div>
                    </td>
                    <td>
                      <code style={{ fontSize: '0.8rem' }}>{asg.serialNumber || 'N/A'}</code>
                    </td>
                    <td>
                      <span className="badge-pill badge-neutral">{asg.condition || 'Good'}</span>
                    </td>
                    <td>
                      {asg.assignedDate ? new Date(asg.assignedDate).toLocaleDateString() : 'Active'}
                    </td>
                    <td>
                      <span className="badge-pill badge-primary">In Your Custody</span>
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={5} style={{ textAlign: 'center', padding: '2.5rem', color: 'var(--text-muted)' }}>
                    <Laptop size={32} color="var(--text-dim)" style={{ margin: '0 auto 0.5rem' }} />
                    <p style={{ margin: 0 }}>No company hardware or devices currently assigned to your profile.</p>
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
    );
  }

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '1.25rem' }}>
      
      {/* Title */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: '1rem' }}>
        <div>
          <h2 style={{ fontSize: '1.4rem', fontWeight: 800, margin: 0, display: 'flex', alignItems: 'center', gap: '8px' }}>
            <Laptop size={22} color="#06b6d4" />
            Company Asset Management
          </h2>
          <p style={{ margin: '0.25rem 0 0', color: 'var(--text-muted)', fontSize: '0.85rem' }}>
            Hardware tracking, device custody, allocations to staff, and condition audits.
          </p>
        </div>

        <div style={{ display: 'flex', gap: '0.5rem' }}>
          {tab === 'inventory' && (
            <button type="button" className="btn-primary" onClick={() => setShowAssetModal(true)}>
              <Plus size={16} /> Register New Asset
            </button>
          )}
          {tab === 'assignments' && (
            <button type="button" className="btn-primary" onClick={() => setShowAssignModal(true)}>
              <UserCheck size={16} /> Assign Asset
            </button>
          )}
          {tab === 'categories' && (
            <button type="button" className="btn-primary" onClick={() => setShowCategoryModal(true)}>
              <Plus size={16} /> Add Category
            </button>
          )}
        </div>
      </div>

      {/* Tabs */}
      <div className="subnav-tabs">
        <button 
          type="button" 
          className={`subnav-btn ${tab === 'inventory' ? 'active' : ''}`}
          onClick={() => setTab('inventory')}
        >
          <Laptop size={16} /> Assets Inventory ({assets.length})
        </button>
        <button 
          type="button" 
          className={`subnav-btn ${tab === 'assignments' ? 'active' : ''}`}
          onClick={() => setTab('assignments')}
        >
          <UserCheck size={16} /> Custody &amp; Assignments ({assignments.length})
        </button>
        <button 
          type="button" 
          className={`subnav-btn ${tab === 'categories' ? 'active' : ''}`}
          onClick={() => setTab('categories')}
        >
          <FolderTree size={16} /> Categories ({categories.length})
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

      {/* TAB 1: INVENTORY TABLE */}
      {tab === 'inventory' && (
        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>Code &amp; Name</th>
                <th>Category</th>
                <th>Serial Number</th>
                <th>Condition</th>
                <th>Purchase Info</th>
                <th>Status</th>
                <th>Current Custody</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {assets.length > 0 ? (
                assets.map((asset) => (
                  <tr key={asset.id}>
                    <td>
                      <strong>{asset.name}</strong>
                      <div style={{ fontSize: '0.75rem', color: '#38bdf8' }}>{asset.assetCode}</div>
                    </td>
                    <td>{asset.categoryName}</td>
                    <td><code style={{ fontSize: '0.8rem' }}>{asset.serialNumber || 'N/A'}</code></td>
                    <td>
                      <span className="badge-pill badge-neutral">{asset.condition}</span>
                    </td>
                    <td>
                      <div>${asset.purchaseCost?.toLocaleString() || '0'}</div>
                      <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>{asset.purchaseDate}</div>
                    </td>
                    <td>
                      <span className={`badge-pill ${
                        asset.status === 'Available' ? 'badge-success' : 
                        asset.status === 'Assigned' ? 'badge-primary' : 'badge-danger'
                      }`}>
                        {asset.status}
                      </span>
                    </td>
                    <td>
                      {asset.currentAssigneeName ? (
                        <div>
                          <strong>{asset.currentAssigneeName}</strong>
                          <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>Since {asset.assignedDate}</div>
                        </div>
                      ) : (
                        <span style={{ color: 'var(--text-muted)', fontSize: '0.8rem' }}>In Stock</span>
                      )}
                    </td>
                    <td>
                      {asset.status === 'Available' && (
                        <button 
                          type="button" 
                          className="action-btn"
                          onClick={() => {
                            setAssignData(prev => ({ ...prev, assetId: asset.id }));
                            setShowAssignModal(true);
                          }}
                        >
                          Assign
                        </button>
                      )}
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={8} style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-muted)' }}>
                    No assets recorded in inventory. Click <strong>Register New Asset</strong>.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      )}

      {/* TAB 2: ASSIGNMENTS TABLE */}
      {tab === 'assignments' && (
        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>Asset</th>
                <th>Assigned To</th>
                <th>Assigned Date</th>
                <th>Initial Condition</th>
                <th>Returned Date</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {assignments.length > 0 ? (
                assignments.map((asg) => (
                  <tr key={asg.id}>
                    <td>
                      <strong>{asg.assetName}</strong>
                      <div style={{ fontSize: '0.75rem', color: '#38bdf8' }}>{asg.assetCode}</div>
                    </td>
                    <td>
                      <strong>{asg.employeeName}</strong>
                      <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>{asg.employeeCode}</div>
                    </td>
                    <td>{asg.assignedDate}</td>
                    <td><span className="badge-pill badge-neutral">{asg.condition}</span></td>
                    <td>{asg.returnedDate || '—'}</td>
                    <td>
                      <span className={`badge-pill ${asg.isReturned ? 'badge-neutral' : 'badge-primary'}`}>
                        {asg.isReturned ? 'Returned' : 'Currently In Use'}
                      </span>
                    </td>
                    <td>
                      {!asg.isReturned && (
                        <button 
                          type="button" 
                          className="action-btn"
                          onClick={() => {
                            setReturnData(prev => ({ ...prev, assignmentId: asg.id }));
                            setShowReturnModal(true);
                          }}
                        >
                          Process Return
                        </button>
                      )}
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={7} style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-muted)' }}>
                    No asset assignments recorded.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      )}

      {/* TAB 3: CATEGORIES */}
      {tab === 'categories' && (
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))', gap: '1rem' }}>
          {categories.length > 0 ? (
            categories.map((cat) => (
              <div key={cat.id} className="glass-card-item">
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <h4 style={{ margin: 0, fontSize: '1rem', fontWeight: 700 }}>{cat.name}</h4>
                  <span className="badge-pill badge-primary">{cat.code}</span>
                </div>
                <div style={{ marginTop: '0.75rem', fontSize: '0.825rem', color: 'var(--text-muted)' }}>
                  Total Items in Category: <strong>{cat.assetsCount}</strong>
                </div>
              </div>
            ))
          ) : (
            <div style={{ gridColumn: '1 / -1', textAlign: 'center', padding: '3rem', color: 'var(--text-muted)' }}>
              No asset categories defined.
            </div>
          )}
        </div>
      )}

      {/* MODAL: REGISTER ASSET */}
      {showAssetModal && (
        <div className="modal-backdrop">
          <div className="modal-content">
            <h3 style={{ margin: '0 0 1rem', fontSize: '1.2rem', fontWeight: 700 }}>Register Company Asset</h3>
            <form onSubmit={handleCreateAsset} style={{ display: 'flex', flexDirection: 'column', gap: '0.85rem' }}>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 2fr', gap: '0.75rem' }}>
                <div className="form-group">
                  <label className="form-label">Asset Code</label>
                  <input 
                    type="text" 
                    className="input-field" 
                    required 
                    value={newAsset.assetCode}
                    onChange={e => setNewAsset({ ...newAsset, assetCode: e.target.value })}
                    placeholder="e.g. AST-LAP-05"
                  />
                </div>
                <div className="form-group">
                  <label className="form-label">Asset Name</label>
                  <input 
                    type="text" 
                    className="input-field" 
                    required 
                    value={newAsset.name}
                    onChange={e => setNewAsset({ ...newAsset, name: e.target.value })}
                    placeholder="e.g. MacBook Pro 16 M3 Max"
                  />
                </div>
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0.75rem' }}>
                <div className="form-group">
                  <label className="form-label">Category</label>
                  <select 
                    className="input-field" 
                    value={newAsset.categoryId}
                    onChange={e => setNewAsset({ ...newAsset, categoryId: parseInt(e.target.value) })}
                  >
                    {categories.map(c => (
                      <option key={c.id} value={c.id}>{c.name}</option>
                    ))}
                  </select>
                </div>
                <div className="form-group">
                  <label className="form-label">Serial Number</label>
                  <input 
                    type="text" 
                    className="input-field" 
                    value={newAsset.serialNumber}
                    onChange={e => setNewAsset({ ...newAsset, serialNumber: e.target.value })}
                    placeholder="Serial or IMEI"
                  />
                </div>
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: '0.75rem' }}>
                <div className="form-group">
                  <label className="form-label">Cost ($)</label>
                  <input 
                    type="number" 
                    className="input-field" 
                    value={newAsset.purchaseCost}
                    onChange={e => setNewAsset({ ...newAsset, purchaseCost: parseFloat(e.target.value) || 0 })}
                  />
                </div>
                <div className="form-group">
                  <label className="form-label">Purchase Date</label>
                  <input 
                    type="date" 
                    className="input-field" 
                    value={newAsset.purchaseDate}
                    onChange={e => setNewAsset({ ...newAsset, purchaseDate: e.target.value })}
                  />
                </div>
                <div className="form-group">
                  <label className="form-label">Condition</label>
                  <select 
                    className="input-field" 
                    value={newAsset.condition}
                    onChange={e => setNewAsset({ ...newAsset, condition: e.target.value })}
                  >
                    <option value="New">Brand New</option>
                    <option value="Good">Good</option>
                    <option value="Fair">Fair</option>
                  </select>
                </div>
              </div>

              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '0.5rem', marginTop: '1rem' }}>
                <button type="button" className="btn-secondary" onClick={() => setShowAssetModal(false)}>Cancel</button>
                <button type="submit" className="btn-primary">Save Asset</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* MODAL: ASSIGN ASSET */}
      {showAssignModal && (
        <div className="modal-backdrop">
          <div className="modal-content">
            <h3 style={{ margin: '0 0 1rem', fontSize: '1.2rem', fontWeight: 700 }}>Assign Asset to Employee</h3>
            <form onSubmit={handleAssignAsset} style={{ display: 'flex', flexDirection: 'column', gap: '0.85rem' }}>
              <div className="form-group">
                <label className="form-label">Select Asset</label>
                <select 
                  className="input-field" 
                  required
                  value={assignData.assetId}
                  onChange={e => setAssignData({ ...assignData, assetId: parseInt(e.target.value) })}
                >
                  <option value="">-- Choose Asset --</option>
                  {assets.filter(a => a.status === 'Available').map(a => (
                    <option key={a.id} value={a.id}>{a.name} ({a.assetCode})</option>
                  ))}
                </select>
              </div>

              <div className="form-group">
                <label className="form-label">Assignee Employee</label>
                <select 
                  className="input-field" 
                  required
                  value={assignData.employeeId}
                  onChange={e => setAssignData({ ...assignData, employeeId: parseInt(e.target.value) })}
                >
                  <option value="">-- Choose Employee --</option>
                  {employees.map(e => (
                    <option key={e.id} value={e.id}>{e.fullName || `${e.firstName} ${e.lastName}`} ({e.employeeCode})</option>
                  ))}
                </select>
              </div>

              <div className="form-group">
                <label className="form-label">Assignment Handover Notes</label>
                <textarea 
                  className="input-field" 
                  rows={2}
                  value={assignData.notes}
                  onChange={e => setAssignData({ ...assignData, notes: e.target.value })}
                  placeholder="Included accessories (charger, bag, mouse)..."
                />
              </div>

              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '0.5rem', marginTop: '1rem' }}>
                <button type="button" className="btn-secondary" onClick={() => setShowAssignModal(false)}>Cancel</button>
                <button type="submit" className="btn-primary">Assign Asset</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* MODAL: RETURN ASSET */}
      {showReturnModal && (
        <div className="modal-backdrop">
          <div className="modal-content">
            <h3 style={{ margin: '0 0 1rem', fontSize: '1.2rem', fontWeight: 700 }}>Process Asset Return</h3>
            <form onSubmit={handleReturnAsset} style={{ display: 'flex', flexDirection: 'column', gap: '0.85rem' }}>
              <div className="form-group">
                <label className="form-label">Return Condition</label>
                <select 
                  className="input-field" 
                  value={returnData.returnCondition}
                  onChange={e => setReturnData({ ...returnData, returnCondition: e.target.value })}
                >
                  <option value="Good">Good / Working</option>
                  <option value="Fair">Fair / Minor Scratches</option>
                  <option value="Damaged">Damaged / Needs Repair</option>
                </select>
              </div>

              <div className="form-group">
                <label className="form-label">Return Inspection Notes</label>
                <textarea 
                  className="input-field" 
                  rows={3}
                  value={returnData.notes}
                  onChange={e => setReturnData({ ...returnData, notes: e.target.value })}
                  placeholder="Device status, hardware check, reset confirmation..."
                />
              </div>

              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '0.5rem', marginTop: '1rem' }}>
                <button type="button" className="btn-secondary" onClick={() => setShowReturnModal(false)}>Cancel</button>
                <button type="submit" className="btn-primary">Confirm Return to Inventory</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* MODAL: ADD CATEGORY */}
      {showCategoryModal && (
        <div className="modal-backdrop">
          <div className="modal-content">
            <h3 style={{ margin: '0 0 1rem', fontSize: '1.2rem', fontWeight: 700 }}>Add Asset Category</h3>
            <form onSubmit={handleCreateCategory} style={{ display: 'flex', flexDirection: 'column', gap: '0.85rem' }}>
              <div className="form-group">
                <label className="form-label">Category Name</label>
                <input 
                  type="text" 
                  className="input-field" 
                  required 
                  value={newCategory.name}
                  onChange={e => setNewCategory({ ...newCategory, name: e.target.value })}
                  placeholder="e.g. Ergonomic Furniture"
                />
              </div>
              <div className="form-group">
                <label className="form-label">Category Code</label>
                <input 
                  type="text" 
                  className="input-field" 
                  required 
                  value={newCategory.code}
                  onChange={e => setNewCategory({ ...newCategory, code: e.target.value.toUpperCase() })}
                  placeholder="e.g. FURNITURE"
                />
              </div>
              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '0.5rem', marginTop: '1rem' }}>
                <button type="button" className="btn-secondary" onClick={() => setShowCategoryModal(false)}>Cancel</button>
                <button type="submit" className="btn-primary">Save Category</button>
              </div>
            </form>
          </div>
        </div>
      )}

    </div>
  );
}
