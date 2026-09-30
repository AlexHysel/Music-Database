function createAlbumElement(album) {
    let albumElement = document.createElement('li');
    albumElement.className = 'album';

    let albumLink = document.createElement('a');
    albumLink.href = `album.html?id=${album.id}`;

    let albumImage = document.createElement('img');
    albumImage.src = album.imageUrl ?? '/placeholder.png';
    albumImage.alt = album.title;

    let albumTitle = document.createElement('h3');
    albumTitle.textContent = album.title;

    let albumMeta = document.createElement('div');
    albumMeta.className = 'meta';
    let artistName = album.artist?.name ?? album.artistName ?? '';
    if (artistName) albumMeta.textContent = artistName;

    albumLink.appendChild(albumImage);
    albumLink.appendChild(albumTitle);
    if (artistName) albumLink.appendChild(albumMeta);
    albumElement.appendChild(albumLink);
    return albumElement;
}

function createAlbumList(albums) {
    let albumList = document.createElement('ul');
    albumList.className = 'albumList';

    for (let album of albums)
        albumList.appendChild(createAlbumElement(album));

    return albumList;
}

function createAlbumListWithMore(albums, options = {}) {
    // options: { showMore: bool, endpoint: string, queryParam: string }
    let list = createAlbumList(albums);
    const showMoreEnabled = options.showMore ?? true;
    if (!showMoreEnabled) return list;

    let showMore = document.createElement('div');
    showMore.className = 'more-wrapper';
    let showMoreBtn = document.createElement('button')
    showMoreBtn.className = 'more-btn btn btn-sm btn-outline-light';
    showMoreBtn.textContent = 'More';
    showMore.appendChild(showMoreBtn);
    list.parentForMore = true; // marker used by pages if needed

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
            url = `album/search?title=${encodeURIComponent(searchLine)}&toSkip=${size}`;
        }

        let response = await fetch(url);
        if (response.ok) {
            for (let newAlbum of await response.json())
                list.appendChild(createAlbumElement(newAlbum));
        } else console.error('Error loading more albums', await response.text());
    });

    list._moreElement = showMore;
    return list;
}