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
using System.Text.RegularExpressions;

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
        public EventHandler<IDebugExceptionEvent2> OnException;

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
    [Guid(PackageGuids.VSCodingBuddyString)]
    [ProvideOptionPage(typeof(SettingsPage),
        Vsix.Name, "General", 0, 0, true)]
    [ProvideOptionPage(typeof(PersonalitiesPage),
        Vsix.Name, "Personalities", 0, 0, true)]
    [ProvideProfile(typeof(PersonalitiesPage), Vsix.Name, "Personalities", 0, 0, true)]
    [ProvideAutoLoad(UIContextGuids80.SolutionBuilding, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(UIContextGuids80.Debugging, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class VSCodingBuddyPackage : AsyncPackage
    {
        static readonly Random RNG = new();
        /// <summary>
        /// This regex matches any build output, that contains any warning or error,
        /// and groups 5 different sections of the line,
        /// that are relevant to the warning / error.
        /// 
        /// The groups are as follows:
        ///     Group 1:
        ///         The project build number.
        ///     Group 2:
        ///         Origin, can either be a file path, containing line information, or an object if linking.
        ///     Group 3:
        ///         The log category, can be warning, error or others.
        ///     Group 4:
        ///         The warning or error code.
        ///     Group 5:
        ///         The warning or error message.
        /// 
        /// </summary>
        static readonly Regex BUILD_LOG_ENTRY_REGEX = new(
                @"^(\d+)>(.+?): (\w+) ([\w\d]+): (.+)$",
                RegexOptions.Multiline);

        /// <summary>
        /// Following constants are the indicies that should be used,
        /// when retrieving specific information from a BUILD_LOG_ENTRY_REGEX group list.
        /// 
        /// BLE: Build Log Entry
        /// 
        /// </summary>
        const int BLE_PROJ_NUM = 1;
        const int BLE_ORIGIN = 2;
        const int BLE_CATEGORY = 3;
        const int BLE_CODE = 4;
        const int BLE_MSG = 5;

        /// <summary>
        /// number of groups the BUILD_LOG_ENTRY_REGEX should produce for a successful match.
        /// </summary>
        const int ENTRY_CAPTURE_COUNT = 6;

        /// <summary>
        /// This regex matches any Origin capture from BUILD_TRACE_REGEX, which contains a file path and series of line numbers,
        /// 
        /// Capture 1:
        ///     The file location of the source file.
        /// Capture 2:
        ///     The line numbers provided.
        ///     often (line nr., char pos in line).
        ///     sometimes (line start, char start, line end, char end)
        /// 
        /// </summary>
        static readonly Regex ORIGIN_FILE_ENTRY_REGEX = new(@"^(.+?)\(((?:\d+?,?)+)\)$");

        public static string VsixPath => System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        Speaker m_error_speaker;

        SettingsPage m_settings;
        PersonalitiesPage m_personalities;
        
        ExceptionEvent m_exception_event = new();

        EnvDTE.BuildEvents m_build_events;
        IVsActivityLog m_activity_log;

        List<string> m_prev_err_codes = new();

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

            m_build_events.OnBuildDone += (s, a) => _ = JoinableTaskFactory.RunAsync(
                async () => await handleBuildDone(s, a)
            );
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

        record ErrorEntry(string origin, int? line, string code, string message);

        /// <summary>
        /// invoked whenever a build has finished.
        /// 
        /// calls checkErrorList in a separate method, or if it is already running, resets the error list check interval.
        /// 
        /// </summary>
        /// <param name="Scope"></param>
        /// <param name="Action"></param>
        private async Task handleBuildDone(vsBuildScope scope, vsBuildAction action)
        {
            // we are not sure here if the build has failed,
            // and we are unable to use IVsBuildStatusCallback, as it simply just doesn't work for some weird reason.
            // 
            // instead we parse the output of the build output window for errors.
            // these logs contain the error code, message and file origin, which is later used when generating the prompt for gpt-3.

            // retrieve text from build output window

            await JoinableTaskFactory.SwitchToMainThreadAsync();

            DTE2 dte = await GetServiceAsync(typeof(DTE)) as DTE2;

            // this should never happen
            if (dte == null)
                return;

            OutputWindowPanes panes = dte.ToolWindows.OutputWindow.OutputWindowPanes;

            EnvDTE.OutputWindowPane build_pane = panes.Item("Build");

            string build_output = build_pane.TextDocument.CreateEditPoint(build_pane.TextDocument.StartPoint).GetText(build_pane.TextDocument.EndPoint);

            // detect if the build action was a build.
            // a rebuild should also be seen as a build.
            if (action != vsBuildAction.vsBuildActionBuild && action != vsBuildAction.vsBuildActionRebuildAll)
                return;

            // match all log entries, which contain a warning, error or other similar messages.

            var matches = BUILD_LOG_ENTRY_REGEX.Matches(build_output);

            // the current project build number.
            // if this changes, the error list should be reset, as we do not want to mix errors from different projects.
            int project_number = 1;
            List<ErrorEntry> error_entries = new();

            foreach(Match entry_match in matches)
            {
                if (entry_match.Groups.Count != ENTRY_CAPTURE_COUNT ||
                    entry_match.Groups[BLE_CATEGORY].Value.ToLower() != "error")
                    continue;

                int curr_proj_num = int.Parse(entry_match.Groups[BLE_PROJ_NUM].Value);

                // if an error is detected, and it is in a new build project,
                // clear the previous errors, as they should not be mixed with the current errors.
                if(project_number != curr_proj_num)
                {
                    project_number = curr_proj_num;
                    error_entries.Clear();
                }

                // ignore if max compile errors has been hit.
                if (error_entries.Count >= m_settings.CompileMessageCount)
                    continue;

                Match file_origin = ORIGIN_FILE_ENTRY_REGEX.Match(entry_match.Groups[BLE_ORIGIN].Value);

                string origin = entry_match.Groups[BLE_ORIGIN].Value;
                int? line = null;

                if(file_origin.Success)
                {
                    origin = file_origin.Groups[1].Value;

                    string err_section_string = file_origin.Groups[2].Value;

                    int separator_indx = err_section_string.IndexOf(',');

                    line = int.Parse(separator_indx == -1 ? err_section_string : err_section_string.Substring(0, separator_indx));
                }

                error_entries.Add(new(
                    origin,
                    line,
                    entry_match.Groups[BLE_CODE].Value,
                    entry_match.Groups[BLE_MSG].Value)
                    );
            }

            // the error list contained no errors, therefore ignore this build.
            if (error_entries.Count == 0)
                return;


            List<string> err_codes = (from entry in error_entries select entry.code).ToList();

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
                await trySpeakCompileError(error_entries);
        }

        /// <summary>
        /// if the rng rolls a 1, this method uses the passed error_items list to generate a small code snippet
        /// of where the latest build error occurred, as well as generating a list of the first error items in the error list.
        /// 
        /// </summary>
        /// <param name="error_items"></param>
        /// <returns></returns>
        private async Task trySpeakCompileError(List<ErrorEntry> error_items)
        {
            string code_snippet = "";
            string error_messages = "";

            for (int i = 0; i < error_items.Count; i++)
            {
                var item = error_items[i];

                // generate code snippet form first error.
                if (i == 0 && error_items[i].line != null)
                {
                    using (StreamReader code_reader = new(item.origin))
                    {
                        for (int j = 0; j < item.line - m_settings.LineRange; j++)
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
                
                error_messages += item.message + '\n';
            }

            // the error messages are truncated in order to reduce prompt token usage.

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