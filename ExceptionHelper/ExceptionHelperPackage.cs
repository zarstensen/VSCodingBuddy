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
using ExceptionHelper.ToolWindows;

namespace ExceptionHelper
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
        SettingsPage.Category, "VSCodingBuddy Page", 0, 0, true)]
    [ProvideAutoLoad(UIContextGuids80.NoSolution, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(UIContextGuids80.SolutionHasSingleProject, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(UIContextGuids80.SolutionHasMultipleProjects, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(UIContextGuids80.Debugging, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class ExceptionHelperPackage : AsyncPackage
    {
        EnvDTE.BuildEvents build_events;
        IVsTaskProvider error_list;
        Speaker? error_speaker;
        SettingsPage settings;
        ExceptionEvent exception_event = new();

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // load settings
            settings = (SettingsPage)GetDialogPage(typeof(SettingsPage));

            string vsix_path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            settings.OnKeyUpdate += (new_key) =>
            {
                try
                {
                    error_speaker = new(new_key,
                            $"{vsix_path}/Phrase.ssml");
                }
                catch (ArgumentNullException ex) { }
            };

            try
            {
                error_speaker = new(settings.OpenAIKey ?? "",
                                $"{vsix_path}/Phrase.ssml");
            }
            catch (ArgumentNullException ex) { }

            await this.RegisterCommandsAsync();

            await JoinableTaskFactory.SwitchToMainThreadAsync();

            // setup exception event

            var ivs_debugger = (await GetServiceAsync(typeof(IVsDebugger))) as IVsDebugger;
            
            ivs_debugger.AdviseDebugEventCallback(exception_event);

            exception_event.OnException += (s, args) =>
            {
                _ = JoinableTaskFactory.RunAsync(async () =>
                {
                    args.GetExceptionDescription(out string exception_description);
                    await error_speaker?.speakResponse($"{settings.Personalities[settings.Personality].ExceptionPrompt}. Keep the responses to no more than 1000 characters, preferably less.", exception_description);
                });
            };

            // setup compile build error event
            var dte = (await GetServiceAsync(typeof(DTE))) as DTE;
            build_events = dte.Events.BuildEvents;
            build_events.OnBuildDone += OnBuildDone;
        }

        private void OnBuildDone(vsBuildScope Scope, vsBuildAction Action)
        {

            _ = JoinableTaskFactory.RunAsync(async () =>
            {
                await JoinableTaskFactory.SwitchToMainThreadAsync();

                // get the compile errors, by getting the entries in the error list.
                var dte = (await GetServiceAsync(typeof(DTE))) as DTE2;

                var err_items = dte.ToolWindows.ErrorList.ErrorItems;

                if (err_items.Count == 0)
                    return;

                int line_range = settings.LineRange;

                string code_snippet = "";
                string error_messages = "";

                for (int i = 0; i < Math.Min(err_items.Count, settings.CompileMessageCount); i++)
                {
                    var item = err_items.Item(i + 1);

                    // generate code snippet form first error.
                    if (i == 0) using (StreamReader code_reader = new(item.FileName))
                        {
                            for (int j = 0; j < item.Line - line_range; j++)
                                await code_reader.ReadLineAsync();


                            for (int j = 0; j < line_range * 2 && !code_reader.EndOfStream; j++)
                                code_snippet += await code_reader.ReadLineAsync() + '\n';

                        }

                    error_messages += item.Description + '\n';
                }

                if (error_messages.Length > 200)
                    error_messages = error_messages.Substring(0, 200);

                await error_speaker.speakResponse(
            $"{settings.Personalities[settings.Personality].CompilePrompt}. The error messages [MSG] will exist above a code snippet [CODE], where the error occurred. Keep the responses to no more than 1000 characters, preferably less.",
            $"[MSG]\n{error_messages}\n[CODE]\n{code_snippet}");
            });

        }
    }
}