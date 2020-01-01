using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Softwarehouse.Bootstrap3
{
    public abstract class InputGroupAddon
    {
        public string Text { get; set; }
        public string IconClass { get; set; }
        public string Type { get; private set; }

        public override string ToString()
        {
            return $@"<span class=""input-group-addon"">{(string.IsNullOrEmpty(IconClass) ? Text : $@"<i class=""{IconClass}""></i>")}</span>";
        }

        protected InputGroupAddon()
        {
            if (this is InputGroupAddonAfter)
            {
                Type = "after";
            }
            else
            {
                Type = "before";
            }
        }

        protected InputGroupAddon(string text = null, string iconClass = null) : this()
        {
            Text = text;
            IconClass = iconClass;
        }
    }

    public class InputGroupAddonBefore : InputGroupAddon
    {
        public InputGroupAddonBefore() : base()
        {
        }

        public InputGroupAddonBefore(string text = null, string iconClass = null) : base(text, iconClass)
        {
        }

    }

    public class InputGroupAddonAfter : InputGroupAddon
    {
        public InputGroupAddonAfter() : base()
        {
        }

        public InputGroupAddonAfter(string text = null, string iconClass = null) : base(text, iconClass)
        {
        }
    }
}
