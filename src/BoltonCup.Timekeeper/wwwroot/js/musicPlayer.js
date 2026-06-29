// Drives a single hidden <audio> element and marshals media events back to the MusicPlayerService.
let audio = null;
let dotnet = null;
let lastWholeSecond = -1;
let pendingStartSec = 0;

export function init(audioEl, dotnetRef) {
    audio = audioEl;
    dotnet = dotnetRef;
    audio.addEventListener('ended', onEnded);
    audio.addEventListener('timeupdate', onTimeUpdate);
    audio.addEventListener('loadedmetadata', onLoadedMetadata);
    audio.addEventListener('error', onError);
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
    try { await audio.play(); return true; } catch { return false; }
}

export function pause() { if (audio) audio.pause(); }
export function seek(sec) { if (audio) audio.currentTime = sec; }
export function setVolume(v) { if (audio) audio.volume = v; }

export function dispose() {
    if (!audio) return;
    audio.removeEventListener('ended', onEnded);
    audio.removeEventListener('timeupdate', onTimeUpdate);
    audio.removeEventListener('loadedmetadata', onLoadedMetadata);
    audio.removeEventListener('error', onError);
    try { audio.pause(); } catch { }
    audio = null;
    dotnet = null;
    lastWholeSecond = -1;
}
