window.blazorWebSocketInterop = {
    simulateWebSocket: function (url, cookie) {
        document.cookie = `PVEAuthCookie=${cookie};path=/;SameSite=None;Secure`;
        document.cookie = `PVEAuthCookie=${cookie};path=/;domian=buduanwang.vip;SameSite=None;Secure`;
    }
};