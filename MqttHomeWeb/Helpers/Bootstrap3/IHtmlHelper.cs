using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Newtonsoft.Json;

namespace Softwarehouse.Bootstrap3
{
    public static class Helpers
    {
        private static List<string> _attributesAlreadySpecified = new List<string>();
        
        public static string ToHtmlString(this IHtmlContent content)
        {
            using (var writer = new System.IO.StringWriter())
            {
                content.WriteTo(writer, HtmlEncoder.Default);
                return writer.ToString();
            }
        }

        public static IHtmlContent RenderMessagesBootstrap(this IHtmlHelper helper, string alertId = null)
        {
            string output = "";
            int alertNumber = 0;

            if (string.IsNullOrEmpty(alertId))
                alertId = $"swh-{idExtension}";

            var conversions = new Dictionary<eAlertStyle, string>
                {
                    {eAlertStyle.Danger, "error"},
                    {eAlertStyle.Warning, "info"},
                    {eAlertStyle.Success, "message"}
                };

            foreach (var conversion in conversions)
            {
                var message = helper.ViewContext.TempData[conversion.Value]?.ToString();
                if (!string.IsNullOrEmpty(message))
                    output += RenderAlert(conversion.Key, message, $"{alertId}-{alertNumber++}");

                message = helper.ViewContext.ViewData[conversion.Value]?.ToString();
                if (!string.IsNullOrEmpty(message))
                    output += RenderAlert(conversion.Key, message, $"{alertId}-{alertNumber++}");
            }

            output += RenderValidationSummary(helper, $"{alertId}-{alertNumber}");

            return new HtmlString(output);
        }

        private static string RenderValidationSummary(this IHtmlHelper helper, string tableId)
        {
            if (!helper.ViewData.ModelState.IsValid)
            {
                string output = "";
                foreach (var keyValuePair in helper.ViewData.ModelState)
                    foreach (var modelError in keyValuePair.Value.Errors)
                        output += $"{modelError.ErrorMessage}<br />";

                // strip last <br />
                if (output.EndsWith("<br />"))
                    output = output.Substring(0, output.Length - 6);

                if (string.IsNullOrEmpty(output))
                    output = "Something went wrong :( Sorry...";

                return RenderAlert(eAlertStyle.Danger, output, tableId);
            }
            return string.Empty;
        }

        private static string RenderAlert(eAlertStyle style, string message, string alertId = null)
        {
            if (string.IsNullOrEmpty(alertId))
                alertId = $"swh-{idExtension}";

            return $@"<div id=""{alertId}"" class=""alert alert-{style.ToString().ToLower()} alert-dismissable"">
    <a href=""#"" class=""close"" data-dismiss=""alert"" aria-label=""close"">&times;</a>
    {message}
</div>";
        }

        public static IHtmlContent AutocompleteBootstrap(this IHtmlHelper html, string name, SelectListItem value, string label = null, string description = null, Infotip infoTip = null, List<InputGroupAddon> inputGroupAddons = null, AutocompleteOptions options = null, object htmlAttributes = null)
        {
            string autocompleteInputName = $"{name}_typeahead_input";
            value = value ?? new SelectListItem();

            var output = TextBoxBootstrap(html, autocompleteInputName, value.Text, label, description, infoTip, inputGroupAddons, htmlAttributes).ToHtmlString();

            options = options ?? new AutocompleteOptions();

            output += $@"<input type=""hidden"" name=""{name}"" value=""{value.Value}"" />
";
            output += $@"
<script type=""text/javascript"">
    $(function(){{
        $(""input[name={autocompleteInputName}]"").typeahead({{
            {options}
        }}).keyup(function(){{
            if(!this.value){{
                 $(""input[name={name}]"").val('');
            }}else{{
                var current = $(this).typeahead(""getActive"");
                if (current) {{
                    // Some item from your model is active!
                    if (current.name == $(this).val()) {{
                        // This means the exact match is found. Use toLowerCase() if you want case insensitive match.  
                    }} else {{
                        // This means it is only a partial match, clear hidden field
                        $(""input[name={name}]"").val('');
                    }}
                }}else {{
                // Nothing is active so it is a new value (or maybe empty value)
                
                }}
            }}
        }});
    }});
</script>
";
            return new HtmlString(output);
        }

