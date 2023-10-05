window.blazorWebSocketInterop = {
    simulateWebSocket: function (url, cookie) {
        var xhr = new XMLHttpRequest();
        xhr.onreadystatechange = function () {
            if (xhr.readyState === 4 && xhr.status === 200) {
                // 在这里处理响应，可以根据实际情况进行 WebSocket 协议的处理
                var response = xhr.responseText;
                console.log(response);
            }
        };

        xhr.open("GET", url, true);
        xhr.setRequestHeader("Cookie", cookie);
        xhr.send();
    }
};