import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Container, Paper, Typography, Box, Button, Card, Avatar, Chip, IconButton, Fade, Alert, List, ListItem, ListItemText } from '@mui/material';
import { showReminderNotification } from '../utils/reminderNotification';
import { medicationService } from '../services/medicationService';
import { assignmentService } from '../services/assignmentService';
import { LocalHospital, Medication, Chat, Warning, Person, ExitToApp, Dashboard as DashboardIcon, Notifications, Settings, CalendarMonth, Favorite, Assignment, Phone, PersonAdd, HourglassEmpty } from '@mui/icons-material';
import api from '../services/api';
import { checkInService } from '../services/checkInService';
import signalRService from '../services/signalRService';

const Dashboard = () => {

  const navigate = useNavigate();
  const [user, setUser] = useState(null);
  const [emergencyAlerts, setEmergencyAlerts] = useState([]);
  const [pendingRequests, setPendingRequests] = useState(0);
  // Medication section state
  const [medications, setMedications] = useState([]);
  const [takenMap, setTakenMap] = useState({});
  const [missedCount, setMissedCount] = useState(0);
  const [markError, setMarkError] = useState('');
  // Caretaker: elders and selection
  const [elders, setElders] = useState([]);
  const [selectedElder, setSelectedElder] = useState('');
  // Daily check-in state
  const [checkInStatus, setCheckInStatus] = useState(null);
  const [showCheckInCard, setShowCheckInCard] = useState(() => {
    // Only show on login, not after refresh if already checked in
    return sessionStorage.getItem('showCheckInCard') !== 'false';
  });
  const [checkInLoading, setCheckInLoading] = useState(false);
  const [checkInMessage, setCheckInMessage] = useState('');

  // Fetch today's check-in status on mount (for elders)
  useEffect(() => {
    if (user && user.role === 'ELDER') {
      checkInService.getToday().then(res => {
        const status = res.data?.data?.FeelingStatus || null;
        setCheckInStatus(status);
        // If already checked in, hide card for this session
        if (status) {
          setShowCheckInCard(false);
          sessionStorage.setItem('showCheckInCard', 'false');
        }
      }).catch(() => setCheckInStatus(null));
    }
  }, [user]);

  // Handle daily check-in submission
  const handleCheckIn = async (status) => {
    setCheckInLoading(true);
    setCheckInMessage('');
    try {
      // Map status to backend expected values
      const FeelingStatus = status === 'OK' ? 'GOOD' : 'NOT_GOOD';
      const ElderId = user.id || user._id || user.Id;
      await checkInService.submit({ ElderId, FeelingStatus });
      setCheckInStatus(status);
      setCheckInMessage('Check-in submitted successfully!');
      setShowCheckInCard(false);
      sessionStorage.setItem('showCheckInCard', 'false');
    } catch {
      setCheckInMessage('Failed to submit check-in.');
    } finally {
      setCheckInLoading(false);
    }
  };

  // Fetch medications for elder or caretaker's selected elder
  const fetchMedications = async (elderId) => {
    try {
      let res;
      if (user && user.role === 'CARETAKER' && elderId) {
        res = await medicationService.getElderMedications(elderId);
      } else {
        res = await medicationService.getAll();
      }
      const meds = res.data.data || [];
      setMedications(meds);
      // Show reminders for all medication times (for elders only)
      if (user && user.role === 'ELDER') {
        meds.forEach(med => {
          let scheduleTimes = [];
          if (Array.isArray(med.schedules) && med.schedules.length > 0) {
            scheduleTimes = med.schedules.map(s => s.time || s.Time || s);
          } else if (Array.isArray(med.scheduleTimes)) {
            scheduleTimes = med.scheduleTimes;
          }
          scheduleTimes.forEach(time => {
            showReminderNotification(
              `Medication Reminder: ${med.medicineName || med.name}`,
              `It's time to take your medicine: ${med.medicineName || med.name} (${med.dosage || ''}) at ${time}`
            );
          });
        });
      }
    } catch {}
  };

  // Mark as taken handler
  const handleMarkAsTaken = async (medId, scheduleTime) => {
    setMarkError('');
    try {
      await medicationService.confirmTaken({ medicationId: medId, scheduleTime, takenAt: new Date() });
      setTakenMap(m => ({ ...m, [`${medId}_${scheduleTime}`]: true }));
    } catch {
      setMarkError('Failed to mark as taken.');
    }
  };

  useEffect(() => {
    const userData = localStorage.getItem('user');
    if (!userData) { navigate('/login'); return; }
    const parsed = JSON.parse(userData);
    setUser(parsed);
    if (parsed.role === 'ELDER') {
      // Always request notification permission on page load for elders
      if (window.Notification && Notification.permission !== 'granted' && Notification.permission !== 'denied') {
        Notification.requestPermission();
      }
      fetchMedications();
    } else if (parsed.role === 'CARETAKER') {
      // Fetch assigned elders for caretaker
      assignmentService.getMyElders().then(res => {
        setElders(res.data.data || []);
      });
    }
  }, [navigate]);

  // Calculate missed doses for today (elder or caretaker viewing selected elder)
  useEffect(() => {
    if (!user) return;
    if (user.role === 'ELDER' || (user.role === 'CARETAKER' && selectedElder)) {
      let missed = 0;
      const today = new Date();
      const todayStr = today.toLocaleDateString();
      medications.forEach(med => {
        let schedules = med.schedules || [];
        schedules.forEach(sch => {
          let isToday = true;
          if (sch.day) {
            isToday = sch.day === todayStr;
          }
          let timeStr = sch.time || sch.Time || sch;
          let [h, m, ampm] = timeStr.match(/(\d+):(\d+)\s*(AM|PM)?/i) || [];
          if (h && m) {
            let hour = parseInt(h, 10);
            if (ampm && ampm.toUpperCase() === 'PM' && hour < 12) hour += 12;
            let schedDate = new Date(today);
            schedDate.setHours(hour, parseInt(m, 10), 0, 0);
            if (isToday && schedDate < today && !sch.isTaken && !takenMap[`${med._id || med.id}_${timeStr}`]) {
              missed++;
            }
          }
        });
      });
      setMissedCount(missed);
    } else {
      setMissedCount(0);
    }
  }, [medications, takenMap, user, selectedElder]);
  // Caretaker: fetch medications when elder is selected
  useEffect(() => {
    if (user && user.role === 'CARETAKER' && selectedElder) {
      fetchMedications(selectedElder);
    }
  }, [user, selectedElder]);

  // Fetch helpers
  const fetchEmergencyAlerts = async () => {
    try {
      const res = await api.get('/emergency/pending');
      if (res.data.success) {
        const normalized = (res.data.data || []).map(a => ({
          id: a.id || a._id || a.Id,
          elderName: a.elderName || a.ElderName,
          message: a.message || a.Message,
          emergencyType: a.alertType || a.AlertType || a.emergencyType,
          triggeredAt: a.triggeredAt || a.createdAt || a.CreatedAt
        }));
        setEmergencyAlerts(normalized);
      }
    } catch (err) {
      console.log('No alerts', err);
    }
  };

  const fetchPendingRequests = async () => {
    try {
      const res = await api.get('/assignment/pending-requests');
      if (res.data.success) setPendingRequests(res.data.data?.length || 0);
    } catch (err) { console.log('No requests'); }
  };

  // SignalR: realtime subscription for caretakers
  useEffect(() => {
    if (!user || user.role !== 'CARETAKER') return;

    const token = localStorage.getItem('token');
    const conn = signalRService.createConnection(token);

    const onAlert = (payload) => {
      const alert = {
        id: payload.id || payload.Id || payload._id,
        elderName: payload.elderName || payload.ElderName,
        message: payload.message,
        emergencyType: payload.emergencyType || payload.alertType,
        triggeredAt: payload.triggeredAt || payload.createdAt
      };
      setEmergencyAlerts(prev => {
        // avoid duplicates
        if ((prev || []).some(a => a.id === alert.id)) return prev;
        return [alert, ...(prev || [])];
      });

      // browser notification (optional)
      try {
        if (typeof Notification !== 'undefined') {
          if (Notification.permission === 'granted') {
            new Notification(`Emergency from ${alert.elderName}`, { body: alert.message || alert.emergencyType });
          } else if (Notification.permission !== 'denied') {
            Notification.requestPermission();
          }
        }
      } catch {}
    };

    conn.on('EmergencyAlert', onAlert);

    conn.start()
      .then(() => {
        // initial fetch after connection established
        fetchEmergencyAlerts();
      })
      .catch(err => console.error('SignalR start error', err));

    // also fetch pending requests once
    fetchPendingRequests();

    return () => {
      try { conn.off('EmergencyAlert', onAlert); conn.stop(); } catch {}
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [user]);

  const handleAcknowledge = async (alertId) => {
    try {
      await api.post(`/emergency/resolve/${alertId}`);
      setEmergencyAlerts(prev => (prev || []).filter(a => a.id !== alertId && a._id !== alertId && a.Id !== alertId));
    } catch (err) { console.log('Failed to acknowledge', err); }
  };

  const handleLogout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    sessionStorage.removeItem('showCheckInCard');
    navigate('/login');
  };

  if (!user) return null;

  const elderCards = [
    { title: 'Select Caretaker', subtitle: 'Send request to caretakers', icon: <PersonAdd sx={{ fontSize: 40 }} />, gradient: 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)', path: '/select-caretaker' },
    { title: 'My Caretakers', subtitle: 'Manage your caretakers', icon: <Person sx={{ fontSize: 40 }} />, gradient: 'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)', path: '/my-caretakers' },
    { title: 'My Activities', subtitle: 'View your activity schedule', icon: <CalendarMonth sx={{ fontSize: 40 }} />, gradient: 'linear-gradient(135deg, #fa709a 0%, #fee140 100%)', path: '/activity' },
    { title: 'My Prescriptions', subtitle: 'View & upload prescriptions', icon: <LocalHospital sx={{ fontSize: 40 }} />, gradient: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)', path: '/prescriptions' },
    { title: 'My Medications', subtitle: 'Track your medicines', icon: <Medication sx={{ fontSize: 40 }} />, gradient: 'linear-gradient(135deg, #11998e 0%, #38ef7d 100%)', path: '/medications' },
    { title: 'Reminders', subtitle: 'Medication & appointments', icon: <Assignment sx={{ fontSize: 40 }} />, gradient: 'linear-gradient(135deg, #43cea2 0%, #185a9d 100%)', path: '/reminders' },
    { title: 'Check-In', subtitle: 'Rozana sehat ka status', icon: <Favorite sx={{ fontSize: 40 }} />, gradient: 'linear-gradient(135deg, #eb3349 0%, #f45c43 100%)', path: '/checkin' },
    { title: 'Health Records', subtitle: 'Check-ins & vitals', icon: <Favorite sx={{ fontSize: 40 }} />, gradient: 'linear-gradient(135deg, #eb3349 0%, #f45c43 100%)', path: '/health' },
    // { title: 'Notes', subtitle: 'Your personal notes', icon: <Assignment sx={{ fontSize: 40 }} />, gradient: 'linear-gradient(135deg, #43cea2 0%, #185a9d 100%)', path: '/note' },
    { title: 'Call Caretaker', subtitle: 'Voice call your caretaker', icon: <Phone sx={{ fontSize: 40 }} />, gradient: 'linear-gradient(135deg, #00c6ff 0%, #0072ff 100%)', path: '/call' },
    { title: 'Chat', subtitle: 'Message caretaker', icon: <Chat sx={{ fontSize: 40 }} />, gradient: 'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)', path: '/chat' },
    { title: 'Emergency', subtitle: 'Alert caretaker', icon: <Warning sx={{ fontSize: 40 }} />, gradient: 'linear-gradient(135deg, #ff416c 0%, #ff4b2b 100%)', path: '/emergency' }
  ];

  const caretakerCards = [
    { title: 'Pending Requests', subtitle: `${pendingRequests} requests waiting`, icon: <HourglassEmpty sx={{ fontSize: 40 }} />, gradient: 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)', path: '/pending-requests', badge: pendingRequests },
    { title: 'Manage Activities', subtitle: 'View & assign activities', icon: <CalendarMonth sx={{ fontSize: 40 }} />, gradient: 'linear-gradient(135deg, #fa709a 0%, #fee140 100%)', path: '/activity' },
    { title: 'Pending Prescriptions', subtitle: 'Review & approve', icon: <Assignment sx={{ fontSize: 40 }} />, gradient: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)', path: '/prescriptions' },
    // { title: 'Notes', subtitle: 'View/add notes', icon: <Assignment sx={{ fontSize: 40 }} />, gradient: 'linear-gradient(135deg, #43cea2 0%, #185a9d 100%)', path: '/note' },
    { title: 'My Elders', subtitle: 'Manage assigned elders', icon: <Person sx={{ fontSize: 40 }} />, gradient: 'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)', path: '/my-elders' },
    { title: 'Medications', subtitle: 'Manage medicines', icon: <Medication sx={{ fontSize: 40 }} />, gradient: 'linear-gradient(135deg, #11998e 0%, #38ef7d 100%)', path: '/medications' },
    { title: 'Call Elder', subtitle: 'Voice call your elder', icon: <Phone sx={{ fontSize: 40 }} />, gradient: 'linear-gradient(135deg, #00c6ff 0%, #0072ff 100%)', path: '/call' },
    { title: 'Chat', subtitle: 'Message elders', icon: <Chat sx={{ fontSize: 40 }} />, gradient: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)', path: '/chat' },
    { title: 'Emergency Alerts', subtitle: `${emergencyAlerts.length} active`, icon: <Warning sx={{ fontSize: 40 }} />, gradient: 'linear-gradient(135deg, #ff416c 0%, #ff4b2b 100%)', path: '/emergencies', badge: emergencyAlerts.length }
  ];

  const cards = user.role === 'ELDER' ? elderCards : caretakerCards;

  return (
    <>
      {/* Daily Check-In Card for Elder - show only if not checked in today */}
      {user.role === 'ELDER' && !checkInStatus && showCheckInCard && (
        <Paper elevation={4} sx={{ mb: 3, p: 3, borderRadius: 4, background: 'linear-gradient(135deg, #f8ffae 0%, #43c6ac 100%)', display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
          <Typography variant="h5" sx={{ fontWeight: 700, color: '#2d3a4a', mb: 1, letterSpacing: 1 }}>
            Daily Check-In
          </Typography>
          <Typography variant="body1" sx={{ mb: 2, color: '#333' }}>
            How are you feeling today? <b>I am OK</b> or <b>Not OK</b>
          </Typography>
          <Box sx={{ display: 'flex', gap: 2, mb: 2 }}>
            <Button variant="contained" color="success" disabled={checkInLoading} onClick={() => handleCheckIn('OK')}>I am OK</Button>
            <Button variant="contained" color="error" disabled={checkInLoading} onClick={() => handleCheckIn('NOT_OK')}>Not OK</Button>
          </Box>
          {checkInMessage && <Typography sx={{ color: checkInMessage.includes('success') ? 'green' : 'red', mt: 1 }}>{checkInMessage}</Typography>}
        </Paper>
      )}
      {/* Show status after check-in */}
      {user.role === 'ELDER' && checkInStatus && (
        <Paper elevation={2} sx={{ mb: 3, p: 2, borderRadius: 4, background: '#e0ffe0', display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
          <Alert severity={checkInStatus === 'OK' ? 'success' : 'error'} sx={{ mb: 1 }}>
            You have checked in: <b>{checkInStatus}</b>
          </Alert>
        </Paper>
      )}
      <Box sx={{ minHeight: '100vh', background: 'linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%)' }}>
      <Paper elevation={0} sx={{ background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)', borderRadius: 0, py: 2, px: 3 }}>
        <Container maxWidth="lg">
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
              <Avatar sx={{ width: 50, height: 50, bgcolor: 'rgba(255,255,255,0.2)', border: '2px solid white' }}>{user.fullName?.charAt(0) || 'U'}</Avatar>
              <Box>
                <Typography variant="h6" sx={{ color: 'white', fontWeight: 600 }}>{user.fullName}</Typography>
                <Chip label={user.role === 'ELDER' ? '👴 Elder' : '👨‍⚕️ Caretaker'} size="small" sx={{ bgcolor: 'rgba(255,255,255,0.2)', color: 'white' }} />
              </Box>
            </Box>
            <Box sx={{ display: 'flex', gap: 1 }}>
              <IconButton sx={{ color: 'white' }}><Notifications /></IconButton>
              <IconButton sx={{ color: 'white' }}><Settings /></IconButton>
              <Button variant="outlined" startIcon={<ExitToApp />} onClick={handleLogout} sx={{ color: 'white', borderColor: 'rgba(255,255,255,0.5)' }}>Logout</Button>
            </Box>
          </Box>
        </Container>
      </Paper>

      <Container maxWidth="lg" sx={{ mt: 2 }}>
        {/* Medication Section for Elder or Caretaker's selected elder */}
        {(user.role === 'ELDER' || (user.role === 'CARETAKER' && selectedElder)) && (
          <Paper elevation={3} sx={{ mb: 4, p: 3, borderRadius: 4, background: 'linear-gradient(135deg, #e0eafc 0%, #cfdef3 100%)' }}>
            <Typography variant="h5" gutterBottom sx={{ fontWeight: 700, color: '#2d3a4a', mb: 2, letterSpacing: 1 }}>
              <Medication sx={{ verticalAlign: 'middle', color: '#11998e', mr: 1 }} />
              {user.role === 'ELDER' ? 'My Assigned Medications' : 'Elder Medications'}
            </Typography>
            {/* Caretaker: Elder selection dropdown */}
            {user.role === 'CARETAKER' && (
              <Box sx={{ mb: 2, maxWidth: 320 }}>
                <Typography variant="subtitle1" sx={{ mb: 1 }}>Select Elder:</Typography>
                <select
                  value={selectedElder}
                  onChange={e => setSelectedElder(e.target.value)}
                  style={{ padding: 8, borderRadius: 4, border: '1px solid #ccc', width: '100%' }}
                >
                  <option value="">-- Select Elder --</option>
                  {elders.map(elder => (
                    <option key={elder.elderId || elder._id || elder.id} value={elder.elderId || elder._id || elder.id}>
                      {elder.elderName || elder.fullName || elder.name || elder.email}
                    </option>
                  ))}
                </select>
              </Box>
            )}
            {(user.role === 'ELDER' || (user.role === 'CARETAKER' && selectedElder)) && missedCount > 0 && (
              <Paper elevation={2} sx={{ p: 2, mb: 2, background: '#fff3e0', border: '1px solid #ff9800', color: '#e65100' }}>
                <strong>Missed Doses Alert:</strong> {user.role === 'ELDER' ? 'You have' : 'This elder has'} missed {missedCount} dose{missedCount > 1 ? 's' : ''} today. Please review the schedule!
              </Paper>
            )}
            {medications.length === 0 && <Typography sx={{ color: '#888', fontStyle: 'italic' }}>No medications assigned.</Typography>}
            <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 3 }}>
              {medications.map((med, idx) => {
                let scheduleTimes = [];
                if (Array.isArray(med.schedules) && med.schedules.length > 0) {
                  scheduleTimes = med.schedules.map(s => s.time || s.Time || s);
                } else if (Array.isArray(med.scheduleTimes)) {
                  scheduleTimes = med.scheduleTimes;
                }
                const endDate = med.endDate ? new Date(med.endDate).toLocaleDateString() : 'N/A';
                const today = new Date();
                return (
                  <Paper key={idx} elevation={2} sx={{ minWidth: 270, maxWidth: 340, p: 2.5, borderRadius: 3, background: 'white', boxShadow: '0 2px 12px #e0eafc', display: 'flex', flexDirection: 'column', alignItems: 'flex-start', position: 'relative' }}>
                    <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                      <Medication sx={{ color: '#11998e', fontSize: 32, mr: 1 }} />
                      <Typography variant="h6" sx={{ fontWeight: 600, color: '#2d3a4a' }}>{med.medicineName || med.name}</Typography>
                    </Box>
                    <Typography sx={{ color: '#555', fontSize: 15, mb: 0.5 }}>
                      <strong>Dosage:</strong> {med.dosage || ''} &nbsp; <strong>Frequency:</strong> {med.frequency || ''}
                    </Typography>
                    <Typography sx={{ color: '#888', fontSize: 14, mb: 0.5 }}>
                      <strong>End Date:</strong> {endDate}
                    </Typography>
                    {med.schedules && med.schedules.length > 0 && (
                      <Box sx={{ mt: 1, mb: 1, display: 'flex', flexWrap: 'wrap', gap: 1 }}>
                        {med.schedules.map((sch, i) => {
                          let timeStr = sch.time || sch.Time || sch;
                          let [h, m, ampm] = timeStr.match(/(\d+):(\d+)\s*(AM|PM)?/i) || [];
                          let status = '';
                          let label = '';
                          let hour = h ? parseInt(h, 10) : 0;
                          if (ampm && ampm.toUpperCase() === 'PM' && hour < 12) hour += 12;
                          let schedDate = new Date(today);
                          schedDate.setHours(hour, m ? parseInt(m, 10) : 0, 0, 0);
                          if (sch.isTaken || takenMap[`${med._id || med.id}_${timeStr}`]) {
                            status = 'success'; label = `✔ Taken: ${timeStr}`;
                          } else if (schedDate < today) {
                            status = 'error'; label = `❌ Missed: ${timeStr}`;
                          } else {
                            status = 'info'; label = `⏳ Upcoming: ${timeStr}`;
                          }
                          return (
                            <Chip
                              key={i}
                              label={label}
                              color={status}
                              sx={{ fontWeight: 500, fontSize: 13, px: 1.5, mb: 0.5 }}
                              disabled={true}
                            />
                          );
                        })}
                      </Box>
                    )}
                  </Paper>
                );
              })}
            </Box>
            {markError && <Typography color="error" sx={{ mt: 2 }}>{markError}</Typography>}
          </Paper>
        )}
        {/* Emergency Alerts Banner - Caretaker Only */}
        {user.role === 'CARETAKER' && emergencyAlerts.length > 0 && (
          <Fade in>
            <Box sx={{ mb: 3 }}>
              {emergencyAlerts.map((alert) => (
                <Alert 
                  key={alert.id} 
                  severity="error" 
                  sx={{ mb: 1, borderRadius: 2, animation: 'pulse 1s infinite', '@keyframes pulse': { '0%, 100%': { opacity: 1 }, '50%': { opacity: 0.7 } } }}
                  action={
                    <Button color="inherit" size="small" onClick={() => handleAcknowledge(alert.id)}>
                      ACKNOWLEDGE
                    </Button>
                  }
                >
                  <Typography sx={{ fontWeight: 600 }}>
                    🚨 EMERGENCY: {alert.elderName || 'Elder'} needs help! - {alert.message}
                  </Typography>
                  <Typography variant="caption">
                    {new Date(alert.triggeredAt).toLocaleTimeString()}
                  </Typography>
                </Alert>
              ))}
            </Box>
          </Fade>
        )}

        <Fade in timeout={600}>
          <Paper elevation={3} sx={{ p: 4, mb: 4, borderRadius: 4 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
              <Avatar sx={{ width: 60, height: 60, background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)' }}><DashboardIcon sx={{ fontSize: 30 }} /></Avatar>
              <Box>
                <Typography variant="h4" sx={{ fontWeight: 700 }}>Welcome, {user.fullName?.split(' ')[0]}! 👋</Typography>
                <Typography variant="body1" color="text.secondary">{user.role === 'ELDER' ? "Here's your health dashboard." : "Here's your care dashboard."}</Typography>
              </Box>
            </Box>
          </Paper>
        </Fade>

        <Typography variant="h5" sx={{ fontWeight: 600, mb: 3 }}>⚡ Quick Actions</Typography>

        <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 3 }}>
          {cards.map((card, i) => (
            <Fade in timeout={800 + i * 100} key={i}>
              <Card sx={{ width: { xs: '100%', sm: 'calc(50% - 12px)', md: 'calc(33.333% - 16px)' }, cursor: 'pointer', borderRadius: 4, transition: 'all 0.3s', position: 'relative', '&:hover': { transform: 'translateY(-8px)', boxShadow: '0 20px 40px rgba(0,0,0,0.15)' } }} onClick={() => navigate(card.path)}>
                {card.badge > 0 && (
                  <Chip label={card.badge} color="error" size="small" sx={{ position: 'absolute', top: 10, right: 10, fontWeight: 700 }} />
                )}
                <Box sx={{ background: card.gradient, p: 3, display: 'flex', alignItems: 'center', gap: 2 }}>
                  <Avatar sx={{ width: 60, height: 60, bgcolor: 'rgba(255,255,255,0.2)' }}>{card.icon}</Avatar>
                  <Box sx={{ color: 'white' }}>
                    <Typography variant="h6" sx={{ fontWeight: 600 }}>{card.title}</Typography>
                    <Typography variant="body2" sx={{ opacity: 0.9 }}>{card.subtitle}</Typography>
                  </Box>
                </Box>
              </Card>
            </Fade>
          ))}
        </Box>

        <Box sx={{ textAlign: 'center', mt: 6, mb: 4 }}>
          <Typography variant="body2" color="text.secondary">🏥 Care Companion System</Typography>
        </Box>
      </Container>
    </Box>
    </>
  );
};

export default Dashboard;