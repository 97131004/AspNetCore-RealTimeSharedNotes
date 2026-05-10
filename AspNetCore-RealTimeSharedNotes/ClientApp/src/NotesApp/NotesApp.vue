<script setup>
import { ref, reactive, onMounted, onUnmounted } from 'vue';
import * as signalR from '@microsoft/signalr';
import { Notify } from '@/notifications.js';

const noteContentMaxLength = window.__noteContentMaxLength;

const notes       = ref([]);
const newContent  = ref('');
const isPosting   = ref(false);
const isLoading   = ref(false);
const isOnline    = ref(navigator.onLine);
const deletingIds = reactive(new Set());
let connection    = null;

function formatTime(iso) {
    const d = new Date(iso);
    return d.toLocaleString(undefined, {
        month: 'short', day: 'numeric',
        hour: '2-digit', minute: '2-digit'
    });
}

function canDelete(note) {
    if (window.__userRole === window.__roleSuperAdmin) {
        return true;
    }
    if (note.isOwn) {
        return true;
    }
    if (window.__userRole === window.__roleAdmin) {
        return note.authorRole === window.__roleUser;
    }
    return false;
}

async function connect() {
    isLoading.value = true;
    connection = new signalR.HubConnectionBuilder()
        .withUrl('/hubs/notes')
        .withAutomaticReconnect()
        .build();

    connection.on('LoadNotes', (data) => {
        notes.value = data;
        isLoading.value = false;
    });
    connection.on('NoteAdded', (note) => {
        notes.value.unshift(note);
    });
    connection.on('NoteRemoved', (noteId) => {
        notes.value = notes.value.filter(n => n.noteId !== noteId);
    });

    connection.onreconnecting(() => {
        isOnline.value = false;
        Notify.showOffline();
    });
    connection.onreconnected(() => {
        isOnline.value = true;
        Notify.hideOffline();
        Notify.show('reconnected', 'success');
    });
    connection.onclose(() => {
        isOnline.value = false;
        Notify.showOffline();
    });

    try {
        await connection.start();
        isOnline.value = true;
    } catch {
        isLoading.value = false;
        isOnline.value = false;
        Notify.showOffline();
    }
}

async function addNote() {
    if (isPosting.value || !newContent.value.trim()) {
        return;
    }
    isPosting.value = true;
    try {
        await connection.invoke('AddNote', newContent.value);
        newContent.value = '';
    } catch {
        Notify.show('failed to post note', 'error');
    } finally {
        isPosting.value = false;
    }
}

async function deleteNote(noteId) {
    if (deletingIds.has(noteId)) {
        return;
    }
    deletingIds.add(noteId);
    try {
        await connection.invoke('DeleteNote', noteId);
    } catch {
        Notify.show('failed to delete note', 'error');
    } finally {
        deletingIds.delete(noteId);
    }
}

function handleOnline() {
    isOnline.value = true;
    Notify.hideOffline();
    if (connection?.state === signalR.HubConnectionState.Disconnected) {
        connect();
    }
}

function handleOffline() {
    isOnline.value = false;
    Notify.showOffline();
}

onMounted(() => {
    connect();
    window.addEventListener('online', handleOnline);
    window.addEventListener('offline', handleOffline);
});

onUnmounted(() => {
    window.removeEventListener('online', handleOnline);
    window.removeEventListener('offline', handleOffline);
    connection?.stop();
});
</script>

<template>
    <div class="notes-wrap">
        <div class="notes-header">
            <h2>notes</h2>
        </div>

        <div class="note-compose">
            <textarea
                v-model="newContent"
                class="note-textarea"
                placeholder="write a note..."
                :maxlength="noteContentMaxLength"
                @keydown.ctrl.enter="addNote"
                data-testid="note-textarea"
            ></textarea>
            <div class="compose-actions">
                <span class="char-count">{{ newContent.length }} / {{ noteContentMaxLength }}</span>
                <button class="btn-primary" @click="addNote" data-testid="note-post-btn">post</button>
            </div>
        </div>

        <div class="notes-list" data-testid="notes-list">
            <p v-if="isLoading" class="loading-text" data-testid="notes-loading">loading...</p>
            <p v-if="!isLoading && notes.length === 0" class="notes-empty">no notes yet. be the first!</p>
            <div class="note-card" v-for="note in notes" :key="note.noteId" data-testid="note-card" :data-note-id="note.noteId">
                <div class="note-content" data-testid="note-content">{{ note.content }}</div>
                <div class="note-meta">
                    <span class="note-author">{{ note.userEmail }}</span>
                    <span class="note-role">{{ note.authorRole }}</span>
                    <span class="note-time">{{ formatTime(note.createdAt) }}</span>
                </div>
                <button v-if="canDelete(note)" class="btn-delete" @click="deleteNote(note.noteId)">
                    <span>✕</span>
                </button>
            </div>
        </div>
    </div>
</template>

<style>
.notes-wrap {
    max-width: 640px;
    margin: 0 auto;
    padding: 2.5rem 1.5rem;
}

.notes-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: 1.75rem;
}

.notes-header h2 {
    font-size: 1.25rem;
    font-weight: 700;
    color: #1a1a2e;
}

.note-compose {
    background: #fff;
    padding: 1rem 1.1rem;
    margin-bottom: 2rem;
}

.note-textarea {
    width: 100%;
    min-height: 90px;
    border: none;
    resize: vertical;
    font-size: 1rem;
    font-family: inherit;
    color: #1a1a2e;
    outline: none;
    background: #fff;
    line-height: 1.5;
}
.note-textarea::placeholder {
    color: #aab4c8;
}

.compose-actions {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-top: 0.75rem;
    padding-top: 0.75rem;
    border-top: 1px solid #eef1f7;
}

.char-count { font-size: 0.85rem; color: #9aa8c0; }

.notes-list {
    display: flex;
    flex-direction: column;
    gap: 0.65rem;
}

.notes-empty {
    font-size: 1rem;
    padding: 1rem 0;
    color: #9aa8c0;
}

.note-card {
    background: #fff;
    padding: 1rem 1.1rem 0.8rem;
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
    position: relative;
}

.note-content {
    font-size: 1rem;
    line-height: 1.6;
    white-space: pre-wrap;
    word-break: break-word;
    padding-right: 2.25rem;
    color: #1a1a2e;
}

.note-meta {
    display: flex;
    gap: 0.5rem;
    align-items: center;
    flex-wrap: wrap;
}

.note-author {
    font-size: 0.85rem;
    font-weight: 600;
    color: #3a5a8a;
}
.note-role {
    font-size: 0.78rem;
    color: #9aa8c0;
}
.note-time {
    font-size: 0.85rem;
    color: #b0bcd0;
}

.note-card .btn-delete {
    position: absolute;
    top: 0.75rem;
    right: 0.75rem;
}

@media (max-width: 600px) {
    .notes-wrap {
        padding: 1.5rem 1rem;
    }
}
</style>
