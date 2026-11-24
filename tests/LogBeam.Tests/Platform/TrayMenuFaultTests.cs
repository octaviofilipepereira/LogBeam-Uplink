// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using LogBeam.Core.Platform;

namespace LogBeam.Tests.Platform;

public class TrayMenuFaultTests
{
    // Rasto da 2.6.1, tal como chegou nos relatórios de erros de 15/11 e 21/11/2026.
    private const string Reported = """
           at Tmds.DBus.SourceGenerator.ComCanonicalDbusmenuHandler.EmitLayoutUpdated(UInt32 revision, Int32 parent)
           at Avalonia.FreeDesktop.DBusMenuExporter.DBusMenuExporterImpl.DoLayoutReset()
           at Avalonia.Threading.DispatcherOperation.InvokeCore()
           at Avalonia.Threading.DispatcherOperation.Execute()
           at Avalonia.Threading.Dispatcher.ExecuteJob(DispatcherOperation job)
           at Avalonia.Threading.Dispatcher.ExecuteJobsCore(Boolean fromExplicitBackgroundProcessingCallback)
           at Avalonia.Threading.Dispatcher.Signaled()
           at Avalonia.X11.X11PlatformThreading.CheckSignaled()
           at Avalonia.X11.X11PlatformThreading.RunLoop(CancellationToken cancellationToken)
           at Avalonia.Threading.DispatcherFrame.Run(IControlledDispatcherImpl impl)
           at Avalonia.Threading.Dispatcher.PushFrame(DispatcherFrame frame)
           at Avalonia.Threading.Dispatcher.MainLoop(CancellationToken cancellationToken)
           at Avalonia.Controls.ApplicationLifetimes.ClassicDesktopStyleApplicationLifetime.StartCore(String[] args)
           at LogBeam.Linux.Program.Main(String[] args)
        """;

    // O mesmo erro visto pelo Dispatcher.UIThread.UnhandledException: o rasto acaba onde foi apanhado.
    private const string AtDispatcher = """
           at Tmds.DBus.SourceGenerator.ComCanonicalDbusmenuHandler.EmitLayoutUpdated(UInt32 revision, Int32 parent)
           at Avalonia.FreeDesktop.DBusMenuExporter.DBusMenuExporterImpl.DoLayoutReset()
           at Avalonia.Threading.DispatcherOperation.InvokeCore()
        """;

    // Um erro no código do LogBeam chamado por um clique num item do menu.
    private const string FromMenuClick = """
           at LogBeam.Linux.MainWindow.ExitApp()
           at Avalonia.Controls.NativeMenuItem.RaiseClick()
           at Avalonia.FreeDesktop.DBusMenuExporter.DBusMenuExporterImpl.HandleEvent(Int32 id, String eventId)
           at Avalonia.Threading.DispatcherOperation.InvokeCore()
        """;

    [Fact]
    public void ReportedStackIsATrayMenuFault() =>
        Assert.True(TrayMenuFault.IsTrayMenuStack(Reported));

    [Fact]
    public void StackSeenByTheDispatcherIsATrayMenuFault() =>
        Assert.True(TrayMenuFault.IsTrayMenuStack(AtDispatcher));

    [Fact]
    public void WindowsLineEndingsAreAccepted() =>
        Assert.True(TrayMenuFault.IsTrayMenuStack(AtDispatcher.Replace("\n", "\r\n")));

    [Fact]
    public void TranslatedFirstWordIsAccepted() =>
        Assert.True(TrayMenuFault.IsTrayMenuStack(AtDispatcher.Replace(" at ", " em ")));

    [Fact]
    public void ErrorInLogBeamCodeCalledByTheMenuIsNotATrayMenuFault() =>
        Assert.False(TrayMenuFault.IsTrayMenuStack(FromMenuClick));

    [Theory]
    [InlineData("   at Avalonia.Controls.Window.Show()\n   at Avalonia.Threading.DispatcherOperation.InvokeCore()")]
    [InlineData("   at LogBeam.Service.Services.ApiClientService.SendAsync()")]
    [InlineData("")]
    [InlineData(null)]
    public void OtherStacksAreNotTrayMenuFaults(string? stack) =>
        Assert.False(TrayMenuFault.IsTrayMenuStack(stack));

    [Fact]
    public void ExceptionThrownByLogBeamCodeIsNotATrayMenuFault()
    {
        Exception? caught = null;
        try { throw new NullReferenceException(); }
        catch (Exception ex) { caught = ex; }

        Assert.False(TrayMenuFault.IsTrayMenuFault(caught));
        Assert.False(TrayMenuFault.IsTrayMenuFault(null));
    }
}
