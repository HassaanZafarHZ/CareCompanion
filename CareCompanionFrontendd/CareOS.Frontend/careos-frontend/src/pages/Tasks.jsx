import React, { useEffect, useState } from 'react';
import { Box, Typography, Paper, Button, List, ListItem, ListItemText, ListItemSecondaryAction, IconButton, CircularProgress } from '@mui/material';
import { Add, CheckCircle, Delete } from '@mui/icons-material';
import { taskService } from '../services/taskService';

const Tasks = () => {
  const [tasks, setTasks] = useState([]);
  const [loading, setLoading] = useState(true);

  const fetchTasks = async () => {
    setLoading(true);
    try {
      const res = await taskService.getAll();
      setTasks(res.data.data || []);
    } catch (e) {
      setTasks([]);
    }
    setLoading(false);
  };

  useEffect(() => {
    fetchTasks();
  }, []);

  const handleComplete = async (id) => {
    await taskService.complete(id);
    fetchTasks();
  };

  const handleDelete = async (id) => {
    await taskService.delete(id);
    fetchTasks();
  };

  return (
    <Box sx={{ mt: 4, display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
      <Paper sx={{ p: 4, minWidth: 350, width: 500 }}>
        <Typography variant="h5" gutterBottom>My Tasks</Typography>
        {loading ? <CircularProgress /> : (
          <List>
            {tasks.length === 0 && <Typography>No tasks found.</Typography>}
            {tasks.map(task => (
              <ListItem key={task._id || task.id} divider>
                <ListItemText
                  primary={task.title}
                  secondary={task.description}
                  style={{ textDecoration: task.completed ? 'line-through' : 'none' }}
                />
                <ListItemSecondaryAction>
                  {!task.completed && (
                    <IconButton edge="end" color="success" onClick={() => handleComplete(task._id || task.id)}>
                      <CheckCircle />
                    </IconButton>
                  )}
                  <IconButton edge="end" color="error" onClick={() => handleDelete(task._id || task.id)}>
                    <Delete />
                  </IconButton>
                </ListItemSecondaryAction>
              </ListItem>
            ))}
          </List>
        )}
        {/* Add Task button can be implemented here */}
      </Paper>
    </Box>
  );
};

export default Tasks;
