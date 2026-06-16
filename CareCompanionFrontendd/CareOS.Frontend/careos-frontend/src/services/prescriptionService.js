import api from './api';

export const prescriptionService = {
  uploadWithChoice: (data) => api.post('/prescription/upload-with-choice', data),
  getPending: () => api.get('/prescription/pending-for-approval'),
  getById: (id) => api.get(`/prescription/${id}`),
  review: (id, data) => api.post(`/prescription/${id}/review`, data),
  addMedicine: (id, data) => api.post(`/prescription/${id}/add-medicine`, data),
  getMyPrescriptions: () => api.get('/prescription/my-prescriptions'),
  deletePrescription: (id) => api.delete(`/prescription/${id}`),
};