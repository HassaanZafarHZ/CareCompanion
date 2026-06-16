import React, { useEffect, useState } from 'react';
import { Container, Paper, Typography, Box, List, ListItem, ListItemAvatar, Avatar, ListItemText, CircularProgress, Button, Dialog, DialogTitle, DialogContent, DialogContentText, DialogActions } from '@mui/material';
import PersonIcon from '@mui/icons-material/Person';
import { assignmentService } from '../services/assignmentService';

const MyCaretakers = () => {
  const [caretaker, setCaretaker] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [removeDialogOpen, setRemoveDialogOpen] = useState(false);
  const [removing, setRemoving] = useState(false);

  const handleRemoveClick = (assignmentId) => {
    setRemoveDialogOpen(true);
  };

  const handleRemoveConfirm = async () => {
    if (!caretaker) return;
    setRemoving(true);
    try {
      const res = await assignmentService.removeAssignment(caretaker.caretakerId);
      if (res?.data?.success) {
        setCaretaker(null);
        setRemoveDialogOpen(false);
      } else {
        setError(res?.data?.message || 'Failed to remove assignment');
      }
    } catch {
      setError('Failed to remove assignment');
    } finally {
      setRemoving(false);
      fetchCaretaker();
    }
  };

  const handleRemoveCancel = () => {
    setRemoveDialogOpen(false);
  };

  useEffect(() => {
    fetchCaretaker();
  }, []);

  const fetchCaretaker = async () => {
    setLoading(true);
    setError('');
    try {
      const res = await assignmentService.getMyAssignment();
      if (res?.data?.success && res.data.data?.caretaker) {
        setCaretaker({
          caretakerId: res.data.data.caretaker.id,
          caretakerName: res.data.data.caretaker.name
        });
      } else {
        setCaretaker(null);
        setError(res?.data?.message || 'No caretaker assigned yet.');
      }
    } catch {
      setError('Failed to load caretaker');
      setCaretaker(null);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Container maxWidth="sm" sx={{ mt: 4 }}>
      <Paper elevation={4} sx={{ p: 3, borderRadius: 4, minHeight: 400, display: 'flex', flexDirection: 'column' }}>
        <Typography variant="h4" align="center" gutterBottom sx={{ fontWeight: 700, color: '#3f51b5' }}>
          My Caretakers
        </Typography>
        <Box sx={{ flex: 1, overflowY: 'auto', mb: 2, maxHeight: 350, bgcolor: '#f5f7fa', borderRadius: 2, p: 2 }}>
          {loading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100%' }}>
              <CircularProgress />
            </Box>
          ) : error ? (
            <Typography color="error" align="center">{error}</Typography>
          ) : !caretaker ? (
            <Typography align="center" color="text.secondary">No caretaker assigned yet.</Typography>
          ) : (
            <List>
              <ListItem key={caretaker.caretakerId} sx={{ mb: 2, borderRadius: 2, bgcolor: '#fff', boxShadow: 1 }}>
                <ListItemAvatar>
                  <Avatar sx={{ bgcolor: '#3f51b5' }}>
                    <PersonIcon />
                  </Avatar>
                </ListItemAvatar>
                <ListItemText
                  primary={<Typography sx={{ fontWeight: 600 }}>{caretaker.caretakerName}</Typography>}
                  secondary={<Typography variant="body2" color="text.secondary">ID: {caretaker.caretakerId}</Typography>}
                />
                <Button
                  variant="outlined"
                  color="error"
                  onClick={() => handleRemoveClick(caretaker.caretakerId)}
                  sx={{ ml: 2 }}
                  disabled={removing}
                >
                  Remove
                </Button>
              </ListItem>
            </List>
          )}
        </Box>
        <Button onClick={fetchCaretaker} variant="outlined" color="primary" sx={{ mt: 2, borderRadius: 2 }}>
          Refresh
        </Button>
      </Paper>

      <Dialog open={removeDialogOpen} onClose={handleRemoveCancel}>
        <DialogTitle>Remove Caretaker</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Kya aap waqai is caretaker ko remove karna chahte hain? Yeh action wapas nahi ho sakta.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleRemoveCancel} disabled={removing}>Cancel</Button>
          <Button onClick={handleRemoveConfirm} color="error" disabled={removing}>
            {removing ? 'Removing...' : 'Remove'}
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
};

export default MyCaretakers;
