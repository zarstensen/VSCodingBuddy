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
using TTS;

namespace ExceptionHelper
{

    public class EventReciever : IDebugEventCallback2
    {
        SpeechSynthesizer synth = new SpeechSynthesizer();
        JoinableTaskFactory JoinableTaskFactory;
        Speaker speaker;

        public EventReciever(JoinableTaskFactory joinableTaskFactory)
        {
            synth.SetOutputToDefaultAudioDevice();
        //    chat_gpt = new ChatGPT("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",
        //"Please explain the following exception error messages in a very rude and condescending way. Keep the responses to no more than 1000 characters, preferably less.");
            JoinableTaskFactory = joinableTaskFactory;

            string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);


            speaker = new("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",
    "Please explain the following exception error messages in a very rude and condescending way. Keep the responses to no more than 1000 characters, preferably less.",
        $"{path}/Phrase.ssml");
        }

        public int Event(IDebugEngine2 pEngine, IDebugProcess2 pProcess, IDebugProgram2 pProgram, IDebugThread2 pThread, IDebugEvent2 pEvent, ref Guid riidEvent, uint dwAttrib)
        {

            if(pEvent is IDebugExceptionEvent2 exception_event)
            {
                exception_event.GetExceptionDescription(out string descr);

                _ = JoinableTaskFactory.RunAsync(async () =>
                {
                    string helpful_message = await speaker.generateResponse(descr);
                    speaker.speakResponse(helpful_message);
                });
            }

            return 0;
        }

    }

    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideToolWindow(typeof(MyToolWindow.Pane), Style = VsDockStyle.Tabbed, Window = WindowGuids.SolutionExplorer)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.ExceptionHelperString)]
    [ProvideService(typeof(ExceptionHelperPackage), IsAsyncQueryable = true)]
    [ProvideAutoLoad(UIContextGuids80.NoSolution, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(UIContextGuids80.SolutionHasSingleProject, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(UIContextGuids80.SolutionHasMultipleProjects, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(UIContextGuids80.Debugging, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class ExceptionHelperPackage : AsyncPackage
    {
        EnvDTE.DebuggerEvents events;
        EnvDTE.BuildEvents bevents;
        IVsTaskProvider error_list;

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
             
            await this.RegisterCommandsAsync();

            //this.RegisterToolWindows();

            await JoinableTaskFactory.RunAsync(async () =>
            {
                await JoinableTaskFactory.SwitchToMainThreadAsync();
                var iv2s = (await GetServiceAsync(typeof(IVsDebugger))) as IVsDebugger;
                iv2s.AdviseDebugEventCallback(new EventReciever(JoinableTaskFactory));
                var dte = (await GetServiceAsync(typeof(DTE))) as DTE;


                //error_list = dte.ToolWindows.ErrorList as IVsTaskProvider;

                //Microsoft.VisualStudio.Workspace.Build

                bevents = dte.Events.BuildEvents;
                bevents.OnBuildDone += BuildEvents_OnBuildDone;
            });

            //var ivs = (await GetServiceAsync(typeof(SVsShellDebugger))) as IVsDebugger;
            //var iv3s = (GetGlobalService(typeof(IVsDebugger))) as IVsDebugger;
            //var iv4s = (GetGlobalService(typeof(SVsShellDebugger))) as IVsDebugger;

            //dte.Events.DebuggerEvents.OnEnterRunMode += DebuggerEvents_OnEnterRunMode;
            //dte.Events.DebuggerEvents.OnExceptionNotHandled += DebuggerEvents_OnExceptionNotHandled;
            //dte.Events.DebuggerEvents.OnExceptionThrown += DebuggerEvents_OnExceptionNotHandled;
            //events = dte.Events.DebuggerEvents;
            //dte.Events.DebuggerEvents.OnEnterBreakMode += DebuggerEvents_OnEnterBreakMode;
            //dte.Events.DebuggerEvents.OnExceptionNotHandled += DebuggerEvents_OnExceptionNotHandled;
            //dte.Events.DebuggerEvents.OnExceptionThrown += DebuggerEvents_OnExceptionNotHandled;
        }

        private void BuildEvents_OnBuildDone(vsBuildScope Scope, vsBuildAction Action)
        {
            //error_list.EnumTaskItems(out IVsEnumTaskItems items);

            //var arr = new IVsTaskItem[1];
            //items.Next(1, arr, null);

            //arr[0].get_Text(out string text)

            //throw new NotImplementedException();
                // Get the IVsTaskList service
                IVsTaskList taskList = Package.GetGlobalService(typeof(SVsTaskList)) as IVsTaskList;

                // Get the latest error message
                IVsEnumTaskItems enumTaskItems;
                taskList.EnumTaskItems(out enumTaskItems);
                {
                    IVsTaskItem[] taskItems = new IVsTaskItem[1];
                    uint fetched;
                enumTaskItems.Next(1, taskItems, null);
                    {
                        taskItems[0].get_Text(out string text);
                        // Do something with the error message
                    }
                }
        }

        private void DebuggerEvents_OnEnterBreakMode(dbgEventReason Reason, ref dbgExecutionAction ExecutionAction)
        {
            //throw new NotImplementedException();
        }

        private void Events_OnEnterDesignMode(dbgEventReason Reason)
        {
            //throw new NotImplementedException();
        }

        private void DebuggerEvents_OnEnterRunMode(dbgEventReason Reason)
        {
            //throw new NotImplementedException();
        }

        private void DebuggerEvents_OnExceptionNotHandled(string ExceptionType, string Name, int Code, string Description, ref dbgExceptionAction ExceptionAction)
        {
            throw new NotImplementedException();
        }
    }
}