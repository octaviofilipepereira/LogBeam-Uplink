// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

namespace LogBeam.Core.Platform;

/// <summary>
/// Falhas do menu do ícone da área de notificação no Linux. O Avalonia publica esse menu pelo D-Bus
/// (com.canonical.dbusmenu) e, na versão 11.3, pode lançar uma NullReferenceException ao avisar o
/// painel de que o menu mudou, quando o objecto do menu já não está registado no D-Bus. O erro chegava
/// ao ciclo da janela sem ser apanhado e fechava o Uplink (versão 2.6.1, relatórios de 15/11 e
/// 21/11/2026). A recepção e o envio de QSOs não dependem deste menu.
/// </summary>
public static class TrayMenuFault
{
    private const string MenuExporter = "Avalonia.FreeDesktop.DBusMenuExporter";

    /// <summary>
    /// Verdadeiro quando a excepção nasceu no código do Avalonia que publica o menu, antes de passar
    /// por código do LogBeam. Um erro no clique de um item do menu (código do LogBeam chamado pelo
    /// menu) não conta: esse continua a ser tratado como os outros.
    /// </summary>
    public static bool IsTrayMenuFault(Exception? ex) => ex is not null && IsTrayMenuStack(ex.StackTrace);

    /// <summary>
    /// Como <see cref="IsTrayMenuFault"/>, a partir do texto do rasto da pilha, lido do ponto onde a
    /// excepção foi lançada para fora. A primeira palavra de cada linha ("at") não é comparada, porque
    /// o .NET pode traduzi-la.
    /// </summary>
    public static bool IsTrayMenuStack(string? stackTrace)
    {
        if (string.IsNullOrEmpty(stackTrace)) return false;

        foreach (var raw in stackTrace.Split('\n'))
        {
            var line  = raw.Trim();
            var space = line.IndexOf(' ');
            if (space < 0) continue;

            var frame = line[(space + 1)..];
            if (frame.StartsWith("LogBeam.", StringComparison.Ordinal)) return false;
            if (frame.StartsWith(MenuExporter, StringComparison.Ordinal)) return true;
        }
        return false;
    }
}
