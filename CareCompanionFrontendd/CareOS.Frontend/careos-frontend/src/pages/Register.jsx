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
  Avatar,
  Fade
} from '@mui/material';
import {
  Email,
  Lock,
  Pin,
  Person,
  Phone,
  HowToReg
} from '@mui/icons-material';

const Register = () => {
  const navigate = useNavigate();
  const [formData, setFormData] = useState({
    fullName: '',
    email: '',
    password: '',
    confirmPassword: '',
    pin: '',
    confirmPin: '',
    role: 'ELDER',
    phoneNumber: ''
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

    if (formData.role === 'ELDER') {
      if (formData.pin !== formData.confirmPin) {
        setError('PIN does not match');
        setLoading(false);
        return;
      }
      if (formData.pin.length !== 4) {
        setError('PIN must be exactly 4 digits');
        setLoading(false);
        return;
      }
    }

    if (formData.role === 'CARETAKER') {
      if (formData.password !== formData.confirmPassword) {
        setError('Passwords do not match');
        setLoading(false);
        return;
      }
      if (formData.password.length < 8) {
        setError('Password must be at least 8 characters');
        setLoading(false);
        return;
      }
    }

    try {
      const registerData = {
        fullName: formData.fullName,
        email: formData.email,
        role: formData.role,
        phoneNumber: formData.phoneNumber,
        pin: formData.role === 'ELDER' ? formData.pin : null,
        password: formData.role === 'CARETAKER' ? formData.password : 'DefaultPass123!'
      };

      const response = await authService.register(registerData);

      if (response.data.success) {
        localStorage.setItem('token', response.data.data.token);
        localStorage.setItem('user', JSON.stringify(response.data.data.user));
        navigate('/dashboard');
      } else {
        setError(response.data.message || 'Registration failed');
      }
    } catch (err) {
      setError(err.response?.data?.message || 'Registration failed. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Box
      sx={{
        minHeight: '100vh',
        background: 'linear-gradient(135deg, #11998e 0%, #38ef7d 100%)',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        padding: 2
      }}
    >
      <Fade in timeout={800}>
        <Container maxWidth="sm">
          <Paper
            elevation={24}
            sx={{
              p: 4,
              borderRadius: 4,
              backdropFilter: 'blur(10px)',
              background: 'rgba(255, 255, 255, 0.95)'
            }}
          >
            <Box sx={{ textAlign: 'center', mb: 3 }}>
              <Avatar
                sx={{
                  width: 70,
                  height: 70,
                  margin: '0 auto 12px',
                  background: 'linear-gradient(135deg, #11998e 0%, #38ef7d 100%)',
                  boxShadow: '0 8px 32px rgba(17, 153, 142, 0.4)'
                }}
              >
                <HowToReg sx={{ fontSize: 35 }} />
              </Avatar>
              <Typography
                variant="h3"
                sx={{
                  fontWeight: 900,
                  background: 'linear-gradient(135deg, #11998e 0%, #38ef7d 100%)',
                  backgroundClip: 'text',
                  WebkitBackgroundClip: 'text',
                  WebkitTextFillColor: 'transparent',
                  letterSpacing: 1.5
                }}
              >
                Care Companion
              </Typography>
              <Typography
                variant="h5"
                sx={{
                  fontWeight: 600,
                  color: '#11998e',
                  mt: 1,
                  letterSpacing: 1
                }}
              >
                Create Your Account
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
                Join the Care Companion System for better health and care.
              </Typography>
            </Box>

            {error && (
              <Alert severity="error" sx={{ mb: 2, borderRadius: 2 }}>
                {error}
              </Alert>
            )}

            <form onSubmit={handleSubmit}>
              <FormControl fullWidth sx={{ mb: 2 }}>
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
                label="Full Name"
                name="fullName"
                value={formData.fullName}
                onChange={handleChange}
                required
                sx={{ mb: 2 }}
                InputProps={{
                  startAdornment: <Person sx={{ mr: 1, color: 'action.active' }} />
                }}
              />

              <Box sx={{ display: 'flex', gap: 2, mb: 2 }}>
                <TextField
                  fullWidth
                  label="Email"
                  name="email"
                  type="email"
                  value={formData.email}
                  onChange={handleChange}
                  required
                  InputProps={{
                    startAdornment: <Email sx={{ mr: 1, color: 'action.active' }} />
                  }}
                />
                <TextField
                  fullWidth
                  label="Phone Number"
                  name="phoneNumber"
                  value={formData.phoneNumber}
                  onChange={handleChange}
                  required
                  InputProps={{
                    startAdornment: <Phone sx={{ mr: 1, color: 'action.active' }} />
                  }}
                />
              </Box>

              {formData.role === 'ELDER' && (
                <Box sx={{ display: 'flex', gap: 2, mb: 2 }}>
                  <TextField
                    fullWidth
                    label="Create PIN"
                    name="pin"
                    type="password"
                    value={formData.pin}
                    onChange={handleChange}
                    required
                    inputProps={{ maxLength: 4 }}
                    helperText="4 digits"
                    InputProps={{
                      startAdornment: <Pin sx={{ mr: 1, color: 'action.active' }} />
                    }}
                  />
                  <TextField
                    fullWidth
                    label="Confirm PIN"
                    name="confirmPin"
                    type="password"
                    value={formData.confirmPin}
                    onChange={handleChange}
                    required
                    inputProps={{ maxLength: 4 }}
                    InputProps={{
                      startAdornment: <Pin sx={{ mr: 1, color: 'action.active' }} />
                    }}
                  />
                </Box>
              )}

              {formData.role === 'CARETAKER' && (
                <Box sx={{ display: 'flex', gap: 2, mb: 2 }}>
                  <TextField
                    fullWidth
                    label="Password"
                    name="password"
                    type="password"
                    value={formData.password}
                    onChange={handleChange}
                    required
                    helperText="Min 8 characters"
                    InputProps={{
                      startAdornment: <Lock sx={{ mr: 1, color: 'action.active' }} />
                    }}
                  />
                  <TextField
                    fullWidth
                    label="Confirm Password"
                    name="confirmPassword"
                    type="password"
                    value={formData.confirmPassword}
                    onChange={handleChange}
                    required
                    InputProps={{
                      startAdornment: <Lock sx={{ mr: 1, color: 'action.active' }} />
                    }}
                  />
                </Box>
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
                  background: 'linear-gradient(135deg, #11998e 0%, #38ef7d 100%)',
                  boxShadow: '0 8px 32px rgba(17, 153, 142, 0.4)',
                  '&:hover': {
                    background: 'linear-gradient(135deg, #0f8a7e 0%, #2ed573 100%)',
                    boxShadow: '0 12px 40px rgba(17, 153, 142, 0.5)'
                  }
                }}
              >
                {loading ? (
                  <CircularProgress size={24} sx={{ color: 'white' }} />
                ) : (
                  '✨ Create Account'
                )}
              </Button>
            </form>

            <Box sx={{ textAlign: 'center', mt: 2 }}>
              <Typography variant="body2" color="text.secondary">
                Already have an account?{' '}
                <Button
                  onClick={() => navigate('/login')}
                  sx={{
                    fontWeight: 600,
                    color: '#11998e',
                    '&:hover': { background: 'rgba(17, 153, 142, 0.1)' }
                  }}
                >
                  Sign In
                </Button>
              </Typography>
            </Box>
          </Paper>
        </Container>
      </Fade>
    </Box>
  );
};

export default Register;