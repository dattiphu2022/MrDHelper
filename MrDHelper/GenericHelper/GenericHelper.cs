using System;
using System.Collections.Generic;
using System.Text;

namespace MrDHelper
{
    public static class GenericHelper
    {
        public static bool IsNull<T>(this T input)
        {
            return input is null;
        }
        public static bool NotNull<T>(this T input)
        {
            return !IsNull(input);
        }
    }
}
