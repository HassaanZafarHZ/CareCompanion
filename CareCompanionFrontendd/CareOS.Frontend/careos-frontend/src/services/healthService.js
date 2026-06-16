import api from './api';

const healthService = {
  recordBloodPressure: (systolic, diastolic) =>
    api.post('/Health/bp/record', { systolic, diastolic }),
  getBPHistory: (userId) => api.get(`/Health/bp/history/${userId}`),
  getLatestBP: () => api.get('/Health/bp/latest'),
  getElderBPHistory: (elderId) => api.get(`/Health/bp/elder/${elderId}/history`),
  getElderLatestBP: (elderId) => api.get(`/Health/bp/elder/${elderId}/latest`),
};

export default healthService;