        public static IHtmlContent AutocompleteBootstrap(this IHtmlHelper html, string name, SelectListItem value, string label = null, List<InputGroupAddon> inputGroupAddons = null, AutocompleteOptions options = null, object htmlAttributes = null)
        {
            return AutocompleteBootstrap(html, name, value, label, null, null, inputGroupAddons, options, htmlAttributes);
        }

        public static IHtmlContent LabelBootstrap(this IHtmlHelper html, string name, string label, IDictionary<string, object> htmlAttributes)
        {
            return LabelBootstrap(html, name, label, null, htmlAttributes);
        }

        public static IHtmlContent LabelBootstrap(this IHtmlHelper html, string name, string label, Infotip infotip, IDictionary<string, object> htmlAttributes)
        {
            return new HtmlString($"<label {AttributeValueFor(name, htmlAttributes, "for", name, false)}>{label}</label>{InfotipBootstrap(html, infotip)}");
        }

        public static IHtmlContent InfotipBootstrap(this IHtmlHelper helper, Infotip infotip)
        {
            return infotip?.Render();
        }

        public static IHtmlContent TextBoxBootstrap(this IHtmlHelper helper, string name, object value, string label = null, string description = null, Infotip infotip = null, List<InputGroupAddon> inputGroupAddons = null, object htmlAttributes = null)
        {
            var attributes = htmlAttributes.ToDictionary();

            inputGroupAddons = inputGroupAddons ?? new List<InputGroupAddon>();

            var output = $@"
<div class=""form-group{((helper?.ViewData.ModelState[name]?.Errors.Any() ?? false) ? " has-error" : "")}"">
    {
                    (string.IsNullOrEmpty(label)
                        ? ""
                        : LabelBootstrap(helper, name, label, infotip, attributes).ToHtmlString())
                }
    {(inputGroupAddons.Any() ? @"<div class=""input-group"">" : "")}
        {
                    (inputGroupAddons.Any(a => a.Type == "before")
                        ? inputGroupAddons.First(a => a.Type == "before").ToString()
                        : "")
                }
        <input type=""text"" 
            {AttributeValueFor(name, attributes, "name", name, false)} 
            {AttributeValueFor(name, attributes, "id", name, false)} 
            {AttributeValueFor(name, attributes, "class", "form-control", true)} 
            value=""{value}"" 
            {InsertOtherAttributes(name, attributes)}
        />
        {
                    (inputGroupAddons.Any(a => a.Type == "after")
                        ? inputGroupAddons.First(a => a.Type == "after").ToString()
                        : "")
                }
    {(inputGroupAddons.Any() ? "</div>" : "")}
    {(string.IsNullOrEmpty(description) ? "" : $"<small>{description}</small>")}
</div>";

            return new HtmlString(output);
        }

        public static IHtmlContent TextBoxBootstrap(this IHtmlHelper helper, string name, object value, string label, object htmlAttributes)
        {
            return TextBoxBootstrap(helper, name, value, label, null, null, htmlAttributes);
        }

        public static IHtmlContent TextBoxBootstrap(this IHtmlHelper helper, string name, object value, string label, string description, object htmlAttributes)
        {
            return TextBoxBootstrap(helper, name, value, label, description, null, null, htmlAttributes);
        }

        public static IHtmlContent TextBoxBootstrap(this IHtmlHelper helper, string name, object value, string label, string description, Infotip infotip, object htmlAttributes)
        {
            return TextBoxBootstrap(helper, name, value, label, description, infotip, null, htmlAttributes);
        }

