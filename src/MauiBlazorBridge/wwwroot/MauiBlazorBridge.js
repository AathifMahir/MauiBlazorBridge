window.resizeListener = null;
window.currentIdiom = 'Unknown';
window.networkListener = null;
window.networkIntervalTimeout = null;

export function getFormFactor() {
    const width = window.innerWidth;
    const height = window.innerHeight;

    if (width <= 767) {
        return JSON.stringify(new DeviceFormFactor(FormFactor.Mobile, width, height));
    } else if (width >= 768 && width <= 1023) {
        return JSON.stringify(new DeviceFormFactor(FormFactor.Tablet, width, height));
    } else if (width >= 1024) {
        return JSON.stringify(new DeviceFormFactor(FormFactor.Desktop, width, height));
    } else {
        return JSON.stringify(new DeviceFormFactor(FormFactor.Unknown, width, height));;
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

const testUrl = 'https://www.google.com/generate_204';

export async function getNetworkStatus() {
    try {
        await fetch(testUrl, { method: 'HEAD', mode: 'no-cors' });
        return true;
    } catch (error) {
        return false;
    }
}

export function registerNetworkListener(dotNetObject, intervalSeconds) {
    if (intervalSeconds > 0) {
        console.log("created new network listener");
        window.networkIntervalTimeout = setInterval(async () => {
            const controller = new AbortController();
            const signal = controller.signal;
            const timeout = setTimeout(() => controller.abort(), 1000); // Timeout after 1 second

            try {
                const response = await fetch(testUrl, { method: 'HEAD', mode: 'no-cors', signal });
                clearTimeout(timeout);
                if (dotNetObject) {
                    dotNetObject.invokeMethodAsync("NotifyNetworkStatusChanged", true);
                }
            } catch (error) {
                clearTimeout(timeout);
                if (dotNetObject) {
                    dotNetObject.invokeMethodAsync("NotifyNetworkStatusChanged", false);
                }
            }
        }, intervalSeconds * 1000);
    }
}

export function disposeNetworkListener() {
    clearInterval(window.networkIntervalTimeout);
    window.networkIntervalTimeout = null;
}


class DeviceFormFactor {
    constructor(formFactor, width, height) {
        this.FormFactor = formFactor;
        this.Width = width;
        this.Height = height;
    }
}

const FormFactor = {
    Desktop: 'Desktop',
    Tablet: 'Tablet',
    Mobile: 'Mobile',
    Unknown: 'Unknown'
};

//window.networkStatus = {
//    dotNetObject: null,
//    initialize: function (dotNetObject) {
//        // Register the DoNetObject to which this applies
//        this.dotNetObject = dotNetObject;
//    },
//    fetchStatus: function () {
//        const controller = new AbortController();
//        const signal = controller.signal;
//        const timeout = setTimeout(() => controller.abort(), 1000); // Timeout after 1 second

//        // Ping a Google service to see if the Internet is active.
//        fetch('https://www.google.com/generate_204', { method: 'HEAD', mode: 'no-cors', signal })
//            .then(response => {
//                clearTimeout(timeout);
//                window.networkStatus.notifyStatusChanged(true);
//            })
//            .catch(error => {
//                clearTimeout(timeout);
//                window.networkStatus.notifyStatusChanged(false);
//            });
//    },
//    async checkStatus() {
//        const controller = new AbortController();
//        const signal = controller.signal;
//        const timeout = setTimeout(() => controller.abort(), 3000);
//        try {
//            await fetch('https://www.google.com/generate_204', { method: 'HEAD', mode: 'no-cors', signal });
//            clearTimeout(timeout);
//            return true;
//        } catch (error) {
//            clearTimeout(timeout);
//            return false;
//        }
//    },
//    monitorStatus: function (seconds) {
//        if (!seconds || seconds <= 0) {
//            clearInterval(this.intervalId);
//        }
//        else if (seconds > 0) {
//            this.intervalId = setInterval(this.fetchStatus, seconds * 1000);
//        }
//    },
//    notifyStatusChanged: function (status) {
//        if (this.dotNetObject) {
//            this.dotNetObject.invokeMethodAsync("NotifyNetworkStatusChanged", status);
//        }
//    },
//    dispose: function () {
//        // Stop the setInterval operation
//        clearInterval(this.intervalId);
//    },
//};
