// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.Text.Json;
using System.Text.Json.Serialization;
using LogBeam.Core.Models;
using Microsoft.Extensions.Logging;

namespace LogBeam.Service.Services;

/// <summary>Um QSO pendente na fila offline, associado ao perfil (logbook) de destino.</summary>
public class QueuedQso
{
    public string ProfileId { get; set; } = string.Empty;
    public QsoRecord Qso { get; set; } = new();
}

/// <summary>
/// Fila persistente de QSOs pendentes: guarda em queue.json os QSOs que falharam
/// o envio (mesmo após retries) para reenvio automático assim que a ligação voltar.
/// Cada entrada está associada a um perfil (logbook) de destino específico.
/// </summary>
public class QsoQueueService
{
    private const int MaxQueueSize = 1000;

    private readonly string _queuePath;
    private readonly List<QueuedQso> _queue = new();
    private readonly object _lock = new();
    private readonly ILogger<QsoQueueService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <param name="queuePath">Ficheiro da fila; por omissão, queue.json junto do executável (os testes usam outro).</param>
    public QsoQueueService(ILogger<QsoQueueService> logger, string? queuePath = null)
    {
        _logger     = logger;
        _queuePath  = queuePath ?? Path.Combine(AppContext.BaseDirectory, "queue.json");
        Load();
    }

    public int Count { get { lock (_lock) return _queue.Count; } }

    public void Enqueue(QsoRecord qso, string profileId)
    {
        lock (_lock)
        {
            _queue.Add(new QueuedQso { ProfileId = profileId, Qso = qso });
            if (_queue.Count > MaxQueueSize)
                _queue.RemoveAt(0);
            Save();
        }
        _logger.LogWarning("QSO {Callsign} colocado na fila offline (perfil {ProfileId}, agora com {Count} pendentes).", qso.Call, profileId, Count);
    }

    /// <summary>Cópia da fila actual, pela ordem de chegada.</summary>
    public List<QueuedQso> Snapshot()
    {
        lock (_lock) return new List<QueuedQso>(_queue);
    }

    public void Remove(QueuedQso item)
    {
        lock (_lock)
        {
            _queue.Remove(item);
            Save();
        }
    }

    private void Load()
    {
        try
        {
            if (!File.Exists(_queuePath)) return;
            var json  = File.ReadAllText(_queuePath);
            var items = JsonSerializer.Deserialize<List<QueuedQso>>(json, JsonOptions);
            if (items != null) _queue.AddRange(items);
            if (_queue.Count > 0)
                _logger.LogInformation("{Count} QSOs pendentes carregados da fila offline.", _queue.Count);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao carregar fila offline de {Path}", _queuePath);
        }
    }

    private void Save()
    {
        try
        {
            File.WriteAllText(_queuePath, JsonSerializer.Serialize(_queue, JsonOptions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao guardar fila offline em {Path}", _queuePath);
        }
    }
}
