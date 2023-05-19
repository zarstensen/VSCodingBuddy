global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using System;
global using Task = System.Threading.Tasks.Task;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Shell.Interop;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Speech.Synthesis;
using System.IO;
using Microsoft.VisualStudio.Threading;
using System.Windows.Shapes;
using VSCodingBuddy.ToolWindows;
using System.Linq;
using System.Windows.Documents;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.VisualStudio.VSHelp;
using Microsoft.VisualStudio.RpcContracts;

namespace VSCodingBuddy
{
    /// <summary>
    /// IDebugEventCallback2 implementation, that raises OnException, when the debugger detects an unhandled exception.
    /// </summary>
    public class ExceptionEvent : IDebugEventCallback2
    {
        /// <summary>
        /// subscribe to this event, to receive the IDebugExceptionEvent2 information, when an unhandled exception is hit.
        /// </summary>
        public EventHandler<IDebugExceptionEvent2>? OnException;

        public int Event(IDebugEngine2 pEngine, IDebugProcess2 pProcess, IDebugProgram2 pProgram, IDebugThread2 pThread, IDebugEvent2 pEvent, ref Guid riidEvent, uint dwAttrib)
        {

            if (pEvent is IDebugExceptionEvent2 exception_event)
                OnException?.Invoke(this, exception_event);

            return 0;
        }

    }

    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.ExceptionHelperString)]
    [ProvideOptionPage(typeof(SettingsPage),
        Vsix.Name, "General", 0, 0, true)]
    [ProvideOptionPage(typeof(PersonalitiesPage),
        Vsix.Name, "Personalities", 0, 0, true)]
    [ProvideProfile(typeof(PersonalitiesPage), Vsix.Name, "Personalities", 0, 0, true)]
    [ProvideAutoLoad(UIContextGuids80.SolutionBuilding, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(UIContextGuids80.Debugging, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class VSCodingBuddyPackage : AsyncPackage
    {
        static readonly Random RNG = new();
        
        public static string VsixPath => System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        Speaker? m_error_speaker;

        SettingsPage m_settings;
        PersonalitiesPage m_personalities;
        
        ExceptionEvent m_exception_event = new();

        EnvDTE.BuildEvents m_build_events;
        IVsActivityLog m_activity_log;

        List<string> m_prev_err_codes = new();
        bool m_checking_err_list;
        DateTime m_err_list_check_start;


        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            // setup logging

            m_activity_log = (await GetServiceAsync(typeof(SVsActivityLog))) as IVsActivityLog;

            // load settings

            m_personalities = (PersonalitiesPage)GetDialogPage(typeof(PersonalitiesPage));
            m_settings = (SettingsPage)GetDialogPage(typeof(SettingsPage));
            m_settings.PersonalitiesPage = m_personalities;


            m_settings.OnApplySettings += updateSpeaker;

            updateSpeaker(m_settings);

            await this.RegisterCommandsAsync();

            // setup exception event

            var ivs_debugger = (await GetServiceAsync(typeof(IVsDebugger))) as IVsDebugger;
            
            ivs_debugger.AdviseDebugEventCallback(m_exception_event);

            m_exception_event.OnException += handleException;

            // setup build error event

            var dte = (await GetServiceAsync(typeof(DTE))) as DTE2;
            m_build_events = dte.Events.BuildEvents;

            m_build_events.OnBuildDone += handleBuildDone;
        }

        /// <summary>
        /// creates a new Speaker instance, which uses the latest OpenAIKey stored in the settings.
        /// </summary>
        private void updateSpeaker(SettingsPage new_settings)
        {
            try
            {
                m_error_speaker = new(new_settings.OpenAIKey);
                m_personalities.Speaker = m_error_speaker;
            }
            catch (ArgumentNullException) { }
        }

        /// <summary>
        /// invoked whenever a build has finished.
        /// 
        /// calls checkErrorList in a separate method, or if it is already running, resets the error list check interval.
        /// 
        /// </summary>
        /// <param name="Scope"></param>
        /// <param name="Action"></param>
        private void handleBuildDone(vsBuildScope Scope, vsBuildAction Action)
        {
            // we are not sure here if the build has failed,
            // and we are unable to use IVsBuildStatusCallback, as it simply just doesn't work for some weird reason.
            // 
            // instead we periodically check the ErrorList for a set time interval, until some error items are detected.
            // if no error items are detected within this interval, it is assumed the build was succesful.
            //
            // To avoid double detects, if HandleBuildDone is invoked, whilst another HandleBuildDone is already checking the error list,
            // the timer interval is simply reset.

            if (!m_checking_err_list)
                // checkErrorList is a separate method, to avoid indent hell.
                _ = JoinableTaskFactory.RunAsync(checkErrorList);
            else
                m_err_list_check_start = DateTime.Now;
        }

        /// <summary>
        /// checks if the error list has any entries, within the BUILD_CHECK_INTERVAL
        /// if it has, and the errors were unique from the last check, then call trySpeakCompileError.
        /// </summary>
        private async Task checkErrorList()
        {
            bool missing_reset = !m_checking_err_list;
            m_checking_err_list = true;

            await JoinableTaskFactory.SwitchToMainThreadAsync();
            // get the compile errors, by getting the entries in the error list.
            var dte = (await GetServiceAsync(typeof(DTE))) as DTE2;

            var err_list = dte.ToolWindows.ErrorList;

            if(missing_reset)
                m_err_list_check_start = DateTime.Now;

            // the error list does not update instantly, so periodically check the error items count, until it is greater than 0
            // as the error list then will contain the errors the build produced.
            //
            // in case something has gone wrong, and this method is fired,
            // even though no errors exists, this while loop is also limited to a maximum number of iterations before it stops
            while (err_list.ErrorItems.Count == 0 && DateTime.Now < m_err_list_check_start.AddMilliseconds(m_settings.ErrorListCheckInterval))
            {
                print("Checking error list");
                System.Threading.Thread.Sleep(50);
            }

            m_checking_err_list = false;

            // either the error list has no items, or is full of warnings,
            // both of which are seen as succesful builds.

            if (err_list.ErrorItems.Count == 0)
                return;

            // this dictionary is used for retrieving the error codes of the errors.
            var err_list_entries = (err_list as IErrorList).TableControl.Entries
            .Select((e, i) => new { Entry = e, Index = i + 1 })
            .ToDictionary(it => it.Index, it => it.Entry);

            var err_items = err_list.ErrorItems;

            List<string> err_codes = new();
            List<ErrorItem> filtered_err_items = new();

            for (int i = 0; i < err_items.Count && filtered_err_items.Count < m_settings.CompileMessageCount; i++)
            {
                var item = err_items.Item(i + 1);

                // ignore warnings.
                if (item.ErrorLevel != vsBuildErrorLevel.vsBuildErrorLevelHigh)
                    continue;

                filtered_err_items.Add(item);

                // retrieve the error code, to be used when checking if this build scenario has already occured.

                err_list_entries[i + 1].TryGetValue("errorcode", out object code);
                err_codes.Add(code as string);
            }

            // the error list contained no errors, therefore ignore this build.
            if (filtered_err_items.Count == 0)
                return;

            // if the current error codes match exactly with the previous error codes, and AvoidRepeat is enabled,
            // then the build message should not be read out loud.

            bool speak;
            
            if (m_settings.AvoidRepeat)
            {
                speak = false;

                // check if the previously used error types are the same as the current ones.

                if (err_codes.Count == m_prev_err_codes.Count)
                {
                    for (int i = 0; i < err_codes.Count; i++)
                    {
                        if (err_codes[i] != m_prev_err_codes[i])
                        {
                            speak = true;
                            break;
                        }
                    }
                }
                else
                    speak = true;
            }
            else
                speak = true;

            m_prev_err_codes = err_codes;

            if (speak)
                await trySpeakCompileError(filtered_err_items);
        }

        /// <summary>
        /// if the rng rolls a 1, this method uses the passed error_items list to generate a small code snippet
        /// of where the latest build error occurred, as well as generating a list of the first error items in the error list.
        /// 
        /// </summary>
        /// <param name="error_items"></param>
        /// <returns></returns>
        private async Task trySpeakCompileError(List<ErrorItem> error_items)
        {
            string code_snippet = "";
            string error_messages = "";

            for (int i = 0; i < error_items.Count; i++)
            {
                var item = error_items[i];

                // generate code snippet form first error.
                if (i == 0)
                {
                    using (StreamReader code_reader = new(item.FileName))
                    {
                        for (int j = 0; j < item.Line - m_settings.LineRange; j++)
                            await code_reader.ReadLineAsync();


                        for (int j = 0; j < m_settings.LineRange * 2 && !code_reader.EndOfStream; j++)
                        {
                            string code_line = await code_reader.ReadLineAsync();

                            if (code_line.Length > m_settings.CodeLineMaxLen)
                                code_line = code_line.Substring(0, m_settings.CodeLineMaxLen) + " (truncated)";

                            code_snippet += code_line + '\n';
                        }
                    }
                }
                
                error_messages += item.Description + '\n';
            }

            // the error messages are truncated in order to reduce prompt token usage.
            // TODO: this should be user configurable, and should be truncated in a smarter way
            // TODO 2: the code snippets should also be truncated.

            if (error_messages.Length > m_settings.ErrMsgMaxLen)
                error_messages = error_messages.Substring(0, m_settings.ErrMsgMaxLen) + " (truncated)";

            await m_error_speaker?.speakResponse(
        $"[MSG] above [CODE], response should be max 1000 characters. Refrain from providing code in the response.\n{m_personalities.Personalities[m_settings.Personality].CompilePrompt}",
        $"[MSG]\n{error_messages}\n[CODE]\n{code_snippet}",
        m_settings.MaxTokens);
        }

        /// <summary>
        /// Invoked whenever an exception is encountered by the debugger.
        /// 
        /// uses the passed IDebugExceptionEvent2 to get the exception message, and pass it on to the speaker instance,
        /// if the rng rolls a 1.
        /// 
        /// </summary>
        private void handleException(object sender, IDebugExceptionEvent2 args)
        {
            _ = JoinableTaskFactory.RunAsync(async () =>
            {
                if (m_settings.ExceptionChance == 0)
                    return;

                // use rng to determine whether the current compile error should be processed.

                if (RNG.Next(1, m_settings.ExceptionChance) > 1)
                    return;

                if (!m_personalities.Personalities.ContainsKey(m_settings.Personality))
                    return;

                args.GetExceptionDescription(out string exception_description);

                await m_error_speaker?.speakResponse(
                    $"Response should be max 1000 characters. Refrain from providing code in the response.\n{m_personalities.Personalities[m_settings.Personality].ExceptionPrompt}",
                    exception_description,
                    m_settings.MaxTokens);
            });
        }

        private void print(string str, __ACTIVITYLOG_ENTRYTYPE type = __ACTIVITYLOG_ENTRYTYPE.ALE_INFORMATION)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            m_activity_log.LogEntry((uint)type, ToString(), str);
        }
    }
}