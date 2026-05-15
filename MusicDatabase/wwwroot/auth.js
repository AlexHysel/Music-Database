function getUsername(){
    let token = localStorage.getItem('token');
    if (!token) return null;
    let payload = JSON.parse(atob(token.split('.')[1]));
    return payload.unique_name;
}