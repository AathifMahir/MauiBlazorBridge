const mobileIdiom = 'Mobile';
const tabletIdiom = 'Tablet';
const desktopIdiom = 'Desktop';
const unknownIdiom = 'Unknown';
window.resizeListener = null;
window.currentIdiom = 'Unknown';

export function getFormFactor() {
    const width = window.innerWidth;

    if (width <= 767) {
        return mobileIdiom;
    } else if (width >= 768 && width <= 1023) {
        return tabletIdiom;
    } else if (width >= 1024) {
        return desktopIdiom;
    } else {
        return unknownIdiom;
    }
}
export function registerResizeListener(dotnetObject){
    if (!window.resizeListener) {
        console.log("created new listeners");
        window.currentIdiom = getFormFactor();

        window.resizeListener = async () => {
            const idiom = getFormFactor();
            if (window.currentIdiom !== idiom) {
                window.currentIdiom = idiom;
                await dotnetObject.invokeMethodAsync("OnIdiomChangedCallback", idiom);
            }
        };
        window.addEventListener('resize', window.resizeListener);
    }
}

export function disposeListeners() {
    if (window.resizeListener) {
        console.log("disposed all the listeners");
        window.removeEventListener('resize', window.resizeListener);
        window.resizeListener = null;
    }
}

export function getPlatform() {
    const userAgent = navigator.userAgent;

    if (userAgent.includes('Windows')) {
        return 'Windows';
    } else if (userAgent.includes('Mac OS X')) {
        return 'Mac';
    } else if (userAgent.includes('Android')) {
        return 'Android';
    } else if (userAgent.includes('iPhone') || userAgent.includes('iPad')) {
        return 'IOS';
    }
    return "Unknown";
}
