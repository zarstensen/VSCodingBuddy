using System.Diagnostics;

namespace TTSE
{
    [Command(PackageIds.MyCommand)]
    internal sealed class MyCommand : BaseCommand<MyCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await VS.MessageBox.ShowWarningAsync("TTSE", "Button clicked");
            Trace.WriteLine("TEST");
        }
    }
}
