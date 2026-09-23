(function (global) {
  var pending = {};
  var seq = 1;

  function post(obj) {
    var payload = typeof obj === "string" ? obj : JSON.stringify(obj);
    if (global.chrome && global.chrome.webview && global.chrome.webview.postMessage) {
      global.chrome.webview.postMessage(payload);
      return;
    }
    throw new Error("WebView2 bridge not available");
  }

  if (global.chrome && global.chrome.webview) {
    global.chrome.webview.addEventListener("message", function (event) {
      var data = event.data;
      if (typeof data === "string") {
        try { data = JSON.parse(data); } catch (e) { return; }
      }
      if (!data || data.id === undefined || data.id === null) return;
      var key = String(data.id);
      var waiter = pending[key];
      if (!waiter) return;
      delete pending[key];
      if (data.error) waiter.reject(data.error);
      else waiter.resolve(data.result);
    });
  }

  function call(method, params) {
    var id = seq++;
    return new Promise(function (resolve, reject) {
      pending[String(id)] = { resolve: resolve, reject: reject };
      post({ jsonrpc: "2.0", id: id, method: method, params: params || {} });
    });
  }

  global.HostApi = { call: call };
})(window);
