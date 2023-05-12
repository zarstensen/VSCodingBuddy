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
    public record SpeakerPersonality(string CompilePrompt, string ExceptionPrompt);

    /// <summary>
    /// settings for the VSCodingBuddy extension
    /// </summary>
    [ComVisible(true)]
    [Guid("1FA49636-E0C4-419C-A264-D8015F920809")]
    public class SettingsPage : DialogPage
    {
        
        public const string Category = "VSCodingBuddy";

        [Category(Category)]
        [DisplayName("OpenAI API Key")]
        [Description("The openai API key, to be used, when requesting prompt completions. (Requires restart to be updated)")]
        public string OpenAIKey { get => m_api_key; 
            set 
            {
                m_api_key = value;
                OnKeyUpdate?.Invoke(value);
            } 
        }

        [Category(Category)]
        [DisplayName("Personality")]
        [Description("The personality prompt to use for each prompt completion. See Personalities, for valid values to be used here.")]
        // only keys of Personalities are valid here.
        public string Personality { get => m_personality;
            set
            {
                if (Personalities.ContainsKey(value))
                    m_personality = value;
            }
        }

        [Category(Category)]
        [DisplayName("Personalities")]
        [Description("Possible personalities to pick from. New custom ones can optionally be added.")]
        public Dictionary<string, SpeakerPersonality> Personalities { get; set; } = new() {
            {"Rude", new("Please explain the following exception error messages in a very rude and condescending way",
                "Please explain the following compile build error messages in a very rude and condescending way") },
            };

        [Category(Category)]
        [DisplayName("Code Snippet Range")]
        [Description("Amount of lines above and below the error line, to send to openai")]
        public int LineRange { get; set; } = 2;

        [Category(Category)]
        [DisplayName("Message Count")]
        [Description("Amount of compile error messages, to send to openai")]
        public int CompileMessageCount { get; set; } = 2;

        public Action<string>? OnKeyUpdate;

        protected string m_personality;
        protected string m_api_key;
    }
}
