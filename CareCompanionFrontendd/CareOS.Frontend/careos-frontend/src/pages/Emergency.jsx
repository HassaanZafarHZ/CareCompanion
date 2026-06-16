import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Container, Paper, Typography, Box, Button, Avatar, CircularProgress, Alert, IconButton } from '@mui/material';
import { ArrowBack, Warning, Phone, LocalHospital } from '@mui/icons-material';
import api from '../services/api';

const Emergency = () => {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [sent, setSent] = useState(false);
  const [error, setError] = useState('');
  const user = JSON.parse(localStorage.getItem('user'));

  const handleEmergency = async () => {
    setLoading(true);
    try {
      await api.post('/emergency/trigger', { 
        elderId: user?.id,
        message: 'Emergency! Need help immediately!',
        emergencyType: 'GENERAL'
      });
      setSent(true);
    } catch (err) { setError(err.response?.data?.message || 'Failed to send alert'); }
    finally { setLoading(false); }
  };

  const handleCall = async (number, service) => {
    try {
      await api.post('/emergency/trigger', { 
        elderId: user?.id,
        message: `Elder is calling ${service} (${number})`,
        emergencyType: 'CALL_SERVICE'
      });
    } catch (err) { console.log('Notification failed'); }
    window.location.href = `tel:${number}`;
  };

  return (
    <Box sx={{ minHeight: '100vh', background: 'linear-gradient(135deg, #ff416c 0%, #ff4b2b 100%)' }}>
      <Paper elevation={0} sx={{ background: 'transparent', py: 2, px: 3 }}>
        <Container maxWidth="sm">
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <IconButton onClick={() => navigate('/dashboard')} sx={{ color: 'white' }}><ArrowBack /></IconButton>
            <Typography variant="h5" sx={{ color: 'white', fontWeight: 600 }}>🚨 Emergency</Typography>
          </Box>
        </Container>
      </Paper>

      <Container maxWidth="sm" sx={{ mt: 4 }}>
        {error && <Alert severity="error" sx={{ mb: 3 }}>{error}</Alert>}
        
        {sent ? (
          <Paper sx={{ p: 4, borderRadius: 4, textAlign: 'center' }}>
            <Avatar sx={{ width: 80, height: 80, margin: '0 auto 16px', bgcolor: '#4caf50' }}><Warning sx={{ fontSize: 40 }} /></Avatar>
            <Typography variant="h5" sx={{ fontWeight: 600, color: '#4caf50' }}>Alert Sent! ✅</Typography>
            <Typography color="text.secondary" sx={{ mt: 1 }}>Your caretaker has been notified.</Typography>
            <Button variant="contained" onClick={() => navigate('/dashboard')} sx={{ mt: 3 }}>Back to Dashboard</Button>
          </Paper>
        ) : (
          <Paper sx={{ p: 4, borderRadius: 4, textAlign: 'center' }}>
            <Avatar sx={{ width: 100, height: 100, margin: '0 auto 16px', bgcolor: '#ff4b2b' }}><Warning sx={{ fontSize: 50 }} /></Avatar>
            <Typography variant="h4" sx={{ fontWeight: 700 }}>Need Help?</Typography>
            <Typography color="text.secondary" sx={{ mt: 1, mb: 3 }}>Press the button to alert your caretaker immediately</Typography>
            
            <Button fullWidth variant="contained" size="large" onClick={handleEmergency} disabled={loading}
              sx={{ py: 3, fontSize: '1.5rem', bgcolor: '#ff4b2b', '&:hover': { bgcolor: '#e53935' }, borderRadius: 4 }}>
              {loading ? <CircularProgress size={30} sx={{ color: 'white' }} /> : '🚨 SEND EMERGENCY ALERT'}
            </Button>

            <Box sx={{ mt: 4, display: 'flex', gap: 2 }}>
              <Button fullWidth variant="outlined" startIcon={<Phone />} onClick={() => handleCall('15', 'Emergency 15')} sx={{ py: 2, color: 'white', borderColor: 'white' }}>Call 15</Button>
              <Button fullWidth variant="outlined" startIcon={<LocalHospital />} onClick={() => handleCall('1122', 'Rescue 1122')} sx={{ py: 2, color: 'white', borderColor: 'white' }}>Call 1122</Button>
            </Box>
          </Paper>
        )}
      </Container>
    </Box>
  );
};

export default Emergency;