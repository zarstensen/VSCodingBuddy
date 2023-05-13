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

namespace VSCodingBuddy
{
    public class NewProjectEvent : IVsSolutionEvents
    {
        public Action<IVsHierarchy>? OnProjectAdd;

        public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            if (fAdded == 1)
                OnProjectAdd?.Invoke(pHierarchy);

            return 0;
        }

        #region unused
        public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel) => 0;

        public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved) => 0;

        public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy) => 0;

        public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel) => 0;

        public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy) => 0;

        public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution) => 0;

        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel) => 0;

        public int OnBeforeCloseSolution(object pUnkReserved) => 0;

        public int OnAfterCloseSolution(object pUnkReserved) => 0;
        #endregion
    }

    /// <summary>
    /// IVsBuildStatusCallback implementation, that raises OnCompileError, when an advised project configuration fails to build.
    /// </summary>
    public class CompileErrorEvent : IVsBuildStatusCallback
    {
        /// <summary>
        /// Raised when the compiler encounters an error.
        /// </summary>
        public Action? OnCompileError;
        public int BuildEnd(int fSuccess)
        {
            if (fSuccess != 1)
                OnCompileError?.Invoke();

            return 0;
        }

        #region unused
        public int BuildBegin(ref int pfContinue) => 0;


        public int Tick(ref int pfContinue) => 0;
        #endregion
    }

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
    [ProvideAutoLoad(UIContextGuids80.SolutionHasSingleProject, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(UIContextGuids80.SolutionHasMultipleProjects, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(UIContextGuids80.SolutionBuilding, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class VSCodingBuddyPackage : AsyncPackage
    {
        const int ERR_LIST_MAX_CHECKS = 50;
        const int ERR_MSG_MAX_LEN = 200;

        Speaker? error_speaker;

        List<string> prev_err_codes = new();
        Random rng = new();

        SettingsPage settings;
        PersonalitiesPage personalities;
        
        ExceptionEvent exception_event = new();
        CompileErrorEvent compile_error_event = new();
        NewProjectEvent new_project_event = new();
        
        IVsSolutionBuildManager build_manager;
        string vsix_path;

        private void updateSpeaker(SettingsPage new_settings)
        {
            try
            {
                error_speaker = new(new_settings.OpenAIKey,
                        $"{vsix_path}/Phrase.ssml");
            }
            catch (ArgumentNullException) { }
        }

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // load settings
            settings = (SettingsPage)GetDialogPage(typeof(SettingsPage));
            personalities = (PersonalitiesPage)GetDialogPage(typeof(PersonalitiesPage));

            vsix_path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            settings.OnApplySettings += updateSpeaker;

            updateSpeaker(settings);

            await this.RegisterCommandsAsync();

            await JoinableTaskFactory.SwitchToMainThreadAsync();

            // setup exception event

            var ivs_debugger = (await GetServiceAsync(typeof(IVsDebugger))) as IVsDebugger;
            
            ivs_debugger.AdviseDebugEventCallback(exception_event);

            exception_event.OnException += (s, args) =>
            {
                _ = JoinableTaskFactory.RunAsync(async () =>
                {
                    if (settings.ExceptionChance == 0)
                        return;

                    // use rng to determine whether the current compile error should be processed.

                    if (rng.Next(1, settings.ExceptionChance) > 1)
                        return;

                    if (!personalities.Personalities.ContainsKey(settings.Personality))
                        return;

                    args.GetExceptionDescription(out string exception_description);
                    await error_speaker?.speakResponse(
                        $"{personalities.Personalities[settings.Personality].ExceptionPrompt}. Keep the responses to no more than 1000 characters, preferably less.",
                        exception_description,
                        settings.MaxTokens);
                });
            };

            // setup compile build error event
            var solution = (await GetServiceAsync(typeof(SVsSolution))) as IVsSolution;
            build_manager = (await GetServiceAsync(typeof(SVsSolutionBuildManager))) as IVsSolutionBuildManager;


            Guid iterated_projects = Guid.Empty;
            solution.GetProjectEnum((uint)__VSENUMPROJFLAGS.EPF_ALLPROJECTS, ref iterated_projects, out IEnumHierarchies projects);
            solution.AdviseSolutionEvents(new_project_event, out uint _);

            new_project_event.OnProjectAdd += (hierarchy) =>
            {
                _ = JoinableTaskFactory.RunAsync(async () =>
                {
                    await JoinableTaskFactory.SwitchToMainThreadAsync();

                    IVsProjectCfg[] project_cfg = new IVsProjectCfg[1];

                    build_manager.FindActiveProjectCfg(IntPtr.Zero, IntPtr.Zero, hierarchy, project_cfg);
                    project_cfg[0].get_BuildableProjectCfg(out IVsBuildableProjectCfg buildable_project_cfg);

                    buildable_project_cfg.AdviseBuildStatusCallback(compile_error_event, out uint _);
                });
            };

            IVsHierarchy[] project = new IVsHierarchy[1];
            IVsProjectCfg[] project_cfg = new IVsProjectCfg[1];

            while(projects.Next(1, project, out uint _) == 0)
            {
                build_manager.FindActiveProjectCfg(IntPtr.Zero, IntPtr.Zero, project[0], project_cfg);
                project_cfg[0].get_BuildableProjectCfg(out IVsBuildableProjectCfg buildable_project_cfg);
                buildable_project_cfg.AdviseBuildStatusCallback(compile_error_event, out uint _);
            }

            compile_error_event.OnCompileError += () =>
            {
                _ = JoinableTaskFactory.RunAsync(async () =>
                {
                    if (settings.BuildErrorChance == 0)
                        return;

                    // use rng to determine whether the current compile error should be processed.

                    if (rng.Next(1, settings.BuildErrorChance) > 1)
                        return;

                    // ignore if the currently selected personality does not exist.

                    if (!personalities.Personalities.ContainsKey(settings.Personality))
                        return;

                    await JoinableTaskFactory.SwitchToMainThreadAsync();

                    // get the compile errors, by getting the entries in the error list.
                    var dte = (await GetServiceAsync(typeof(DTE))) as DTE2;

                    var err_list = dte.ToolWindows.ErrorList;

                    int list_checks = 0;

                    // the error list does not update instantly, so periodically check the error items count, until it is greater than 0
                    // as the error list then will contain the errors the build produced.
                    //
                    // in case something has gone wrong, and this method is fired,
                    // even though no errors exists, this while loop is also limited to a maximum number of iterations before it stops
                    while(err_list.ErrorItems.Count == 0 && list_checks++ < ERR_LIST_MAX_CHECKS)
                        await Task.Delay(100);

                    if (err_list.ErrorItems.Count == 0)
                        return;

                    var err_list_entries = (err_list as IErrorList).TableControl.Entries
                    .Select((e, i) => new {Entry = e, Index = i + 1})
                    .ToDictionary(it => it.Index, it => it.Entry);

                    var err_items = err_list.ErrorItems;
                    int line_range = settings.LineRange;

                    string code_snippet = "";
                    string error_messages = "";

                    List<string> err_codes = new();

                    for (int i = 0; i < Math.Min(err_items.Count, settings.CompileMessageCount); i++)
                    {
                        var item = err_items.Item(i + 1);

                        // retrieve the error code, to be used when checking if this build scenario has already occured.

                        err_list_entries[i + 1].TryGetValue("errorcode", out object code);
                        err_codes.Add(code as string);

                        // generate code snippet form first error.
                        if (i == 0)
                        {
                            using (StreamReader code_reader = new(item.FileName))
                            {
                                for (int j = 0; j < item.Line - line_range; j++)
                                    await code_reader.ReadLineAsync();


                                for (int j = 0; j < line_range * 2 && !code_reader.EndOfStream; j++)
                                    code_snippet += await code_reader.ReadLineAsync() + '\n';

                            }
                        }

                        error_messages += item.Description + '\n';
                    }

                    bool speak;

                    if (settings.AvoidRepeat)
                    {
                        speak = false;

                        // check if the previously used error types are the same as the current ones.

                        if (err_codes.Count == prev_err_codes.Count)
                        {
                            for (int i = 0; i < err_codes.Count; i++)
                            {
                                if (err_codes[i] != prev_err_codes[i])
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

                    prev_err_codes = err_codes;

                    if (!speak)
                        return;

                    if (error_messages.Length > ERR_MSG_MAX_LEN)
                        error_messages = error_messages.Substring(0, ERR_MSG_MAX_LEN);

                    await error_speaker.speakResponse(
                $"{personalities.Personalities[settings.Personality].CompilePrompt}. The error messages [MSG] will exist above a code snippet [CODE], where the error occurred. Keep the responses to no more than 1000 characters, preferably less.",
                $"[MSG]\n{error_messages}\n[CODE]\n{code_snippet}",
                settings.MaxTokens);
                });
            };
        }
    }
}