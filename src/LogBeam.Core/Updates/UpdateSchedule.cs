// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

namespace LogBeam.Core.Updates;

/// <summary>
/// Quando verificar se há versão nova: um minuto depois de arrancar (com o arranque do sistema a rede
/// pode ainda não estar pronta), 15 minutos depois se a verificação falhar e, a partir daí, uma vez
/// por dia. Conta pelo relógio do sistema, para continuar certo depois de o PC estar suspenso.
/// </summary>
public sealed class UpdateSchedule
{
    public static readonly TimeSpan FirstDelay = TimeSpan.FromMinutes(1);
    public static readonly TimeSpan RetryDelay = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan Interval   = TimeSpan.FromDays(1);

    public DateTime NextCheckUtc { get; private set; }

    public UpdateSchedule(DateTime startUtc) => NextCheckUtc = startUtc + FirstDelay;

    public bool IsDue(DateTime nowUtc)
    {
        // O relógio andou para trás (acertado à mão): a espera nunca passa de um dia.
        if (NextCheckUtc - nowUtc > Interval) NextCheckUtc = nowUtc + Interval;
        return nowUtc >= NextCheckUtc;
    }

    public void Record(UpdateCheckResult result, DateTime nowUtc) =>
        NextCheckUtc = nowUtc + (result.Error is null ? Interval : RetryDelay);
}

/// <summary>
/// O aviso automático de uma versão nova aparece no máximo uma vez por dia (também entre arranques:
/// a interface guarda a versão e a hora do último aviso). A verificação pelo menu Ajuda mostra sempre.
/// </summary>
public static class UpdateNotice
{
    public static readonly TimeSpan MinInterval = TimeSpan.FromDays(1);

    public static bool ShouldNotify(string version, string? lastVersion, DateTime? lastNoticeUtc, DateTime nowUtc)
    {
        if (!string.Equals(version, lastVersion, StringComparison.Ordinal) || lastNoticeUtc is null) return true;
        // Um aviso "no futuro" (relógio acertado para trás) não pode calar os seguintes.
        if (lastNoticeUtc.Value > nowUtc) return true;
        return nowUtc - lastNoticeUtc.Value >= MinInterval;
    }
}
