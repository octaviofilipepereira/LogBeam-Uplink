// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.Text.Json;

namespace LogBeam.Service.Telemetry;

/// <summary>Erro à espera de envio; as ocorrências repetidas somam-se em <see cref="Count"/>.</summary>
public sealed class PendingError
{
    public string   Key        { get; set; } = string.Empty;
    public string   Component  { get; set; } = string.Empty;
    public string   ErrorType  { get; set; } = string.Empty;
    public string   Message    { get; set; } = string.Empty;
    public string   StackTrace { get; set; } = string.Empty;
    public DateTime FirstAtUtc { get; set; }
    public int      Count      { get; set; } = 1;
}

/// <summary>
/// Estado local dos relatórios de erros e dos dados da instalação, guardado em telemetry.json:
/// erros por enviar (agrupados — o mesmo erro só é enviado uma vez por hora, com a contagem) e o
/// resumo do último envio da instalação. Sem rede, os erros ficam à espera; no máximo
/// <see cref="MaxReportsPerHour"/> relatórios por hora, para um erro repetido não inundar o servidor.
/// </summary>
public class TelemetryStore
{
    public const int MaxReportsPerHour = 20;
    public const int MaxPending        = 200;

    private static readonly TimeSpan Hour = TimeSpan.FromHours(1);
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };

    private readonly object _lock = new();
    private readonly string? _path;
    private readonly Func<DateTime> _utcNow;
    private State _state = new();

    private sealed class State
    {
        public string LastInstallFingerprint { get; set; } = string.Empty;
        public string PendingDeletionId { get; set; } = string.Empty;
        public List<PendingError> Pending { get; set; } = new();
        public Dictionary<string, DateTime> LastSentByKey { get; set; } = new();
        public List<DateTime> SentTimes { get; set; } = new();
    }

    /// <param name="path">Ficheiro de estado; null = só em memória (testes).</param>
    public TelemetryStore(string? path, Func<DateTime>? utcNow = null)
    {
        _path   = path;
        _utcNow = utcNow ?? (() => DateTime.UtcNow);
        Load();
    }

    public string LastInstallFingerprint
    {
        get { lock (_lock) return _state.LastInstallFingerprint; }
        set { lock (_lock) { _state.LastInstallFingerprint = value; Save(); } }
    }

    public int PendingCount { get { lock (_lock) return _state.Pending.Count; } }

    /// <summary>Regista uma ocorrência de erro (caminhos com o nome do utilizador já retirados).</summary>
    public void Add(string component, string errorType, string message, string stackTrace)
    {
        message    = Truncate(PathScrubber.Scrub(message), ErrorReport.MaxMessage);
        stackTrace = Truncate(PathScrubber.Scrub(stackTrace), ErrorReport.MaxStackTrace);
        var key    = $"{component}|{errorType}|{message}";

        lock (_lock)
        {
            var existing = _state.Pending.FirstOrDefault(p => p.Key == key);
            if (existing is not null)
            {
                existing.Count++;
            }
            else
            {
                if (_state.Pending.Count >= MaxPending) _state.Pending.RemoveAt(0);
                _state.Pending.Add(new PendingError
                {
                    Key = key, Component = component, ErrorType = errorType,
                    Message = message, StackTrace = stackTrace, FirstAtUtc = _utcNow()
                });
            }
            Save();
        }
    }

    /// <summary>
    /// Erros que podem ser enviados agora: cada um só uma vez por hora, e no total até
    /// <see cref="MaxReportsPerHour"/> na última hora. Não os retira — ver <see cref="MarkSent"/>.
    /// </summary>
    public IReadOnlyList<PendingError> NextBatch()
    {
        lock (_lock)
        {
            var now = _utcNow();
            _state.SentTimes.RemoveAll(t => now - t >= Hour);
            var room = MaxReportsPerHour - _state.SentTimes.Count;
            if (room <= 0) return Array.Empty<PendingError>();

            return _state.Pending
                .Where(p => !_state.LastSentByKey.TryGetValue(p.Key, out var last) || now - last >= Hour)
                .Take(room)
                .ToList();
        }
    }

    public void MarkSent(PendingError error)
    {
        lock (_lock)
        {
            var now = _utcNow();
            _state.Pending.Remove(error);
            _state.LastSentByKey[error.Key] = now;
            _state.SentTimes.Add(now);

            // Esquecer chaves enviadas há mais de uma hora (o ficheiro não cresce sem fim).
            foreach (var old in _state.LastSentByKey.Where(kv => now - kv.Value >= Hour).Select(kv => kv.Key).ToList())
                _state.LastSentByKey.Remove(old);
            Save();
        }
    }

    /// <summary>
    /// Instalação cujo apagamento no servidor ainda não foi confirmado (consentimento retirado
    /// sem rede, por exemplo); vazio se não houver.
    /// </summary>
    public string PendingDeletionId
    {
        get { lock (_lock) return _state.PendingDeletionId; }
        set { lock (_lock) { _state.PendingDeletionId = value; Save(); } }
    }

    /// <summary>
    /// O operador retirou o consentimento: apaga os erros pendentes e o resumo do último envio,
    /// e guarda o identificador para pedir ao servidor que apague o que lá está.
    /// </summary>
    public void Revoke(string installationId)
    {
        lock (_lock) { _state = new State { PendingDeletionId = installationId }; Save(); }
    }

    private void Load()
    {
        if (_path is null || !File.Exists(_path)) return;
        try { _state = JsonSerializer.Deserialize<State>(File.ReadAllText(_path), Json) ?? new State(); }
        catch { _state = new State(); }   // ficheiro danificado: recomeça vazio
    }

    private void Save()
    {
        if (_path is null) return;
        try { File.WriteAllText(_path, JsonSerializer.Serialize(_state, Json)); }
        catch { /* sem escrita em disco: o estado continua em memória */ }
    }

    private static string Truncate(string s, int max) => s.Length <= max ? s : s[..max];
}
