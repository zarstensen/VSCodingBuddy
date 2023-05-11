using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ExceptionHelper.ToolWindows
{
    [ComVisible(true)]
    [Guid("5D474FEC-5CCB-4CB4-81E5-4C92FFA8E0C9")]
    public class SettingsWindow : DialogPage
    {
        public string OptionString { get; set; }
    }

    [ComVisible(true)]
    public class SettingsPage : DialogPage
    {

        [Category("My Category")]
        [DisplayName("My Integer Option")]
        [Description("My integer Option")]
        public int OptionInteger { get; set; }

        private void InitializeComponent()
        {

        }
    }
}
