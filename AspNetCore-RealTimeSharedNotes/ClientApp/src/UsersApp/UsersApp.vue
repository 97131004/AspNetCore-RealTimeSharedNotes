<script setup>
import { ref, reactive, onMounted } from 'vue';
import { Notify } from '@/notifications.js';

const isSuperAdmin = window.__userRole === window.__roleSuperAdmin;
const roleUser = window.__roleUser;
const roleAdmin = window.__roleAdmin;

const users = ref([]);
const isLoading = ref(false);
const isCreating = ref(false);
const createError = ref('');
const deletingIds = reactive(new Set());
const newApiKey = ref(null);

const form = reactive({ email: '', password: '', role: window.__roleUser });

function getAntiForgeryToken() {
    return document.querySelector('input[name="__RequestVerificationToken"]')?.value ?? '';
}

async function loadUsers() {
    isLoading.value = true;
    try {
        const res = await fetch('/Users/GetUsers', { credentials: 'include' });
        if (!res.ok) {
            throw new Error();
        }
        users.value = await res.json();
    } catch {
        Notify.show('failed to load users', 'error');
    } finally {
        isLoading.value = false;
    }
}

async function createUser() {
    createError.value = '';
    if (!form.email || !form.password) {
        createError.value = 'email and password are required';
        return;
    }
    if (form.password.length < window.__passwordMinLength) {
        createError.value = `password must be at least ${window.__passwordMinLength} characters`;
        return;
    }
    isCreating.value = true;
    try {
        const res = await fetch('/Users/CreateUser', {
            method: 'POST',
            credentials: 'include',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getAntiForgeryToken()
            },
            body: JSON.stringify({ email: form.email, password: form.password, role: form.role })
        });
        const data = await res.json();
        if (!res.ok) {
            createError.value = data.error ?? 'failed to create user';
            return;
        }
        newApiKey.value = data.apiKey;
        form.email = '';
        form.password = '';
        form.role = window.__roleUser;
        Notify.show('user created', 'success');
        await loadUsers();
    } catch {
        createError.value = 'failed to create user';
    } finally {
        isCreating.value = false;
    }
}

async function deleteUser(userId) {
    if (deletingIds.has(userId)) {
        return;
    }
    deletingIds.add(userId);
    try {
        const res = await fetch('/Users/DeleteUser', {
            method: 'POST',
            credentials: 'include',
            headers: { 'RequestVerificationToken': getAntiForgeryToken() },
            body: new URLSearchParams({ userId })
        });
        const data = await res.json();
        if (!res.ok || !data.success) {
            Notify.show('cannot delete this user', 'error');
            return;
        }
        users.value = users.value.filter(u => u.userId !== userId);
        Notify.show('user deleted', 'success');
    } catch {
        Notify.show('failed to delete user', 'error');
    } finally {
        deletingIds.delete(userId);
    }
}

function canDeleteUser(user) {
    if (user.userId === window.__userId) {
        return false;
    }
    if (isSuperAdmin) {
        return user.role !== window.__roleSuperAdmin;
    }
    return user.role === __roleUser;
}

onMounted(() => {
    loadUsers();
});
</script>

