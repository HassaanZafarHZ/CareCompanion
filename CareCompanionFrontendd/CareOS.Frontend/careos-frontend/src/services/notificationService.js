import api from './api';

export const notificationService = {
  getAll: () => api.get('/notification'),
  markAsRead: (id) => api.put(`/notification/${id}/read`),
  getUnreadCount: () => api.get('/notification/unread-count'),
};