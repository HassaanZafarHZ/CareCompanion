import React, { useEffect, useState, useRef } from 'react';
import { Container, Paper, Typography, Box, TextField, Button, List, ListItem, Avatar, CircularProgress, MenuItem, Select, FormControl, InputLabel } from '@mui/material';
import SendIcon from '@mui/icons-material/Send';
import api from '../services/api';
import { assignmentService } from '../services/assignmentService';

const Chat = () => {
	const [messages, setMessages] = useState([]);
	const [newMessage, setNewMessage] = useState('');
	const [loading, setLoading] = useState(true);
	const [error, setError] = useState('');
	const [user, setUser] = useState(null);
	const [caretaker, setCaretaker] = useState(null); // For elder: assigned caretaker
	const [elders, setElders] = useState([]); // For caretaker: assigned elders
	const [selectedElderId, setSelectedElderId] = useState('');
	const messagesEndRef = useRef(null);

	useEffect(() => {
		const userData = localStorage.getItem('user');
		if (!userData) return;
		const parsedUser = JSON.parse(userData);
		setUser(parsedUser);
		// If user is elder, fetch their assigned caretaker
		if (parsedUser && parsedUser.role && parsedUser.role.toLowerCase() === 'elder') {
			assignmentService.getMyAssignment().then(res => {
				if (res?.data?.success && res.data.data) {
					setCaretaker(res.data.data.caretaker); // expects { id, name }
				}
			});
		} else if (parsedUser && parsedUser.role && parsedUser.role.toLowerCase() === 'caretaker') {
			assignmentService.getMyElders().then(res => {
				if (res?.data?.success && Array.isArray(res.data.data)) {
					// Map to { id, name }
					const mapped = res.data.data.map(e => ({ id: e.elderId, name: e.elderName }));
					setElders(mapped);
					if (mapped.length > 0) setSelectedElderId(mapped[0].id);
				}
			});
		}
		fetchMessages();
		// eslint-disable-next-line
	}, []);

	const fetchMessages = async () => {
		setLoading(true);
		setError('');
		try {
			const res = await api.get('/chat');
			setMessages(res?.data?.data || []);
		} catch {
			setError('Failed to load messages');
		} finally {
			setLoading(false);
			scrollToBottom();
		}
	};

	const handleSend = async (e) => {
		e.preventDefault();
		if (!newMessage.trim()) return;
		setError('');
		try {
			if (!user || !(user.id || user._id || user.Id)) {
				setError('User ID not found.');
				return;
			}
			const senderId = user.id || user._id || user.Id;
			let receiverId = null;
			if (user.role && user.role.toLowerCase() === 'elder') {
				if (!caretaker || !caretaker.id) {
					setError('No caretaker assigned.');
					return;
				}
				receiverId = caretaker.id;
			} else if (user.role && user.role.toLowerCase() === 'caretaker') {
				if (!selectedElderId) {
					setError('No elder selected.');
					return;
				}
				receiverId = selectedElderId;
			} else {
				setError('Chat not available for this role.');
				return;
			}
			// SendMessageDto expects: SenderId, ReceiverId, MessageText, MessageType
			const payload = {
				SenderId: senderId,
				ReceiverId: receiverId,
				MessageText: newMessage,
				MessageType: 'text'
			};
			const res = await api.post('/chat/send', payload);
			if (res.data.success) {
				setNewMessage('');
				fetchMessages();
			} else {
				setError(res.data.message || 'Failed to send message');
			}
		} catch {
			setError('Failed to send message');
		}
	};

	const scrollToBottom = () => {
		if (messagesEndRef.current) {
			messagesEndRef.current.scrollIntoView({ behavior: 'smooth' });
		}
	};

	useEffect(() => {
		scrollToBottom();
	}, [messages]);

	return (
		<Container maxWidth="sm" sx={{ mt: 4 }}>
			<Paper elevation={4} sx={{ p: 3, borderRadius: 4, minHeight: 500, display: 'flex', flexDirection: 'column' }}>
				<Typography variant="h4" align="center" gutterBottom sx={{ fontWeight: 700, color: '#3f51b5' }}>
					Chat
				</Typography>
				<Box sx={{ flex: 1, overflowY: 'auto', mb: 2, maxHeight: 350, bgcolor: '#f5f7fa', borderRadius: 2, p: 2 }}>
					{loading ? (
						<Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100%' }}>
							<CircularProgress />
						</Box>
					) : (
						<List>
							{/* Render messages in natural order (oldest to newest) */}
							  {messages.slice().reverse().map((msg, idx) => (
								<ListItem key={idx} alignItems={msg.senderId === user?.id ? 'right' : 'left'} sx={{
									display: 'flex',
									flexDirection: msg.senderId === user?.id ? 'row-reverse' : 'row',
									alignItems: 'flex-start',
									mb: 1
								}}>
									<Avatar sx={{ bgcolor: msg.senderId === user?.id ? '#3f51b5' : '#757575', ml: msg.senderId === user?.id ? 2 : 0, mr: msg.senderId === user?.id ? 0 : 2 }}>
										{msg.senderName ? msg.senderName[0] : '?'}
									</Avatar>
									<Box sx={{
										bgcolor: msg.senderId === user?.id ? '#e3f2fd' : '#fff',
										color: '#333',
										borderRadius: 3,
										px: 2,
										py: 1,
										maxWidth: '70%',
										boxShadow: 1
									}}>
										<Typography variant="body2" sx={{ fontWeight: 600 }}>{msg.senderName || 'User'}</Typography>
										<Typography variant="body1">{msg.messageText || msg.content}</Typography>
										<Typography variant="caption" sx={{ color: '#888' }}>{msg.createdAt ? new Date(msg.createdAt).toLocaleTimeString() : ''}</Typography>
									</Box>
								</ListItem>
							))}
							<div ref={messagesEndRef} />
						</List>
					)}
				</Box>
				{/* Show caretaker name for elder */}
				{user && user.role && user.role.toLowerCase() === 'elder' && caretaker && (
					<Typography align="center" sx={{ mb: 1, fontWeight: 500 }}>
						Sending to: <span style={{ color: '#3f51b5' }}>{caretaker.name}</span>
					</Typography>
				)}
				{/* Dropdown for caretaker to select elder */}
				{user && user.role && user.role.toLowerCase() === 'caretaker' && elders.length > 0 && (
					<FormControl fullWidth sx={{ mb: 2 }}>
						<InputLabel id="elder-select-label">Select Elder</InputLabel>
						<Select
							labelId="elder-select-label"
							value={selectedElderId}
							label="Select Elder"
							onChange={e => setSelectedElderId(e.target.value)}
						>
							{elders.map(e => (
								<MenuItem key={e.id} value={e.id}>{e.name}</MenuItem>
							))}
						</Select>
					</FormControl>
				)}
				{error && <Typography color="error" align="center">{error}</Typography>}
				<Box component="form" onSubmit={handleSend} sx={{ display: 'flex', gap: 2, mt: 2 }}>
					<TextField
						value={newMessage}
						onChange={e => setNewMessage(e.target.value)}
						placeholder="Type your message..."
						fullWidth
						variant="outlined"
						sx={{ borderRadius: 2 }}
					/>
					<Button type="submit" variant="contained" color="primary" endIcon={<SendIcon />} sx={{ borderRadius: 2, minWidth: 100 }}>
						Send
					</Button>
				</Box>
			</Paper>
		</Container>
	);
};

export default Chat;
