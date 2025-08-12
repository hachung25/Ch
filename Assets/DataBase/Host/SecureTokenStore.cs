using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class SecureTokenStore
{
    private static readonly string FilePath = Path.Combine(Application.persistentDataPath, "auth.dat");
    // Tự đặt muối riêng của game (đừng commit public)
    private const string Salt = "YourGame$Pepper#2025";

    static byte[] GetKeyAndIV(out byte[] iv)
    {
        // KHÔNG dùng device id làm khoá duy nhất, chỉ làm nguyên liệu dẫn xuất
        string material = SystemInfo.deviceUniqueIdentifier + "|" + Salt;
        using var sha = SHA256.Create();
        byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(material));
        // 32 bytes key, 16 bytes iv từ hash
        byte[] key = hash;
        iv = new byte[16];
        Array.Copy(hash, 0, iv, 0, 16);
        return key;
    }

    public static void Save(string token, bool rememberMe)
    {
        var payload = JsonUtility.ToJson(new TokenPayload { token = token, remember = rememberMe });
        byte[] plain = Encoding.UTF8.GetBytes(payload);
        byte[] key = GetKeyAndIV(out var iv);

        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            cs.Write(plain, 0, plain.Length);
        File.WriteAllBytes(FilePath, ms.ToArray());
    }

    public static bool TryLoad(out string token, out bool remember)
    {
        token = null; remember = false;
        if (!File.Exists(FilePath)) return false;

        byte[] cipher = File.ReadAllBytes(FilePath);
        byte[] key = GetKeyAndIV(out var iv);

        try
        {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            using var ms = new MemoryStream(cipher);
            using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var sr = new StreamReader(cs, Encoding.UTF8);
            var json = sr.ReadToEnd();
            var p = JsonUtility.FromJson<TokenPayload>(json);
            token = p.token; remember = p.remember;
            return !string.IsNullOrEmpty(token);
        }
        catch { return false; }
    }

    public static void DeleteIfNotRemember()
    {
        if (TryLoad(out _, out bool remember))
        {
            if (!remember && File.Exists(FilePath)) File.Delete(FilePath);
        }
    }

    public static void ForceDelete()
    {
        if (File.Exists(FilePath)) File.Delete(FilePath);
    }

    [Serializable] class TokenPayload { public string token; public bool remember; }
}
