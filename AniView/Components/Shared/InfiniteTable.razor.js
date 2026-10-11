// Beobachtet das Element am Tabellenende und meldet an .NET, sobald es (fast) sichtbar wird.
// root = null (Viewport) berücksichtigt auch Scroll-Container der Vorfahren.
const observers = new WeakMap();

export function observe(sentinel, dotNetReference, preloadDistance) {
    dispose(sentinel);

    const observer = new IntersectionObserver(entries => {
        if (entries.some(entry => entry.isIntersecting)) {
            dotNetReference.invokeMethodAsync("OnSentinelVisible");
        }
    }, { rootMargin: `0px 0px ${preloadDistance}px 0px` });

    observer.observe(sentinel);
    observers.set(sentinel, observer);
}

export function refresh(sentinel) {
    const observer = observers.get(sentinel);
    if (observer) {
        observer.unobserve(sentinel);
        observer.observe(sentinel);
    }
}

export function dispose(sentinel) {
    const observer = observers.get(sentinel);
    if (observer) {
        observer.disconnect();
        observers.delete(sentinel);
    }
}
