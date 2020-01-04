$(function () {
    //$('a[data-toggle="tooltip"]').tooltip({
    //    animated: 'fade',
    //    //placement: 'bottom',
    //    trigger: 'click',
    //    delay: { "show": 500, "hide": 100 }
    //});

    $('body').tooltip({
        selector: '[data-toggle=tooltip]',
        animated: 'fade',
        //placement: 'bottom',
        trigger: 'click',
        delay: { "show": 500, "hide": 100 }
    });
});

function SubscribeToPush() {
    navigator.serviceWorker && navigator.serviceWorker.ready.then(
        function (registration) {
            RegisterPushSubscription(registration);
        }
    );
}

function Notify(messageHtml, duration) {
    // create container if there isnt one
    if (!window.notifyContainer) {
        var container = $("<div style=\"position: fixed; top: 51px; left: 50%; transform: translate(-50%); z-index: 99999;\"></div>");
        $("body").append(container);
        window.notifyContainer = container;
    }

    // create object of new message
    var message = $(messageHtml);

    // add new message to view
    window.notifyContainer.append(message);

    // hide message after specified duration (default 2000ms)
    setTimeout(function () {
        message.hide(200, function () {
            message.remove();
        });
    }, duration ? duration : 2000);
}

function RegisterPushSubscription(serviceWorkerRegistration) {
    return new Promise(function (resolve, reject) {
        serviceWorkerRegistration.pushManager.getSubscription()
            .then(function whatevs(subscription) {
                if (subscription) {
                    //MESSAGING.subscribed = true;

                    //DATA.GetSetting("push-notified", false).then(function (setting) {
                    //    if (!setting)
                    //        MESSAGING.PushNotify(subscription);
                    //});

                    debug && console.log("Already subscribed to push notifications");
                    window.subscription = MESSAGING.subscription = subscription;
                    resolve(subscription);
                } else {
                    // create a new subscription
                    serviceWorkerRegistration.pushManager.subscribe({
                        userVisibleOnly: true,
                        applicationServerKey: MESSAGING.urlB64ToUint8Array(publicKey)
                    }).then(function (subscription) {
                        if (subscription) {
                            MESSAGING.subscribed = true;
                            debug && console.log("Newly subscribed to push notifications");
                            window.subscription = MESSAGING.subscription = subscription;

                            DATA.GetSetting("push-notified", false).then(function (setting) {
                                //if (!setting)
                                MESSAGING.PushNotify(subscription);
                            });

                            resolve(subscription);
                        }
                        reject();
                    }).catch(function (error) {
                        MESSAGING.get("/userapi/pushdenied");
                    });
                }
            });
    });
}