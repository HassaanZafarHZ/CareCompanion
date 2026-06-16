
import React, { useEffect, useState } from 'react';
import { Container, Typography, Paper, List, ListItem, ListItemText, Button, Box, TextField, Alert, MenuItem } from '@mui/material';
import api from '../services/api';

const Activity = () => {
  const [activities, setActivities] = useState([]);
  // const [loading, setLoading] = useState(true); // removed unused
  const [error, setError] = useState('');
  const [newActivity, setNewActivity] = useState({
    ElderId: '',
    ActivityType: '',
    ScheduledTime: '',
    DurationMinutes: 30,
    Repeat: 'DAILY',
    Notes: ''
  });
  const [success, setSuccess] = useState('');
  const [user, setUser] = useState(null);
  const [elders, setElders] = useState([]); // For caretaker's assigned elders


  useEffect(() => {
    const userData = localStorage.getItem('user');
    if (!userData) return;
    const parsedUser = JSON.parse(userData);
    setUser(parsedUser);
    if (parsedUser.role === 'ELDER') {
      setNewActivity((prev) => ({ ...prev, ElderId: parsedUser.id || parsedUser._id || parsedUser.Id }));
    }
    if (parsedUser.role === 'CARETAKER') {
      fetchAssignedElders();
    }
    fetchActivities(parsedUser);
  }, []);

  // Fetch assigned elders for caretaker
  const fetchAssignedElders = async () => {
    try {
      const res = await api.get('/assignment/my-elders');
      // Assuming res.data.data is array of elders with id and name
      setElders(res?.data?.data || []);
    } catch {
      setElders([]);
    }
  };

  const fetchActivities = async (user) => {
    // setLoading(true); // removed
    setError('');
    try {
      let res;
      if (user.role === 'ELDER') {
        res = await api.get('/activity/today');
      } else if (user.role === 'CARETAKER') {
        // For demo, fetch today's activities for first assigned elder (or show empty)
        // In real app, caretaker should select elder
        // Here, just show empty or you can fetch all elders and pick one
        setActivities([]);
        // setLoading(false); // removed
        return;
      }
      setActivities(res?.data?.data || []);
    } catch (err) {
      setError('Failed to load activities');
    } finally {
      // setLoading(false); // removed
    }
  };

  const handleChange = (e) => {
    setNewActivity({ ...newActivity, [e.target.name]: e.target.value });
  };

  const handleCreate = async (e) => {
    e.preventDefault();
    setError('');
    setSuccess('');
    try {
      const payload = { ...newActivity };
      // Remove ElderId if empty
      if (!payload.ElderId) {
        setError('ElderId is required');
        return;
      }
      const res = await api.post('/activity/create', payload);
      if (res.data.success) {
        setSuccess('Activity created successfully!');
        setNewActivity((prev) => ({ ...prev, ActivityType: '', ScheduledTime: '', DurationMinutes: 30, Repeat: 'DAILY', Notes: '' }));
        const userData = localStorage.getItem('user');
        if (userData) fetchActivities(JSON.parse(userData));
      } else {
        setError(res.data.message || 'Failed to create activity');
      }
    } catch (err) {
      setError('Failed to create activity');
    }
  };

  return (
    <Container maxWidth="md" sx={{ mt: 4 }}>
      <Paper elevation={3} sx={{ p: 4, borderRadius: 4 }}>
        <Typography variant="h4" gutterBottom>Activity Schedule</Typography>
        {error && <Alert severity="error">{error}</Alert>}
        {success && <Alert severity="success">{success}</Alert>}
        {/* Caretaker only: show create form */}
        {user && user.role === 'CARETAKER' && (
          <Box component="form" onSubmit={handleCreate} sx={{ mb: 3 }}>
            <TextField
              select
              label="Elder"
              name="ElderId"
              value={newActivity.ElderId}
              onChange={handleChange}
              fullWidth
              sx={{ mb: 2 }}
              required
            >
              {elders.length === 0 && <MenuItem value="">No assigned elders</MenuItem>}
              {elders.map((elder) => {
                // For assignments, ElderName is the correct property for display
                const id = elder.elderId || elder.ElderId || elder.id || elder._id || elder.Id;
                const name = elder.elderName || elder.ElderName || elder.name || elder.fullName || elder.username || elder.email || '';
                return (
                  <MenuItem key={id} value={id}>
                    {name}
                  </MenuItem>
                );
              })}
            </TextField>
            <TextField
              select
              label="Activity Type"
              name="ActivityType"
              value={newActivity.ActivityType}
              onChange={handleChange}
              fullWidth
              sx={{ mb: 2 }}
              required
            >
              <MenuItem value="SLEEP">Sleep</MenuItem>
              <MenuItem value="WALK">Walk</MenuItem>
              <MenuItem value="EXERCISE">Exercise</MenuItem>
              <MenuItem value="MEAL">Meal</MenuItem>
              <MenuItem value="MEDICINE">Medicine</MenuItem>
            </TextField>
            <TextField
              label="Scheduled Time"
              name="ScheduledTime"
              type="time"
              value={newActivity.ScheduledTime}
              onChange={handleChange}
              fullWidth
              sx={{ mb: 2 }}
              InputLabelProps={{ shrink: true }}
              required
            />
            <TextField
              label="Duration (minutes)"
              name="DurationMinutes"
              type="number"
              value={newActivity.DurationMinutes}
              onChange={handleChange}
              fullWidth
              sx={{ mb: 2 }}
              required
            />
            <TextField
              select
              label="Repeat"
              name="Repeat"
              value={newActivity.Repeat}
              onChange={handleChange}
              fullWidth
              sx={{ mb: 2 }}
            >
              <MenuItem value="DAILY">Daily</MenuItem>
              <MenuItem value="WEEKLY">Weekly</MenuItem>
            </TextField>
            <TextField
              label="Notes"
              name="Notes"
              value={newActivity.Notes}
              onChange={handleChange}
              fullWidth
              sx={{ mb: 2 }}
            />
            <Button type="submit" variant="contained" color="primary">Create Activity</Button>
          </Box>
        )}
        <List>
          {activities.map((act, idx) => (
            <ListItem key={idx} divider>
              <ListItemText
                primary={act.activityType || act.ActivityType}
                secondary={`Time: ${act.scheduledTime || act.ScheduledTime} | Duration: ${act.durationMinutes || act.DurationMinutes} min | Repeat: ${act.repeat || act.Repeat} | Notes: ${act.notes || act.Notes || ''}`}
              />
              {/* Elder: show complete button if not completed */}
              {user && user.role === 'ELDER' && !act.completedAt && (
                <Button
                  variant="outlined"
                  color="success"
                  size="small"
                  sx={{ ml: 2 }}
                  onClick={async () => {
                    try {
                      await api.post('/activity/complete', { ScheduleId: act.id || act._id || act.Id });
                      const userData = localStorage.getItem('user');
                      if (userData) fetchActivities(JSON.parse(userData));
                    } catch {
                      setError('Failed to mark complete');
                    }
                  }}
                >
                  Mark Complete
                </Button>
              )}
              {user && user.role === 'ELDER' && act.completedAt && (
                <Typography variant="caption" color="success.main" sx={{ ml: 2 }}>
                  Completed
                </Typography>
              )}
            </ListItem>
          ))}
        </List>
      </Paper>
    </Container>
  );
};

export default Activity;
