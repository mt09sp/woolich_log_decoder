using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
