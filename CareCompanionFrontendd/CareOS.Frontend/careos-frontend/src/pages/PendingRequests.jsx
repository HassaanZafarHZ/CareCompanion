import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Container, Paper, Typography, Box, Button, Card, CardContent, Avatar, Chip, CircularProgress, Alert, IconButton, Fade } from '@mui/material';
import { ArrowBack, Person, Phone, Email, CheckCircle, Cancel, HourglassEmpty } from '@mui/icons-material';
import { assignmentService } from '../services/assignmentService';

const PendingRequests = () => {
  const navigate = useNavigate();
  const [requests, setRequests] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [processing, setProcessing] = useState(null);

  useEffect(() => { fetchRequests(); }, []);

  const fetchRequests = async () => {
    try {
      setLoading(true);
      const res = await assignmentService.getPendingRequests();
      if (res.data.success) setRequests(res.data.data || []);
    } catch (err) { setError('Failed to load requests'); }
    finally { setLoading(false); }
  };

  const handleApprove = async (id) => {
    setProcessing(id);
    try {
      const res = await assignmentService.approveRequest(id);
      if (res.data.success) fetchRequests();
      else setError(res.data.message);
    } catch (err) { setError(err.response?.data?.message || 'Failed'); }
    finally { setProcessing(null); }
  };

  const handleReject = async (id) => {
    setProcessing(id);
    try {
      const res = await assignmentService.rejectRequest(id);
      if (res.data.success) fetchRequests();
      else setError(res.data.message);
    } catch (err) { setError(err.response?.data?.message || 'Failed'); }
    finally { setProcessing(null); }
  };

  return (
    <Box sx={{ minHeight: '100vh', background: 'linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%)' }}>
      <Paper elevation={0} sx={{ background: 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)', borderRadius: 0, py: 2, px: 3 }}>
        <Container maxWidth="lg">
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <IconButton onClick={() => navigate('/dashboard')} sx={{ color: 'white' }}><ArrowBack /></IconButton>
            <Avatar sx={{ bgcolor: 'rgba(255,255,255,0.2)' }}><HourglassEmpty /></Avatar>
            <Typography variant="h5" sx={{ color: 'white', fontWeight: 600 }}>Pending Requests ({requests.length})</Typography>
          </Box>
        </Container>
      </Paper>

      <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
        {error && <Alert severity="error" sx={{ mb: 3 }}>{error}</Alert>}
        
        <Paper sx={{ p: 3, mb: 3, borderRadius: 4 }}>
          <Typography variant="h6" sx={{ fontWeight: 600 }}>👴 Elder Requests</Typography>
          <Typography variant="body2" color="text.secondary">Review elder details and accept/reject their requests. You can manage up to 3 elders.</Typography>
        </Paper>

        {loading ? <Box sx={{ textAlign: 'center', py: 8 }}><CircularProgress /></Box> : 
         requests.length === 0 ? <Paper sx={{ p: 6, textAlign: 'center', borderRadius: 4 }}><HourglassEmpty sx={{ fontSize: 60, color: '#ccc' }} /><Typography variant="h6" color="text.secondary">No pending requests</Typography></Paper> :
          <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 3 }}>
            {requests.map((r, i) => (
              <Fade in timeout={600 + i * 100} key={r.id}>
                <Card sx={{ width: { xs: '100%', md: 'calc(50% - 12px)' }, borderRadius: 4 }}>
                  <CardContent sx={{ p: 3 }}>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
                      <Avatar sx={{ width: 60, height: 60, background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)' }}>{r.elderName?.charAt(0)}</Avatar>
                      <Box>
                        <Typography variant="h6" sx={{ fontWeight: 600 }}>{r.elderName}</Typography>
                        <Chip icon={<HourglassEmpty />} label="Pending" color="warning" size="small" />
                      </Box>
                    </Box>
                    <Box sx={{ bgcolor: '#f5f5f5', p: 2, borderRadius: 2, mb: 2 }}>
                      <Typography variant="subtitle2" color="primary">Elder Details:</Typography>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mt: 1 }}><Email fontSize="small" /><Typography variant="body2">{r.elderEmail || 'N/A'}</Typography></Box>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mt: 0.5 }}><Phone fontSize="small" /><Typography variant="body2">{r.elderPhone || 'N/A'}</Typography></Box>
                      <Typography variant="caption" color="text.secondary" sx={{ mt: 1, display: 'block' }}>Requested: {new Date(r.assignedAt).toLocaleDateString()}</Typography>
                    </Box>
                    <Box sx={{ display: 'flex', gap: 2 }}>
                      <Button fullWidth variant="contained" color="success" startIcon={processing === r.id ? <CircularProgress size={20} sx={{color:'white'}}/> : <CheckCircle />} disabled={processing === r.id} onClick={() => handleApprove(r.id)} sx={{ borderRadius: 2 }}>Accept</Button>
                      <Button fullWidth variant="outlined" color="error" startIcon={<Cancel />} disabled={processing === r.id} onClick={() => handleReject(r.id)} sx={{ borderRadius: 2 }}>Reject</Button>
                    </Box>
                  </CardContent>
                </Card>
              </Fade>
            ))}
          </Box>
        }
      </Container>
    </Box>
  );
};

export default PendingRequests;