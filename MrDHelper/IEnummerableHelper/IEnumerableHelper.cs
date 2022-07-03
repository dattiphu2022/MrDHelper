using System;
using System.Collections.Generic;
using System.Text;

namespace MrDHelper
{
    public static class IEnumerableHelper
    {
        public static void ForEach<T>(this IEnumerable<T>? enumeration, Action<T> action)
        {
            if (enumeration == null)
            {
                return;
            }
            foreach (T item in enumeration)
            {
                action(item);
            }
        }
    }
}
