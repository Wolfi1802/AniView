// Meldet die aktuelle Höhe der Filterleiste als CSS-Variable (--av-filter-bar-height) am Dokument.
// Der klebende Tabellenkopf nutzt sie als Abstand nach oben, damit er direkt unter der Leiste einrastet,
// auch wenn die Leiste auf schmalen Bildschirmen umbricht und höher wird.
const VARIABLE = "--av-filter-bar-height";
const observers = new WeakMap();

export function observe(bar) {
    dispose(bar);

    const update = () => document.documentElement.style.setProperty(VARIABLE, `${bar.offsetHeight}px`);
    const observer = new ResizeObserver(update);

    observer.observe(bar);
    observers.set(bar, observer);
    update();
}

export function dispose(bar) {
    const observer = observers.get(bar);
    if (observer) {
        observer.disconnect();
        observers.delete(bar);
    }
    document.documentElement.style.removeProperty(VARIABLE);
}
