window.resizeListener = null;
window.currentFormFactor = 'Unknown';

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

export function initialize(dotnetObject) {
    if (!window.resizeListener) {
        console.log("created new listeners");
        window.currentFormFactor = getFormFactor();

        window.resizeListener = async () => {
            const formFactor = getFormFactor();
            if (window.currentIdiom !== formFactor) {
                window.currentIdiom = formFactor;
                await dotnetObject.invokeMethodAsync("NotifyFormFactorChanged", formFactor);
            }
        };
        window.addEventListener('resize', window.resizeListener);
    }
}

export function dispose() {
    if (window.resizeListener) {
        console.log("disposed all the listeners");
        window.removeEventListener('resize', window.resizeListener);
        window.resizeListener = null;
    }
}