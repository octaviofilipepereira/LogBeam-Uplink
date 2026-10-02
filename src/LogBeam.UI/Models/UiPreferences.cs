// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

namespace LogBeam.UI.Models;

/// <summary>
/// Preferências exclusivas da UI (não partilhadas com o Service). Guardadas em ui.json.
/// </summary>
public class UiPreferences
{
    public string Language { get; set; } = "PT";

    /// <summary>Aviso no tabuleiro a cada QSO enviado. Desligado: só falhas e confirmações LogBeam.</summary>
    public bool NotifyEachQso { get; set; } = false;
}
