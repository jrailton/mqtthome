using Microsoft.AspNetCore.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Softwarehouse.Bootstrap3
{
    public class Infotip
    {
        public eInfotipStyle Style = eInfotipStyle.tooltip;
        public string CssClass = "fa fa-question-circle";
        public string Title;
        public string Content;

        public Infotip(string title, string content): this(title)
        {
            Style = eInfotipStyle.popover;
            Content = content;
        }

        public Infotip(string title)
        {
            Title = title;
        }

        public IHtmlContent Render()
        {
            return new HtmlString($@"<span style=""cursor:pointer"" data-toggle=""{Style}"" data-html=""true"" title=""{Title}""{(Style == eInfotipStyle.popover ? $" data-content=\"{Content}\"" : "")}>
    <i class=""{CssClass}""></i>
</span>");
        }
    }

    public enum eInfotipStyle
    {
        tooltip,
        popover
    }
}
