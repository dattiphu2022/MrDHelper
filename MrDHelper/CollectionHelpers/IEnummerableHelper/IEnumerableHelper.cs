//document source: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/yield

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MrDHelper
{
    #region Loop helper
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
        public static async Task ForEachAsync<T>(this IEnumerable<T>? enumeration, Func<T, Task>? func, bool shouldThrowException = false)
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
        #endregion

        #region CRUD helper
        public static IEnumerable<T> AddLast<T>(this IEnumerable<T> enumeration, T itemToAdd)
        {
            try
            {
                if (enumeration.IsNull())
                {
                    throw new ArgumentNullException(nameof(enumeration));
                }
            }
            catch (Exception)
            {

                throw;
            }
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            foreach (var item in enumeration)
            {
                yield return item;
            }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            yield return itemToAdd;
        }
        public static IEnumerable<T> InsertAt<T>(this IEnumerable<T>? enumeration, int index, T itemToAdd)
        {
            if (enumeration.IsNull())
            {
                throw new ArgumentNullException(nameof(enumeration));
            }
            if (index < 0 || index > enumeration.Count())
            {
                throw new IndexOutOfRangeException(nameof(index));
            }
            int currentIndex = 0;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            foreach (var item in enumeration)
            {
                if (currentIndex == index)
                    yield return itemToAdd;

                yield return item;
                currentIndex++;
            }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }
        public static IEnumerable<T> UpdateAt<T>(this IEnumerable<T>? enumeration, int index, T itemToAdd)
        {
            if (enumeration.IsNull())
            {
                throw new ArgumentNullException(nameof(enumeration));
            }
            if (index < 0 || index >= enumeration.Count())
            {
                throw new IndexOutOfRangeException(nameof(index));
            }
            int currentIndex = 0;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            foreach (var item in enumeration)
            {
                yield return currentIndex == index ? itemToAdd : item;
                currentIndex++;
            }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }
        public static IEnumerable<T> RemoveAt<T>(this IEnumerable<T>? enumeration, int index)
        {
            if (enumeration.IsNull())
            {
                throw new ArgumentNullException(nameof(enumeration));
            }
            if (index < 0 || index >= enumeration.Count())
            {
                throw new IndexOutOfRangeException(nameof(index));
            }
            int currentIndex = 0;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            foreach (var item in enumeration)
            {
                if (currentIndex != index)
                    yield return item;

                currentIndex++;
            }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }
        #endregion
    }
}
