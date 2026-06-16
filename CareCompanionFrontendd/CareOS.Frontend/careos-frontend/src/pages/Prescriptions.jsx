import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Container,
  Paper,
  Typography,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Avatar,
  Fade,
  CircularProgress,
  IconButton
} from '@mui/material';
import {
  ArrowBack,
  Add,
  LocalHospital,
  AccessTime,
  CheckCircle,
  Cancel,
  Pending,
  Visibility
} from '@mui/icons-material';
import { prescriptionService } from '../services/prescriptionService';

const Prescriptions = () => {
  const navigate = useNavigate();
  const [prescriptions, setPrescriptions] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const user = JSON.parse(localStorage.getItem('user'));

  useEffect(() => {
    fetchPrescriptions();
  }, []);

  const fetchPrescriptions = async () => {
    try {
      setLoading(true);
      let response;
      
      if (user?.role === 'ELDER') {
        response = await prescriptionService.getMyPrescriptions();
      } else {
        response = await prescriptionService.getPending();
      }

      console.log('Prescriptions Response:', response.data);

      if (response.data.success) {
        setPrescriptions(response.data.data || []);
      }
    } catch (err) {
      setError('Failed to load prescriptions');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const getStatusChip = (status) => {
    const statusConfig = {
      PENDING: { color: 'warning', icon: <Pending />, label: 'Pending' },
      APPROVED: { color: 'success', icon: <CheckCircle />, label: 'Approved' },
      REJECTED: { color: 'error', icon: <Cancel />, label: 'Rejected' },
      MODIFIED: { color: 'info', icon: <CheckCircle />, label: 'Modified' }
    };
    const config = statusConfig[status] || statusConfig.PENDING;
    return (
      <Chip
        icon={config.icon}
        label={config.label}
        color={config.color}
        size="small"
      />
    );
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
            <IconButton onClick={() => navigate('/dashboard')} sx={{ color: 'white' }}>
              <ArrowBack />
            </IconButton>
            <Avatar sx={{ bgcolor: 'rgba(255,255,255,0.2)' }}>
              <LocalHospital />
            </Avatar>
            <Typography variant="h5" sx={{ color: 'white', fontWeight: 600 }}>
              {user?.role === 'ELDER' ? 'My Prescriptions' : 'Pending Approvals'}
            </Typography>
          </Box>
        </Container>
      </Paper>

      <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
        {user?.role === 'ELDER' && (
          <Fade in timeout={600}>
            <Button
              variant="contained"
              size="large"
              startIcon={<Add />}
              onClick={() => navigate('/prescription/upload')}
              sx={{
                mb: 3,
                py: 1.5,
                px: 4,
                borderRadius: 3,
                background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                boxShadow: '0 8px 32px rgba(102, 126, 234, 0.3)'
              }}
            >
              Upload New Prescription
            </Button>
          </Fade>
        )}

        {loading && (
          <Box sx={{ textAlign: 'center', py: 8 }}>
            <CircularProgress size={50} />
            <Typography sx={{ mt: 2 }}>Loading prescriptions...</Typography>
          </Box>
        )}

        {error && (
          <Paper sx={{ p: 3, textAlign: 'center', bgcolor: '#ffebee' }}>
            <Typography color="error">{error}</Typography>
          </Paper>
        )}

        {!loading && prescriptions.length === 0 && (
          <Paper sx={{ p: 6, textAlign: 'center', borderRadius: 4 }}>
            <LocalHospital sx={{ fontSize: 60, color: '#ccc', mb: 2 }} />
            <Typography variant="h6" color="text.secondary">
              No prescriptions found
            </Typography>
            {user?.role === 'ELDER' && (
              <Button
                variant="outlined"
                sx={{ mt: 2 }}
                onClick={() => navigate('/prescription/upload')}
              >
                Upload your first prescription
              </Button>
            )}
          </Paper>
        )}

        <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 3 }}>
          {prescriptions.map((prescription, index) => (
            <Fade in timeout={600 + index * 100} key={prescription.id || index}>
              <Card
                sx={{
                  width: { xs: '100%', md: 'calc(50% - 12px)' },
                  borderRadius: 4,
                  cursor: 'pointer',
                  transition: 'all 0.3s ease',
                  '&:hover': {
                    transform: 'translateY(-4px)',
                    boxShadow: '0 12px 40px rgba(0,0,0,0.12)'
                  }
                }}
                onClick={() => navigate(`/prescription/${prescription.id}`)}
              >
                <CardContent sx={{ p: 3 }}>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 2 }}>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                      <Avatar
                        sx={{
                          background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)'
                        }}
                      >
                        <LocalHospital />
                      </Avatar>
                      <Box>
                        <Typography variant="h6" sx={{ fontWeight: 600 }}>
                          {prescription.analysis?.doctorName || 'Unknown Doctor'}
                        </Typography>
                        <Typography variant="body2" color="text.secondary">
                          {prescription.elderName || 'Patient'}
                        </Typography>
                      </Box>
                    </Box>
                    {getStatusChip(prescription.status)}
                  </Box>

                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
                    <AccessTime fontSize="small" color="action" />
                    <Typography variant="body2" color="text.secondary">
                      {new Date(prescription.uploadedAt).toLocaleDateString()}
                    </Typography>
                  </Box>

                  {/* Medicines Preview */}
                  {prescription.analysis?.medicines?.length > 0 && (
                    <Box sx={{ mb: 2, p: 2, bgcolor: '#f5f5f5', borderRadius: 2 }}>
                      <Typography variant="caption" color="primary" sx={{ fontWeight: 600 }}>
                        💊 Medicines ({prescription.analysis.medicines.length}):
                      </Typography>
                      {prescription.analysis.medicines.slice(0, 3).map((med, i) => (
                        <Typography key={i} variant="body2" color="text.secondary">
                          • {med.medicineName} - {med.dosage} ({med.frequency})
                        </Typography>
                      ))}
                      {prescription.analysis.medicines.length > 3 && (
                        <Typography variant="caption" color="text.secondary">
                          +{prescription.analysis.medicines.length - 3} more...
                        </Typography>
                      )}
                    </Box>
                  )}

                  <Button
                    fullWidth
                    variant="outlined"
                    startIcon={<Visibility />}
                    sx={{ borderRadius: 2 }}
                  >
                    View Details
                  </Button>
                </CardContent>
              </Card>
            </Fade>
          ))}
        </Box>
      </Container>
    </Box>
  );
};

export default Prescriptions;