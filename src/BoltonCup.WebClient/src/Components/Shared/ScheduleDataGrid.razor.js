export function initStickyTournamentHeader(dotNetRef, containerEl) {
    const sentinels = new Map(); // element -> tournament name

    function observe() {
        sentinels.forEach((_, el) => observer.unobserve(el));
        sentinels.clear();

        containerEl.querySelectorAll('[data-tournament-sentinel]').forEach(el => {
            sentinels.set(el, {
                name: el.getAttribute('data-tournament-sentinel'),
                logo: el.getAttribute('data-tournament-logo') || null,
            });
            observer.observe(el);
        });
    }

    // Track which sentinels are above the viewport (i.e. their tournament header has scrolled past)
    const visible = new Set();

    const observer = new IntersectionObserver(entries => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                visible.delete(entry.target);
            } else if (entry.boundingClientRect.top < 0) {
                visible.add(entry.target);
            } else {
                visible.delete(entry.target);
            }
        });

        // The current sticky tournament is the last sentinel that has scrolled above the viewport
        let current = null;
        sentinels.forEach((info, el) => {
            if (visible.has(el)) current = info;
        });

        dotNetRef.invokeMethodAsync('OnStickyTournamentChanged', current?.name ?? null, current?.logo ?? null);
    }, { threshold: 0 });

    observe();

    // Re-observe when the table reloads (MudTable replaces DOM on server reload)
    const mutationObserver = new MutationObserver(() => observe());
    mutationObserver.observe(containerEl, { childList: true, subtree: true });

    return { observer, mutationObserver };
}

export function dispose(handle) {
    if (!handle) return;
    handle.observer.disconnect();
    handle.mutationObserver.disconnect();
}
