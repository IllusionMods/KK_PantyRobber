using System;
using BepInEx.Configuration;

namespace KK_PantyRobber
{
    internal sealed class ConfigurationManagerAttributes
    {
        public bool? Browsable;

        public string Category;

        public Action<ConfigEntryBase> CustomDrawer;

        public object DefaultValue;

        public string Description;

        public string DispName;

        public bool? HideDefaultButton;

        public bool? IsAdvanced;

        public Func<object, string> ObjToStr;

        public int? Order;

        public bool? ReadOnly;
        public bool? ShowRangeAsPercent;

        public Func<string, object> StrToObj;
    }
}