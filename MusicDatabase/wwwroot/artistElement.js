function createArtistElement(artist) {
    let artistElement = document.createElement('div');
    artistElement.className = 'artist'

    let artistName = document.createElement('a');
    artistName.textContent = artist.name;
    artistName.href = `artist.html?id=${artist.id}`;

    let addToFavoritesBtn = document.createElement('button')
    addToFavoritesBtn.textContent = 'Like';
    addToFavoritesBtn.addEventListener('click', async (event) => {
        let response = await fetch(`user/me/favorites/artists?id=${artist.id}`, {
            method: 'POST',
            headers: {
                'Content-Type':'application/json',
                'Authorization': `Bearer ${localStorage.getItem('token')}`
            }
        });
        if (response.ok){
            alert('artist was added to favorites');
        }
        else{
            alert('artist was not added to favorites');
        }
    });

    let removeFromFavoritesBtn = document.createElement('button')
    removeFromFavoritesBtn.textContent = 'Unlike'
    removeFromFavoritesBtn.addEventListener('click', async (event) => {
        let response = await fetch(`user/me/favorites/artists?id=${artist.id}`, {
            method: 'DELETE',
            headers: {
                'Content-Type':'application/json',
                'Authorization': `Bearer ${localStorage.getItem('token')}`
            }
        })
        if (response.ok){
            alert('artist was removed from favorites');
        }
        else{
            alert('artist was not removed from favorites');
        }
    });

    artistElement.appendChild(artistName);
    artistElement.appendChild(addToFavoritesBtn);
    artistElement.appendChild(removeFromFavoritesBtn);
    return artistElement;
}