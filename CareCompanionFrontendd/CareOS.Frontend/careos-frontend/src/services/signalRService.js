import * as signalR from '@microsoft/signalr';

const getApiBase = () => (process.env.REACT_APP_API_BASE || 'http://localhost:5204');

export const createConnection = (token) => {
  const conn = new signalR.HubConnectionBuilder()
    .withUrl(`${getApiBase()}/hubs/call`, {
      accessTokenFactory: () => token || localStorage.getItem('token') || ''
    })
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Warning)
    .build();

  // Optional helpers
  conn.onreconnecting((err) => console.warn('SignalR reconnecting', err));
  conn.onreconnected(() => console.info('SignalR reconnected'));
  conn.onclose((err) => console.info('SignalR connection closed', err));

  return conn;
};

const signalRService = {
  createConnection
};
export default signalRService;