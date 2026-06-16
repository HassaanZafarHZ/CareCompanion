import api from './api';

export const authService = {
  // Register
  register: (data) => api.post('/auth/register', data),
  // Login - role bhi bhejo
  login: (data) => api.post('/auth/login', data),
  // Logout
  logout: () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
  },
  // Get user from localStorage
  getUser: () => {
    try {
      return JSON.parse(localStorage.getItem('user'));
    } catch {
      return null;
    }
  },
};