using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VSCodingBuddy.ToolWindows
{
    [ComVisible(true)]
    [Guid("E11CF3F4-57EF-46AB-BB17-4AA1FA67FC15")]
    public class PersonalitiesPage : DialogPage
    {
        protected override IWin32Window Window
        {
            get
            {
                SettingsView settings_view = new();
                settings_view.settings = this;
                settings_view.Initialize();

                return settings_view;
            }
        }
        
        public Dictionary<string, SpeakerPersonality> Personalities { get; set; } = new() {
            {"Rude", new("Please explain the following compile build error messages in a very rude and condescending way",
                "Please explain the following exception error messages in a very rude and condescending way") },
            };
    }
}
