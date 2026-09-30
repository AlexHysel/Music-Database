function createAlbumElement(album) {
    let albumElement = document.createElement('li');
    albumElement.className = 'album'

    let albumLink = document.createElement('a');
    albumLink.href = `album.html?id=${album.id}`;

    let albumTitle = document.createElement('h3');
    albumTitle.textContent = album.title;

    let albumImage = document.createElement('img');
    albumImage.src = album.imageUrl;
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

function createAlbumList(albums)
{
    let albumList = document.createElement('ul');
    albumList.className = 'albumList';

    for (let album of albums)
        albumList.appendChild(createAlbumElement(album));

    let showMore = document.createElement('li');
    let showMoreBtn = document.createElement('button')
    showMore.appendChild(showMoreBtn);
    albumList.appendChild(showMore);

    showMoreBtn.textContent = 'More';
    showMoreBtn.addEventListener('click', async (event) => {
        let searchLine = document.getElementById('searchLine').value;
        let size = albumList.children.length - 1;
        let response = await fetch(`album/search?name=${searchLine}&toSkip=${size}`);
        if (response.ok){
            let btn = albumList.lastChild;
            albumList.removeChild(btn);
            for (let newAlbum of await response.json())
                albumList.appendChild(createAlbumElement(newAlbum));
            albumList.append(btn);
        }
        else alert("Error");
    });

    return albumList;
}