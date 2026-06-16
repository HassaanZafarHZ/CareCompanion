import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
 Container,
 Paper,
 Typography,
 Box,
 Button,
 Avatar,
 CircularProgress,
 Alert,
 IconButton,
 FormControl,
 InputLabel,
 Select,
 MenuItem,
 TextField
} from '@mui/material';
import { ArrowBack, Warning, Phone, LocalHospital } from '@mui/icons-material';
import api from '../services/api';

const Emergency = () => {
 const navigate = useNavigate();
 const [loading, setLoading] = useState(false);
 const [sent, setSent] = useState(false);
 const [error, setError] = useState('');
 const [emergencyType, setEmergencyType] = useState('GENERAL');
 const [message, setMessage] = useState('');
 const user = JSON.parse(localStorage.getItem('user'));

 const handleEmergency = async () => {
 setLoading(true);
 setError('');
 try {
 await api.post('/emergency/trigger', {
 elderId: user?.id,
 emergencyType: emergencyType,
 message: message || undefined
 });
 setSent(true);
 } catch (err) {
 setError(err.response?.data?.message || 'Failed to send alert');
 } finally {
 setLoading(false);
 }
 };

 const handleCall = async (number, serviceLabel) => {
 try {
 // notify backend before opening dialer (best-effort)
 await api.post('/emergency/trigger', {
 elderId: user?.id,
 emergencyType: `CALL_${number}`,
 message: `Elder is calling ${serviceLabel} (${number})`
 });
 } catch (err) {
 // ignore notification errors, still open dialer
 console.log('Notification failed', err?.message || err);
 }
 // open phone dialer
 window.location.href = `tel:${number}`;
 };

 return (
 <Box sx={{ minHeight: '100vh', background: 'linear-gradient(135deg, #ff416c0%, #ff4b2b100%)' }}>
 <Paper elevation={0} sx={{ background: 'transparent', py:2, px:3 }}>
 <Container maxWidth="sm">
 <Box sx={{ display: 'flex', alignItems: 'center', gap:2 }}>
 <IconButton onClick={() => navigate('/dashboard')} sx={{ color: 'white' }}>
 <ArrowBack />
 </IconButton>
 <Typography variant="h5" sx={{ color: 'white', fontWeight:600 }}>?? Emergency</Typography>
 </Box>
 </Container>
 </Paper>

 <Container maxWidth="sm" sx={{ mt:4 }}>
 {error && <Alert severity="error" sx={{ mb:3 }}>{error}</Alert>}

 {sent ? (
 <Paper sx={{ p:4, borderRadius:4, textAlign: 'center' }}>
 <Avatar sx={{ width:80, height:80, margin: '0 auto16px', bgcolor: '#4caf50' }}><Warning sx={{ fontSize:40 }} /></Avatar>
 <Typography variant="h5" sx={{ fontWeight:600, color: '#4caf50' }}>Alert Sent! ?</Typography>
 <Typography color="text.secondary" sx={{ mt:1 }}>Your caretaker has been notified.</Typography>
 <Button variant="contained" onClick={() => navigate('/dashboard')} sx={{ mt:3 }}>Back to Dashboard</Button>
 </Paper>
 ) : (
 <Paper sx={{ p:4, borderRadius:4, textAlign: 'center' }}>
 <Avatar sx={{ width:100, height:100, margin: '0 auto16px', bgcolor: '#ff4b2b' }}><Warning sx={{ fontSize:50 }} /></Avatar>
 <Typography variant="h4" sx={{ fontWeight:700 }}>Need Help?</Typography>
 <Typography color="text.secondary" sx={{ mt:1, mb:3 }}>Select a type (optional), add a short note (optional), then send alert to your caretaker.</Typography>

 <FormControl fullWidth sx={{ mb:2 }}>
 <InputLabel>Emergency Type (optional)</InputLabel>
 <Select
 value={emergencyType}
 label="Emergency Type (optional)"
 onChange={(e) => setEmergencyType(e.target.value)}
 >
 <MenuItem value="GENERAL">General</MenuItem>
 <MenuItem value="FALL">Fall</MenuItem>
 <MenuItem value="FALL_SERIOUS">Serious Fall</MenuItem>
 <MenuItem value="NOT_FEELING_WELL">Not feeling well</MenuItem>
 <MenuItem value="MEDICAL">Medical emergency</MenuItem>
 <MenuItem value="OTHER">Other</MenuItem>
 </Select>
 </FormControl>

 <TextField
 fullWidth
 multiline
 rows={3}
 placeholder="Optional note for caretaker (e.g., chest pain, injured leg, severe dizziness)"
 value={message}
 onChange={(e) => setMessage(e.target.value)}
 sx={{ mb:2 }}
 />

 <Button fullWidth variant="contained" size="large" onClick={handleEmergency} disabled={loading} sx={{ py:2, fontSize: '1.1rem', bgcolor: '#ff4b2b', '&:hover': { bgcolor: '#e53935' }, borderRadius:2 }}>
 {loading ? <CircularProgress size={24} sx={{ color: 'white' }} /> : '?? SEND EMERGENCY ALERT'}
 </Button>

 <Box sx={{ mt:3, display: 'flex', gap:2 }}>
 <Button fullWidth variant="outlined" startIcon={<Phone />} onClick={() => handleCall('15', 'Emergency15')} sx={{ py:1 }}>Call15</Button>
 <Button fullWidth variant="outlined" startIcon={<LocalHospital />} onClick={() => handleCall('1122', 'Rescue1122')} sx={{ py:1 }}>Call1122</Button>
 </Box>
 </Paper>
 )}
 </Container>
 </Box>
 );
};

export default Emergency;
