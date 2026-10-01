function getUsername(){
    let payload = getPayload();
    return payload?.unique_name ?? payload?.name ?? null;
}

function getPayload(){
    let token = localStorage.getItem('token');
    if (!token) return null;
    try{
        return JSON.parse(atob(token.split('.')[1]));
    } catch(e){
        console.error('Invalid token payload', e);
        return null;
    }
}

function getRole(){
    const payload = getPayload();
    if (!payload) return null;
    return payload.role ?? payload.Role ?? null;
}

function logout(){
    localStorage.removeItem('token');
    window.location.reload();
}

function renderAuthHeader(){
    const root = document.querySelector('header.app-header nav') || document.querySelector('nav.auth-nav-root');
    const wrapper = document.getElementById('globalAuthNav') || document.createElement('div');
    wrapper.id = 'globalAuthNav';
    wrapper.className = 'auth-nav';

    const username = getUsername();

    if (username) {
        wrapper.innerHTML = `
            <a href="user.html" class="auth-user-pill">${username}</a>
            <button type="button" class="btn btn-outline-light btn-sm auth-logout-btn" onclick="logout()">Logout</button>
        `;
    } else {
        wrapper.innerHTML = `
            <button type="button" class="btn btn-outline-light btn-sm" onclick="window.location.href='login.html'">Login</button>
            <button type="button" class="btn btn-outline-light btn-sm" onclick="window.location.href='signup.html'">Sign Up</button>
        `;
    }

    if (root) {
        if (!root.contains(wrapper)) {
            root.appendChild(wrapper);
        }
    } else {
        let shell = document.getElementById('authHeaderShell');
        if (!shell) {
            shell = document.createElement('div');
            shell.id = 'authHeaderShell';
            shell.className = 'auth-header-shell';
            document.body.insertBefore(shell, document.body.firstChild);
        }
        shell.innerHTML = '';
        shell.appendChild(wrapper);
    }
}

window.logout = logout;
window.getUsername = getUsername;
window.getPayload = getPayload;
window.getRole = getRole;
window.renderAuthHeader = renderAuthHeader;

document.addEventListener('DOMContentLoaded', renderAuthHeader);