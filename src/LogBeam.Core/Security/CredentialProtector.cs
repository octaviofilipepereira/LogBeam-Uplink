// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.Security.Cryptography;
using System.Text;

namespace LogBeam.Core.Security;

/// <summary>
/// Encrypts and decrypts sensitive strings using Windows DPAPI.
/// Encrypted data is tied to the current Windows user account.
/// </summary>
public static class CredentialProtector
{
    private static readonly byte[] Entropy =
        Encoding.UTF8.GetBytes("LogBeam-CT7BFV-Salt-2026");

    /// <summary>
    /// Encrypts a plain text string. Returns Base64-encoded ciphertext.
    /// </summary>
    public static string Protect(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return string.Empty;

        var data = Encoding.UTF8.GetBytes(plainText);
        var encrypted = ProtectedData.Protect(
            data, Entropy, DataProtectionScope.CurrentUser);

        return Convert.ToBase64String(encrypted);
    }

    /// <summary>
    /// Decrypts a Base64-encoded ciphertext string.
    /// Returns empty string if decryption fails.
    /// </summary>
    public static string Unprotect(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return string.Empty;

        try
        {
            var data = Convert.FromBase64String(cipherText);
            var decrypted = ProtectedData.Unprotect(
                data, Entropy, DataProtectionScope.CurrentUser);

            return Encoding.UTF8.GetString(decrypted);
        }
        catch
        {
            return string.Empty;
        }
    }

    public static bool IsEncrypted(string value)
    {
        if (string.IsNullOrEmpty(value)) return false;
        try
        {
            var data = Convert.FromBase64String(value);
            return data.Length > 0;
        }
        catch
        {
            return false;
        }
    }
}
