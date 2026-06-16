
import React, { useEffect, useState } from 'react';
import { medicationService } from '../services/medicationService';
import { Container, Typography, Paper, List, ListItem, ListItemText, Button, Box, MenuItem, Select, InputLabel, FormControl, Chip } from '@mui/material';
import { showReminderNotification } from '../utils/reminderNotification';


const Medications = () => {
  const [medications, setMedications] = useState([]);
  const [elders, setElders] = useState([]);
  const [selectedElder, setSelectedElder] = useState('');
  const [user, setUser] = useState(null);
  // Add Medication form state
  const [showAddForm, setShowAddForm] = useState(false);
  const [form, setForm] = useState({
    medicineName: '',
    dosage: '',
    frequency: '',
    scheduleTimes: '', // comma separated string
    endDate: ''
  });
  const [formError, setFormError] = useState('');
  const [takenMap, setTakenMap] = useState({}); // {medId_time: true}
  const [missedCount, setMissedCount] = useState(0);
  const [markError, setMarkError] = useState('');

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
    if (userData) {
      const parsed = JSON.parse(userData);
      setUser(parsed);
      if (parsed.role === 'ELDER') {
        // Always request notification permission on page load for elders
        if (window.Notification && Notification.permission !== 'granted' && Notification.permission !== 'denied') {
          Notification.requestPermission();
        }
        fetchMedications();
      } else if (parsed.role === 'CARETAKER') {
        fetchElders();
      }
    }
  }, []);

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

  const fetchElders = async () => {
    // Get assigned elders for caretaker
    try {
      const res = await import('../services/assignmentService').then(m => m.assignmentService.getMyElders());
      setElders(res.data.data || []);
    } catch {
      setElders([]);
    }
  };

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
    } catch (err) {
      // error handling
    }
  };

  // Add Medication submit handler
  const handleAddMedication = async (e) => {
    e.preventDefault();
    setFormError('');
    if (!selectedElder) {
      setFormError('Please select an elder.');
      return;
    }
    if (!form.medicineName || !form.dosage || !form.frequency || !form.scheduleTimes) {
      setFormError('All fields except End Date are required.');
      return;
    }
    try {
      const payload = {
        elderId: selectedElder,
        medicineName: form.medicineName,
        dosage: form.dosage,
        frequency: form.frequency,
        scheduleTimes: form.scheduleTimes.split(',').map(s => s.trim()),
        endDate: form.endDate ? new Date(form.endDate) : null
      };
      await medicationService.create(payload);
      setShowAddForm(false);
      setForm({ medicineName: '', dosage: '', frequency: '', scheduleTimes: '', endDate: '' });
      fetchMedications(selectedElder);
    } catch (err) {
      setFormError('Failed to add medication.');
    }
  };

  return (
    <Container maxWidth="md" sx={{ mt: 4 }}>
      <Paper elevation={3} sx={{ p: 4, borderRadius: 4 }}>
        <Typography variant="h4" gutterBottom>My Medications</Typography>
        {(user && ((user.role === 'ELDER' && missedCount > 0) || (user.role === 'CARETAKER' && selectedElder && missedCount > 0))) && (
          <Paper elevation={2} sx={{ p: 2, mb: 2, background: '#fff3e0', border: '1px solid #ff9800', color: '#e65100' }}>
            <strong>Missed Doses Alert:</strong> {user.role === 'ELDER' ? 'You have' : 'This elder has'} missed {missedCount} dose{missedCount > 1 ? 's' : ''} today. Please review the schedule!
          </Paper>
        )}
        {user && user.role === 'CARETAKER' && (
          <>
            <FormControl fullWidth sx={{ mb: 3 }}>
              <InputLabel id="elder-select-label">Select Elder</InputLabel>
              <Select
                labelId="elder-select-label"
                value={selectedElder}
                label="Select Elder"
                onChange={e => {
                  setSelectedElder(e.target.value);
                  fetchMedications(e.target.value);
                }}
              >
                {elders.map((elder) => (
                  <MenuItem key={elder.elderId || elder._id || elder.id} value={elder.elderId || elder._id || elder.id}>
                    {elder.elderName || elder.fullName || elder.name || elder.email}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
            <Box sx={{ mt: 2, mb: 2 }}>
              <Button variant="contained" color="primary" onClick={() => setShowAddForm(!showAddForm)} disabled={!selectedElder}>
                {showAddForm ? 'Cancel' : 'Add Medication'}
              </Button>
            </Box>
            {showAddForm && (
              <Box component="form" onSubmit={handleAddMedication} sx={{ mb: 3, p: 2, border: '1px solid #eee', borderRadius: 2 }}>
                <Typography variant="h6" gutterBottom>Add Medication for Elder</Typography>
                <FormControl fullWidth sx={{ mb: 2 }}>
                  <InputLabel shrink htmlFor="medicineName">Medicine Name</InputLabel>
                  <input
                    id="medicineName"
                    type="text"
                    value={form.medicineName}
                    onChange={e => setForm(f => ({ ...f, medicineName: e.target.value }))}
                    required
                    style={{ padding: 8, borderRadius: 4, border: '1px solid #ccc' }}
                  />
                </FormControl>
                <FormControl fullWidth sx={{ mb: 2 }}>
                  <InputLabel shrink htmlFor="dosage">Dosage</InputLabel>
                  <input
                    id="dosage"
                    type="text"
                    value={form.dosage}
                    onChange={e => setForm(f => ({ ...f, dosage: e.target.value }))}
                    required
                    style={{ padding: 8, borderRadius: 4, border: '1px solid #ccc' }}
                  />
                </FormControl>
                <FormControl fullWidth sx={{ mb: 2 }}>
                  <InputLabel shrink htmlFor="frequency">Frequency</InputLabel>
                  <input
                    id="frequency"
                    type="text"
                    value={form.frequency}
                    onChange={e => setForm(f => ({ ...f, frequency: e.target.value }))}
                    required
                    style={{ padding: 8, borderRadius: 4, border: '1px solid #ccc' }}
                  />
                </FormControl>
                <FormControl fullWidth sx={{ mb: 2 }}>
                  <InputLabel shrink htmlFor="scheduleTimes">Schedule Times (comma separated)</InputLabel>
                  <input
                    id="scheduleTimes"
                    type="text"
                    value={form.scheduleTimes}
                    onChange={e => setForm(f => ({ ...f, scheduleTimes: e.target.value }))}
                    required
                    placeholder="08:00 AM, 08:00 PM"
                    style={{ padding: 8, borderRadius: 4, border: '1px solid #ccc' }}
                  />
                </FormControl>
                <FormControl fullWidth sx={{ mb: 2 }}>
                  <InputLabel shrink htmlFor="endDate">End Date (optional)</InputLabel>
                  <input
                    id="endDate"
                    type="date"
                    value={form.endDate}
                    onChange={e => setForm(f => ({ ...f, endDate: e.target.value }))}
                    style={{ padding: 8, borderRadius: 4, border: '1px solid #ccc' }}
                  />
                </FormControl>
                {formError && <Typography color="error" sx={{ mb: 1 }}>{formError}</Typography>}
                <Button type="submit" variant="contained" color="success">Submit</Button>
              </Box>
            )}
          </>
        )}
        <List>
          {medications.map((med, idx) => {
            let scheduleTimes = [];
            if (Array.isArray(med.schedules) && med.schedules.length > 0) {
              scheduleTimes = med.schedules.map(s => s.time || s.Time || s);
            } else if (Array.isArray(med.scheduleTimes)) {
              scheduleTimes = med.scheduleTimes;
            }
            const endDate = med.endDate ? new Date(med.endDate).toLocaleDateString() : 'N/A';
            // For each schedule, determine status
            const today = new Date();
            const todayStr = today.toLocaleDateString();
            return (
              <ListItem key={idx} divider alignItems="flex-start">
                <ListItemText
                  primary={
                    <>
                      <strong>{med.medicineName || med.name}</strong>
                      {scheduleTimes.length > 0 && (
                        <span style={{ marginLeft: 12, color: '#555', fontSize: 13 }}>
                          | Times: {scheduleTimes.join(', ')}
                        </span>
                      )}
                    </>
                  }
                  secondary={
                    <>
                      <span>Dosage: {med.dosage || ''} | Frequency: {med.frequency || ''}</span>
                      <span style={{ marginLeft: 12 }}>End Date: {endDate}</span>
                      {(user && ((user.role === 'ELDER' && scheduleTimes.length > 0) || (user.role === 'CARETAKER' && selectedElder && scheduleTimes.length > 0))) && (
                        <Box sx={{ mt: 1 }}>
                          {med.schedules && med.schedules.length > 0 ? med.schedules.map((sch, i) => {
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
                                sx={{ mr: 1, mb: 1, fontWeight: 500, fontSize: 13 }}
                                disabled={true}
                              />
                            );
                          }) : null}
                        </Box>
                      )}
                    </>
                  }
                />
              </ListItem>
            );
          })}
        </List>
        {markError && <Typography color="error" sx={{ mt: 1 }}>{markError}</Typography>}
      </Paper>
    </Container>
  );
};

export default Medications;