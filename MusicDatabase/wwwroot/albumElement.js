function createAlbumElement(album) {
    let albumElement = document.createElement('div');
    albumElement.className = 'album'

    let albumTitle = document.createElement('a');
    albumTitle.textContent = album.title;
    albumTitle.href = `album.html?id=${album.id}`;

    let addToFavoritesBtn = document.createElement('button')
    addToFavoritesBtn.textContent = 'Like';
    addToFavoritesBtn.addEventListener('click', async (event) => {
        let response = await fetch(`user/me/favorites/albums?id=${album.id}`, {
            method: 'POST',
            headers: {
                'Content-Type':'application/json',
                'Authorization': `Bearer ${localStorage.getItem('token')}`
            }
        });
        if (response.ok){
            alert('album was added to favorites');
        }
        else{
            alert('album was not added to favorites');
        }
    });

    let removeFromFavoritesBtn = document.createElement('button')
    removeFromFavoritesBtn.textContent = 'Unlike'
    removeFromFavoritesBtn.addEventListener('click', async (event) => {
        let response = await fetch(`user/me/favorites/albums?id=${album.id}`, {
            method: 'DELETE',
            headers: {
                'Content-Type':'application/json',
                'Authorization': `Bearer ${localStorage.getItem('token')}`
            }
        })
        if (response.ok){
            alert('album was removed from favorites');
        }
        else{
            alert('album was not removed from favorites');
        }
    });

    albumElement.appendChild(albumTitle);
    albumElement.appendChild(addToFavoritesBtn);
    albumElement.appendChild(removeFromFavoritesBtn);
    return albumElement;
}