        public static IHtmlContent TextBoxBootstrap(this IHtmlHelper helper, string name, object value, string label)
        {
            return TextBoxBootstrap(helper, name, value, label, null);
        }

        public static IHtmlContent TextBoxBootstrap(this IHtmlHelper helper, string name, object value, string label, string description)
        {
            return TextBoxBootstrap(helper, name, value, label, description, null, null);
        }

        public static IHtmlContent PasswordBootstrap(this IHtmlHelper helper, string name, string label = null, string description = null, object htmlAttributes = null)
        {
            var attributes = htmlAttributes.ToDictionary();

            var id = name;
            if (attributes.ContainsKey("id"))
                id = attributes["id"].ToString();

            var output = $@"
<div id=""{id}-showhidegroup"" class=""form-group{((helper.ViewData.ModelState[name]?.Errors.Any() ?? false) ? " has-error" : "")}"">
    {
                    (string.IsNullOrEmpty(label)
                        ? ""
                        : $"<label {AttributeValueFor(name, attributes, "for", name, false)}>{label}</label>")
                }
        <div class=""input-group"">
            <input type=""password"" 
                {AttributeValueFor(name, attributes, "name", name, false)} 
                {AttributeValueFor(name, attributes, "id", name, false)} 
                {AttributeValueFor(name, attributes, "class", "form-control", true)} 
                {InsertOtherAttributes(name, attributes)}
            />
            <span class=""input-group-addon"">
                <span title=""Show Password"" class=""glyphicon glyphicon-eye-open""></span>
            </span>
        </div>
        {(string.IsNullOrEmpty(description) ? "" : $"<small>{description}</small>")}
</div>

<script type=""text/javascript"">
    $(function(){{
        $('#{id}-showhidegroup .glyphicon').on('click', function() {{
            var $this = $(this);
            $this
                .toggleClass('glyphicon-eye-close')
                .toggleClass('glyphicon-eye-open')
                .attr('title', $this.hasClass('glyphicon-eye-close') ? 'Hide Password' : 'Show Password');

            $('#{id}').attr('type', $this.hasClass('glyphicon-eye-close') ? 'text' : 'password');
        }});
    }});
</script>";

            return new HtmlString(output);
        }

        public static IHtmlContent PasswordBootstrap(this IHtmlHelper helper, string name, string label, string description)
        {
            return PasswordBootstrap(helper, name, label, description, null);
        }

        public static IHtmlContent PasswordBootstrap(this IHtmlHelper helper, string name, string label, object htmlAttributes)
        {
            return PasswordBootstrap(helper, name, label, null, htmlAttributes);
        }

        public static IHtmlContent CheckboxBootstrap(this IHtmlHelper html, string name, bool isChecked, string label = "", object htmlAttributes = null)
        {
            var attributes = htmlAttributes.ToDictionary();

            var id = name;

            // if "value" hasnt been specified, or it has and its "true" then this is a boolean checkbox so add a "false" afterwards
            if (attributes != null && attributes.ContainsKey("id"))
                id = attributes["id"].ToString();

            var output = $@"<input type=""checkbox"" {AttributeValueFor(name, attributes, "id", name, false)} name=""{name}"" {AttributeValueFor(name, attributes, "value", "true", false)} class=""form-control""{(isChecked ? " checked=\"checked\"" : "")} />";

            output = $@"<div {AttributeValueFor(name, attributes, "class", "checkbox checkbox-primary", true)}>
{output}
<label for=""{id}"">{label}</label></div>";

            // if "value" hasnt been specified, or it has and its "true" then this is a boolean checkbox so add a "false" afterwards
            if (attributes == null || !attributes.ContainsKey("value") || attributes["value"].ToString().Equals("true"))
                output += $@"<input type=""hidden"" name=""{name}"" value=""false"" />";

