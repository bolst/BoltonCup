// IndexedDB blob cache for game audio. Keyed by R2 file key so the shared base pool is stored once.
const DB_NAME = 'bc-music';
const STORE = 'tracks';

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
export async function download(key, url) {
    try {
        const resp = await fetch(url);
        if (!resp.ok) return { ok: false, status: resp.status, size: 0 };
        const blob = await resp.blob();
        const db = await openDb();
        await new Promise((resolve, reject) => {
            const tx = db.transaction(STORE, 'readwrite');
            tx.objectStore(STORE).put({ key, blob, size: blob.size, cachedAt: Date.now() });
            tx.oncomplete = () => resolve();
            tx.onerror = () => reject(tx.error);
        });
        return { ok: true, status: resp.status, size: blob.size };
    } catch (e) {
        return { ok: false, status: 0, size: 0 };
    }
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
