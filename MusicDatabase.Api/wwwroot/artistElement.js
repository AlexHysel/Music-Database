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

function createArtistListWithMore(pageResult, options = {}) {
    let list = createArtistList(getPagedItems(pageResult));
    const hasMore = getHasMore(pageResult, options.showMore ?? false);
    const showMoreEnabled = options.showMore ?? hasMore;
    if (!showMoreEnabled) return list;

    let moreItem = createMoreCard(async (event) => {
        event.preventDefault();
        let size = list.querySelectorAll('.artist').length;
        let url;
        if (options.endpoint) {
            let params = new URLSearchParams();
            if (options.queryParam && document.getElementById('searchLine')) {
                let searchLine = document.getElementById('searchLine').value;
                params.set(options.queryParam, searchLine);
            }
            params.set('toSkip', String(size));
            url = `${options.endpoint}?${params.toString()}`;
        } else {
            let searchLineEl = document.getElementById('searchLine');
            let searchLine = searchLineEl ? searchLineEl.value : '';
            url = `artist/search?name=${encodeURIComponent(searchLine)}&toSkip=${size}`;
        }

        let response = await fetch(url);
        if (response.ok) {
            let data = await response.json();
            let newArtists = getPagedItems(data);
            for (let newArtist of newArtists)
                list.insertBefore(createArtistElement(newArtist), moreItem);

            if (!getHasMore(data)) {
                if (list.lastElementChild && list.lastElementChild.classList.contains('more-card'))
                    list.removeChild(list.lastElementChild);
            }
        } else console.error('Error loading more artists', await response.text());
    });

    list.appendChild(moreItem);
    list._moreElement = moreItem;
    return list;
}