using System;
using System.Collections.Generic;
using System.Text;

namespace MrDHelper
{
    /// <summary>
    /// Extension methods for <see cref="IEnumerable{T}"/>
    /// </summary>
    public static class IEnumerableHelper
    {
        /// <summary>
        /// Apply <paramref name="action"/> to every item in <paramref name="enumeration"/>.
        /// Skip ForEach if <paramref name="enumeration"/> is <see cref="null"/>
        /// </summary>
        /// <typeparam name="T">object item</typeparam>
        /// <param name="enumeration">IEnummerable<typeparamref name="T"/></param>
        /// <param name="action"></param>
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
