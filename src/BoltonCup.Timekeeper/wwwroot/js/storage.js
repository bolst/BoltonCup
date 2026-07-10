// Full local reset for the Timekeeper: wipes localStorage and every IndexedDB database on this origin
// (offline event records, the downloaded music cache, and anything else persisted on the device).
export async function clearAll() {
    try {
        localStorage.clear();
    } catch { /* storage may be unavailable */ }
    await clearIndexedDb();
}

async function clearIndexedDb() {
    // Modern browsers can enumerate databases; delete each one.
    if (indexedDB.databases) {
        try {
            const dbs = await indexedDB.databases();
            await Promise.all(dbs.map(d => (d && d.name) ? deleteDb(d.name) : Promise.resolve()));
            return;
        } catch { /* fall through to the known-database fallback */ }
    }
    // Fallback for browsers without databases(): delete the databases we know about.
    await deleteDb('bc-music');
}

function deleteDb(name) {
    return new Promise((resolve) => {
        const req = indexedDB.deleteDatabase(name);
        req.onsuccess = () => resolve();
        req.onerror = () => resolve();
        req.onblocked = () => resolve();
    });
}
