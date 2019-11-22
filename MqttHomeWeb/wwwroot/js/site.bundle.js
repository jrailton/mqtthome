function SubscribeToPush() {
    navigator.serviceWorker && navigator.serviceWorker.ready.then(
        function (registration) {
            RegisterPushSubscription(registration);
        }
    );
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
function SwitchOff(id) {
    if (confirm('Are you sure you want to turn ' + id + ' off?'))
        $("#message-content").load("/switch/off/" + id);
}

function SwitchOn(id) {
    if (confirm('Are you sure you want to turn ' + id + ' on?'))
        $("#message-content").load("/switch/on/" + id);
}