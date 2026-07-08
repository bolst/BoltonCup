// Drives a single hidden <audio> element and marshals media events back to the MusicPlayerService.
// Audio is routed through a Web Audio gain node so per-track loudness normalization can be applied
// (see musicCache.js for how the gain value is measured and stored).
let audio = null;
let dotnet = null;
let lastWholeSecond = -1;
let pendingStartSec = 0;

// Web Audio graph: <audio> -> MediaElementSource -> GainNode -> destination.
// If graph setup fails (unsupported/blocked), we fall back to the bare element and setGain is a no-op.
let audioCtx = null;
let sourceNode = null;
let gainNode = null;

export function init(audioEl, dotnetRef) {
    audio = audioEl;
    dotnet = dotnetRef;
    // Must be set before any src is loaded so the Web Audio graph can read cross-origin (R2) media.
    audio.crossOrigin = 'anonymous';
    audio.addEventListener('ended', onEnded);
    audio.addEventListener('timeupdate', onTimeUpdate);
    audio.addEventListener('loadedmetadata', onLoadedMetadata);
    audio.addEventListener('error', onError);
    setupGraph();
}

function setupGraph() {
    if (gainNode) return;
    try {
        const Ctx = window.AudioContext || window.webkitAudioContext;
        if (!Ctx) return;
        audioCtx = new Ctx();
        sourceNode = audioCtx.createMediaElementSource(audio);
        gainNode = audioCtx.createGain();
        gainNode.gain.value = 1;
        sourceNode.connect(gainNode);
        gainNode.connect(audioCtx.destination);
    } catch {
        // Graph unavailable — leave audio playing through the element directly; setGain becomes a no-op.
        audioCtx = null;
        sourceNode = null;
        gainNode = null;
    }
}

function onEnded() {
    dotnet && dotnet.invokeMethodAsync('OnEnded');
}

function onTimeUpdate() {
    if (!audio) return;
    const cur = Math.floor(audio.currentTime || 0);
    if (cur !== lastWholeSecond) {
        lastWholeSecond = cur;
        const dur = isFinite(audio.duration) ? audio.duration : 0;
        dotnet && dotnet.invokeMethodAsync('OnTimeUpdate', audio.currentTime || 0, dur);
    }
}

function onLoadedMetadata() {
    if (!audio) return;
    // Apply the start offset once metadata (and the seekable range) is known.
    if (pendingStartSec > 0) {
        try { audio.currentTime = pendingStartSec; } catch { }
        pendingStartSec = 0;
    }
    const dur = isFinite(audio.duration) ? audio.duration : 0;
    dotnet && dotnet.invokeMethodAsync('OnLoadedMetadata', dur);
}

function onError() {
    const msg = audio && audio.error ? ('audio error code ' + audio.error.code) : 'unknown audio error';
    dotnet && dotnet.invokeMethodAsync('OnPlaybackError', msg);
}

export function load(objectUrl, startSec) {
    if (!audio) return;
    lastWholeSecond = -1;
    pendingStartSec = startSec > 0 ? startSec : 0;
    audio.src = objectUrl;
    audio.load();
    // Best-effort immediate seek; onLoadedMetadata applies it reliably once seekable.
    if (pendingStartSec > 0) {
        try { audio.currentTime = pendingStartSec; } catch { }
    }
}

export async function play() {
    if (!audio) return false;
    // The context starts suspended (created before a user gesture); play() is user-initiated, so resume here.
    if (audioCtx && audioCtx.state === 'suspended') {
        try { await audioCtx.resume(); } catch { }
    }
    try { await audio.play(); return true; } catch { return false; }
}

export function pause() { if (audio) audio.pause(); }
export function seek(sec) { if (audio) audio.currentTime = sec; }
export function setVolume(v) { if (audio) audio.volume = v; }

// Per-track loudness normalization multiplier (1 = unchanged). No-op if the Web Audio graph is unavailable.
export function setGain(v) {
    if (gainNode) {
        const g = (typeof v === 'number' && isFinite(v) && v > 0) ? v : 1;
        gainNode.gain.value = g;
    }
}

export function dispose() {
    if (!audio) return;
    audio.removeEventListener('ended', onEnded);
    audio.removeEventListener('timeupdate', onTimeUpdate);
    audio.removeEventListener('loadedmetadata', onLoadedMetadata);
    audio.removeEventListener('error', onError);
    try { audio.pause(); } catch { }
    try { if (audioCtx) audioCtx.close(); } catch { }
    audio = null;
    dotnet = null;
    audioCtx = null;
    sourceNode = null;
    gainNode = null;
    lastWholeSecond = -1;
}
