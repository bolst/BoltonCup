export function initCarousel(sectionEl) {
    const track = sectionEl?.querySelector('[data-track]');
    if (!track) {
        return;
    }

    // Blazor re-renders the tiles and dashes when goals change, so look them up on every call.
    const tiles = () => [...track.children];
    const offsetOf = (tile) => tile.offsetLeft - track.offsetLeft;

    const activeIndex = () => {
        const left = track.scrollLeft;
        let best = 0;
        tiles().forEach((tile, i) => {
            if (Math.abs(offsetOf(tile) - left) < Math.abs(offsetOf(tiles()[best]) - left)) {
                best = i;
            }
        });
        return best;
    };

    const update = () => {
        const index = activeIndex();
        sectionEl.querySelectorAll('[data-dash]').forEach((dash, i) => dash.classList.toggle('active', i === index));
        const prev = sectionEl.querySelector('[data-prev]');
        const next = sectionEl.querySelector('[data-next]');
        if (prev) {
            prev.disabled = track.scrollLeft <= 0;
        }
        if (next) {
            next.disabled = track.scrollLeft >= track.scrollWidth - track.clientWidth - 1;
        }
    };

    const scrollToIndex = (index) => {
        const all = tiles();
        const tile = all[Math.max(0, Math.min(index, all.length - 1))];
        track.scrollTo({ left: offsetOf(tile) });
    };

    if (!sectionEl.dataset.wired) {
        sectionEl.dataset.wired = 'true';
        track.addEventListener('scroll', update, { passive: true });
        sectionEl.addEventListener('click', (e) => {
            if (e.target.closest('[data-prev]')) {
                scrollToIndex(activeIndex() - 1);
            } else if (e.target.closest('[data-next]')) {
                scrollToIndex(activeIndex() + 1);
            }
        });
    }
    update();
}