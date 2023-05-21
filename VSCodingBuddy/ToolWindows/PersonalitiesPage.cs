using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        /// <summary>
        /// Dictionary mapping a personality name, to a SpeakerPersonality instance,
        /// containing all the relevant information about the personality.
        /// 
        /// Also manages a json file, which stores the dictionary in a file, loaded on startup.
        /// 
        /// </summary>
        public Dictionary<string, SpeakerPersonality> Personalities { get; set; } = new() {
            { "Helpful", new(
                    "Please helpfully explain the following build error message",
                    "Please helpfully explain the following exception error message"
                ) },
            { "Rude", new(
                "Please explain the following compile build error messages in a very rude and condescending way",
                "Please explain the following exception error messages in a very rude and condescending way"
                ) },
            { "Maid", new(
                "Please explain the following compile build error message like a flustered female cat maid. Make heavy use of phrases like 'UwU', 'Meowster' etc...",
                "Please explain the following exception error message like a flustered female cat maid. Make heavy use of phrases like 'UwU', 'Meowster' etc..."
                ) }
            };

        /// <summary>
        /// save the Personalities dictionary to the setting store, making it later loadable, when visual studio is restarted
        /// </summary>
        public override void SaveSettingsToStorage()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            SettingsManager settingsManager = new ShellSettingsManager(ServiceProvider.GlobalProvider);
            WritableSettingsStore store = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);

            // the dictionary is stored as a json string, which is serialized and deserialized when loading and storing.

            store.CreateCollection(Vsix.Name);
            store.SetString(Vsix.Name, nameof(Personalities), JsonConvert.SerializeObject(Personalities));
        }

        /// <summary>
        /// load a stored personality dictionary from the setting store.
        /// </summary>
        public override void LoadSettingsFromStorage()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            SettingsManager settingsManager = new ShellSettingsManager(ServiceProvider.GlobalProvider);
            WritableSettingsStore store = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
            
            // if no entry for the personalities is found, nothing is modified,
            // and the default value of the dictionary property is preserved

            string serialized_personalities = store.GetString(Vsix.Name, nameof(Personalities), string.Empty);

            if (serialized_personalities == string.Empty)
                return;

            Personalities = JsonConvert.DeserializeObject<Dictionary<string, SpeakerPersonality>>(serialized_personalities);
        }

        /// <summary>
        /// Speaker instance, that is used for compressing the speaker personality prompts.
        /// </summary>
        public Speaker Speaker { get; set; }

        protected override IWin32Window Window
        {
            get
            {
                PersonalitiesView personalities_view = new();
                personalities_view.settings = this;
                personalities_view.Initialize();

                return personalities_view;
            }
        }
    }
}
