function createArtistElement(artist) {
    let artistElement = document.createElement('li');
    artistElement.className = 'artist';

    let artistLink = document.createElement('a');
    artistLink.href = `artist.html?id=${artist.id}`;

    let artistImage = document.createElement('img');
    artistImage.src = artist.imageUrl ?? '/placeholder.png';
    artistImage.alt = artist.name;

    let artistName = document.createElement('h3');
    artistName.textContent = artist.name;

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

    return artistList;
}

function createArtistListWithMore(artists, options = {}) {
    // options: { showMore: bool, endpoint: string, queryParam: string }
    let list = createArtistList(artists);
    const showMoreEnabled = options.showMore ?? true;
    if (!showMoreEnabled) return list;

    let showMore = document.createElement('div');
    showMore.className = 'more-wrapper';
    let showMoreBtn = document.createElement('button');
    showMoreBtn.className = 'more-btn btn btn-sm btn-outline-light';
    showMoreBtn.textContent = 'More';
    showMore.appendChild(showMoreBtn);

    showMoreBtn.addEventListener('click', async (event) => {
        event.preventDefault();
        let size = list.children.length;
        let url;
        if (options.endpoint) {
            if (options.queryParam && document.getElementById('searchLine')) {
                let searchLine = document.getElementById('searchLine').value;
                url = `${options.endpoint}?${options.queryParam}=${encodeURIComponent(searchLine)}&toSkip=${size}`;
            } else {
                url = `${options.endpoint}?toSkip=${size}`;
            }
        } else {
            let searchLineEl = document.getElementById('searchLine');
            let searchLine = searchLineEl ? searchLineEl.value : '';
            url = `artist/search?name=${encodeURIComponent(searchLine)}&toSkip=${size}`;
        }

        let response = await fetch(url);
        if (response.ok) {
            for (let newArtist of await response.json())
                list.appendChild(createArtistElement(newArtist));
        } else console.error('Error loading more artists', await response.text());
    });

    list._moreElement = showMore;
    return list;
}