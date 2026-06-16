import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { authService } from '../services/authService';
import {
  Container,
  Paper,
  TextField,
  Button,
  Typography,
  Box,
  Alert,
  CircularProgress,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Fade
} from '@mui/material';
import {
  Email,
  Lock,
  Pin
} from '@mui/icons-material';

const Login = () => {
  const navigate = useNavigate();
  function isTokenExpired(token) {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      if (!payload.exp) return false;
      return Date.now() >= payload.exp * 1000;
    } catch {
      return false;
    }
  }
  // ...existing code...
  const [formData, setFormData] = useState({
    email: '',
    password: '',
    pin: '',
    role: 'ELDER'
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value
    });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError('');

    try {
      let loginData = {
        email: formData.email,
        role: formData.role
      };

      if (formData.role === 'ELDER') {
        loginData.pin = formData.pin;
        loginData.password = '';
      }
      
      if (formData.role === 'CARETAKER') {
        loginData.password = formData.password;
        loginData.pin = '';
      }

      const response = await authService.login(loginData);
      
      if (response.data.success) {
        // Robust token extraction for all possible response shapes
        const token = response.data.data?.token || response.data.token || response.data.Token;
        const user = response.data.data?.user || response.data.user || response.data.User;
        if (!token) {
          setError('Login response me token nahi mila. Backend response structure check karein.');
          return;
        }
        localStorage.setItem('token', token);
        localStorage.setItem('user', JSON.stringify(user));
        navigate('/dashboard');
      } else {
        setError(response.data.message || 'Login failed');
      }
    } catch (err) {
      setError(err.response?.data?.message || 'Login failed. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Box
      sx={{
        minHeight: '100vh',
        background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        padding: 2
      }}
    >
      <Fade in timeout={800}>
        <Container maxWidth="sm">
          <Paper elevation={6} sx={{ p: 4, borderRadius: 4 }}>
            {error && (
              <Alert severity="error" sx={{ mb: 3, borderRadius: 2 }}>
                {error}
              </Alert>
            )}

            <form onSubmit={handleSubmit}>
              <FormControl fullWidth sx={{ mb: 3 }}>
                <InputLabel>I am a</InputLabel>
                <Select
                  name="role"
                  value={formData.role}
                  onChange={handleChange}
                  label="I am a"
                >
                  <MenuItem value="ELDER">👴 Elder (Patient)</MenuItem>
                  <MenuItem value="CARETAKER">👨‍⚕️ Caretaker</MenuItem>
                </Select>
              </FormControl>

              <TextField
                fullWidth
                label="Email Address"
                name="email"
                type="email"
                value={formData.email}
                onChange={handleChange}
                required
                sx={{ mb: 3 }}
                InputProps={{
                  startAdornment: <Email sx={{ mr: 1, color: 'action.active' }} />
                }}
              />

              {formData.role === 'ELDER' && (
                <TextField
                  fullWidth
                  label="Enter your 4-digit PIN"
                  name="pin"
                  type="password"
                  value={formData.pin}
                  onChange={handleChange}
                  required
                  sx={{ mb: 3 }}
                  inputProps={{ maxLength: 4, autoComplete: 'current-password' }}
                  InputProps={{
                    startAdornment: <Pin sx={{ mr: 1, color: 'action.active' }} />
                  }}
                  autoComplete="current-password"
                />
              )}

              {formData.role === 'CARETAKER' && (
                <TextField
                  fullWidth
                  label="Password"
                  name="password"
                  type="password"
                  value={formData.password}
                  onChange={handleChange}
                  required
                  autoComplete="current-password"
                  sx={{ mb: 3 }}
                  InputProps={{
                    startAdornment: <Lock sx={{ mr: 1, color: 'action.active' }} />
                  }}
                />
              )}

              <Button
                type="submit"
                fullWidth
                variant="contained"
                size="large"
                disabled={loading}
                sx={{
                  py: 1.5,
                  fontSize: '1.1rem',
                  background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                  boxShadow: '0 8px 32px rgba(102, 126, 234, 0.4)',
                  '&:hover': {
                    background: 'linear-gradient(135deg, #5a67d8 0%, #6b46c1 100%)',
                    boxShadow: '0 12px 40px rgba(102, 126, 234, 0.5)'
                  }
                }}
              >
                {loading ? (
                  <CircularProgress size={24} sx={{ color: 'white' }} />
                ) : (
                  '🔐 Sign In'
                )}
              </Button>
            </form>

            <Box sx={{ textAlign: 'center', mt: 3 }}>
              <Typography variant="body2" color="text.secondary">
                Don't have an account?{' '}
                <Button
                  onClick={() => navigate('/register')}
                  sx={{
                    fontWeight: 600,
                    color: '#667eea',
                    '&:hover': { background: 'rgba(102, 126, 234, 0.1)' }
                  }}
                >
                  Sign Up
                </Button>
              </Typography>
            </Box>
          </Paper>
        </Container>
      </Fade>
    </Box>
  );
};

export default Login;