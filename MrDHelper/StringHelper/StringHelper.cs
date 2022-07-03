using System;
using System.Collections.Generic;
using System.Text;

namespace MrDHelper
{
    /// <summary>
    /// String extension methods.
    /// </summary>
    public static class StringHelper
    {
        /// <summary>
        /// Get MD5 hash from a string.
        /// </summary>
        /// <param name="input"></param>
        /// <returns>Null if <paramref name="input"/> is null; MD5 if <paramref name="input"/> is not null.</returns>
        public static string? GetMd5(this string? input)
        {
            if (input == null) return null;
            using var provider = System.Security.Cryptography.MD5.Create();
            StringBuilder builder = new StringBuilder();

            foreach (byte b in provider.ComputeHash(Encoding.UTF8.GetBytes(input)))
                builder.Append(b.ToString("x2").ToLower());

            return builder.ToString();
        }
    }
}
