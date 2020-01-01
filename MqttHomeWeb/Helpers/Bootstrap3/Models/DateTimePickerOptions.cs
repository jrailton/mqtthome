using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Softwarehouse.Bootstrap3
{
    public class DateTimePickerOptions
    {
        /// <summary>
        /// Non-conforming date/time formats -- be careful
        /// </summary>
        public string format { get; set; } = "D MMM YYYY HH:mm";

        /// <summary>
        /// If true, the picker will show on textbox focus and icon click when used in a button group
        /// </summary>
        public bool allowInputToggle { get; set; } = true;

        public string maxDate { get; set; } = null;
        public string minDate { get; set; } = null;

        public bool showTodayButton { get; set; } = true;
        public bool showClose { get; set; } = true;

        /// <summary>
        /// Will cause the date picker to stay open after selecting a date.
        /// </summary>
        public bool keepOpen { get; set; } = false;
        /// <summary>
        /// Will display the picker inline without the need of a input field. This will also hide borders and shadows.
        /// </summary>
        public bool inline { get; set; } = false;

        /// <summary>
        /// Will cause the date picker to not revert or overwrite invalid dates.
        /// </summary>
        public bool keepInvalid { get; set; } = true;

        /// <summary>
        /// If false, the textbox will not be given focus when the picker is shown.
        /// </summary>
        public bool focusOnShow { get; set; } = true;
        public bool showClear { get; set; } = true;
        public int[] daysOfWeekDisabled { get; set; }
        public int[] enabledHours { get; set; }
        public int[] disabledHours { get; set; }

        /// <summary>
        /// Takes an array of string, Date or moment of values and allows the user to select those days. Setting this takes precedence over options.minDate, options.maxDate configuration. May conflict with disabledDates option
        /// </summary>
        public string[] enabledDates { get; set; }
        /// <summary>
        /// Takes an array of string, Date or moment of values and disallows the user to select those days. Setting this takes precedence over options.minDate, options.maxDate configuration. Also calling this function removes the configuration of options.enabledDates if such exist.
        /// </summary>
        public string[] disabledDates { get; set; }
        /// <summary>
        /// Takes a boolean variable to set if the week numbers will appear to the left on the days view
        /// </summary>
        public bool calendarWeeks { get; set; } = false;

        /// <summary>
        /// If sideBySide is true and the time picker is used, both components will display side by side instead of collapsing.
        /// </summary>
        public bool sideBySide { get; set; } = false;

        [JsonConverter(typeof(StringEnumConverter))]
        public eViewMode viewMode { get; set; } = eViewMode.days;

        [JsonConverter(typeof(StringEnumConverter))]
        public eToolbarPlacement toolbarPlacement { get; set; } = eToolbarPlacement.@default;

        public Icons icons { get; set; } = new Icons();

        /// <summary>
        /// Changes the heading of the datepicker when in "days" view. Default is "MMMM YYYY"
        /// </summary>
        public string dayViewHeaderFormat { get; set; }

        /// <summary>
        /// Number of minutes the up/down arrow's will move the minutes value in the time picker. Default is 1.
        /// </summary>
        public int stepping { get; set; }
    }

    public class Icons
    {
        public string today { get; set; } = "fa fa-calendar";
        public string time { get; set; } =  "fa fa-clock-o";
        public string date { get; set; } =  "fa fa-calendar";
        public string up { get; set; } =  "fa fa-chevron-up";
        public string down { get; set; } =  "fa fa-chevron-down";
        public string previous { get; set; } =  "fa fa-chevron-left";
        public string next { get; set; } =  "fa fa-chevron-right";
        public string clear { get; set; } = "fa fa-ban";
        public string close { get; set; } = "fa fa-window-close-o";
    }

    public enum eViewMode
    {
        decades,
        years,
        months,
        days
    }

    public enum eToolbarPlacement
    {
        @default,
        top,
        bottom
    }
}
