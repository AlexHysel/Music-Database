function createTrackElement(track) {
    let trackElement = document.createElement('div');
    trackElement.className = 'track'

    let trackTitle = document.createElement('a');
    trackTitle.textContent = track.title;
    trackTitle.href = `track.html?id=${track.id}`;

    let addToFavoritesBtn = document.createElement('button')
    addToFavoritesBtn.textContent = 'Like';
    addToFavoritesBtn.addEventListener('click', async (event) => {
        let response = await fetch(`user/me/favorites/tracks?id=${track.id}`, {
            method: 'POST',
            headers: {
                'Content-Type':'application/json',
                'Authorization': `Bearer ${localStorage.getItem('token')}`
            }
        });
        if (response.ok){
            alert('Track was added to favorites');
        }
        else{
            alert('Track was not added to favorites');
        }
    });

    let removeFromFavoritesBtn = document.createElement('button')
    removeFromFavoritesBtn.textContent = 'Unlike'
    removeFromFavoritesBtn.addEventListener('click', async (event) => {
        let response = await fetch(`user/me/favorites/tracks?id=${track.id}`, {
            method: 'DELETE',
            headers: {
                'Content-Type':'application/json',
                'Authorization': `Bearer ${localStorage.getItem('token')}`
            }
        })
        if (response.ok){
            alert('Track was removed from favorites');
        }
        else{
            alert('Track was not removed from favorites');
        }
    });

    trackElement.appendChild(trackTitle);
    trackElement.appendChild(addToFavoritesBtn);
    trackElement.appendChild(removeFromFavoritesBtn);
    return trackElement;
}