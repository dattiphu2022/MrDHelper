using System;
using System.Collections.Generic;
using System.Text;

namespace MrDHelper
{
    public static class StringHelper
    {
        public static string GetMd5(this string input)
        {
            using var provider = System.Security.Cryptography.MD5.Create();
            StringBuilder builder = new StringBuilder();

            foreach (byte b in provider.ComputeHash(Encoding.UTF8.GetBytes(input)))
                builder.Append(b.ToString("x2").ToLower());

            return builder.ToString();
        }
    }
}
