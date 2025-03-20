window.networkIntervalTimeout = null;
window.isInternetConnected = false;

const testUrl = 'https://www.google.com/generate_204';


export async function getNetworkStatus() {
    const result = await fetchNetworkStatus();
    window.isInternetConnected = result;
    return result;
}

export async function initializeListener(dotNetObject, intervalSeconds) {
    if (intervalSeconds > 0) {
        window.networkIntervalTimeout = setInterval(async () => {
            const controller = new AbortController();
            const signal = controller.signal;
            const timeout = setTimeout(() => controller.abort(), 1000);

            if (window.location.hostname === 'localhost') {
                console.log('Checking network status...', new Date().toLocaleTimeString());
            }
            const result = await fetchNetworkStatus(signal);

            if (result !== window.isInternetConnected) {

                window.isInternetConnected = result;

                if (dotNetObject) {
                    dotNetObject.invokeMethodAsync("NotifyConnectivityStatusChanged", result);
                }
            }
            clearTimeout(timeout);

        }, intervalSeconds * 1000);
    }
}

export function disposeListener() {
    clearInterval(window.networkIntervalTimeout);
    window.networkIntervalTimeout = null;
}

async function fetchNetworkStatus(abortSignal = null) {
    try {
        if (abortSignal) {
            await fetch(testUrl, { method: 'HEAD', mode: 'no-cors', signal: abortSignal });
        }
        else {
            await fetch(testUrl, { method: 'HEAD', mode: 'no-cors' });
        }
        return true;
    } catch (error) {
        return false;
    }
}