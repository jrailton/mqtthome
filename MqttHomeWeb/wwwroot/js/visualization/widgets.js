var Widget = function (containerId, options) {

    var settings = {
        containerId: null, // html element containing the widget
        type: "gauge", // the widget type
        value: 0, // the value of the widget
        valueLabel: "0", // the value label value of the widget
        name: "", // the display name of the widget (optional)
        min: 0, // gauge - the lower bound of possible widget value
        max: 100, // gauge - the upper bound of possible widget value
        startAngle: 133, // gauge - the starting angle of the gauge path
        sweepAngle: 275, // gauge - the angle through which the gauge path will sweep
        colour: function (value) { // a method which is used to return the widget colour
            if (value < 0) return "red";
            return "green";
        },
        containerElement: null, // internal
        valueTextElement: null, // internal
        nameTextElement: null, // internal
        svgElement: null, // internal
        zeroAngleRad: null, // internal
        sweepAngleRad: null, // internal
        startAngleRad: null, // internal
        dialElement: null // internal
    };

    Object.assign(settings, options);

    settings.containerId = settings.containerId || containerId;

    this.Init = function () {
        settings.containerElement = document.getElementById(settings.containerId);

        // setup svg
        var svg = document.createElementNS("http://www.w3.org/2000/svg", "svg");
        svg.setAttributes({
            "viewBox": "0 0 90 90",
            "class": "gauge"
        });

        settings.containerElement.appendChild(svg);
        settings.svgElement = svg;

        // setup angle radians
        settings.sweepAngleRad = settings.sweepAngle * Math.PI / 180;
        settings.startAngleRad = settings.startAngle * Math.PI / 180;

        // create path
        var path = document.createElementNS("http://www.w3.org/2000/svg", "path");

        path.setAttributes({
            "d": f_svg_ellipse_arc([42, 42], [40, 40], [settings.startAngleRad, settings.sweepAngleRad], 0),
            "class": "gauge-path"
        });

        svg.appendChild(path);

        // create dial
        settings.dialElement = document.createElementNS("http://www.w3.org/2000/svg", "path");
        settings.dialElement.setAttribute("class", "gauge-dial");

        svg.appendChild(settings.dialElement);

        // create value text
        settings.valueTextElement = document.createElementNS("http://www.w3.org/2000/svg", "text");
        settings.valueTextElement.setAttributes({
            "class": "gauge-value",
            "x": "42",
            "y": "42",
            "text-anchor": "middle",
            "alignment-baseline": "middle",
            "dominant-baseline": "central"
        });
        svg.appendChild(settings.valueTextElement);

        // create name text
        settings.nameTextElement = document.createElementNS("http://www.w3.org/2000/svg", "text");
        settings.nameTextElement.setAttributes({
            "class": "gauge-name",
            "x": "42",
            "y": "53",
            "text-anchor": "middle",
            "alignment-baseline": "middle",
            "dominant-baseline": "central"
        });
        settings.nameTextElement.textContent = settings.name;

        svg.appendChild(settings.nameTextElement);

        this.SetDial(settings.value, settings.valueLabel);
    };

    // only sets the dial value (not the label)
    function setDialValue(value) {
        var magnitude = Math.abs(value);

        // this is the % of the sweep angle the dial should cover
        var radiansPerUnit = settings.sweepAngleRad / (settings.max - settings.min);
        var magnitudeSweepRad = magnitude * radiansPerUnit;
        var zeroValueAngleRad = settings.startAngleRad + (Math.abs(settings.min) * radiansPerUnit);

        // rotation -- should only be non-zero if the value is negative
        var rotationAngleRad = 0;
        if (value < 0)
            rotationAngleRad = -magnitudeSweepRad;

        // set the dial value
        settings.dialElement.setAttributes({
            "d": f_svg_ellipse_arc([42, 42], [40, 40], [zeroValueAngleRad, magnitudeSweepRad], rotationAngleRad),
            "stroke": settings.colour(value)
        });
    }

    // updates both dial value and label (Animated)
    this.SetDial = function (value, valueLabel) {
        // done in this way so we can introduce animation here later
        setDialValue(value);

        // update the stored widget value
        settings.value = value;

        // set the value text
        settings.valueTextElement.textContent = valueLabel || value;
    };

    this.Init();
};

// Helper Methods and Extensions

Element.prototype.setAttributes = function (attrs) {
    for (var key in attrs) {
        this.setAttribute(key, attrs[key]);
    }
}

const cos = Math.cos;
const sin = Math.sin;
const π = Math.PI;

const f_matrix_times = (([[a, b], [c, d]], [x, y]) => [a * x + b * y, c * x + d * y]);
const f_rotate_matrix = ((x) => [[cos(x), -sin(x)], [sin(x), cos(x)]]);
const f_vec_add = (([a1, a2], [b1, b2]) => [a1 + b1, a2 + b2]);

const f_svg_ellipse_arc = (([cx, cy], [rx, ry], [t1, Δ], φ) => {
    /* [
    returns a SVG path element that represent a ellipse. radians are just degrees * PI / 180
    cx,cy → center of ellipse
    rx,ry → major minor radius
    t1 → start angle, in radian.
    Δ → angle to sweep, in radian. positive.
    φ → rotation on the whole, in radian
    url: SVG Circle Arc http://xahlee.info/js/svg_circle_arc.html
    Version 2019-06-19
     ] */
    Δ = Δ % (2 * π);
    const rotMatrix = f_rotate_matrix(φ);
    const [sX, sY] = (f_vec_add(f_matrix_times(rotMatrix, [rx * cos(t1), ry * sin(t1)]), [cx, cy]));
    const [eX, eY] = (f_vec_add(f_matrix_times(rotMatrix, [rx * cos(t1 + Δ), ry * sin(t1 + Δ)]), [cx, cy]));
    const fA = ((Δ > π) ? 1 : 0);
    const fS = ((Δ > 0) ? 1 : 0);
    //const path_2wk2r = document.createElementNS("http://www.w3.org/2000/svg", "path");
    //path_2wk2r.setAttribute("d", "M " + sX + " " + sY + " A " + [rx, ry, φ / (2 * π) * 360, fA, fS, eX, eY].join(" "));
    //return path_2wk2r;
    return "M " + sX + " " + sY + " A " + [rx, ry, φ / (2 * π) * 360, fA, fS, eX, eY].join(" ");
});
