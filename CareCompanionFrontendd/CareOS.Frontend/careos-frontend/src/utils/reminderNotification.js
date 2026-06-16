// Simple local notification utility for reminders
export function showReminderNotification(title, body) {
  if (window.Notification && Notification.permission === 'granted') {
    new Notification(title, { body });
  } else if (window.Notification && Notification.permission !== 'denied') {
    Notification.requestPermission().then(permission => {
      if (permission === 'granted') {
        new Notification(title, { body });
      }
    });
  }
}
