import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { ThemeProvider, CssBaseline } from '@mui/material';
import theme from './theme';
import Login from './pages/Login';
import Register from './pages/Register';
import Dashboard from './pages/Dashboard';
import Prescriptions from './pages/Prescriptions';
import PrescriptionUpload from './pages/PrescriptionUpload';
import PrescriptionView from './pages/PrescriptionView';
import SelectCaretaker from './pages/SelectCaretaker';
import PendingRequests from './pages/PendingRequests';
import Emergency from './pages/Emergency';
import Health from './pages/Health';
import Activity from './pages/Activity';
// import Note from './pages/Note';
import Chat from './pages/Chat';
import MyElders from './pages/MyElders';
import MyCaretakers from './pages/MyCaretakers';
import Medications from './pages/Medications';
import Call from './pages/Call';
import Tasks from './pages/Tasks';
import Reminders from './pages/Reminders';
import CheckIn from './pages/CheckIn';
const ProtectedRoute = ({ children }) => {
  const token = localStorage.getItem('token');
  if (!token) return <Navigate to="/login" />;
  return children;
};

function App() {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <Router>
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          <Route path="/dashboard" element={<ProtectedRoute><Dashboard /></ProtectedRoute>} />
          <Route path="/prescriptions" element={<ProtectedRoute><Prescriptions /></ProtectedRoute>} />
          <Route path="/prescription/upload" element={<ProtectedRoute><PrescriptionUpload /></ProtectedRoute>} />
          <Route path="/prescription/:id" element={<ProtectedRoute><PrescriptionView /></ProtectedRoute>} />
          <Route path="/select-caretaker" element={<ProtectedRoute><SelectCaretaker /></ProtectedRoute>} />
          <Route path="/pending-requests" element={<ProtectedRoute><PendingRequests /></ProtectedRoute>} />
          <Route path="/health" element={<ProtectedRoute><Health /></ProtectedRoute>} />
          <Route path="/activity" element={<ProtectedRoute><Activity /></ProtectedRoute>} />
          {/* <Route path="/note" element={<ProtectedRoute><Note /></ProtectedRoute>} /> */}
          <Route path="/emergency" element={<ProtectedRoute><Emergency /></ProtectedRoute>} />
          <Route path="/chat" element={<ProtectedRoute><Chat /></ProtectedRoute>} />
          <Route path="/my-elders" element={<ProtectedRoute><MyElders /></ProtectedRoute>} />
          <Route path="/my-caretakers" element={<ProtectedRoute><MyCaretakers /></ProtectedRoute>} />
          <Route path="/medications" element={<ProtectedRoute><Medications /></ProtectedRoute>} />
          <Route path="/call" element={<ProtectedRoute><Call /></ProtectedRoute>} />
          <Route path="/tasks" element={<ProtectedRoute><Tasks /></ProtectedRoute>} />
          <Route path="/reminders" element={<ProtectedRoute><Reminders /></ProtectedRoute>} />
          <Route path="/checkin" element={<ProtectedRoute><CheckIn /></ProtectedRoute>} />
          <Route path="/" element={<Navigate to="/login" />} />
          <Route path="*" element={<Navigate to="/login" />} />
        </Routes>
      </Router>
    </ThemeProvider>
  );
}

export default App;