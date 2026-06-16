import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Container, Paper, Typography, Box, Button, Card, CardContent, Avatar, Chip, CircularProgress, Alert, IconButton, Fade } from '@mui/material';
import { ArrowBack, Person, Phone, Email, Send, CheckCircle, HourglassEmpty, Cancel } from '@mui/icons-material';
import { assignmentService } from '../services/assignmentService';

const SelectCaretaker = () => {
  const navigate = useNavigate();
  const [caretakers, setCaretakers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [sendingTo, setSendingTo] = useState(null);
  const [sentRequests, setSentRequests] = useState([]);
  const user = JSON.parse(localStorage.getItem('user'));

  useEffect(() => { fetchData(); }, []);

  const fetchData = async () => {
    try {
      setLoading(true);
      const [res1, res2] = await Promise.all([
        assignmentService.getAvailableCaretakers(),
        assignmentService.getMySentRequests()
      ]);
      if (res1.data.success) setCaretakers(res1.data.data || []);
      if (res2.data.success) setSentRequests(res2.data.data || []);
    } catch (err) { setError('Failed to load'); }
    finally { setLoading(false); }
  };

  const getRequestStatus = (caretakerId) => {
    const req = sentRequests.find(r => r.caretakerId === caretakerId);
    return req ? req.status : null;
  };

  const hasApprovedCaretaker = sentRequests.some(r => r.status === 'APPROVED');

  const handleSendRequest = async (caretakerId) => {
    setSendingTo(caretakerId);
    setError('');
    try {
      const res = await assignmentService.sendRequest({ elderId: user?.id, caretakerId });
      if (res.data.success) fetchData();
      else setError(res.data.message);
    } catch (err) { setError(err.response?.data?.message || 'Failed'); }
    finally { setSendingTo(null); }
  };

  const getButtonContent = (caretaker) => {
    const status = getRequestStatus(caretaker.id);
    if (status === 'APPROVED') return <Chip icon={<CheckCircle />} label="Your Caretaker ✓" color="success" />;
    if (status === 'PENDING') return <Chip icon={<HourglassEmpty />} label="Request Sent - Waiting" color="warning" />;
    if (status === 'REJECTED') return <Chip icon={<Cancel />} label="Rejected" color="error" />;
    if (hasApprovedCaretaker) return <Button fullWidth disabled>You have a caretaker</Button>;
    return (
      <Button fullWidth variant="contained" disabled={!caretaker.isAvailable || sendingTo === caretaker.id}
        startIcon={sendingTo === caretaker.id ? <CircularProgress size={20} sx={{color:'white'}}/> : <Send />}
        onClick={() => handleSendRequest(caretaker.id)}
        sx={{ borderRadius: 2, background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)' }}>
        {caretaker.isAvailable ? 'Send Request' : 'Not Available'}
      </Button>
    );
  };

  return (
    <Box sx={{ minHeight: '100vh', background: 'linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%)' }}>
      <Paper elevation={0} sx={{ background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)', borderRadius: 0, py: 2, px: 3 }}>
        <Container maxWidth="lg">
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <IconButton onClick={() => navigate('/dashboard')} sx={{ color: 'white' }}><ArrowBack /></IconButton>
            <Avatar sx={{ bgcolor: 'rgba(255,255,255,0.2)' }}><Person /></Avatar>
            <Typography variant="h5" sx={{ color: 'white', fontWeight: 600 }}>Select Caretaker</Typography>
          </Box>
        </Container>
      </Paper>

      <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
        {hasApprovedCaretaker && <Alert severity="success" sx={{ mb: 3 }}>🎉 You have an assigned caretaker!</Alert>}
        {error && <Alert severity="error" sx={{ mb: 3 }}>{error}</Alert>}
        
        <Paper sx={{ p: 3, mb: 3, borderRadius: 4 }}>
          <Typography variant="h6" sx={{ fontWeight: 600 }}>👨‍⚕️ Send Request to Caretakers</Typography>
          <Typography variant="body2" color="text.secondary">Send requests to multiple caretakers. First one to accept becomes your caretaker!</Typography>
        </Paper>

        {loading ? <Box sx={{ textAlign: 'center', py: 8 }}><CircularProgress /></Box> : 
         caretakers.length === 0 ? <Paper sx={{ p: 6, textAlign: 'center' }}><Person sx={{ fontSize: 60, color: '#ccc' }} /><Typography>No caretakers available</Typography></Paper> :
          <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 3 }}>
            {caretakers.map((c, i) => (
              <Fade in timeout={600 + i * 100} key={c.id}>
                <Card sx={{ width: { xs: '100%', sm: 'calc(50% - 12px)', md: 'calc(33.333% - 16px)' }, borderRadius: 4, opacity: c.isAvailable ? 1 : 0.6 }}>
                  <CardContent sx={{ p: 3, textAlign: 'center' }}>
                    <Avatar sx={{ width: 70, height: 70, margin: '0 auto 12px', background: c.isAvailable ? 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)' : '#ccc' }}>{c.fullName?.charAt(0)}</Avatar>
                    <Typography variant="h6" sx={{ fontWeight: 600 }}>{c.fullName}</Typography>
                    <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 1, mt: 1 }}><Email fontSize="small" /><Typography variant="body2">{c.email}</Typography></Box>
                    {c.phoneNumber && <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 1 }}><Phone fontSize="small" /><Typography variant="body2">{c.phoneNumber}</Typography></Box>}
                    <Chip label={`${c.assignedEldersCount}/3 Elders`} size="small" color={c.isAvailable ? 'success' : 'default'} sx={{ mt: 2 }} />
                    <Box sx={{ mt: 2 }}>{getButtonContent(c)}</Box>
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

export default SelectCaretaker;