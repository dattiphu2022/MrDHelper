using System;
using System.Collections.Generic;
using System.Text;

namespace MrDHelper
{
    /// <summary>
    /// <see cref="IList{T}"/> extension methods.
    /// </summary>
    public static class IListHelper
    {
        /// <summary>
        /// Add dummy items to the <see cref="IList{T}"/> collection to increase collection count = <paramref name="collectionFinalCount"/>
        /// with the fillin value is <paramref name="fillValue"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="collectionFinalCount"></param>
        /// <param name="fillValue"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static void AddDummyItemsToMaximumCountOf<T>(this IList<T> collection,
            int collectionFinalCount, T fillValue)
        {
            if (collection.IsNull())
            {
                throw new ArgumentNullException(nameof(collection));
            }
            if (collectionFinalCount < collection.Count || collectionFinalCount < 0)
            {
                throw new InvalidOperationException($"{nameof(collectionFinalCount)}.Value can't < {nameof(collection)}.Count, Or < 0");
            }

            var itemCountToAdd = collectionFinalCount - collection.Count;

            //dummyCollection is not null here
            var dummyCollection = new T[itemCountToAdd];
            Array.Fill(dummyCollection, fillValue);
            Array.ForEach(dummyCollection, (dummyItem) =>
            {
                collection.Add(dummyItem);
            });
        }
    }
}
