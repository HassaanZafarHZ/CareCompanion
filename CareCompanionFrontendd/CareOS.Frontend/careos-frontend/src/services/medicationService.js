import api from './api';

export const medicationService = {
  // For elders
  getAll: () => api.get('/medication/my-medications'),
  // For caretakers: get medications for a specific elder
  getElderMedications: (elderId) => api.get(`/medication/elder/${elderId}`),
  getById: (id) => api.get(`/medication/${id}`),
  // For caretakers: use /medication/create endpoint
  create: (data) => api.post('/medication/create', data),
  update: (id, data) => api.put(`/medication/${id}`, data),
  delete: (id) => api.delete(`/medication/${id}`),
  getSchedule: () => api.get('/medication/schedule'),
  // Elder: Mark medication as taken
  confirmTaken: (data) => api.post('/medication/confirm-taken', data),
};