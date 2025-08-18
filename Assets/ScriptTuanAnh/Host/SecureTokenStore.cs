using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class SecureTokenStore
{
    private static readonly string FilePath = Path.Combine(Application.persistentDataPath, "session.dat");
    private static readonly byte[] Salt = Encoding.UTF8.GetBytes("Change_This_Salt!");

    // Tạo 64 byte, chia 32 cho mã hoá + 32 cho HMAC
    private static void GetKeys(out byte[] encKey, out byte[] macKey)
    {
        var baseStr = SystemInfo.deviceUniqueIdentifier + "_YourAppKey"; // Đổi chuỗi này cho app của bạn
        using var kdf = new Rfc2898DeriveBytes(baseStr, Salt, 10000, HashAlgorithmName.SHA256);
        var key64 = kdf.GetBytes(64);
        encKey = new byte[32]; macKey = new byte[32];
        Buffer.BlockCopy(key64, 0, encKey, 0, 32);
        Buffer.BlockCopy(key64, 32, macKey, 0, 32);
    }

    private static byte[] NewIV()
    {
        var iv = new byte[16];
        RandomNumberGenerator.Fill(iv);
        return iv;
    }

    public static void Save(string idToken, bool rememberMe, string userId)
    {
        // Giữ payload cũ để tương thích; có thể bổ sung version sau.
        var payload = $"{userId}|{rememberMe}|{idToken}";
        GetKeys(out var encKey, out var macKey);
        var iv = NewIV();

        using var aes = Aes.Create();
        aes.Key = encKey; aes.IV = iv; aes.Mode = CipherMode.CBC; aes.Padding = PaddingMode.PKCS7;

        byte[] cipherBytes;
        using (var ms = new MemoryStream())
        {
            using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs, Encoding.UTF8))
                sw.Write(payload);
            cipherBytes = ms.ToArray();
        }

        // Gói dữ liệu: [IV][CIPHERTEXT][HMAC(IV||CIPHERTEXT)]
        var pack = new byte[iv.Length + cipherBytes.Length];
        Buffer.BlockCopy(iv, 0, pack, 0, iv.Length);
        Buffer.BlockCopy(cipherBytes, 0, pack, iv.Length, cipherBytes.Length);

        byte[] tag;
        using (var hmac = new HMACSHA256(macKey))
            tag = hmac.ComputeHash(pack);

        var final = new byte[pack.Length + tag.Length];
        Buffer.BlockCopy(pack, 0, final, 0, pack.Length);
        Buffer.BlockCopy(tag, 0, final, pack.Length, tag.Length);

        File.WriteAllBytes(FilePath, final);
    }

    public static (bool ok, string userId, bool rememberMe, string idToken) TryLoad()
    {
        try
        {
            if (!File.Exists(FilePath)) return (false, null, false, null);
            var all = File.ReadAllBytes(FilePath);
            if (all.Length < 16 + 1 + 32) return (false, null, false, null);

            GetKeys(out var encKey, out var macKey);

            // Tách tag
            var tag = new byte[32];
            Buffer.BlockCopy(all, all.Length - 32, tag, 0, 32);
            var packLen = all.Length - 32;
            var pack = new byte[packLen];
            Buffer.BlockCopy(all, 0, pack, 0, packLen);

            // Verify HMAC
            byte[] calcTag;
            using (var hmac = new HMACSHA256(macKey))
                calcTag = hmac.ComputeHash(pack);
            if (!CryptographicOperations.FixedTimeEquals(tag, calcTag))
                return (false, null, false, null);

            // Tách IV + ciphertext
            var iv = new byte[16];
            Buffer.BlockCopy(pack, 0, iv, 0, 16);
            var cipher = new byte[pack.Length - 16];
            Buffer.BlockCopy(pack, 16, cipher, 0, cipher.Length);

            using var aes = Aes.Create();
            aes.Key = encKey; aes.IV = iv; aes.Mode = CipherMode.CBC; aes.Padding = PaddingMode.PKCS7;

            using var ms = new MemoryStream(cipher);
            using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var sr = new StreamReader(cs, Encoding.UTF8);

            var text = sr.ReadToEnd();
            var parts = text.Split('|');
            if (parts.Length != 3) return (false, null, false, null);
            return (true, parts[0], bool.Parse(parts[1]), parts[2]);
        }
        catch
        {
            return (false, null, false, null);
        }
    }

    public static void DeleteIfNotRemembered()
    {
        var t = TryLoad();
        if (t.ok && !t.rememberMe) TryDelete();
    }

    public static void TryDelete()
    {
        if (File.Exists(FilePath)) File.Delete(FilePath);
    }
}
