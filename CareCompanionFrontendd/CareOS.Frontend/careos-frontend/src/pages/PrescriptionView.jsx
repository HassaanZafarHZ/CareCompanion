import React, { useState, useEffect, useCallback } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { Container, Paper, Typography, Box, Button, Avatar, Fade, CircularProgress, Alert, Chip, Card, CardContent, IconButton, Dialog, DialogTitle, DialogContent, DialogActions, TextField } from '@mui/material';
import { ArrowBack, LocalHospital, CheckCircle, Cancel, Pending, Person, CalendarMonth, Medication, AccessTime, Add, Edit, Schedule, Delete } from '@mui/icons-material';
import { prescriptionService } from '../services/prescriptionService';

const PrescriptionView = () => {
  const navigate = useNavigate();
  const { id } = useParams();
  const [prescription, setPrescription] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [reviewDialog, setReviewDialog] = useState(false);
  const [reviewStatus, setReviewStatus] = useState('');
  const [reviewNotes, setReviewNotes] = useState('');
  const [reviewLoading, setReviewLoading] = useState(false);
  const [addMedDialog, setAddMedDialog] = useState(false);
  const [newMed, setNewMed] = useState({ medicineName: '', dosage: '', frequency: '', duration: '', time1: '08:00', time2: '20:00' });
  const [addMedLoading, setAddMedLoading] = useState(false);
  const [deleteLoading, setDeleteLoading] = useState(false);
  const user = JSON.parse(localStorage.getItem('user'));
    
  const fetchPrescription = useCallback(async () => {
    try {
      setLoading(true);
      const response = await prescriptionService.getById(id);
      if (response.data.success) setPrescription(response.data.data);
      else setError('Prescription not found');
    } catch (err) { setError('Failed to load prescription'); }
    finally { setLoading(false); }
  }, [id]);

  useEffect(() => { fetchPrescription(); }, [fetchPrescription]);

  const handleReview = async () => {
    setReviewLoading(true);
    try {
      const response = await prescriptionService.review(id, { status: reviewStatus, notes: reviewNotes });
      if (response.data.success) { setReviewDialog(false); fetchPrescription(); }
      else setError(response.data.message);
    } catch (err) { setError('Failed to review'); }
    finally { setReviewLoading(false); }
  };

  const handleAddMedicine = async () => {
    setAddMedLoading(true);
    try {
      const response = await prescriptionService.addMedicine(id, {
        medicineName: newMed.medicineName, dosage: newMed.dosage, frequency: newMed.frequency, duration: newMed.duration
      });
      if (response.data.success) { setAddMedDialog(false); setNewMed({ medicineName: '', dosage: '', frequency: '', duration: '', time1: '08:00', time2: '20:00' }); fetchPrescription(); }
      else setError(response.data.message);
    } catch (err) { setError('Failed to add medicine'); }
    finally { setAddMedLoading(false); }
  };

  const handleDelete = async () => {
    if (window.confirm('Are you sure you want to delete this prescription?')) {
      setDeleteLoading(true);
      try {
        const response = await prescriptionService.deletePrescription(id);
        if (response.data.success) {
          navigate('/prescriptions');
        } else {
          setError(response.data.message);
        }
      } catch (err) {
        setError(err.response?.data?.message || 'Failed to delete prescription');
      } finally {
        setDeleteLoading(false);
      }
    }
  };

  const getStatusChip = (status) => {
    const config = { PENDING: { color: 'warning', icon: <Pending />, label: 'Pending Approval' }, APPROVED: { color: 'success', icon: <CheckCircle />, label: 'Approved' }, REJECTED: { color: 'error', icon: <Cancel />, label: 'Rejected' }, MODIFIED: { color: 'info', icon: <Edit />, label: 'Modified' } };
    const c = config[status] || config.PENDING;
    return <Chip icon={c.icon} label={c.label} color={c.color} />;
  };

  if (loading) return <Box sx={{ minHeight: '100vh', display: 'flex', alignItems: 'center', justifyContent: 'center' }}><CircularProgress size={50} /></Box>;
  if (error && !prescription) return <Container maxWidth="sm" sx={{ mt: 8 }}><Alert severity="error">{error}</Alert><Button onClick={() => navigate('/prescriptions')} sx={{ mt: 2 }}>Go Back</Button></Container>;

  return (
    <Box sx={{ minHeight: '100vh', background: 'linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%)' }}>
      <Paper elevation={0} sx={{ background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)', borderRadius: 0, py: 2, px: 3 }}>
        <Container maxWidth="lg">
          <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
              <IconButton onClick={() => navigate('/prescriptions')} sx={{ color: 'white' }}><ArrowBack /></IconButton>
              <Avatar sx={{ bgcolor: 'rgba(255,255,255,0.2)' }}><LocalHospital /></Avatar>
              <Typography variant="h5" sx={{ color: 'white', fontWeight: 600 }}>Prescription Details</Typography>
            </Box>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
              {getStatusChip(prescription?.status)}
              {user?.role === 'ELDER' && prescription?.status !== 'APPROVED' && (
                <Button variant="contained" color="error" size="small" startIcon={deleteLoading ? <CircularProgress size={16} sx={{ color: 'white' }} /> : <Delete />} onClick={handleDelete} disabled={deleteLoading} sx={{ ml: 1 }}>
                  Delete
                </Button>
              )}
            </Box>
          </Box>
        </Container>
      </Paper>

      <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
        
        {user?.role === 'ELDER' && (
          <Alert severity={prescription?.status === 'APPROVED' ? 'success' : prescription?.status === 'REJECTED' ? 'error' : 'info'} sx={{ mb: 3 }}>
            {prescription?.status === 'APPROVED' && '✅ Prescription approved! Follow the medicine schedule below.'}
            {prescription?.status === 'REJECTED' && '❌ Prescription rejected. Contact your caretaker.'}
            {prescription?.status === 'PENDING' && '⏳ Waiting for caretaker approval...'}
            {prescription?.status === 'MODIFIED' && '✏️ Caretaker has updated medicines. Check below.'}
          </Alert>
        )}
        
        <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 3 }}>
          <Box sx={{ width: { xs: '100%', md: '40%' } }}>
            <Fade in><Paper sx={{ p: 3, borderRadius: 4 }}>
              <Typography variant="h6" sx={{ mb: 2, fontWeight: 600 }}>📄 Prescription Image</Typography>
              {prescription?.base64Image ? <img src={prescription.base64Image} alt="Prescription" style={{ width: '100%', borderRadius: 8 }} /> : <Box sx={{ p: 4, textAlign: 'center', bgcolor: '#f5f5f5', borderRadius: 2 }}><Typography>No image</Typography></Box>}
            </Paper></Fade>
          </Box>

          <Box sx={{ width: { xs: '100%', md: '58%' } }}>
            <Fade in><Paper sx={{ p: 3, borderRadius: 4, mb: 3 }}>
              <Typography variant="h6" sx={{ mb: 2, fontWeight: 600 }}>📋 Information</Typography>
              <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 2 }}>
                <Box sx={{ width: '45%', display: 'flex', alignItems: 'center', gap: 1 }}><Person color="primary" /><Box><Typography variant="caption">Doctor</Typography><Typography sx={{ fontWeight: 600 }}>{prescription?.analysis?.doctorName || 'N/A'}</Typography></Box></Box>
                <Box sx={{ width: '45%', display: 'flex', alignItems: 'center', gap: 1 }}><Person color="secondary" /><Box><Typography variant="caption">Patient</Typography><Typography sx={{ fontWeight: 600 }}>{prescription?.elderName || 'N/A'}</Typography></Box></Box>
                <Box sx={{ width: '45%', display: 'flex', alignItems: 'center', gap: 1 }}><CalendarMonth /><Box><Typography variant="caption">Uploaded</Typography><Typography>{prescription?.uploadedAt ? new Date(prescription.uploadedAt).toLocaleDateString() : 'N/A'}</Typography></Box></Box>
                <Box sx={{ width: '45%', display: 'flex', alignItems: 'center', gap: 1 }}><AccessTime /><Box><Typography variant="caption">Status</Typography><Typography sx={{ fontWeight: 600 }}>{prescription?.status}</Typography></Box></Box>
              </Box>
              {prescription?.notes && <Box sx={{ mt: 2, p: 2, bgcolor: '#f0f7ff', borderRadius: 2 }}><Typography variant="caption" color="primary">Caretaker Notes:</Typography><Typography variant="body2">{prescription.notes}</Typography></Box>}
            </Paper></Fade>

            <Fade in><Paper sx={{ p: 3, borderRadius: 4 }}>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 2 }}>
                <Typography variant="h6" sx={{ fontWeight: 600 }}>💊 Medicines ({prescription?.analysis?.medicines?.length || 0})</Typography>
                {user?.role === 'CARETAKER' && prescription?.status !== 'REJECTED' && <Button size="small" startIcon={<Add />} variant="contained" onClick={() => setAddMedDialog(true)} sx={{ background: 'linear-gradient(135deg, #11998e 0%, #38ef7d 100%)' }}>Add Medicine</Button>}
              </Box>
              
              {prescription?.analysis?.medicines?.map((med, i) => (
                <Card key={i} sx={{ mb: 2, bgcolor: '#fafafa' }}>
                  <CardContent>
                    <Box sx={{ display: 'flex', gap: 2 }}>
                      <Avatar sx={{ bgcolor: '#667eea' }}><Medication /></Avatar>
                      <Box sx={{ flex: 1 }}>
                        <Typography variant="h6" sx={{ fontWeight: 600 }}>{med.medicineName}</Typography>
                        <Box sx={{ display: 'flex', gap: 1, mt: 1, flexWrap: 'wrap' }}>
                          <Chip label={med.dosage || 'N/A'} size="small" variant="outlined" />
                          <Chip label={med.frequency || 'N/A'} size="small" variant="outlined" />
                          <Chip label={med.duration || 'N/A'} size="small" variant="outlined" />
                        </Box>
                        {med.suggestedTimes?.length > 0 && (
                          <Box sx={{ mt: 2, p: 2, bgcolor: '#e8f5e9', borderRadius: 2 }}>
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}><Schedule color="success" /><Typography variant="subtitle2" color="success.main">Take At:</Typography></Box>
                            <Box sx={{ display: 'flex', gap: 1, mt: 1 }}>{med.suggestedTimes.map((t, j) => <Chip key={j} label={t} color="success" />)}</Box>
                          </Box>
                        )}
                        {med.warnings?.length > 0 && <Box sx={{ mt: 1 }}>{med.warnings.map((w, j) => <Typography key={j} variant="caption" color="warning.main" display="block">{w}</Typography>)}</Box>}
                      </Box>
                    </Box>
                  </CardContent>
                </Card>
              ))}
            </Paper></Fade>

            {user?.role === 'CARETAKER' && prescription?.status === 'PENDING' && (
              <Fade in><Paper sx={{ p: 3, borderRadius: 4, mt: 3 }}>
                <Typography variant="h6" sx={{ mb: 2, fontWeight: 600 }}>✅ Review</Typography>
                <Box sx={{ display: 'flex', gap: 2 }}>
                  <Button variant="contained" color="success" fullWidth startIcon={<CheckCircle />} onClick={() => { setReviewStatus('APPROVED'); setReviewDialog(true); }}>Approve</Button>
                  <Button variant="contained" color="error" fullWidth startIcon={<Cancel />} onClick={() => { setReviewStatus('REJECTED'); setReviewDialog(true); }}>Reject</Button>
                </Box>
              </Paper></Fade>
            )}
          </Box>
        </Box>
      </Container>

      <Dialog open={reviewDialog} onClose={() => setReviewDialog(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{reviewStatus === 'APPROVED' ? '✅ Approve' : '❌ Reject'}</DialogTitle>
        <DialogContent><TextField fullWidth multiline rows={3} label="Notes" value={reviewNotes} onChange={(e) => setReviewNotes(e.target.value)} sx={{ mt: 2 }} /></DialogContent>
        <DialogActions><Button onClick={() => setReviewDialog(false)}>Cancel</Button><Button variant="contained" color={reviewStatus === 'APPROVED' ? 'success' : 'error'} onClick={handleReview} disabled={reviewLoading}>{reviewLoading ? <CircularProgress size={24} /> : 'Confirm'}</Button></DialogActions>
      </Dialog>

      <Dialog open={addMedDialog} onClose={() => setAddMedDialog(false)} maxWidth="sm" fullWidth>
        <DialogTitle>💊 Add Medicine</DialogTitle>
        <DialogContent>
          <TextField fullWidth label="Medicine Name *" value={newMed.medicineName} onChange={(e) => setNewMed({...newMed, medicineName: e.target.value})} sx={{ mt: 2 }} />
          <TextField fullWidth label="Dosage" value={newMed.dosage} onChange={(e) => setNewMed({...newMed, dosage: e.target.value})} sx={{ mt: 2 }} />
          <TextField fullWidth label="Frequency" value={newMed.frequency} onChange={(e) => setNewMed({...newMed, frequency: e.target.value})} sx={{ mt: 2 }} />
          <TextField fullWidth label="Duration" value={newMed.duration} onChange={(e) => setNewMed({...newMed, duration: e.target.value})} sx={{ mt: 2 }} />
          <Box sx={{ display: 'flex', gap: 2, mt: 2 }}>
            <TextField fullWidth label="Time 1" type="time" value={newMed.time1} onChange={(e) => setNewMed({...newMed, time1: e.target.value})} InputLabelProps={{ shrink: true }} />
            <TextField fullWidth label="Time 2" type="time" value={newMed.time2} onChange={(e) => setNewMed({...newMed, time2: e.target.value})} InputLabelProps={{ shrink: true }} />
          </Box>
        </DialogContent>
        <DialogActions><Button onClick={() => setAddMedDialog(false)}>Cancel</Button><Button variant="contained" onClick={handleAddMedicine} disabled={addMedLoading || !newMed.medicineName}>{addMedLoading ? <CircularProgress size={24} /> : 'Add'}</Button></DialogActions>
      </Dialog>
    </Box>
  );
};

export default PrescriptionView;