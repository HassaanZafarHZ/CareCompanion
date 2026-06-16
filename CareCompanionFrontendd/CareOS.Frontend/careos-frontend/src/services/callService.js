import api from './api';

export const callService = {
  initiateCall: (data) => api.post('/call/initiate', data),
  endCall: (callId) => api.post(`/call/${callId}/end`),
  getCallHistory: () => api.get('/call/history'),
  getCallById: (id) => api.get(`/call/${id}`),
};