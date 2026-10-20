// Meldet die aktuelle Höhe der Filterleiste (--av-filter-bar-height) und der Kopfzeile (--av-appbar-height)
// als CSS-Variablen am Dokument. Der klebende Tabellenkopf und die Filterleiste nutzen sie als Abstand nach oben.
// Die Höhe der Kopfzeile wird gemessen, weil sie je nach Bildschirmgröße und Ausrichtung zwischen 48 und 64 px liegt
// und --mud-appbar-height das nicht abbildet. Die Filterleiste wird auf schmalen Bildschirmen höher, wenn sie umbricht.
const BAR_VARIABLE = "--av-filter-bar-height";
const APPBAR_VARIABLE = "--av-appbar-height";
const observers = new WeakMap();

export function observe(bar) {
    dispose(bar);

    const appBar = document.querySelector(".mud-appbar");
    const update = () => {
        const root = document.documentElement.style;
        root.setProperty(BAR_VARIABLE, `${bar.getBoundingClientRect().height}px`);
        if (appBar) {
            root.setProperty(APPBAR_VARIABLE, `${appBar.getBoundingClientRect().height}px`);
        }
    };
    const observer = new ResizeObserver(update);

    observer.observe(bar);
    if (appBar) {
        observer.observe(appBar);
    }
    observers.set(bar, observer);
    update();
}

export function dispose(bar) {
    const observer = observers.get(bar);
    if (observer) {
        observer.disconnect();
        observers.delete(bar);
    }
    document.documentElement.style.removeProperty(BAR_VARIABLE);
    document.documentElement.style.removeProperty(APPBAR_VARIABLE);
}
