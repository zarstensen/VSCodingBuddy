using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace VSCodingBuddy.ToolWindows
{
    public record SpeakerPersonality(string CompilePrompt, string ExceptionPrompt);

    /// <summary>
    /// settings for the VSCodingBuddy extension
    /// 
    /// see the description attribute for an overview of the properties functionality in the codebase.
    /// 
    /// </summary>
    [ComVisible(true)]
    [Guid("1FA49636-E0C4-419C-A264-D8015F920809")]
    public class SettingsPage : DialogPage
    {
        public const string OpenAICategory = "OpenAI";
        public const string BuildCategory = "Build";
        public const string ChancesCategory = "Chances";
        public const string GeneralCategory = "General";

        
        internal PersonalitiesPage PersonalitiesPage { get; set; }

        [Category(OpenAICategory)]
        [DisplayName("OpenAI API Key")]
        [Description("The openai API key, to be used, when requesting prompt completions. (Requires restart to be updated)")]
        public string OpenAIKey { get; set; }

        [Category(OpenAICategory)]
        [DisplayName("Max Tokens")]
        [Description("Maximum number of tokens to be sent and received from openai.\nThis includes the prompt as well as the response from the model.")]
        public int MaxTokens { get; set; } = 600;

        [Category(GeneralCategory)]
        [DisplayName("Personality")]
        [Description("The personality prompt to use for each prompt completion. See Personalities, for valid values to be used here.")]
        // only keys of Personalities are valid here.
        public string Personality
        {
            get => m_personality;
            set
            {
                if (PersonalitiesPage?.Personalities?.ContainsKey(value) ?? true)
                    m_personality = value;
            }
                
        }

        [Category(BuildCategory)]
        [DisplayName("Code Snippet Range")]
        [Description("Amount of lines above and below the error line, to send to openai")]
        public int LineRange { get; set; } = 2;

        [Category(BuildCategory)]
        [DisplayName("Message Count")]
        [Description("Amount of compile error messages, to send to openai")]
        public int CompileMessageCount { get; set; } = 2;
        
        [Category(GeneralCategory)]
        [DisplayName("Avoid Repeats")]
        [Description("Avoid speaking if the current build errors are the same as the last builds errors.")]
        public bool AvoidRepeat { get; set; } = true;
        
        [Category(ChancesCategory)]
        [DisplayName("Exception Speak Chance")]
        [Description("How likely the extension is to speak when an exception is hit.\nShould be interpreted as 1 in x, meaning higher values lead to a smaller chance for a speech to happen. 0 = disabled")]
        public int ExceptionChance { get => m_build_error_chance;
            set
            {
                if (value >= 0)
                    m_build_error_chance = value;
            }
        }

        [Category(ChancesCategory)]
        [DisplayName("Build Speak Chance")]
        [Description("How likely the extension is to speak when a build error is hit.\nShould be interpreted as 1 in x, meaning higher values lead to a smaller chance for a speech to happen. 0 = disabled")]
        public int BuildErrorChance { get => m_exception_chance;
            set
            {
                if (value >= 0)
                    m_exception_chance = value;
            }
        }

        [Category(BuildCategory)]
        [DisplayName("Errors Max Length")]
        [Description("The maximum length the error section of the compile error prompt ca be, before it is truncated. -1 = do not truncate" +
            "This number should be decreased if too many tokens are used per openai completion.")]
        public int ErrMsgMaxLen { get; set; } = 200;

        [Category(BuildCategory)]
        [DisplayName("Code Line Max Length")]
        [Description("The maximum possible length of a single line inside a code snippet, before it is truncated. -1 = do not truncate." +
            "This number should be decreased if too many tokens are used per openai completion.")]
        public int CodeLineMaxLen { get; set; } = 100;

        [Category(BuildCategory)]
        [DisplayName("Error Check Interval")]
        [Description("For how long the Error List is monitored (in ms), after a build has finished.\nIf errors are not detected, increase this value.")]
        public int ErrorListCheckInterval { get; set; } = 5000;

        /// <summary>
        /// Should be subscribed to in order to apply any updated settings in the settings page instance.
        /// </summary>
        public Action<SettingsPage>? OnApplySettings;

        protected override void OnApply(PageApplyEventArgs e)
        {
            base.OnApply(e);

            OnApplySettings?.Invoke(this);
        }

        protected int m_exception_chance = 1;
        protected int m_build_error_chance = 1;
        private string m_personality = "Rude";
    }
}
