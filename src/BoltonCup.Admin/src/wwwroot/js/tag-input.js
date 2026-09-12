// Blazor's @onkeydown:preventDefault is fixed at render time and would suppress every key,
// including character insertion. Trapping Tab needs a per-key decision at event time, and the
// element's data-trap-tab attribute carries the component's current dropdown state.
window.tagInput = {
    attach: function (element) {
        if (!element || element.dataset.tagInputBound === 'true') {
            return;
        }

        element.addEventListener('keydown', function (e) {
            if (e.key === 'Tab' && !e.shiftKey && element.dataset.trapTab === 'true') {
                e.preventDefault();
            }
        });

        element.dataset.tagInputBound = 'true';
    }
};
