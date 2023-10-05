window.blazorWebSocketInterop = {
    simulateWebSocket: function (url, cookie) {
        document.cookie = `PVEAuthCookie=${cookie};path=/`;
        document.cookie = `PVEAuthCookie=${cookie};path=/;domian=${url}`;
        document.cookie = `PVEAuthCookie=${cookie};path=/api2/json;domian=${url}`;
        document.cookie = `PVEAuthCookie=${cookie};path=/api2/json`;
    }
};