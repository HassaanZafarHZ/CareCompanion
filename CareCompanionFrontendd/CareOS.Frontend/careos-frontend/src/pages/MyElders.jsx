import React, { useEffect, useState } from 'react';
import { Container, Paper, Typography, Box, List, ListItem, ListItemAvatar, Avatar, ListItemText, CircularProgress, Button, Dialog, DialogTitle, DialogContent, DialogContentText, DialogActions } from '@mui/material';
import PersonIcon from '@mui/icons-material/Person';
import { assignmentService } from '../services/assignmentService';

const MyElders = () => {
  const [elders, setElders] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [removeDialogOpen, setRemoveDialogOpen] = useState(false);
  const [selectedAssignmentId, setSelectedAssignmentId] = useState(null);
  const [removing, setRemoving] = useState(false);

  const handleRemoveClick = (assignmentId) => {
    setSelectedAssignmentId(assignmentId);
    setRemoveDialogOpen(true);
  };

  const handleRemoveConfirm = async () => {
    if (!selectedAssignmentId) return;
    setRemoving(true);
    try {
      const res = await assignmentService.removeAssignment(selectedAssignmentId);
      if (res?.data?.success) {
        setElders(elders.filter(e => e.assignmentId !== selectedAssignmentId && e.id !== selectedAssignmentId && e._id !== selectedAssignmentId));
        setRemoveDialogOpen(false);
        setSelectedAssignmentId(null);
      } else {
        setError(res?.data?.message || 'Failed to remove assignment');
      }
    } catch {
      setError('Failed to remove assignment');
    } finally {
      setRemoving(false);
      fetchElders();
    }
  };

  const handleRemoveCancel = () => {
    setRemoveDialogOpen(false);
    setSelectedAssignmentId(null);
  };

  useEffect(() => {
    fetchElders();
  }, []);

  const fetchElders = async () => {
    setLoading(true);
    setError('');
    try {
      const res = await assignmentService.getMyElders();
      if (res?.data?.success && Array.isArray(res.data.data)) {
        setElders(res.data.data);
      } else {
        setError(res?.data?.message || 'Failed to load elders');
      }
    } catch {
      setError('Failed to load elders');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Container maxWidth="sm" sx={{ mt: 4 }}>
      <Paper elevation={4} sx={{ p: 3, borderRadius: 4, minHeight: 400, display: 'flex', flexDirection: 'column' }}>
        <Typography variant="h4" align="center" gutterBottom sx={{ fontWeight: 700, color: '#3f51b5' }}>
          My Elders
        </Typography>
        <Box sx={{ flex: 1, overflowY: 'auto', mb: 2, maxHeight: 350, bgcolor: '#f5f7fa', borderRadius: 2, p: 2 }}>
          {loading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100%' }}>
              <CircularProgress />
            </Box>
          ) : error ? (
            <Typography color="error" align="center">{error}</Typography>
          ) : elders.length === 0 ? (
            <Typography align="center" color="text.secondary">No elders assigned yet.</Typography>
          ) : (
            <List>
              {elders.map((elder, idx) => (
                <ListItem key={elder.elderId || idx} sx={{ mb: 2, borderRadius: 2, bgcolor: '#fff', boxShadow: 1 }}>
                  <ListItemAvatar>
                    <Avatar sx={{ bgcolor: '#3f51b5' }}>
                      <PersonIcon />
                    </Avatar>
                  </ListItemAvatar>
                  <ListItemText
                    primary={<Typography sx={{ fontWeight: 600 }}>{elder.elderName}</Typography>}
                    secondary={
                      <>
                        <Typography variant="body2" color="text.secondary">ID: {elder.elderId}</Typography>
                        {elder.notes && <Typography variant="body2" color="text.secondary">Notes: {elder.notes}</Typography>}
                      </>
                    }
                  />
                  <Button
                    variant="outlined"
                    color="error"
                    onClick={() => handleRemoveClick(elder.assignmentId || elder.elderId || elder._id || idx)}
                    sx={{ ml: 2 }}
                    disabled={removing && selectedAssignmentId === (elder.assignmentId || elder.elderId || elder._id || idx)}
                  >
                    Remove
                  </Button>
                </ListItem>
              ))}
            </List>
          )}
        </Box>

        <Button onClick={fetchElders} variant="outlined" color="primary" sx={{ mt: 2, borderRadius: 2 }}>
          Refresh
        </Button>
      </Paper>

      <Dialog open={removeDialogOpen} onClose={handleRemoveCancel}>
        <DialogTitle>Remove Elder</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Are you sure you want to remove this elder from your list? This action cannot be undone.
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

export default MyElders;
