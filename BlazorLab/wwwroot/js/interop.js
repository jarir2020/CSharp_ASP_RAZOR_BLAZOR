// This file is an ES module. Blazor imports it once for the interactive page.

export function getBrowserSnapshot() {
    return {
        userAgent: navigator.userAgent,
        width: window.innerWidth,
        height: window.innerHeight
    };
}

export function focusElement(elementId) {
    document.getElementById(elementId)?.focus();
}

export function saveNote(key, value) {
    window.localStorage.setItem(key, value);
}

export function loadNote(key) {
    return window.localStorage.getItem(key);
}

export async function copyText(value) {
    if (!navigator.clipboard) {
        return false;
    }

    await navigator.clipboard.writeText(value);
    return true;
}

export function startClock(dotNetReceiver) {
    // JS invokes the instance method exposed by DotNetObjectReference.
    return window.setInterval(() => {
        dotNetReceiver.invokeMethodAsync("ReceiveBrowserTime", new Date().toLocaleTimeString());
    }, 1000);
}

export function stopClock(timerId) {
    window.clearInterval(timerId);
}
