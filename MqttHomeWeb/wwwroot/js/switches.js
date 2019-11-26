function SwitchOff(id) {
    if (confirm('Are you sure you want to turn ' + id + ' off?'))
        $.get("/switch/off/" + id, function (response) {
            Notify(response);
        });
}

function SwitchOn(id) {
    if (confirm('Are you sure you want to turn ' + id + ' on?'))
        $.get("/switch/on/" + id, function (response) {
            Notify(response);
        });
}