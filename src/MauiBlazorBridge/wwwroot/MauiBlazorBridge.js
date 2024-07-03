const mobileIdiom = 'Mobile';
const tabletIdiom = 'Tablet';
const desktopIdiom = 'Desktop';
const unknownIdiom = 'Unknown';
window.resizeListener = null;

export function getFormFactor() {
    const width = window.innerWidth;

    if (width <= 480) {
        return mobileIdiom;
    } else if (width > 480 && width <= 768) {
        return mobileIdiom;
    } else if (width > 768 && width <= 1024) {
        return tabletIdiom;
    } else if (width > 1024) {
        return desktopIdiom;
    } else {
        return unknownIdiom;
    }
}
export function registerResizeListener(dotnetObject){
    if (!window.resizeListener) {
        console.log("created new listeners");
        window.resizeListener = async () => {
            await dotnetObject.invokeMethodAsync("OnIdiomChangedCallback", getFormFactor());
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
