import React, { useEffect, useState } from 'react';
import { Box, Typography, Paper, Button, List, ListItem, ListItemText, IconButton, TextField, MenuItem, Dialog, DialogTitle, DialogContent, DialogActions } from '@mui/material';
import { Add, Delete } from '@mui/icons-material';
import { reminderService } from '../services/reminderService';

const repeatOptions = [
  { value: '', label: 'None' },
  { value: 'Daily', label: 'Daily' },
  { value: 'Weekly', label: 'Weekly' },
];

const Reminders = () => {
  const [reminders, setReminders] = useState([]);
  const [open, setOpen] = useState(false);
  const [form, setForm] = useState({ title: '', description: '', dateTime: '', repeatRule: '', type: 'Medication' });

  const fetchReminders = async () => {
    const res = await reminderService.getAll();
    setReminders(res.data || []);
  };

  useEffect(() => { fetchReminders(); }, []);

  const handleOpen = () => { setOpen(true); };
  const handleClose = () => { setOpen(false); setForm({ title: '', description: '', dateTime: '', repeatRule: '', type: 'Medication' }); };

  const handleChange = (e) => {
    setForm(f => ({ ...f, [e.target.name]: e.target.value }));
  };

  const handleSave = async () => {
    if (!form.title || !form.dateTime) return;
    const payload = { ...form, dateTime: new Date(form.dateTime).toISOString() };
    await reminderService.create(payload);
    fetchReminders();
    handleClose();
  };

  const handleDelete = async (id) => {
    await reminderService.delete(id);
    fetchReminders();
  };

  return (
    <Box sx={{ mt: 4, display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
      <Paper sx={{ p: 4, minWidth: 350, width: 500 }}>
        <Typography variant="h5" gutterBottom>Reminders</Typography>
        <Button variant="contained" startIcon={<Add />} onClick={handleOpen} sx={{ mb: 2 }}>Add Reminder</Button>
        <List>
          {reminders.map(rem => (
            <ListItem key={rem.id} secondaryAction={
              <IconButton edge="end" onClick={() => handleDelete(rem.id)}><Delete /></IconButton>
            }>
              <ListItemText primary={rem.title} secondary={`${rem.type} | ${rem.dateTime} | ${rem.repeatRule}`} />
            </ListItem>
          ))}
        </List>
      </Paper>
      <Dialog open={open} onClose={handleClose}>
        <DialogTitle>Add Reminder</DialogTitle>
        <DialogContent sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 1 }}>
          <TextField label="Title" name="title" value={form.title} onChange={handleChange} fullWidth />
          <TextField label="Description" name="description" value={form.description} onChange={handleChange} fullWidth />
          <TextField label="Date & Time" name="dateTime" type="datetime-local" value={form.dateTime} onChange={handleChange} fullWidth InputLabelProps={{ shrink: true }} />
          <TextField select label="Repeat" name="repeatRule" value={form.repeatRule} onChange={handleChange} fullWidth>
            {repeatOptions.map(opt => <MenuItem key={opt.value} value={opt.value}>{opt.label}</MenuItem>)}
          </TextField>
          <TextField select label="Type" name="type" value={form.type} onChange={handleChange} fullWidth>
            <MenuItem value="Medication">Medication</MenuItem>
            <MenuItem value="Appointment">Appointment</MenuItem>
          </TextField>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleClose}>Cancel</Button>
          <Button onClick={handleSave} variant="contained">Save</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default Reminders;
