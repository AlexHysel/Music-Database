function initializeTags(resource, resourceId, initialTags) {
    const list = document.querySelector('#tagsSection .tag-list');
    const empty = document.getElementById('emptyTags');
    const add = document.getElementById('addTagButton');
    const form = document.getElementById('newTagForm');
    const input = document.getElementById('newTagInput');
    const counter = document.getElementById('tagCounter');
    const cancel = document.getElementById('cancelTagButton');
    const submit = document.getElementById('submitTagButton');
    const error = document.getElementById('tagError');
    const status = document.getElementById('tagStatus');
    let submittedTags = new Set();
    let saving = false;

    const userTagsSection = document.createElement('div');
    userTagsSection.className = 'user-tags-section mt-3';
    userTagsSection.hidden = true;

    const userTagsHeading = document.createElement('h3');
    userTagsHeading.className = 'tag-subheading h6 mb-2';
    userTagsHeading.textContent = 'Your tags';

    const userTagsList = document.createElement('ul');
    userTagsList.className = 'tag-list user-tag-list';

    userTagsSection.append(userTagsHeading, userTagsList);
    list.after(userTagsSection);

    const normalizeTagNames = (tags) =>
        (Array.isArray(tags) ? tags : [])
            .map(tag => typeof tag === 'string' ? tag : (tag && typeof tag === 'object' ? (tag.name ?? tag.Name ?? '') : ''))
            .map(name => String(name).trim())
            .filter(Boolean);

    function renderTags(tags) {
        const names = normalizeTagNames(tags);
        list.replaceChildren();
        for (const name of names) {
            const chip = document.createElement('li');
            chip.className = 'tag-chip';
            const hash = document.createElement('span');
            hash.className = 'tag-hash';
            hash.textContent = '#';
            hash.setAttribute('aria-hidden', 'true');
            chip.append(hash, document.createTextNode(name));
            list.appendChild(chip);
        }
        empty.hidden = list.children.length > 0;
    }

    function renderUserTags(tags) {
        const names = normalizeTagNames(tags);
        submittedTags = new Set(names);
        userTagsList.replaceChildren();
        for (const name of names) {
            const chip = document.createElement('li');
            chip.className = 'tag-chip tag-chip-removable';
            const button = document.createElement('button');
            button.type = 'button';
            button.className = 'tag-chip-button';
            button.textContent = `# ${name}`;
            button.title = 'Remove this tag';
            button.setAttribute('aria-label', `Remove tag ${name}`);
            button.addEventListener('click', async () => {
                if (saving) return;
                saving = true;
                button.disabled = true;
                setStatus('Removing tag…');
                try {
                    const response = await fetch(`${resource}/mytags`, {
                        method: 'DELETE',
                        headers: {
                            'Content-Type': 'application/json',
                            'Authorization': `Bearer ${localStorage.getItem('token')}`
                        },
                        body: JSON.stringify({ tagName: name, [`${resource}Id`]: resourceId, userId })
                    });

                    if (!response.ok) {
                        setStatus('This tag could not be removed.');
                        return;
                    }

                    setStatus(`Removed “${name}” from your tags.`);
                    await refreshUserTags();
                    const refreshed = await fetch(`${resource}?id=${encodeURIComponent(resourceId)}`, { cache: 'no-store' });
                    if (refreshed.ok) {
                        const detail = await refreshed.json();
                        renderTags(detail.tags ?? []);
                    }
                } catch {
                    setStatus('Connection interrupted while removing the tag.');
                } finally {
                    saving = false;
                    button.disabled = false;
                }
            });
            chip.appendChild(button);
            userTagsList.appendChild(chip);
        }
        userTagsSection.hidden = names.length === 0;
    }

    function setError(message = '') {
        error.textContent = message;
        error.hidden = !message;
        input.setAttribute('aria-invalid', String(Boolean(message)));
    }

    function setStatus(message = '') {
        status.textContent = message;
        status.hidden = !message;
    }

    function updateInput() {
        counter.textContent = `${input.value.length} / 20`;
        submit.disabled = saving || !input.value.trim();
    }

    function showForm(show) {
        form.hidden = !show;
        add.hidden = show;
        add.setAttribute('aria-expanded', String(show));
        setError();
        if (show) {
            setStatus();
            input.focus();
        } else {
            input.value = '';
            updateInput();
            add.focus();
        }
    }

    async function refreshUserTags() {
        const payload = getPayload();
        const userId = payload?.nameid ?? payload?.sub;
        if (!userId || !payload || (payload.exp && payload.exp * 1000 <= Date.now())) {
            renderUserTags([]);
            return;
        }

        try {
            const params = new URLSearchParams();
            params.set(resource === 'album' ? 'albumId' : 'trackId', String(resourceId));
            params.set('userId', String(userId));
            const response = await fetch(`${resource}/mytags?${params.toString()}`, {
                headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
            });
            if (!response.ok) {
                submittedTags = new Set();
                renderUserTags([]);
                return;
            }
            const tags = await response.json();
            renderUserTags(tags ?? []);
        } catch {
            submittedTags = new Set();
            renderUserTags([]);
        }
    }

    renderTags(initialTags);
    const payload = getPayload();
    const userId = payload?.nameid ?? payload?.sub;
    const canTag = Boolean(userId && (!payload.exp || payload.exp * 1000 > Date.now()));
    add.hidden = !canTag;
    document.getElementById('tagLoginHint').hidden = canTag;
    if (!canTag) return;

    refreshUserTags();
    add.addEventListener('click', () => showForm(true));
    cancel.addEventListener('click', () => showForm(false));
    input.addEventListener('input', () => {
        setError();
        updateInput();
    });
    form.addEventListener('keydown', event => {
        if (event.key === 'Escape' && !saving) {
            event.preventDefault();
            showForm(false);
        }
    });

    form.addEventListener('submit', async event => {
        event.preventDefault();
        if (saving) return;
        const name = input.value.trim().toLowerCase();
        if (!name || name.length > 20) {
            setError(!name ? 'Enter a tag first.' : 'Keep your tag within 20 characters.');
            input.focus();
            return;
        }
        if (submittedTags.has(name)) {
            setError('You have already added this tag. Try another one.');
            input.focus();
            return;
        }

        saving = true;
        input.disabled = true;
        cancel.disabled = true;
        submit.textContent = 'Adding…';
        form.setAttribute('aria-busy', 'true');
        setError();
        setStatus();
        updateInput();

        try {
            const response = await fetch(`${resource}/addtag`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${localStorage.getItem('token')}`
                },
                body: JSON.stringify({ tagName: name, [`${resource}Id`]: resourceId, userId })
            });

            if (!response.ok) {
                const detail = await response.text();
                if (response.status === 409 || /already added this tag/i.test(detail)) {
                    submittedTags.add(name);
                    setError('You have already added this tag. Try another one.');
                } else if (response.status === 401 || response.status === 403) {
                    setError('Your session has expired. Sign in again to add a tag.');
                } else if (response.status === 404) {
                    setError(`This ${resource} is no longer available. Refresh the page.`);
                } else {
                    setError('Could not add this tag. Please try again.');
                }
                return;
            }

            submittedTags.add(name);
            showForm(false);
            setStatus(`Your tag “${name}” was added. Thank you!`);

            try {
                const updated = await fetch(`${resource}?id=${encodeURIComponent(resourceId)}`, { cache: 'no-store' });
                if (!updated.ok) throw new Error('Tag refresh failed');
                const detail = await updated.json();
                renderTags(detail.tags ?? []);
                await refreshUserTags();
                if (!normalizeTagNames(detail.tags ?? []).includes(name)) {
                    setStatus(`Your tag “${name}” was added. Only the community’s top tags appear here.`);
                }
            } catch {
                setStatus(`Your tag “${name}” was saved. Refresh the page to update the top tags.`);
            }
        } catch {
            setError('Connection interrupted. Check your connection and try again.');
        } finally {
            saving = false;
            input.disabled = false;
            cancel.disabled = false;
            submit.textContent = 'Add tag';
            form.removeAttribute('aria-busy');
            updateInput();
            if (!form.hidden) input.focus();
        }
    });
}