import api from './api';

export const assignmentService = {
  // Available caretakers list
  getAvailableCaretakers: () => api.get('/assignment/available-caretakers'),
  
  // Elder: Send request to caretaker
  sendRequest: (data) => api.post('/assignment/request', data),
  
  // Elder: See my sent requests status
  getMySentRequests: () => api.get('/assignment/my-sent-requests'),
  
  // Elder: My assigned caretaker (only if approved)
  getMyAssignment: () => api.get('/assignment/my-assignment'),
  
  // Caretaker: Pending requests from elders
  getPendingRequests: () => api.get('/assignment/pending-requests'),
  
  // Caretaker: Approve request
  approveRequest: (id) => api.post(`/assignment/${id}/approve`),
  
  // Caretaker: Reject request
  rejectRequest: (id) => api.post(`/assignment/${id}/reject`),
  
  // Caretaker: My assigned elders (only approved)
  getMyElders: () => api.get('/assignment/my-elders'),
  
  // Remove assignment
  removeAssignment: (id) => api.delete(`/assignment/${id}`),
};