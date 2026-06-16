import React, { useState, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Container,
  Paper,
  Typography,
  Box,
  Button,
  IconButton,
  Avatar,
  Fade,
  CircularProgress,
  Alert,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Card
} from '@mui/material';
import {
  ArrowBack,
  CloudUpload,
  CameraAlt,
  CheckCircle
} from '@mui/icons-material';
import { prescriptionService } from '../services/prescriptionService';

const PrescriptionUpload = () => {
  const navigate = useNavigate();
  const fileInputRef = useRef(null);
  const [imagePreview, setImagePreview] = useState(null);
  const [scanMethod, setScanMethod] = useState('OCR');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState(false);
  const [result, setResult] = useState(null);
  const user = JSON.parse(localStorage.getItem('user'));

  const handleImageSelect = (e) => {
    const file = e.target.files[0];
    if (file) {
      const reader = new FileReader();
      reader.onloadend = () => {
        setImagePreview(reader.result);
      };
      reader.readAsDataURL(file);
    }
  };

  const handleUpload = async () => {
    if (!imagePreview) {
      setError('Please select an image first');
      return;
    }

    setLoading(true);
    setError('');

    try {
      const response = await prescriptionService.uploadWithChoice({
        base64Image: imagePreview,
        elderName: user?.fullName || 'Unknown',
        scanMethod: scanMethod
      });

      if (response.data.success) {
        setSuccess(true);
        setResult(response.data);
      } else {
        setError(response.data.message || 'Upload failed');
      }
    } catch (err) {
      setError(err.response?.data?.message || 'Upload failed. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Box
      sx={{
        minHeight: '100vh',
        background: 'linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%)'
      }}
    >
      <Paper
        elevation={0}
        sx={{
          background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
          borderRadius: 0,
          py: 2,
          px: 3
        }}
      >
        <Container maxWidth="lg">
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <IconButton onClick={() => navigate('/prescriptions')} sx={{ color: 'white' }}>
              <ArrowBack />
            </IconButton>
            <Avatar sx={{ bgcolor: 'rgba(255,255,255,0.2)' }}>
              <CloudUpload />
            </Avatar>
            <Typography variant="h5" sx={{ color: 'white', fontWeight: 600 }}>
              Upload Prescription
            </Typography>
          </Box>
        </Container>
      </Paper>

      <Container maxWidth="sm" sx={{ mt: 4, mb: 4 }}>
        <Fade in timeout={600}>
          <Paper sx={{ p: 4, borderRadius: 4 }}>
            {success ? (
              <Box sx={{ textAlign: 'center', py: 4 }}>
                <Avatar
                  sx={{
                    width: 80,
                    height: 80,
                    margin: '0 auto 16px',
                    bgcolor: '#4caf50'
                  }}
                >
                  <CheckCircle sx={{ fontSize: 50 }} />
                </Avatar>
                <Typography variant="h5" sx={{ fontWeight: 600, mb: 2 }}>
                  Prescription Uploaded!
                </Typography>
                <Typography color="text.secondary" sx={{ mb: 3 }}>
                  Your prescription has been scanned and sent for approval.
                </Typography>
                
                {result?.analysis && (
                  <Card sx={{ p: 2, mb: 3, bgcolor: '#f5f5f5', textAlign: 'left' }}>
                    <Typography variant="subtitle2" color="primary">
                      Extracted Information:
                    </Typography>
                    <Typography variant="body2">
                      👨‍⚕️ Doctor: {result.analysis.doctorName || 'N/A'}
                    </Typography>
                    <Typography variant="body2">
                      💊 Medicines: {result.analysis.medicines?.length || 0} found
                    </Typography>
                  </Card>
                )}

                <Button
                  variant="contained"
                  onClick={() => navigate('/prescriptions')}
                  sx={{
                    borderRadius: 3,
                    background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)'
                  }}
                >
                  View My Prescriptions
                </Button>
              </Box>
            ) : (
              <>
                {error && (
                  <Alert severity="error" sx={{ mb: 3, borderRadius: 2 }}>
                    {error}
                  </Alert>
                )}

                <Box
                  sx={{
                    border: '2px dashed #ccc',
                    borderRadius: 4,
                    p: 4,
                    textAlign: 'center',
                    cursor: 'pointer',
                    transition: 'all 0.3s ease',
                    '&:hover': {
                      borderColor: '#667eea',
                      bgcolor: 'rgba(102, 126, 234, 0.05)'
                    }
                  }}
                  onClick={() => fileInputRef.current?.click()}
                >
                  {imagePreview ? (
                    <Box>
                      <img
                        src={imagePreview}
                        alt="Preview"
                        style={{
                          maxWidth: '100%',
                          maxHeight: 300,
                          borderRadius: 8
                        }}
                      />
                      <Typography variant="body2" sx={{ mt: 2 }} color="text.secondary">
                        Click to change image
                      </Typography>
                    </Box>
                  ) : (
                    <Box>
                      <Avatar
                        sx={{
                          width: 80,
                          height: 80,
                          margin: '0 auto 16px',
                          bgcolor: 'rgba(102, 126, 234, 0.1)'
                        }}
                      >
                        <CameraAlt sx={{ fontSize: 40, color: '#667eea' }} />
                      </Avatar>
                      <Typography variant="h6" sx={{ mb: 1 }}>
                        Upload Prescription Image
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        Click to select your prescription
                      </Typography>
                    </Box>
                  )}
                </Box>

                <input
                  type="file"
                  ref={fileInputRef}
                  onChange={handleImageSelect}
                  accept="image/*"
                  style={{ display: 'none' }}
                />

                <FormControl fullWidth sx={{ mt: 3 }}>
                  <InputLabel>Scan Method</InputLabel>
                  <Select
                    value={scanMethod}
                    onChange={(e) => setScanMethod(e.target.value)}
                    label="Scan Method"
                  >
                    <MenuItem value="OCR">🔍 Local OCR (Fast & Free)</MenuItem>
                    <MenuItem value="GEMINI">🤖 Gemini AI (More Accurate)</MenuItem>
                  </Select>
                </FormControl>

                <Button
                  fullWidth
                  variant="contained"
                  size="large"
                  disabled={!imagePreview || loading}
                  onClick={handleUpload}
                  sx={{
                    mt: 3,
                    py: 1.5,
                    borderRadius: 3,
                    background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)'
                  }}
                >
                  {loading ? (
                    <CircularProgress size={24} sx={{ color: 'white' }} />
                  ) : (
                    '📤 Upload & Scan'
                  )}
                </Button>
              </>
            )}
          </Paper>
        </Fade>
      </Container>
    </Box>
  );
};

export default PrescriptionUpload;