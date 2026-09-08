function createAlbumElement(album) {
    let albumElement = document.createElement('li');
    albumElement.className = 'album'

    let albumLink = document.createElement('a');
    albumLink.href = `album.html?id=${album.id}`;

    let albumTitle = document.createElement('h3');
    albumTitle.textContent = album.title;

    let albumImage = document.createElement('img');
    albumImage.src = `https://i.scdn.co/image/ab67616d00001e02bb4d8804974a61ab74f33ede`//album.imageUrl;
    albumImage.alt = album.title;

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

    albumLink.appendChild(albumImage);
    albumLink.appendChild(albumTitle);
    albumElement.appendChild(albumLink);
    return albumElement;
}