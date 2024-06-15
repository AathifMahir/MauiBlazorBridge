const mobileIdiom = 'Mobile';
const tabletIdiom = 'Tablet';
const desktopIdiom = 'Desktop';
let resizeListener = null;

export function getFormFactor() {
    const width = window.innerWidth;

    if (width <= 480) {
        return mobileIdiom;
    } else if (width > 480 && width <= 768) {
        return mobileIdiom;
    } else if (width > 768 && width <= 1024) {
        return tabletIdiom;
    } else if (width > 1024 && width <= 1200) {
        return desktopIdiom;
    } else if (width > 1200 && width <= 1600) {
        return desktopIdiom;
    } else if (width > 1600 && width <= 1920) {
        return desktopIdiom;
    } else {
        return desktopIdiom;
    }
}
export function registerResizeListener(assemblyName)
{
    if (!resizeListener) {
        console.log("created new listeners");
        resizeListener = window.addEventListener('resize', async () => {
            await DotNet.invokeMethodAsync(assemblyName, 'OnIdiomChangedCallback', getFormFactor());
        });
    }
}

export function disposeListeners() {
    if (resizeListener) {
        console.log("deleted all the listeners");
        window.removeEventListener('resize', resizeListener);
        resizeListener = null;
    }
}
