import React, { useEffect, useState } from 'react';
import { Box, Typography, Paper, Button, CircularProgress } from '@mui/material';
import { checkInService } from '../services/checkInService';

const CheckIn = () => {
  const [status, setStatus] = useState('');
  const [loading, setLoading] = useState(true);
  const [history, setHistory] = useState([]);

  const fetchToday = async () => {
    setLoading(true);
    const res = await checkInService.getToday();
    setStatus(res.data?.status || '');
    setLoading(false);
  };

  const fetchHistory = async () => {
    const res = await checkInService.getHistory();
    setHistory(res.data?.data || []);
  };

  useEffect(() => {
    fetchToday();
    fetchHistory();
  }, []);

  const handleCheckIn = async () => {
    // Get user from localStorage
    const userData = localStorage.getItem('user');
    let ElderId = '';
    if (userData) {
      const user = JSON.parse(userData);
      ElderId = user.id || user._id || user.Id || '';
    }
    // Send correct payload
    await checkInService.submit({
      ElderId,
      FeelingStatus: 'GOOD',
      Notes: ''
    });
    fetchToday();
    fetchHistory();
  };

  return (
    <Box sx={{ mt: 4, display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
      <Paper sx={{ p: 4, minWidth: 350, width: 400, mb: 3 }}>
        <Typography variant="h5" gutterBottom>Daily Check-In</Typography>
        {loading ? <CircularProgress /> : (
          <>
            <Typography variant="body1" sx={{ mb: 2 }}>
              Today’s Status: <b>{status ? status : 'Not checked in'}</b>
            </Typography>
            <Button variant="contained" color="success" onClick={handleCheckIn} disabled={status === 'OK'}>
              I’m OK
            </Button>
          </>
        )}
      </Paper>
      <Paper sx={{ p: 3, minWidth: 350, width: 400 }}>
        <Typography variant="h6" gutterBottom>Check-In History</Typography>
        {history.length === 0 ? <Typography>No history found.</Typography> : (
          <ul>
            {history.map((c, i) => (
              <li key={i}>{c.CheckInTime?.substring(0, 10)} - {c.FeelingStatus}</li>
            ))}
          </ul>
        )}
      </Paper>
    </Box>
  );
};

export default CheckIn;
