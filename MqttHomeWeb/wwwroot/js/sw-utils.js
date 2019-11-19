"use strict";

var debug = false;

// register the service worker
navigator.serviceWorker && navigator.serviceWorker.register("/sw.js").then(
    function (registration) { // succeeded
        debug && console.log("Service worker registered with scope: ", registration.scope);
    },
    function () { // failed
        debug && console.log("Failed to register service worker");
    }
);