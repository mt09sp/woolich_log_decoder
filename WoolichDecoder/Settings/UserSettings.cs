using System.Configuration;

namespace WoolichDecoder.Settings
{
    internal class UserSettings : ApplicationSettingsBase
    {
        [UserScopedSetting()]
        // [DefaultSettingValue("white")]
        public string LogDirectory
        {
            get
            {
                return ((string)this["LogDirectory"]);
            }
            set
            {
                this["LogDirectory"] = (string)value;
            }
        }
    }
}
