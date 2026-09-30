function getPagedItems(value, fallback = []) {
    if (Array.isArray(value)) return value;
    if (!value) return fallback;
    if (Array.isArray(value.items)) return value.items;
    if (Array.isArray(value.Items)) return value.Items;
    return fallback;
}

function getHasMore(value, fallback = false) {
    if (!value) return fallback;
    if (typeof value.hasMore === 'boolean') return value.hasMore;
    if (typeof value.HasMore === 'boolean') return value.HasMore;
    return fallback;
}

function createMoreCard(onClick) {
    let moreItem = document.createElement('li');
    moreItem.className = 'more-card';

    let moreBtn = document.createElement('button');
    moreBtn.type = 'button';
    moreBtn.className = 'more-btn btn btn-sm btn-outline-light';
    moreBtn.textContent = 'More';
    moreBtn.addEventListener('click', onClick);

    moreItem.appendChild(moreBtn);
    return moreItem;
}

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

function createAlbumListWithMore(pageResult, options = {}) {
    let list = createAlbumList(getPagedItems(pageResult));
    const hasMore = getHasMore(pageResult, options.showMore ?? false);
    const showMoreEnabled = options.showMore ?? hasMore;
    if (!showMoreEnabled) return list;

    let moreItem = createMoreCard(async (event) => {
        event.preventDefault();
        let size = list.querySelectorAll('.album').length;
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
            url = `album/search?title=${encodeURIComponent(searchLine)}&toSkip=${size}`;
        }

        let response = await fetch(url);
        if (response.ok) {
            let data = await response.json();
            let newAlbums = getPagedItems(data);
            for (let newAlbum of newAlbums)
                list.insertBefore(createAlbumElement(newAlbum), moreItem);

            if (!getHasMore(data)) {
                if (list.lastElementChild && list.lastElementChild.classList.contains('more-card'))
                    list.removeChild(list.lastElementChild);
            }
        } else console.error('Error loading more albums', await response.text());
    });

    list.appendChild(moreItem);
    list._moreElement = moreItem;
    return list;
}