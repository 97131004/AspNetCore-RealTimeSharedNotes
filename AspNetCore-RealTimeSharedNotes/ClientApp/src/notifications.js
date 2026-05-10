// standalone entry loaded by _Layout so window.Notify is available on every page
import './notifications.css';

// notification logic — also exposes window.Notify for inline Razor scripts
const getContainer = () => document.getElementById('notifications-container');
let offlineEl = null;

function show(message, type = 'info', permanent = false) {
    const el = document.createElement('div');
    el.className = `toast ${type}`;
    el.textContent = message;
    getContainer().appendChild(el);
    if (permanent) {
        return el;
    }
    const duration = type === 'error' ? 5000 : 3500;
    setTimeout(() => dismiss(el), duration);
    return el;
}

function dismiss(el) {
    if (!el || !el.parentNode) {
        return;
    }
    el.remove();
}

function showOffline() {
    if (offlineEl) {
        return;
    }
    offlineEl = show('internet offline - live updates paused', 'offline', true);
}

function hideOffline() {
    if (!offlineEl) {
        return;
    }
    dismiss(offlineEl);
    offlineEl = null;
}

export const Notify = { show, dismiss, showOffline, hideOffline };
window.Notify = Notify;
