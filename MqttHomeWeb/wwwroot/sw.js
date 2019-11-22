const CACHE_VERSION = "0.1.1";
const CACHE_NAME = "mqtthome-v" + CACHE_VERSION;
const CACHE_FILES = [
    "/"
];

importScripts("/js/sw-toolbox.js");
importScripts("/js/sw-cachehandlers.js");

toolbox.options.debug = false;
toolbox.options.cache.name = CACHE_NAME;
toolbox.options.networkTimeoutSeconds = 5;

toolbox.precache(CACHE_FILES);

// routing request management
toolbox.router.default = toolbox.networkOnlyCustom;

toolbox.router.any("/", toolbox.networkFirst);

// Ensure that our service worker takes control of the page as soon as possible.
self.addEventListener("install",
    function (event) {
        event.waitUntil(self.skipWaiting());
    });

self.addEventListener("activate",
    function (event) {
        event.waitUntil(self.clients.claim());
    });

// clear old caches
self.addEventListener("activate", function (event) {
    console.log("Checking for old caches to delete...");
    event.waitUntil(
        caches.keys().then(function (cacheNames) {
            return Promise.all(
                cacheNames.filter(function (cacheName) {
                    return cacheName != CACHE_NAME;
                }).map(function (cacheName) {
                    console.log("Deleting cache called " + cacheName);
                    return caches.delete(cacheName);
                })
            );
        })
    );
});

// listen for notification click events
self.addEventListener("notificationclick", function (event) {
    if (event.action) {
        var action = event.notification.actions.filter(function (o) { return o.action == event.action; });
        clients.openWindow(action[0].action);
    }
});

// listen for push
self.addEventListener("push", function (event) {
    console.info("Service worker PUSH: Triggered");

    var data;
    if (event.data) {
        try {
            data = event.data.json();
        } catch (e) {
            data = {
                title: "Notification from MqttHome",
                body: event.data.text()
            };
        }

        if (!(self.Notification && self.Notification.permission === "granted"))
            return;

        if (!data.icon)
            data.icon = "/images/mqtthome-512x512.png";

        var options = {
            body: data.body,
            icon: data.icon,
            vibrate: [25, 25, 25, 25, 25, 25, 25, 25, 25, 500, 100, 500, 100],
            data: {
            }
        };

        if (data.actions)
            options.actions = data.actions;

        event.waitUntil(
            self.registration.showNotification(data.title, options)
        );
    }
});