            return new HtmlString(output);
        }

        public static IHtmlContent RadioButtonBootstrap(this IHtmlHelper html, string name, object value, bool isChecked, string label = "", object htmlAttributes = null)
        {
            var attributes = htmlAttributes.ToDictionary();

            var id = $"{name}-{idExtension}"; // a default value for the element's ID

            if (attributes.ContainsKey("id"))
                id = attributes["id"].ToString();

            var output = $@"<div class=""radio radio-primary"" style=""display: inline-block; margin-right: 10px;"">
                <input type=""radio"" id=""{id}"" name=""{name}"" value=""{value}"" {(isChecked ? @"checked=""checked""" : "")} />
                <label for=""{id}"">{label}</label>                
            </div>";

            return new HtmlString(output);
        }

        public static IHtmlContent ButtonBootstrap(this IHtmlHelper html, string text, object htmlAttributes = null)
        {
            return ButtonBootstrap(html, text, htmlAttributes.ToDictionary());
        }

        public static IHtmlContent ButtonBootstrap(this IHtmlHelper html, string text, IDictionary<string, object> htmlAttributes)
        {
            var id = $"swh-{idExtension}"; // a default value for the element's ID

            if (htmlAttributes.ContainsKey("id"))
                id = htmlAttributes["id"].ToString();

            var output = $@"
<button {AttributeValueFor(id, htmlAttributes, "name", id, false)} {AttributeValueFor(id, htmlAttributes, "id", id, false)} {AttributeValueFor(id, htmlAttributes, "type", "button", false)} {AttributeValueFor(id, htmlAttributes, "class", "btn btn-default", false)} {InsertOtherAttributes(id, htmlAttributes)}>
    {text}
</button>
";
            return new HtmlString(output);
        }

        public static IHtmlContent SubmitButtonBootstrap(this IHtmlHelper html, string text, object htmlAttributes = null)
        {
            var attributes = htmlAttributes.ToDictionary();

            attributes.AddProperty("type", "submit");
            attributes.AddProperty("class", "btn btn-primary", false);

            return ButtonBootstrap(html, text, attributes);
        }

        public static IHtmlContent TextAreaBootstrap(this IHtmlHelper helper, string name, object value, string label = null, string description = null, object htmlAttributes = null)
        {
            var attributes = htmlAttributes.ToDictionary();

            var output = $@"
<div class=""form-group"">
    {
                    (string.IsNullOrEmpty(label)
                        ? ""
                        : $"<label {AttributeValueFor(name, attributes, "for", name, false)}>{label}</label>")
                }
        <textarea
            {AttributeValueFor(name, attributes, "name", name, false)} 
            {AttributeValueFor(name, attributes, "id", name, false)} 
            {AttributeValueFor(name, attributes, "class", "form-control", true)} 
            {InsertOtherAttributes(name, attributes)}
        >{value}</textarea>
        {(string.IsNullOrEmpty(description) ? "" : $"<small>{description}</small>")}
</div>";

            return new HtmlString(output);
        }

        public static IHtmlContent TextAreaBootstrap(this IHtmlHelper helper, string name, object value, string label, string description)
        {
            return TextAreaBootstrap(helper, name, value, label, description, null);
        }

        public static IHtmlContent TextAreaBootstrap(this IHtmlHelper helper, string name, object value, string label)
        {
            return TextAreaBootstrap(helper, name, value, label, null, null);
        }

        public static IHtmlContent TextAreaBootstrap(this IHtmlHelper helper, string name, object value, string label, object htmlAttributes)
        {
            return TextAreaBootstrap(helper, name, value, label, null, htmlAttributes);
        }

        public static IHtmlContent DropDownListBootstrap(this IHtmlHelper helper, string name, IEnumerable<SelectListItem> selectList)
        {
            return DropDownListBootstrap(helper, name, selectList, null, null, null, null);
        }

        public static IHtmlContent DropDownListBootstrap(this IHtmlHelper helper, string name, IEnumerable<SelectListItem> selectList, string label)
        {
            return DropDownListBootstrap(helper, name, selectList, label, null, null, null);
        }

        public static IHtmlContent DropDownListBootstrap(this IHtmlHelper helper, string name, IEnumerable<SelectListItem> selectList, string label, string optionLabel)
        {
            return DropDownListBootstrap(helper, name, selectList, label, optionLabel, null, null);
        }

        public static IHtmlContent DropDownListBootstrap(this IHtmlHelper helper, string name, IEnumerable<SelectListItem> selectList, string label = null, string optionLabel = null, object htmlAttributes = null)
        {
            return DropDownListBootstrap(helper, name, selectList, label, optionLabel, null, htmlAttributes);
        }

        public static IHtmlContent DropDownListBootstrap(this IHtmlHelper helper, string name, IEnumerable<SelectListItem> selectList, string label = null, string optionLabel = null, string description = null, object htmlAttributes = null)
        {
            var attributes = htmlAttributes.ToDictionary();

            selectList = selectList ?? new List<SelectListItem>();

            var options = new StringBuilder();

            if (optionLabel != null) // the optionLabel can be empty string and still be included here
                options.Append($@"<option value>{optionLabel}</option>");

            // populate the options for the dropdown
            foreach (var item in selectList)
            {
                options.Append(
                    $@"<option {(item.Selected ? AttributeValueFor(name, attributes, "selected", "selected", false) : "")} {(string.IsNullOrEmpty(item.Value) ? "" : $@"value=""{item.Value}""")}>{item.Text}</option>");
            }

            var output = $@"
<div class=""form-group"">
    {
                    (string.IsNullOrEmpty(label)
                        ? ""
                        : $"<label {AttributeValueFor(name, attributes, "for", name, false)}>{label}</label>")
                }
        <select
            {AttributeValueFor(name, attributes, "name", name, false)} 
            {AttributeValueFor(name, attributes, "id", name, false)} 
            {AttributeValueFor(name, attributes, "class", "form-control", true)}
            {InsertOtherAttributes(name, attributes)}
        >
            {options}
        </select>
        {(string.IsNullOrEmpty(description) ? "" : $"<small>{description}</small>")}
</div>";

            return new HtmlString(output);
        }

        public static IHtmlContent DateTimePickerBootstrap(this IHtmlHelper helper, string name, object value, string label = null, DateTimePickerOptions datetimePickerOptions = null)
        {
            return DateTimePickerBootstrap(helper, name, value, label, null, null, datetimePickerOptions);
        }

        public static IHtmlContent DateTimePickerBootstrap(this IHtmlHelper helper, string name, object value, string label = null, string description = null, Infotip infotip = null, DateTimePickerOptions datetimePickerOptions = null, object htmlAttributes = null)
        {
            var attributes = htmlAttributes.ToDictionary();
            attributes.AddProperty("autocomplete", "off");

            datetimePickerOptions = datetimePickerOptions ?? new DateTimePickerOptions();

            var output = new StringBuilder();

            // to show calendar icon near input box
            List<InputGroupAddon> inputGroupAddons = new List<InputGroupAddon>()
            {
                new InputGroupAddonAfter(null, "fa fa-calendar")
            };
            var mvcHtml = TextBoxBootstrap(helper, name, value, label, description, infotip, inputGroupAddons, attributes);

            output.Append(mvcHtml);
            output.Append($@"
<script type = ""text/javascript"" >
    $(function() {{
        $('input[name={name}]').datetimepicker(
            {
                    JsonConvert.SerializeObject(datetimePickerOptions, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        Formatting = Formatting.Indented,
                        DefaultValueHandling = DefaultValueHandling.Ignore
                    })
                }
        );
    }});
</script>
");
            return new HtmlString(output.ToString());
        }

        public static IHtmlContent PaginationBootstrap<T>(this IHtmlHelper helper, PaginatedList<T> results, int pageNumbers = 5, bool prevNextButtons = false, bool startEndButtons = false, object htmlAttributes = null)
        {
            return PaginationBootstrap(helper, results.TotalListings, results.Page, results.PerPage, pageNumbers, prevNextButtons, startEndButtons, htmlAttributes);
        }

        public static IHtmlContent PaginationBootstrap(this IHtmlHelper helper, int totalListings, int page, int perPage, int pageNumbers = 5, bool prevNextButtons = false, bool startEndButtons = false, object htmlAttributes = null)
        {
            var name = Guid.NewGuid().ToString(); // purely to identify the control for the attributes helper
            var attributes = htmlAttributes.ToDictionary();

            int totalPages = (int)Math.Ceiling((decimal)totalListings / perPage);

            if (page < 1)
                page = 1;

            int startPage = page - (int)Math.Floor((decimal)pageNumbers / 2);

            if (startPage < 1)
                startPage = 1;

            int endPage = startPage + pageNumbers - 1;

            if (endPage > totalPages)
                endPage = totalPages;

            var output = new StringBuilder();
            if (totalPages > 0)
            {
                // get current query string from URL if any (for filters)
                var queryString = helper.ViewContext.HttpContext.Request.QueryString.ToDictionary();

                queryString.Remove("page");

                var pageUrl = new StringBuilder();

                if (queryString.Count > 0)
                {
                    pageUrl.Append("?");
                    pageUrl.Append(string.Join("&",
                        queryString.Keys.Select(t => $"{t}={HttpUtility.UrlEncode(queryString[t].ToString())}")));
                    pageUrl.Append("&");
                }
                else
                {
                    pageUrl.Append("?");
                }
                pageUrl.Append("page=");

                output.Append($@"
<nav aria-label=""Page navigation"">
    Page {page} of {totalPages}
    <ul class=""pagination"" {InsertOtherAttributes(name, attributes)}>
        ");

                if (startEndButtons)
                {
                    var firstPageLink = (startPage == 1 ? "#" : $"{pageUrl}1");

                    output.Append($@"
        <li {(startPage == 1 ? @"class=""disabled""" : "")}>
            <a href=""{firstPageLink}"" title=""Go To First"" aria-label=""Go To First"">
                <span aria-hidden=""true"">&laquo;</span>
            </a>
        </li>
");
                }

                if (prevNextButtons)
                {
                    var previousPageLink = (startPage == 1 ? "#" : $"{pageUrl}{page - 1}");

                    output.Append($@"
        <li {(startPage == 1 ? @"class=""disabled""" : "")}>
            <a href=""{previousPageLink}"" title=""Previous"" aria-label=""Previous"">
                <span aria-hidden=""true"">&lsaquo;</span>
            </a>
        </li>
");
                }

                for (int i = startPage; i <= endPage; i++)
                    output.Append($@"<li {(page == i ? @"class=""active""" : "")}><a href=""{(page == i ? "#" : $"{pageUrl}{i}")}"">{i}</a></li>");

                if (prevNextButtons)
                {
                    var nextPageLink = (page >= totalPages ? "#" : $"{pageUrl}{page + 1}");
                    output.Append($@"
        <li {(page >= totalPages ? @"class=""disabled""" : "")}>
            <a href=""{nextPageLink}"" title=""Next"" aria-label=""Next"">
                <span aria-hidden=""true"">&rsaquo;</span>
            </a>
        </li>
