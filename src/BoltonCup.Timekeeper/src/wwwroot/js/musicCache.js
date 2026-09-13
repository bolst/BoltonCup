// IndexedDB blob cache for game audio. Keyed by R2 file key so the shared base pool is stored once.
// Optionally measures each track's loudness on download and stores a playback gain multiplier so the
// player can normalize levels (loud/quiet songs even out) without any per-play analysis lag.
const DB_NAME = 'bc-music';
const STORE = 'tracks';

// Loudness normalization is attenuate-to-reference: the target sits BELOW the RMS of typical tracks,
// so every track is turned *down* toward it (gain <= 1). We can't safely boost quiet tracks that
// already peak near full scale (that needs a limiter), so instead we bring the loud ones down to a
// common floor — which is exactly the "some songs are much louder than others" fix. Tracks quieter
// than the target are left as-is. The peak cap still prevents any clipping.
const TARGET_DBFS = -20;
const MIN_GAIN = 0.1;
const MAX_GAIN = 3.0;

let decodeCtx = null;

// Decodes the audio and returns a playback gain multiplier that brings its RMS loudness toward
// TARGET_DBFS, capped to avoid clipping (never exceeds ~0.99 / peak) and clamped to a sane range.
// Returns 1 if the audio can't be decoded/measured.
async function computeGain(arrayBuffer) {
    try {
        const Ctx = window.AudioContext || window.webkitAudioContext;
        if (!Ctx) return 1;
        decodeCtx = decodeCtx || new Ctx();
        // decodeAudioData detaches the buffer, so callers must pass a buffer they don't reuse.
        const buffer = await decodeCtx.decodeAudioData(arrayBuffer);

        let sumSquares = 0;
        let sampleCount = 0;
        let peak = 0;
        for (let ch = 0; ch < buffer.numberOfChannels; ch++) {
            const data = buffer.getChannelData(ch);
            for (let i = 0; i < data.length; i++) {
                const s = data[i];
                sumSquares += s * s;
                const a = Math.abs(s);
                if (a > peak) peak = a;
            }
            sampleCount += data.length;
        }
        if (sampleCount === 0 || peak === 0) return 1;

        const rms = Math.sqrt(sumSquares / sampleCount);
        if (rms === 0) return 1;

        const rmsDbFs = 20 * Math.log10(rms);
        let gain = Math.pow(10, (TARGET_DBFS - rmsDbFs) / 20);

        // Cap so the loudest sample stays below full scale, then clamp to a reasonable range.
        const noClipCap = 0.99 / peak;
        gain = Math.min(gain, noClipCap);
        gain = Math.max(MIN_GAIN, Math.min(MAX_GAIN, gain));
        return Number.isFinite(gain) ? gain : 1;
    } catch {
        return 1;
    }
}

function openDb() {
    return new Promise((resolve, reject) => {
        const req = indexedDB.open(DB_NAME, 1);
        req.onupgradeneeded = () => {
            const db = req.result;
            if (!db.objectStoreNames.contains(STORE)) {
                db.createObjectStore(STORE, { keyPath: 'key' });
            }
        };
        req.onsuccess = () => resolve(req.result);
        req.onerror = () => reject(req.error);
    });
}

export async function has(key) {
    const db = await openDb();
    return new Promise((resolve) => {
        const req = db.transaction(STORE, 'readonly').objectStore(STORE).getKey(key);
        req.onsuccess = () => resolve(req.result !== undefined);
        req.onerror = () => resolve(false);
    });
}

// Returns the subset of keys already cached, in one pass.
export async function hasMany(keys) {
    const db = await openDb();
    const found = [];
    await Promise.all((keys || []).map(key => new Promise((resolve) => {
        const req = db.transaction(STORE, 'readonly').objectStore(STORE).getKey(key);
        req.onsuccess = () => { if (req.result !== undefined) found.push(key); resolve(); };
        req.onerror = () => resolve();
    })));
    return found;
}

// Fetches the url and stores the full blob. Only stores on a successful, complete download.
// When normalize is true, measures loudness and stores a per-track gain alongside the blob.
export async function download(key, url, normalize) {
    try {
        const resp = await fetch(url);
        if (!resp.ok) return { ok: false, status: resp.status, size: 0 };
        const blob = await resp.blob();
        let gain = 1;
        if (normalize) {
            // arrayBuffer() returns a fresh buffer, so decoding it doesn't disturb the stored blob.
            gain = await computeGain(await blob.arrayBuffer());
        }
        const db = await openDb();
        await new Promise((resolve, reject) => {
            const tx = db.transaction(STORE, 'readwrite');
            tx.objectStore(STORE).put({ key, blob, size: blob.size, cachedAt: Date.now(), gain });
            tx.oncomplete = () => resolve();
            tx.onerror = () => reject(tx.error);
        });
        return { ok: true, status: resp.status, size: blob.size };
    } catch (e) {
        return { ok: false, status: 0, size: 0 };
    }
}

// Ensures an already-cached record has a measured gain, computing it from the stored blob if missing.
// Lets tracks cached before normalization existed get normalized on the next preload without re-downloading.
export async function ensureGain(key) {
    const db = await openDb();
    const rec = await new Promise((resolve) => {
        const req = db.transaction(STORE, 'readonly').objectStore(STORE).get(key);
        req.onsuccess = () => resolve(req.result || null);
        req.onerror = () => resolve(null);
    });
    if (!rec || !rec.blob) return 1;
    if (typeof rec.gain === 'number') return rec.gain;

    const gain = await computeGain(await rec.blob.arrayBuffer());
    rec.gain = gain;
    await new Promise((resolve) => {
        const tx = db.transaction(STORE, 'readwrite');
        tx.objectStore(STORE).put(rec);
        tx.oncomplete = () => resolve();
        tx.onerror = () => resolve();
    });
    return gain;
}

// Returns the stored playback gain for a cached track, or 1 if not cached / not measured.
export async function getGain(key) {
    const db = await openDb();
    return new Promise((resolve) => {
        const req = db.transaction(STORE, 'readonly').objectStore(STORE).get(key);
        req.onsuccess = () => {
            const rec = req.result;
            resolve(rec && typeof rec.gain === 'number' ? rec.gain : 1);
        };
        req.onerror = () => resolve(1);
    });
}

export async function getObjectUrl(key) {
    const db = await openDb();
    return new Promise((resolve) => {
        const req = db.transaction(STORE, 'readonly').objectStore(STORE).get(key);
        req.onsuccess = () => {
            const rec = req.result;
            resolve(rec && rec.blob ? URL.createObjectURL(rec.blob) : null);
        };
        req.onerror = () => resolve(null);
    });
}

export function revokeObjectUrl(objectUrl) {
    if (objectUrl && objectUrl.startsWith('blob:')) {
        URL.revokeObjectURL(objectUrl);
    }
}

export async function estimate() {
    if (navigator.storage && navigator.storage.estimate) {
        const e = await navigator.storage.estimate();
        return { usage: e.usage || 0, quota: e.quota || 0 };
    }
    return { usage: 0, quota: 0 };
}

export async function requestPersist() {
    if (navigator.storage && navigator.storage.persist) {
        try { return await navigator.storage.persist(); } catch { return false; }
    }
    return false;
}

export async function remove(key) {
    const db = await openDb();
    await new Promise((resolve) => {
        const tx = db.transaction(STORE, 'readwrite');
        tx.objectStore(STORE).delete(key);
        tx.oncomplete = () => resolve();
        tx.onerror = () => resolve();
    });
}
