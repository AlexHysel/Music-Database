function getUsername(){
    let payload = getPayload();
    return payload.unique_name;
}

function getPayload(){
    let token = localStorage.getItem('token');
    if (!token) return null;
    return JSON.parse(atob(token.split('.')[1]));
}

function getRole(){
    const payload = getPayload();
    if (!payload) return null;
    return payload;
}

window.getUsername = getUsername;
window.getPayload = getPayload;
window.getRole = getRole;