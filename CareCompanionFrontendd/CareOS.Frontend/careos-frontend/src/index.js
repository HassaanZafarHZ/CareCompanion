import React from 'react';
import ReactDOM from 'react-dom/client';
import './index.css';
import App from './App';
import reportWebVitals from './reportWebVitals';

const root = ReactDOM.createRoot(document.getElementById('root'));
root.render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
);

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals();

// Register service worker and request notification permission
if ('serviceWorker' in navigator && 'PushManager' in window) {
  window.addEventListener('load', () => {
    navigator.serviceWorker.register('/service-worker.js')
      .then(registration => {
        console.log('Service Worker registered:', registration);
        // Request notification permission
        if (Notification.permission !== 'granted') {
          Notification.requestPermission().then(permission => {
            if (permission === 'granted') {
              console.log('Notification permission granted.');
              // setupPushSubscription();
            }
          });
        } else if (Notification.permission === 'granted') {
          // setupPushSubscription();
        }
      })
      .catch(error => {
        console.error('Service Worker registration failed:', error);
      });
  });
}
