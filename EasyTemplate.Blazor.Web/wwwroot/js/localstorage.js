function LocalStorageSet(key, value) {
    localStorage.setItem(key, value);
};
function LocalStorageGet(key) {
    return localStorage.getItem(key);
};
function LocalStorageRemove(key) {
    localStorage.removeItem(key);
}