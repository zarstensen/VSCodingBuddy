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

namespace ExceptionHelper
{
    public class EventReciever : IDebugEventCallback2
    {
        public int Event(IDebugEngine2 pEngine, IDebugProcess2 pProcess, IDebugProgram2 pProgram, IDebugThread2 pThread, IDebugEvent2 pEvent, ref Guid riidEvent, uint dwAttrib)
        {

            if(pEvent is IDebugExceptionEvent2 exception_event)
            {
                exception_event.GetExceptionDescription(out string descr);
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
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
             
            await this.RegisterCommandsAsync();

            //this.RegisterToolWindows();
            
            
            await JoinableTaskFactory.RunAsync(async () =>
            {
                await JoinableTaskFactory.SwitchToMainThreadAsync();
                var iv2s = (await GetServiceAsync(typeof(IVsDebugger))) as IVsDebugger;
                iv2s.AdviseDebugEventCallback(new EventReciever());

            });

            //var dte = (await GetServiceAsync(typeof(DTE))) as DTE2;
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