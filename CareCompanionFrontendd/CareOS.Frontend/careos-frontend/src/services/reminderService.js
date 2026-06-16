import api from './api';

export const reminderService = {
  getAll: () => api.get('/reminder'),
  create: (data) => api.post('/reminder', data),
  update: (id, data) => api.put(`/reminder/${id}`, data),
  delete: (id) => api.delete(`/reminder/${id}`),
};