");
                }

                if (startEndButtons)
                {
                    var lastPageLink = (page >= totalPages ? "#" : $"{pageUrl}{totalPages}");
                    output.Append($@"
        <li {(page >= totalPages ? @"class=""disabled""" : "")}>
            <a href=""{lastPageLink}"" title=""Go To Last"" aria-label=""Go To Last"">
                <span aria-hidden=""true"">&raquo;</span>
            </a>
        </li>
");
                }

                output.Append(@"
    </ul>
</nav>");
            }
            return new HtmlString(output.ToString());
        }

        /// <summary>
        /// Inserts custom htmlAttributes that have NOT already been inserted on the control being rendered
        /// </summary>
        /// <param name="attributes"></param>
        /// <returns></returns>
        private static string InsertOtherAttributes(string controlId, IDictionary<string, object> attributes)
        {
            // N.B. underscore (_) in attribute keys are replaced with hyphen (-)
            return attributes?.Where(a => !_attributesAlreadySpecified.Contains("{controlId}-{a.Key}")).Aggregate("", (current, attribute) => current + $@"{attribute.Key.Replace("_", "-")}=""{attribute.Value}""");
        }

        /// <summary>
        /// Get the name=value string for an attribute, taking into account the specified list of custom htmlAttributes that may have been specified by the caller
        /// </summary>
        /// <param name="customAttributes">Optional custom attributes specified by the caller</param>
        /// <param name="name">The attribute name</param>
        /// <param name="value">The attribute value to be set</param>
        /// <param name="append">This flag determines whether or not the value specified may be overwritten by an existing attribute with the same name in customAttributes, or if it will include both values e.g. multiple 'class' definitions</param>
        /// <returns></returns>
        private static string AttributeValueFor(string controlId, IDictionary<string, object> customAttributes, string name, string value, bool append)
        {
            // make note of which attribute is being determined (used by InsertOtherAttributes)
            _attributesAlreadySpecified.Add($"{controlId}-{name}");

            if (customAttributes != null && customAttributes.ContainsKey(name))
            {
                return append ? $"{name}=\"{customAttributes[name]} {value}\"" : $"{name}=\"{customAttributes[name]}\"";
            }

            return string.IsNullOrEmpty(value) ? "" : $"{name}=\"{value}\"";
        }

        public static string AntiForgeryTokenString(this IHtmlHelper helper)
        {
            var antiForgeryInputTag = helper.AntiForgeryToken().ToString();
            // Above gets the following: <input name="__RequestVerificationToken" type="hidden" value="PnQE7R0MIBBAzC7SqtVvwrJpGbRvPgzWHo5dSyoSaZoabRjf9pCyzjujYBU_qKDJmwIOiPRDwBV1TNVdXFVgzAvN9_l2yt9-nf4Owif0qIDz7WRAmydVPIm6_pmJAI--wvvFQO7g0VvoFArFtAR2v6Ch1wmXCZ89v0-lNOGZLZc1" />
            var removedStart = antiForgeryInputTag.Replace(@"<input name=""__RequestVerificationToken"" type=""hidden"" value=""", "");
            var tokenValue = removedStart.Replace(@""" />", "");
            return tokenValue;
        }

        public static string idExtension
        {
            get => Guid.NewGuid().ToString().Substring(0, 5);
        }

        public static void AddProperty(this IDictionary<string, object> obj, string name, object value, bool replaceExisting = true)
        {
            if (obj == null)
            {
                obj = new Dictionary<string, object> { { name, value } };
            }
            else
            {
                if (obj.ContainsKey(name))
                {
                    if (replaceExisting)
                        obj[name] = value;
                }
                else
                {
                    obj.Add(name, value);
                }
            }
        }

        // helper
        public static IDictionary<string, object> ToDictionary(this object obj)
        {
            if (obj == null)
                return new Dictionary<string, object>();

            if (obj.GetType().Name.Contains("Dictionary"))
                return (IDictionary<string, object>)obj;

            var result = new Dictionary<string, object>();
            var properties = TypeDescriptor.GetProperties(obj);
            foreach (PropertyDescriptor property in properties)
            {
                result.Add(property.Name, property.GetValue(obj));
            }
            return result;
        }
    }

    public enum eAlertStyle
    {
        Success,
        Warning,
        Danger,
        Info
    }

}
