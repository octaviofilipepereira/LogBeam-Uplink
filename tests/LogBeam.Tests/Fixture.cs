// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

namespace LogBeam.Tests;

/// <summary>Lê os pacotes reais guardados em Fixtures/ (copiados para a pasta de saída dos testes).</summary>
internal static class Fixture
{
    public static byte[] Bytes(string relativePath) =>
        File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Fixtures", relativePath));

    public static string Text(string relativePath) =>
        System.Text.Encoding.UTF8.GetString(Bytes(relativePath));
}
