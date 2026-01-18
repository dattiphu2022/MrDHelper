using MrDHelper.GenericHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MrDHelper.CollectionHelpers.IEnummerableHelper
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
        public static void ForEach<T>(this IEnumerable<T>? enumeration, Action<T>? action, bool shouldThrowException = false)
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
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            foreach (T item in enumeration)
            {
                action(item);
            }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }

        /// <summary>
        /// Apply <paramref name="func"/> to every item in <paramref name="enumeration"/>.
        /// Skip ForEachSync if <paramref name="enumeration"/> or <paramref name="func"/> is <see cref="null"/> by default.
        /// Custom throw exception by using <paramref name="shouldThrowException"/>
        /// </summary>
        /// <typeparam name="T">object item</typeparam>
        /// <param name="enumeration">IEnummerable<typeparamref name="T"/></param>
        /// <param name="func"></param>
        /// <param name="shouldThrowException"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static async Task ForEachAsync<T>(this IEnumerable<T>? enumeration, Func<T,Task>? func, bool shouldThrowException = false)
        {
            if (enumeration.IsNull())
            {
                if (shouldThrowException)
                {
                    throw new ArgumentNullException(nameof(enumeration));
                }
                return;
            }
            if (func.IsNull())
            {
                if (shouldThrowException)
                {
                    throw new ArgumentNullException(nameof(func));
                }
                return;
            }
            
            //if we go here so far, there are must not errors.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            foreach (T item in enumeration)
            {
                await func.Invoke(item);
            }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }
        /// <summary>
        /// Dispose all item in current collection.
        /// </summary>
        /// <param name="collection"></param>
        public static void Dispose<T>(this IEnumerable<T>? collection)
        {
            if (collection != null)
            {
                foreach (var obj in collection.OfType<IDisposable>())
                {
                    obj?.Dispose();
                }
            }
        }
    }
}
