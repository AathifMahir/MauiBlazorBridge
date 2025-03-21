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
