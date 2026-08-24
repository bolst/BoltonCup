// Detects a *deliberate* "over-scroll" on the horizontally scrollable carousel
// pages and advances the MudCarousel via the .NET `Advance` callback.
//
// The tricky part: a quick flick that scrolls a page to its end carries
// momentum (wheel) / finger travel (touch) *past* the edge. That leftover must
// NOT count as over-scroll, otherwise the same gesture that reveals the last
// videos immediately pages away from them. So we only count scrolling that
// happens once the page has come to rest against the edge.
//
// `root` is a stable ancestor; the active page's scroll container is resolved
// per-event with `.closest('.mw-overscroll')`, so it survives MudCarousel item
// swaps.

export function initOverscroll(root, dotNet) {
    const WHEEL_THRESHOLD = 220;  // px of deliberate edge over-scroll (trackpad)
    const WHEEL_SETTLE_MS = 260;  // a gap this long means a fresh, intentional push
    const TOUCH_THRESHOLD = 110;  // px of finger drag *past* the edge (touch)
    const COOLDOWN_MS = 700;      // ignore further triggers briefly after advancing
    const IDLE_RESET_MS = 150;    // drop accumulation once scrolling pauses

    let cooling = false;

    function trigger(direction) {
        if (cooling) return;
        cooling = true;
        dotNet.invokeMethodAsync('Advance', direction);
        setTimeout(() => { cooling = false; }, COOLDOWN_MS);
    }

    function edgesOf(el) {
        const atLeft = el.scrollLeft <= 1;
        const atRight = Math.ceil(el.scrollLeft + el.clientWidth) >= el.scrollWidth - 1;
        return { atLeft, atRight };
    }

    // --- Trackpad / mouse wheel (horizontal intent) ---
    let accum = 0;
    let edge = null;      // 'left' | 'right' currently being over-scrolled
    let armed = false;    // has a fresh, deliberate push begun at this edge?
    let lastWheelTs = 0;
    let idleTimer = null;

    function disarmWheel() {
        accum = 0;
        edge = null;
        armed = false;
    }

    function onWheel(e) {
        const el = e.target.closest('.mw-overscroll');
        if (!el) return;

        // Let vertical scrolling fall through to the page.
        if (Math.abs(e.deltaX) < Math.abs(e.deltaY)) return;
        const dx = e.deltaX;
        if (dx === 0) return;

        const now = performance.now();
        const gap = now - lastWheelTs;
        lastWheelTs = now;

        const { atLeft, atRight } = edgesOf(el);
        const dir = dx > 0 ? 'right' : 'left';
        const atThisEdge = (dir === 'right' && atRight) || (dir === 'left' && atLeft);

        // Still scrolling within the content (or toward the far edge): this is
        // the flick reaching the edge, not an over-scroll. Stay disarmed.
        if (!atThisEdge) {
            disarmWheel();
            return;
        }

        // Pinned at the edge. Only arm when a genuine pause preceded this event,
        // which is what separates "I stopped, then pushed again" from the
        // continuous momentum tail of the flick that just hit the edge.
        if (gap > WHEEL_SETTLE_MS) {
            armed = true;
            edge = dir;
            accum = 0;
        }

        if (!armed || dir !== edge) return;

        accum += Math.abs(dx);
        e.preventDefault(); // suppress the browser's horizontal rubber-band

        clearTimeout(idleTimer);
        idleTimer = setTimeout(disarmWheel, IDLE_RESET_MS);

        if (accum >= WHEEL_THRESHOLD) {
            trigger(edge === 'right' ? 1 : -1);
            disarmWheel();
        }
    }

    // --- Touch drag ---
    let touchEl = null;
    let touchStartX = null;
    let touchEdge = null;    // edge the finger is currently pushing against
    let edgeAnchorX = null;  // finger x at the moment that edge was reached
    let touchOver = 0;       // finger travel past the anchor

    function onTouchStart(e) {
        const el = e.target.closest('.mw-overscroll');
        touchEl = el;
        touchStartX = el ? e.touches[0].clientX : null;
        touchEdge = null;
        edgeAnchorX = null;
        touchOver = 0;
    }

    function onTouchMove(e) {
        if (!touchEl || touchStartX == null) return;

        const x = e.touches[0].clientX;
        const { atLeft, atRight } = edgesOf(touchEl);
        // Direction of the overall swipe: finger left => scrolling toward right edge.
        const dragDir = x < touchStartX ? 'right' : (x > touchStartX ? 'left' : null);

        if (atRight && dragDir === 'right') {
            // Anchor at the point the edge was first reached; only travel beyond
            // that (finger still moving left) counts as over-scroll.
            if (touchEdge !== 'right') { touchEdge = 'right'; edgeAnchorX = x; }
            touchOver = Math.max(0, edgeAnchorX - x);
        } else if (atLeft && dragDir === 'left') {
            if (touchEdge !== 'left') { touchEdge = 'left'; edgeAnchorX = x; }
            touchOver = Math.max(0, x - edgeAnchorX);
        } else {
            touchEdge = null;
            edgeAnchorX = null;
            touchOver = 0;
        }
    }

    function endTouch() {
        if (touchEdge && touchOver >= TOUCH_THRESHOLD) {
            trigger(touchEdge === 'right' ? 1 : -1);
        }
        touchEl = null;
        touchStartX = null;
        touchEdge = null;
        edgeAnchorX = null;
        touchOver = 0;
    }

    root.addEventListener('wheel', onWheel, { passive: false });
    root.addEventListener('touchstart', onTouchStart, { passive: true });
    root.addEventListener('touchmove', onTouchMove, { passive: true });
    root.addEventListener('touchend', endTouch, { passive: true });
    root.addEventListener('touchcancel', endTouch, { passive: true });

    return {
        dispose() {
            clearTimeout(idleTimer);
            root.removeEventListener('wheel', onWheel);
            root.removeEventListener('touchstart', onTouchStart);
            root.removeEventListener('touchmove', onTouchMove);
            root.removeEventListener('touchend', endTouch);
            root.removeEventListener('touchcancel', endTouch);
        }
    };
}
