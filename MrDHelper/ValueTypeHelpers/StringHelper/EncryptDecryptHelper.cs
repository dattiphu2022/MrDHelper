namespace MrDHelper
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// String extension methods.
    /// </summary>
    public static partial class StringHelper
    {
        /* The Input String (UTF8) is sealed with a MD5-128-Bit-Hash and then Crypted to a Base64 String */

        public static string EncryptToBase64(this string originalText, string password)
        {
            var userBytes = Encoding.UTF8.GetBytes(originalText); // UTF8 saves Space
            var userHash = MD5.Create().ComputeHash(userBytes);
            SymmetricAlgorithm crypt = Aes.Create(); // (Default: AES-CCM (Counter with CBC-MAC))
            crypt.Key = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(password)); // MD5: 128 Bit Hash
            crypt.IV = new byte[16]; // by Default. IV[] to 0.. is OK simple crypt
            using var memoryStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memoryStream, crypt.CreateEncryptor(), CryptoStreamMode.Write);
            cryptoStream.Write(userBytes, 0, userBytes.Length); // User Data
            cryptoStream.Write(userHash, 0, userHash.Length); // Add HASH
            cryptoStream.FlushFinalBlock();
            var resultString = Convert.ToBase64String(memoryStream.ToArray());
            return resultString;
        }
        /* Try to get original (decrypted) String. Password (and Base64-format) must be correct */
        public static string DecryptFromBase64(this string encryptedText, string password)
        {
            var encryptedBytes = Convert.FromBase64String(encryptedText);
            SymmetricAlgorithm crypt = Aes.Create();
            crypt.Key = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(password));
            crypt.IV = new byte[16];
            using var memoryStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memoryStream, crypt.CreateDecryptor(), CryptoStreamMode.Write);
            cryptoStream.Write(encryptedBytes, 0, encryptedBytes.Length);
            cryptoStream.FlushFinalBlock();
            var allBytes = memoryStream.ToArray();
            var userLen = allBytes.Length - 16;
            if (userLen < 0) throw new Exception("Invalid Len");   // No Hash?
            var userHash = new byte[16];
            Array.Copy(allBytes, userLen, userHash, 0, 16); // Get the 2 Hashes
            var decryptHash = MD5.Create().ComputeHash(allBytes, 0, userLen);
            if (userHash.SequenceEqual(decryptHash) == false) throw new Exception("Invalid Hash");
            var resultString = Encoding.UTF8.GetString(allBytes, 0, userLen);
            return resultString;
        }
    }
}
