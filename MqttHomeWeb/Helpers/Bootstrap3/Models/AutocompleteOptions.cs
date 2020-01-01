using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Softwarehouse.Bootstrap3
{
    public class AutocompleteOptions
    {
        public eAutocompleteSourceType SourceType { get; set; } = eAutocompleteSourceType.StaticList;

        public List<SelectListItem> StaticListItems { get; set; } = new List<SelectListItem>();

        public string DynamicAjaxUrl { get; set; }

        /// <summary>
        /// Number of items to display in results (Default 10)
        /// </summary>
        public int ItemsToShow { get; set; } = 10;

        /// <summary>
        /// Minimum characters user must type before results are displayed (Default 1)
        /// </summary>
        public int MinLength { get; set; } = 1;

        /// <summary>
        /// Number of pixels the scrollable parent container scrolled down (Default 0)
        /// </summary>
        public int ScrollHeight { get; set; } = 0;

        /// <summary>
        /// Auto selects the first item (Default true)
        /// </summary>
        public bool AutoSelect { get; set; } = true;

        /// <summary>
        /// Callback - must be a javascript function(){ }
        /// </summary>
        public string OnAfterSelect { get; set; } = "$.noop";

        /// <summary>
        /// Callback - must be a javascript function(){ }
        /// </summary>
        public string OnAfterEmptySelect { get; set; } = "$.noop";

        /// <summary>
        /// Ajax lookup delay prior to keypress (milliseconds) (Default 300)
        /// </summary>
        public int Delay { get; set; } = 300;

        /// <summary>
        /// Whether the suggestion list should contain the searched term (Default false)
        /// </summary>
        public bool Contains { get; set; } = false;

        /// <summary>
        /// Whether to clear the input box if no suggestion is selected (Default false)
        /// </summary>
        public bool Matches { get; set; } = false;

        public string MenuTemplate { get; set; } = "<ul class=\"typeahead dropdown-menu\" role=\"listbox\"></ul>";
        public string ItemTemplate { get; set; } = "<li><a class=\"dropdown-item\" href=\"#\" role=\"option\"></a></li>";
        public string HeaderTemplate { get; set; } = "<li class=\"dropdown-header\"></li>";
        public string HeaderDividerTemplate { get; set; } = "<li class=\"divider\" role=\"separator\"></li>";
        public string ItemContentSelector { get; set; } = "a";

        public override string ToString()
        {
            if (!(StaticListItems?.Any() ?? false) && !string.IsNullOrEmpty(DynamicAjaxUrl) && SourceType == eAutocompleteSourceType.StaticList)
                SourceType = eAutocompleteSourceType.DynamicAjax;

            // render the options
            var output = @"";

            // data source
            if (SourceType == eAutocompleteSourceType.DynamicAjax)
            {
                output += $@"source: function (query, process) {{
    return $.get('{DynamicAjaxUrl}?q=' + query + '&limit={ItemsToShow}&contains={Contains.ToString().ToLower()}', function (data) {{
        return process(data);
    }});
}},";
            }
            else
            {
                output += $@"source: [{string.Join(",", StaticListItems.Select(o => $@"{{ id: ""{o.Value}"", name: ""{o.Text}"" }}"))}],";
            }

            // how many items to show
            output += $@"
items: {ItemsToShow},";

            // min length to trigger the suggestion list
            output += $@"
minLength: {MinLength},";

            // number of pixels the scrollable parent container scrolled down
            output += $@"
scrollHeight: {ScrollHeight},";

            // callbacks
            output += $@"
afterSelect: function(item) {{
                    $('input[name=' + this.$element[0].name.replace(""_typeahead_input"", """") + ']').val(item.id).trigger(""change"");
                    {OnAfterSelect}
                }},";

            // if matches is set to true then override the afterSelect and afterEmptySelect methods
            if (Matches)
            {
                output += $@"
autoSelect: false,";

                output += $@"
afterEmptySelect: function(){{
                    $('input[name=' + this.$element[0].name + ']').val('');
                    $('input[name=' + this.$element[0].name.replace(""_typeahead_input"", """") + ']').val('').trigger(""change"");
                    {OnAfterEmptySelect}
                }},";
            }
            else
            {
                // auto selects the first item
                output += $@"
autoSelect: {AutoSelect.ToString().ToLower()},";

                output += $@"
afterEmptySelect: {OnAfterEmptySelect},";

            }

            // adds an item to the end of the list - NO IDEA WHAT THIS DOES
            output += $@"
addItem: false,";

            // delay between lookups
            output += $@"
delay: {Delay},";

            // templates
            output += $@"
menu: ""{MenuTemplate.Replace("\"", "\\\"")}"",";
            output += $@"
item: ""{ItemTemplate.Replace("\"", "\\\"")}"",";
            output += $@"
headerHtml: ""{HeaderTemplate.Replace("\"", "\\\"")}"",";
            output += $@"
headerDivider: ""{HeaderDividerTemplate.Replace("\"", "\\\"")}"",";
            output += $@"
itemContentSelector: ""{ItemContentSelector.Replace("\"", "\\\"")}""";

            return output;
        }
    }

    public enum eAutocompleteSourceType
    {
        StaticList,
        DynamicAjax
    }
}
