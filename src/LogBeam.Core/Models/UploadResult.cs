// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

namespace LogBeam.Core.Models;

public class UploadResult
{
    public bool Success { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public string Call { get; set; } = string.Empty;

    public static UploadResult Ok(string provider, string call) => new()
    {
        Success = true,
        ProviderName = provider,
        Call = call
    };

    public static UploadResult Fail(string provider, string call, string error) => new()
    {
        Success = false,
        ProviderName = provider,
        Call = call,
        ErrorMessage = error
    };
}
