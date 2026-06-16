import api from './api';

export const chatService = {
  getConversation: (otherUserId, page = 1) => 
    api.get(`/chat/conversation/${otherUserId}?page=${page}`),
  sendMessage: (data) => api.post('/chat/send', data),
  getUnreadCount: () => api.get('/chat/unread-count'),
  markAsRead: (messageId) => api.post(`/chat/mark-read/${messageId}`),
  getRecentChats: () => api.get('/chat/recent'),
};