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

function createTrackMoreToggle(onClick) {
    let moreItem = document.createElement('li');
    moreItem.className = 'track-more-toggle';

    let moreBtn = document.createElement('button');
    moreBtn.type = 'button';
    moreBtn.className = 'track-more-btn';
    moreBtn.textContent = 'More';
    moreBtn.addEventListener('click', onClick);

    moreItem.appendChild(moreBtn);
    return moreItem;
}

function getTrackOthers(track) {
    const rawOthers = Array.isArray(track?.others) ? track.others
        : Array.isArray(track?.Others) ? track.Others
        : [];

    if (rawOthers.length > 0) {
        return rawOthers.map((item, index) => {
            const name = typeof item === 'string' ? item : (item?.name ?? item?.Name ?? '');
            const id = typeof item === 'string' ? '' : (item?.id ?? item?.Id ?? '');
            return { name, id: String(id ?? '') };
        }).filter(item => item.name);
    }

    const namesRaw = track?.othersNames ?? track?.OthersNames ?? '';
    const idsRaw = track?.othersIds ?? track?.OthersIds ?? '';
    const names = String(namesRaw).split(',').filter(Boolean);
    const ids = String(idsRaw).split(',').filter(Boolean);

    return names.map((name, index) => ({
        name,
        id: ids[index] || ''
    }));
}

function createTrackElement(track) {
    const id = track?.id ?? track?.Id;
    const title = track?.title ?? track?.Title ?? '';
    const albumTitle = track?.albumTitle ?? track?.AlbumTitle ?? track?.album?.title ?? track?.album?.Title ?? '';
    const albumImageUrl = track?.albumImageUrl ?? track?.AlbumImageUrl ?? track?.imageUrl ?? track?.ImageUrl ?? track?.album?.imageUrl ?? track?.album?.ImageUrl ?? '';
    const artistName = track?.artistName ?? track?.ArtistName ?? track?.artist?.name ?? track?.artist?.Name ?? '';
    const albumId = track?.albumId ?? track?.AlbumId ?? track?.album?.id ?? track?.album?.Id ?? '';
    const artistId = track?.artistId ?? track?.ArtistId ?? track?.artist?.id ?? track?.artist?.Id ?? '';
    const others = getTrackOthers(track);

    let trackElement = document.createElement('li');
    trackElement.className = 'track'

    let trackImage = document.createElement('img');
    trackImage.src = albumImageUrl || '/placeholder.png';
    trackImage.alt = title;

    let trackTitle = document.createElement('a');
    trackTitle.className = 'title';
    trackTitle.textContent = title;
    trackTitle.href = id ? `track.html?id=${id}` : '#';

    let info = document.createElement('div');
    info.className = 'info';
    let meta = document.createElement('div');
    meta.className = 'meta';

    const appendMetaLink = (text, href, className = 'meta-link') => {
        if (!text) return;
        const link = document.createElement('a');
        link.href = href || '#';
        link.textContent = text;
        link.className = className;
        meta.appendChild(link);
    };

    const addDivider = () => {
        const divider = document.createElement('span');
        divider.className = 'meta-separator';
        divider.textContent = '·';
        meta.appendChild(divider);
    };

    if (artistName) {
        const artistHref = artistId ? `artist.html?id=${artistId}` : '#';
        appendMetaLink(artistName, artistHref);
    }

    if (others.length > 0) {
        if (artistName) addDivider();
        const featLabel = document.createElement('span');
        featLabel.className = 'meta-feat';
        featLabel.textContent = 'feat.';
        meta.appendChild(featLabel);

        others.forEach((other, index) => {
            if (index > 0) {
                const comma = document.createElement('span');
                comma.className = 'meta-separator';
                comma.textContent = ',';
                meta.appendChild(comma);
            }
            appendMetaLink(other.name, other.id ? `artist.html?id=${other.id}` : '#');
        });
    }

    if (albumTitle) {
        if (artistName || others.length > 0) addDivider();
        const albumHref = albumId ? `album.html?id=${albumId}` : '#';
        appendMetaLink(albumTitle, albumHref);
    }

    info.appendChild(trackTitle);
    info.appendChild(meta);

    let actions = document.createElement('div');
    actions.className = 'actions d-flex';

    let addToFavoritesBtn = document.createElement('button')
    addToFavoritesBtn.className = 'btn-like';
    addToFavoritesBtn.textContent = 'Like';
    addToFavoritesBtn.addEventListener('click', async (event) => {
        event.preventDefault();
        let response = await fetch(`user/me/favorites/tracks?id=${id}`, {
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
    removeFromFavoritesBtn.className = 'btn-unlike';
    removeFromFavoritesBtn.textContent = 'Unlike'
    removeFromFavoritesBtn.addEventListener('click', async (event) => {
        event.preventDefault();
        let response = await fetch(`user/me/favorites/tracks?id=${id}`, {
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

    actions.appendChild(addToFavoritesBtn);
    actions.appendChild(removeFromFavoritesBtn);

    trackElement.appendChild(trackImage);
    trackElement.appendChild(info);
    trackElement.appendChild(actions);
    return trackElement;
}

function createTrackList(pageResult, options = {})
{
    let trackList = document.createElement('ul');
    trackList.className = 'trackList';
    if (options.compact) trackList.classList.add('compact');

    const existingTrackIds = new Set();
    const appendUniqueTracks = (tracks) => {
        for (let track of tracks) {
            const id = track?.id ?? track?.Id;
            if (id && existingTrackIds.has(String(id))) continue;
            const element = createTrackElement(track);
            if (id) {
                element.dataset.trackId = String(id);
                existingTrackIds.add(String(id));
            }
            trackList.appendChild(element);
        }
    };

    let tracks = getPagedItems(pageResult, []);
    appendUniqueTracks(tracks);

    const hasMore = getHasMore(pageResult, options.showMore ?? false);
    const showMoreEnabled = options.showMore ?? hasMore;
    if (showMoreEnabled)
    {
        let moreItem = createTrackMoreToggle(async (event) => {
            event.preventDefault();
            let size = trackList.querySelectorAll('.track').length;
            let url;
            if (options.endpoint)
            {
                let params = new URLSearchParams();
                if (options.artistId) {
                    params.set('id', options.artistId);
                } else if (options.queryParam && document.getElementById('searchLine'))
                {
                    let searchLine = document.getElementById('searchLine').value;
                    params.set(options.queryParam, searchLine);
                }
                params.set('toSkip', String(size));
                url = `${options.endpoint}?${params.toString()}`;
            }
            else
            {
                let searchLineEl = document.getElementById('searchLine');
                let searchLine = searchLineEl ? searchLineEl.value : '';
                url = `track/search?title=${encodeURIComponent(searchLine)}&toSkip=${size}`;
            }

            let response = await fetch(url);
            if (response.ok){
                let data = await response.json();
                let nextTracks = getPagedItems(data, []);
                if (trackList.lastElementChild && trackList.lastElementChild.classList.contains('track-more-toggle'))
                    trackList.removeChild(trackList.lastElementChild);

                appendUniqueTracks(nextTracks);

                if (getHasMore(data, false)) {
                    trackList.appendChild(moreItem);
                }
            }
            else console.error("Error loading more tracks", await response.text());
        });
        trackList.appendChild(moreItem);
    }

    return trackList;
}