<template>
    <div class="users-wrap">
        <h2>users</h2>

        <div class="create-user-box">
            <h3>create user</h3>
            <div class="form-group">
                <label>email</label>
                <input v-model="form.email" class="form-input" type="email" placeholder="user@example.com" data-testid="create-email-input" />
            </div>
            <div class="form-group">
                <label>password</label>
                <input v-model="form.password" class="form-input" type="password" placeholder="min 6 characters" data-testid="create-password-input" />
            </div>
            <div class="form-group" v-if="isSuperAdmin">
                <label>role</label>
                <select v-model="form.role" class="form-input" data-testid="create-role-select">
                    <option :value="roleUser">user</option>
                    <option :value="roleAdmin">admin</option>
                </select>
            </div>
            <div class="form-error" v-if="createError" data-testid="create-error">{{ createError }}</div>
            <button class="btn-primary" @click="createUser" data-testid="create-user-btn">create user</button>
        </div>

        <div class="apikey-modal" v-if="newApiKey" data-testid="apikey-modal">
            <div class="apikey-box">
                <h3>api key created</h3>
                <p>save these credentials, the secret won't be shown again.</p>
                <div class="apikey-field"><strong>client id:</strong> {{ newApiKey.clientId }}</div>
                <div class="apikey-field"><strong>client secret:</strong> {{ newApiKey.clientSecret }}</div>
                <button class="btn-primary" @click="newApiKey = null" data-testid="apikey-modal-close">close</button>
            </div>
        </div>

        <div class="users-list" data-testid="users-list">
            <p v-if="isLoading" class="loading-text" data-testid="users-loading">loading...</p>
            <p v-if="!isLoading && users.length === 0">no users found.</p>
            <div class="user-row" v-for="user in users" :key="user.userId" data-testid="user-row">
                <div class="user-info">
                    <span class="user-email" data-testid="user-email">{{ user.email }}</span>
                    <span class="user-role role-badge" :class="'role-' + user.role">{{ user.role }}</span>
                </div>
                <button class="btn-delete" v-if="canDeleteUser(user)" @click="deleteUser(user.userId)">
                    <span>✕</span>
                </button>
            </div>
        </div>
    </div>
</template>

<style>
.users-wrap {
    max-width: 640px;
    margin: 0 auto;
    padding: 2.5rem 1.5rem;
}

.users-wrap h2 {
    font-size: 1.25rem;
    font-weight: 700;
    margin-bottom: 1.75rem;
    color: #1a1a2e;
    letter-spacing: 0.01em;
}

.create-user-box {
    background: #fff;
    padding: 1.75rem;
    margin-bottom: 2rem;
}

.create-user-box h3 {
    font-size: 0.95rem;
    font-weight: 700;
    margin-bottom: 1.1rem;
    color: #222;
    text-transform: uppercase;
    letter-spacing: 0.06em;
}

.apikey-modal {
    position: fixed;
    inset: 0;
    background: rgba(200,210,230,0.55);
    backdrop-filter: blur(2px);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 500;
    padding: 1rem;
}

.apikey-box {
    background: #fff;
    padding: 2rem;
    max-width: 480px;
    width: 100%;
}

.apikey-box h3 {
    font-size: 1rem;
    font-weight: 700;
    margin-bottom: 0.5rem;
    color: #1a1a2e;
}

.apikey-box p {
    font-size: 0.85rem;
    margin-bottom: 1.25rem;
    color: #6b7a99;
}

.apikey-field {
    background: #f4f6fa;
    padding: 0.65rem 0.85rem;
    font-size: 0.85rem;
    font-family: monospace;
    word-break: break-all;
    margin-bottom: 0.6rem;
    color: #2a3a5e;
}

.apikey-box .btn-primary {
    margin-top: 1rem;
}

.users-list {
    display: flex;
    flex-direction: column;
    gap: 0.6rem;
}

.loading-text {
    font-size: 1rem;
    color: #9aa8c0;
}

.user-row {
    background: #fff;
    padding: 0.9rem 1.1rem;
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 1rem;
}

.user-info {
    display: flex;
    align-items: center;
    gap: 0.75rem;
    flex-wrap: wrap;
    min-width: 0;
}

.user-email {
    font-size: 0.97rem;
    font-weight: 600;
    word-break: break-all;
    color: #1a1a2e;
}

.role-badge {
    font-size: 0.72rem;
    padding: 0.2rem 0.6rem;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 0.05em;
}

.role-superadmin {
    background: #e8eeff;
    color: #3a4db5;
}
.role-admin {
    background: #e8f4ff;
    color: #1a6abf;
}
.role-user {
    background: #f0f4fa;
    color: #5a6a8a;
}

@media (max-width: 600px) {
    .users-wrap {
        padding: 1.5rem 1rem;
    }

    .user-info {
        flex-direction: column;
        align-items: flex-start;
        gap: 0.3rem;
    }
}
</style>
