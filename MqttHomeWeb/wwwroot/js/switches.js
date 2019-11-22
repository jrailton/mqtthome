function SwitchOff(id) {
    if (confirm('Are you sure you want to turn ' + id + ' off?'))
        $("#message-content").load("/switch/off/" + id);
}

function SwitchOn(id) {
    if (confirm('Are you sure you want to turn ' + id + ' on?'))
        $("#message-content").load("/switch/on/" + id);
}