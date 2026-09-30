function createArtistElement(artist) {
    let artistElement = document.createElement('li');
    artistElement.className = 'artist'

    let artistLink = document.createElement('a');
    artistLink.href = `artist.html?id=${artist.id}`;

    let artistImage = document.createElement('img');
    artistImage.src = artist.imageUrl;
    artistImage.alt = artist.name;

    let artistName = document.createElement('h3');
    artistName.textContent = artist.name;

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

    artistLink.appendChild(artistImage);
    artistLink.appendChild(artistName);
    artistElement.appendChild(artistLink);
    return artistElement;
}

function createArtistList(artists)
{
    let artistList = document.createElement('ul');
    artistList.className = 'artistList';

    for (let artist of artists)
        artistList.appendChild(createArtistElement(artist));

    let showMore = document.createElement('li');
    let showMoreBtn = document.createElement('button');
    showMore.appendChild(showMoreBtn);
    showMoreBtn.textContent = 'More';
    showMoreBtn.addEventListener('click', async (event) => {
        let searchLine = document.getElementById('searchLine').value;
        let size = artistList.children.length - 1;
        let response = await fetch(`artist/search?name=${searchLine}&toSkip=${size}`);
        if (response.ok){
            let btn = artistList.lastChild;
            artistList.removeChild(btn);
            for (let newArtist of await response.json())
                artistList.appendChild(createArtistElement(newArtist));
            artistList.append(btn);
        }
        else alert("Error");
    });
    artistList.appendChild(showMore);

    return artistList;
}