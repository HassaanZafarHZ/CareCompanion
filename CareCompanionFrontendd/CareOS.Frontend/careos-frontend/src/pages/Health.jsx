import React, { useEffect, useState } from 'react';
import healthService from '../services/healthService';
import { Container, Typography, Paper, List, ListItem, ListItemText } from '@mui/material';

const Health = () => {
  const [records, setRecords] = useState([]);

  useEffect(() => {
    const fetchHealthRecords = async () => {
      try {
        const res = await healthService.getAll();
        setRecords(res.data.data || []);
      } catch (err) {
        // error handling
      }
    };
    fetchHealthRecords();
  }, []);

  return (
    <Container maxWidth="md" sx={{ mt: 4 }}>
      <Paper elevation={3} sx={{ p: 4, borderRadius: 4 }}>
        <Typography variant="h4" gutterBottom>Health Records</Typography>
        <List>
          {records.map((rec, idx) => (
            <ListItem key={idx} divider>
              <ListItemText
                primary={`Date: ${rec.date || rec.createdAt || ''}`}
                secondary={`BP: ${rec.bp || ''} | Sugar: ${rec.sugar || ''} | Notes: ${rec.notes || ''}`}
              />
            </ListItem>
          ))}
        </List>
      </Paper>
    </Container>
  );
};

export default Health;