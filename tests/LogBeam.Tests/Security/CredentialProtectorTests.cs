// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using LogBeam.Core.Security;

namespace LogBeam.Tests.Security;

/// <summary>
/// Testa o CredentialProtector. Usa Windows DPAPI — só corre em Windows,
/// ligado à conta do utilizador actual.
/// </summary>
public class CredentialProtectorTests
{
    [Fact]
    public void Protect_Unprotect_RoundTrips_OriginalValue()
    {
        const string original = "MinhaPasswordSuperSecreta123!";

        var protectedValue   = CredentialProtector.Protect(original);
        var unprotectedValue = CredentialProtector.Unprotect(protectedValue);

        Assert.Equal(original, unprotectedValue);
    }

    [Fact]
    public void Protect_ProducesDifferentCiphertext_ThanPlainText()
    {
        const string original = "hunter2";

        var protectedValue = CredentialProtector.Protect(original);

        Assert.NotEqual(original, protectedValue);
        Assert.True(CredentialProtector.IsEncrypted(protectedValue));
    }

    [Fact]
    public void Protect_EmptyString_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty, CredentialProtector.Protect(string.Empty));
    }

    [Fact]
    public void Unprotect_EmptyString_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty, CredentialProtector.Unprotect(string.Empty));
    }

    [Fact]
    public void Unprotect_InvalidCiphertext_ReturnsEmptyString_InsteadOfThrowing()
    {
        var result = CredentialProtector.Unprotect("isto-nao-e-base64-valido-!!!");

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void IsEncrypted_PlainText_ReturnsFalse()
    {
        Assert.False(CredentialProtector.IsEncrypted("password-em-texto-simples"));
    }
}
