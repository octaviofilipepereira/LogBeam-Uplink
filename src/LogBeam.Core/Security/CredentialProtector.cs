// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using LogBeam.Core.Platform;

namespace LogBeam.Core.Security;

/// <summary>
/// Cifra e decifra textos sensíveis. No Windows usa o DPAPI, ligado à conta do utilizador.
/// No Linux usa AES-256-GCM com uma chave aleatória guardada na pasta de dados, acessível
/// apenas ao utilizador.
/// </summary>
public static class CredentialProtector
{
    private const int NonceSize = 12;
    private const int TagSize   = 16;
    private const int KeySize   = 32;

    private static readonly byte[] Entropy =
        Encoding.UTF8.GetBytes("LogBeam-CT7BFV-Salt-2026");

    /// <summary>
    /// Encrypts a plain text string. Returns Base64-encoded ciphertext.
    /// </summary>
    public static string Protect(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return string.Empty;

        var data = Encoding.UTF8.GetBytes(plainText);
        var encrypted = OperatingSystem.IsWindows()
            ? ProtectedData.Protect(data, Entropy, DataProtectionScope.CurrentUser)
            : Seal(data);

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
            var decrypted = OperatingSystem.IsWindows()
                ? ProtectedData.Unprotect(data, Entropy, DataProtectionScope.CurrentUser)
                : Open(data);

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

    // ─── Linux ──────────────────────────────────────────────────────────────

    [UnsupportedOSPlatform("windows")]
    private static byte[] Seal(byte[] plain)
    {
        var nonce  = RandomNumberGenerator.GetBytes(NonceSize);
        var tag    = new byte[TagSize];
        var cipher = new byte[plain.Length];

        using var aes = new AesGcm(UserKey(), TagSize);
        aes.Encrypt(nonce, plain, cipher, tag, Entropy);

        return [.. nonce, .. tag, .. cipher];
    }

    [UnsupportedOSPlatform("windows")]
    private static byte[] Open(byte[] sealedData)
    {
        var nonce  = sealedData[..NonceSize];
        var tag    = sealedData[NonceSize..(NonceSize + TagSize)];
        var cipher = sealedData[(NonceSize + TagSize)..];
        var plain  = new byte[cipher.Length];

        using var aes = new AesGcm(UserKey(), TagSize);
        aes.Decrypt(nonce, cipher, tag, plain, Entropy);

        return plain;
    }

    /// <summary>Lê a chave do utilizador; da primeira vez cria-a, só com permissões para ele.</summary>
    [UnsupportedOSPlatform("windows")]
    private static byte[] UserKey()
    {
        var path = Path.Combine(AppPaths.DataDir, "credentials.key");
        if (File.Exists(path)) return File.ReadAllBytes(path);

        var key = RandomNumberGenerator.GetBytes(KeySize);
        using var file = new FileStream(path, new FileStreamOptions
        {
            Mode           = FileMode.CreateNew,
            Access         = FileAccess.Write,
            UnixCreateMode = UnixFileMode.UserRead | UnixFileMode.UserWrite
        });
        file.Write(key);
        return key;
    }
}
