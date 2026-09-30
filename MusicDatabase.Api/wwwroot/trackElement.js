function createTrackElement(track) {
    let trackElement = document.createElement('li');
    trackElement.className = 'track'

    let trackImage = document.createElement('img');
    trackImage.src = track.imageUrl || '/placeholder.png';
    trackImage.alt = track.title;

    let trackTitle = document.createElement('a');
    trackTitle.className = 'title';
    trackTitle.textContent = track.title;
    trackTitle.href = `track.html?id=${track.id}`;

    let info = document.createElement('div');
    info.className = 'info';
    let meta = document.createElement('div');
    meta.className = 'meta';
    // album title and artist may or may not be present in the DTO
    let albumName = track.album?.title ?? track.albumTitle ?? '';
    let artistName = track.artist?.name ?? track.artistName ?? '';
    if (albumName && artistName)
        meta.textContent = `${artistName} · ${albumName}`;
    else if (albumName)
        meta.textContent = albumName;
    else if (artistName)
        meta.textContent = artistName;

    info.appendChild(trackTitle);
    info.appendChild(meta);

    let actions = document.createElement('div');
    actions.className = 'actions d-flex';

    let addToFavoritesBtn = document.createElement('button')
    addToFavoritesBtn.className = 'btn-like';
    addToFavoritesBtn.textContent = 'Like';
    addToFavoritesBtn.addEventListener('click', async (event) => {
        event.preventDefault();
        let response = await fetch(`user/me/favorites/tracks?id=${track.id}`, {
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
        let response = await fetch(`user/me/favorites/tracks?id=${track.id}`, {
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

function createTrackList(tracks, options = {})
{
    // options: { showMore: bool, endpoint: string, queryParam: string, compact: bool }
    let trackList = document.createElement('ul');
    trackList.className = 'trackList';
    if (options.compact) trackList.classList.add('compact');

    for (let track of tracks)
        trackList.appendChild(createTrackElement(track));

    const showMoreEnabled = options.showMore ?? true;
    if (showMoreEnabled)
    {
        let showMore = document.createElement('li');
        let showMoreBtn = document.createElement('button');
        showMoreBtn.className = 'more-btn btn btn-sm btn-outline-light';
        showMore.appendChild(showMoreBtn);
        showMoreBtn.textContent = 'More';
        showMoreBtn.addEventListener('click', async (event) => {
            event.preventDefault();
            let size = trackList.children.length - 1;
            let url;
            if (options.endpoint)
            {
                // endpoint may accept query param search or only toSkip
                if (options.queryParam && document.getElementById('searchLine'))
                {
                    let searchLine = document.getElementById('searchLine').value;
                    url = `${options.endpoint}?${options.queryParam}=${encodeURIComponent(searchLine)}&toSkip=${size}`;
                }
                else
                {
                    url = `${options.endpoint}?toSkip=${size}`;
                }
            }
            else
            {
                // default to track/search using searchLine
                let searchLineEl = document.getElementById('searchLine');
                let searchLine = searchLineEl ? searchLineEl.value : '';
                url = `track/search?title=${encodeURIComponent(searchLine)}&toSkip=${size}`;
            }

            let response = await fetch(url);
            if (response.ok){
                let btn = trackList.lastChild;
                trackList.removeChild(btn);
                for (let newTrack of await response.json())
                    trackList.appendChild(createTrackElement(newTrack));
                trackList.append(btn);
            }
            else console.error("Error loading more tracks", await response.text());
        });
        trackList.appendChild(showMore);
    }

    return trackList;
}