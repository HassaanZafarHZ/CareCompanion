import api from './api';

export const checkInService = {
  submit: (data) => api.post('/CheckIn/create', data, { headers: { 'Content-Type': 'application/json' } }),
  getToday: () => api.get('/CheckIn/today'),
  getHistory: () => api.get('/CheckIn/my-history'),
};
