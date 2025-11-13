namespace NKE.BlazorUI.AppData.Converters
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    public static class AesEncryptionHelper
    {
        public static string Encrypt(string plainText, string key)
        {
            if (string.IsNullOrWhiteSpace(plainText)) return string.Empty;

            using var aes = Aes.Create();
            aes.Key = SHA256.HashData(Encoding.UTF8.GetBytes(key)).AsSpan(0, 32).ToArray();
            aes.IV = new byte[16]; // 128-bit zero IV, bạn có thể random hơn và lưu kèm IV nếu muốn

            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }
            return Convert.ToBase64String(ms.ToArray());
        }

        public static string Decrypt(string cipherText, string key)
        {
            if (string.IsNullOrWhiteSpace(cipherText)) return string.Empty;

            var bytes = Convert.FromBase64String(cipherText);
            using var aes = Aes.Create();
            aes.Key = SHA256.HashData(Encoding.UTF8.GetBytes(key)).AsSpan(0, 32).ToArray();
            aes.IV = new byte[16];

            using var ms = new MemoryStream(bytes);
            using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }
    }

}
