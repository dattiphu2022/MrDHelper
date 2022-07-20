using System;
using System.Collections.Generic;
using System.Text;

namespace MrDHelper
{
    /// <summary>
    /// String extension methods.
    /// </summary>
    public static partial class StringHelper
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
        /// <summary>
        /// Short way of "string.IsNullOrEmpty(input)"
        /// </summary>
        /// <param name="input">string to check</param>
        /// <returns><see cref="Boolean"/></returns>
        public static bool IsNullOrEmpty(this string? input)
        {
            return string.IsNullOrEmpty(input);
        }

        /// <summary>
        /// Short way of "string.IsNullOrWhiteSpace(input)"
        /// </summary>
        /// <param name="input">string to check</param>
        /// <returns><see cref="Boolean"/></returns>
        public static bool IsNullOrWhiteSpace(this string? input)
        {
            return string.IsNullOrWhiteSpace(input);
        }
        /// <summary>
        /// Short way of "!string.IsNullOrEmpty(input)"
        /// </summary>
        /// <param name="input">string to check</param>
        /// <returns><see cref="Boolean"/></returns>
        public static bool NotNullOrEmpty(this string? input)
        {
            return !string.IsNullOrEmpty(input);
        }

        /// <summary>
        /// Short way of "!string.IsNullOrWhiteSpace(input)"
        /// </summary>
        /// <param name="input">string to check</param>
        /// <returns><see cref="Boolean"/></returns>
        public static bool NotNullOrWhiteSpace(this string? input)
        {
            return !string.IsNullOrWhiteSpace(input);
        }
    }
}
