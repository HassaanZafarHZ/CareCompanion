import React, { useRef, useState, useEffect } from 'react';
import { Button, Typography, Box, Paper, Select, MenuItem, Dialog, DialogTitle, DialogContent, DialogActions } from '@mui/material';
import signalRService from '../services/signalRService';
import { assignmentService } from '../services/assignmentService';

// Basic WebRTC + SignalR audio call UI
const Call = () => {
      // SignalR event handlers
      useEffect(() => {
        let conn;
        let mounted = true;
        (async () => {
          conn = await getOrCreateConn();
          // Incoming call notification
          conn.on("IncomingCall", (data) => {
            if (!mounted) return;
            setIncomingCall({
              callId: data.callId,
              callerName: data.callerName,
              callerId: data.callerId || data.from || data.callerId,
              offer: data.offer || null,
            });
          });
          // Receive offer (callee)
          conn.on("ReceiveOffer", async (offer) => {
            if (!mounted) return;
            setIncomingCall((prev) => ({ ...prev, offer }));
          });
          // Receive answer (caller)
          conn.on("ReceiveAnswer", async (answer) => {
            if (!mounted) return;
            if (pcRef.current) {
              const parsedAnswer = typeof answer === 'string' ? JSON.parse(answer) : answer;
              await pcRef.current.setRemoteDescription(new RTCSessionDescription(parsedAnswer));
            }
          });
          // Receive ICE candidate
          conn.on("ReceiveIceCandidate", async (candidate) => {
            if (!mounted) return;
            if (pcRef.current) {
              const parsedCandidate = typeof candidate === 'string' ? JSON.parse(candidate) : candidate;
              try {
                await pcRef.current.addIceCandidate(new RTCIceCandidate(parsedCandidate));
              } catch (e) {
                // ignore
              }
            }
          });

          // Call ended by other user
          conn.on("CallEnded", () => {
            endCall();
          });
        })();
        return () => {
          mounted = false;
          if (conn) {
            conn.off("IncomingCall");
            conn.off("ReceiveOffer");
            conn.off("ReceiveAnswer");
            conn.off("ReceiveIceCandidate");
            conn.off("CallEnded");
          }
        };
      }, []);
    // Ensure getOrCreateConn is defined before use
    const getOrCreateConn = async () => {
      if (connRef.current) return connRef.current;
      const conn = signalRService.createConnection(localStorage.getItem('token'));
      connRef.current = conn;
      await conn.start();
      return conn;
    };
  const [inCall, setInCall] = useState(false);
  const [incomingCall, setIncomingCall] = useState(null); // { callerId, callerName, callId }
  const [callAccepted, setCallAccepted] = useState(false);
  const [remoteUser, setRemoteUser] = useState('');
  const [elders, setElders] = useState([]); // For caretakers
  const [caretaker, setCaretaker] = useState(null); // For elders
  const [myStream, setMyStream] = useState(null);
  const localAudio = useRef();
  const remoteAudio = useRef();
  const pcRef = useRef();
  const connRef = useRef();

  // Dummy user role logic (replace with real auth)
  const user = JSON.parse(localStorage.getItem('user') || '{}');

  // Fetch assigned users on mount
  useEffect(() => {
    if (user.role === 'CARETAKER') {
      assignmentService.getMyElders().then(res => {
        setElders(res.data.data || []);
      });
    } else if (user.role === 'ELDER') {
      assignmentService.getMyAssignment().then(res => {
        console.log('API /assignment/my-assignment:', res.data);
        setCaretaker(res.data.data?.caretaker || null);
      });
    }
  }, [user.role]);

  // SignalR connection (shared for both caller and callee)


  // Accept incoming call
  const acceptCall = async () => {
    if (!incomingCall) return;
    setCallAccepted(true);
    setInCall(true);
    const conn = await getOrCreateConn();
    const pc = new RTCPeerConnection();
    pcRef.current = pc;
    const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
    setMyStream(stream);
    if (localAudio.current) localAudio.current.srcObject = stream;
    stream.getTracks().forEach(track => pc.addTrack(track, stream));
    const parsedOffer = typeof incomingCall.offer === 'string' ? JSON.parse(incomingCall.offer) : incomingCall.offer;
    await pc.setRemoteDescription(new RTCSessionDescription(parsedOffer));
    const answer = await pc.createAnswer();
    await pc.setLocalDescription(answer);
    await conn.invoke('SendAnswer', incomingCall.callerId, JSON.stringify(answer));
    pc.ontrack = event => {
      if (remoteAudio.current) remoteAudio.current.srcObject = event.streams[0];
    };
    setIncomingCall(null);
  };

  // Reject incoming call
  const rejectCall = async () => {
    if (!incomingCall) return;
    const conn = await getOrCreateConn();
    await conn.invoke('NotifyCallDeclined', incomingCall.callerId, incomingCall.callId);
    setIncomingCall(null);
  };
  const startCall = async () => {
    const pc = new RTCPeerConnection();
    pcRef.current = pc;
    const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
    setMyStream(stream);
    stream.getTracks().forEach(track => pc.addTrack(track, stream));
    localAudio.current.srcObject = stream;
    // SignalR connection
    const conn = await getOrCreateConn();
    // Ensure connection is started
    if (conn.state !== 'Connected') {
      try {
        await conn.start();
      } catch (e) {
        alert('Failed to connect to call server.');
        return;
      }
    }
    pc.onicecandidate = (e) => {
      if (e.candidate) conn.invoke('SendIceCandidate', remoteUser, JSON.stringify(e.candidate));
    };
    pc.ontrack = (e) => {
      remoteAudio.current.srcObject = e.streams[0];
    };
    // Notify callee of incoming call
    const callId = Math.random().toString(36).substring(2, 15);
    await conn.invoke('NotifyIncomingCall', remoteUser, callId, 'audio', user.fullName || user.email || user.id);
    // Create offer
    const offer = await pc.createOffer();
    await pc.setLocalDescription(offer);
    await conn.invoke('SendOffer', remoteUser, JSON.stringify(offer));
    setInCall(true);
    // Accept incoming call
    const acceptCall = async () => {
      setCallAccepted(true);
      setInCall(true);
      setIncomingCall(null);
      const conn = await getOrCreateConn();
      const pc = new RTCPeerConnection();
      pcRef.current = pc;
      const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
      setMyStream(stream);
      stream.getTracks().forEach(track => pc.addTrack(track, stream));
      localAudio.current.srcObject = stream;
      pc.onicecandidate = (e) => {
        if (e.candidate) conn.invoke('SendIceCandidate', incomingCall.callerId, JSON.stringify(e.candidate));
      };
      pc.ontrack = (e) => {
        remoteAudio.current.srcObject = e.streams[0];
      };
      // Set remote offer
      const parsedOffer = typeof incomingCall.offer === 'string' ? JSON.parse(incomingCall.offer) : incomingCall.offer;
      await pc.setRemoteDescription(new RTCSessionDescription(parsedOffer));
      const answer = await pc.createAnswer();
      await pc.setLocalDescription(answer);
      await conn.invoke('SendAnswer', incomingCall.callerId, JSON.stringify(answer));
      // Notify caller
      await conn.invoke('NotifyCallAccepted', incomingCall.callerId, incomingCall.callId);
    };

    // Reject incoming call
    const rejectCall = async () => {
      setIncomingCall(null);
      setCallAccepted(false);
      const conn = await getOrCreateConn();
      await conn.invoke('NotifyCallDeclined', incomingCall.callerId, incomingCall.callId);
    };
  };

  // End call
  const endCall = () => {
    if (pcRef.current) pcRef.current.close();
    if (myStream) myStream.getTracks().forEach(t => t.stop());
    setInCall(false);
    setMyStream(null);
    setCallAccepted(false);
    setIncomingCall(null);
  };

  return (
    <>
      {/* Incoming call dialog */}
      <Dialog open={!!incomingCall} onClose={rejectCall}>
        <DialogTitle>Incoming Call</DialogTitle>
        <DialogContent>
          <Typography>{incomingCall?.callerName || 'Unknown'} is calling you...</Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={rejectCall} color="error">Reject</Button>
          <Button onClick={acceptCall} color="primary">Accept</Button>
        </DialogActions>
      </Dialog>
      <Box sx={{ mt: 4, display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
        <Paper sx={{ p: 4, minWidth: 350 }}>
          <Typography variant="h5" gutterBottom>Audio Call</Typography>
          {!inCall && (
            <>
              <Select
                value={remoteUser}
                onChange={e => setRemoteUser(e.target.value)}
                displayEmpty
                fullWidth
                sx={{ mb: 2 }}
              >
                <MenuItem value="">{user.role === 'ELDER' ? 'Select Caretaker' : 'Select Elder'}</MenuItem>
                {user.role === 'CARETAKER' && elders.map(e => (
                  <MenuItem key={e.elderId || e._id || e.id} value={e.elderId || e._id || e.id}>
                    {e.elderName || e.fullName || e.name || e.email}
                  </MenuItem>
                ))}
                {user.role === 'ELDER' && caretaker && (
                  <MenuItem value={caretaker.id}>
                    {caretaker.name}
                  </MenuItem>
                )}
              </Select>
              <Button variant="contained" color="primary" onClick={startCall} disabled={!remoteUser}>Start Call</Button>
            </>
          )}
          {inCall && (
            <>
              <Typography>Call in progress...</Typography>
              <Button variant="contained" color="error" onClick={endCall} sx={{ mt: 2 }}>End Call</Button>
            </>
          )}
          <Box sx={{ mt: 3 }}>
            <audio ref={localAudio} autoPlay muted />
            <audio ref={remoteAudio} autoPlay />
          </Box>
        </Paper>
      </Box>
    </>
  );
};

export default Call;
