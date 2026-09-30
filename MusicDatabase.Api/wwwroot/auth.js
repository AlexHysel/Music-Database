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

window.getUsername = getUsername;
window.getPayload = getPayload;
window.getRole = getRole;