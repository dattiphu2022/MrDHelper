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
        /// Skip ForEach if <paramref name="enumeration"/> or <paramref name="action"/> is <see cref="null"/> by default.
        /// Custom throw exception by using <paramref name="shouldThrowException"/>
        /// </summary>
        /// <typeparam name="T">object item</typeparam>
        /// <param name="enumeration">IEnummerable<typeparamref name="T"/></param>
        /// <param name="action"></param>
        /// <param name="shouldThrowException"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void ForEach<T>(this IEnumerable<T>? enumeration, Action<T> action, bool shouldThrowException = false)
        {
            if (enumeration.IsNull())
            {
                if (shouldThrowException)
                {
                    throw new ArgumentNullException(nameof(enumeration));
                }
                return;
            }
            if (action.IsNull())
            {
                if (shouldThrowException)
                {
                    throw new ArgumentNullException(nameof(action));
                }
                return;
            }
            //if we go here so far, there are must not errors.
            foreach (T item in enumeration)
            {
                action(item);
            }
        }
    }
}
