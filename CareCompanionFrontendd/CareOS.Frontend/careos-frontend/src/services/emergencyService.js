import api from './api';

export const emergencyService = {
  sendAlert: (data) => api.post('/emergency/alert', data),
  getAlerts: () => api.get('/emergency/alerts'),
  respondToAlert: (alertId) => api.post(`/emergency/${alertId}/respond`),
  resolveAlert: (alertId) => api.post(`/emergency/${alertId}/resolve`),
};