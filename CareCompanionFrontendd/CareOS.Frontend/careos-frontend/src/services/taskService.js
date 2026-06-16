import api from './api';

export const taskService = {
  getAll: () => api.get('/task'),
  getById: (id) => api.get(`/task/${id}`),
  create: (data) => api.post('/task', data),
  update: (id, data) => api.put(`/task/${id}`, data),
  delete: (id) => api.delete(`/task/${id}`),
  complete: (id) => api.post(`/task/${id}/complete`),
};