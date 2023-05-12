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
        SettingsPage.Category, "VSCodingBuddy Page", 0, 0, true)]
    [ProvideAutoLoad(UIContextGuids80.SolutionHasSingleProject, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(UIContextGuids80.SolutionHasMultipleProjects, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class VSCodingBuddyPackage : AsyncPackage
    {
        Speaker? error_speaker;
        SettingsPage settings;
        ExceptionEvent exception_event = new();
        CompileErrorEvent compile_error_event = new();
        NewProjectEvent new_project_event = new();
        IVsSolutionBuildManager build_manager;
        string vsix_path;

        protected void updateSpeaker(string new_key)
        {
            try
            {
                error_speaker = new(new_key,
                        $"{vsix_path}/Phrase.ssml");
            }
            catch (ArgumentNullException ex) { }
        }

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // load settings
            settings = (SettingsPage)GetDialogPage(typeof(SettingsPage));

            vsix_path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            settings.OnKeyUpdate += updateSpeaker;

            updateSpeaker(settings.OpenAIKey);

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

                    if (error_messages.Length > 200)
                        error_messages = error_messages.Substring(0, 200);

                    await error_speaker.speakResponse(
                $"{settings.Personalities[settings.Personality].CompilePrompt}. The error messages [MSG] will exist above a code snippet [CODE], where the error occurred. Keep the responses to no more than 1000 characters, preferably less.",
                $"[MSG]\n{error_messages}\n[CODE]\n{code_snippet}");
                });
            };
        }
    }
}