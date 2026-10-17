export function scrollSelectedIntoView(stripEl) {
    const selected = stripEl?.querySelector('[data-selected="true"]');
    if (!selected) {
        return;
    }
    selected.scrollIntoView({ block: 'nearest', inline: 'center', behavior: 'smooth' });
}