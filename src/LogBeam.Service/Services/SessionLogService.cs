// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using LogBeam.Core.Models;

namespace LogBeam.Service.Services;

/// <summary>
/// Guarda em memória os QSOs enviados com sucesso na sessão actual (desde que o
/// serviço arrancou), para exportação local para ADIF a pedido do utilizador.
/// </summary>
public class SessionLogService
{
    private readonly List<QsoRecord> _qsos = new();
    private readonly object _lock = new();

    public void Add(QsoRecord qso)
    {
        lock (_lock) _qsos.Add(qso);
    }

    public List<QsoRecord> Snapshot()
    {
        lock (_lock) return new List<QsoRecord>(_qsos);
    }
}
