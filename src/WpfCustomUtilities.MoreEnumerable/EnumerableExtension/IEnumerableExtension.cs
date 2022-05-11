using System;
using System.Collections.Generic;
using System.Linq;

using MoreLinq;

namespace WpfCustomUtilities.MoreEnumerable.EnumerableExtension
{
    public static class IEnumerableExtension
    {
        public static IEnumerable<TResult> FullJoin<T, TSecond, TKey, TResult>(this IEnumerable<T> collection,
                                                                                IEnumerable<TSecond> secondCollection,
                                                                                Func<T, TKey> firstKeySelector,
                                                                                Func<TSecond, TKey> secondKeySelector,
                                                                                Func<T, TResult> firstSelector,
                                                                                Func<TSecond, TResult> secondSelector,
                                                                                Func<T, TSecond, TResult> bothSelector)
        {
            return MoreLinq.MoreEnumerable.FullJoin(collection, secondCollection,
                                           firstKeySelector,
                                           secondKeySelector,
                                           firstSelector,
                                           secondSelector,
                                           bothSelector);
        }

        public static IEnumerable<TResult> LeftJoin<T, TKey, TResult>(this IEnumerable<T> collection,
                                                                        IEnumerable<T> secondCollection,
                                                                        Func<T, TKey> keySelector,
                                                                        Func<T, TResult> firstSelector,
                                                                        Func<T, T, TResult> bothSelector)
        {
            return MoreLinq.MoreEnumerable.LeftJoin(collection, secondCollection, keySelector, firstSelector, bothSelector);
        }

        public static IEnumerable<T> Maxima<T, TValue>(this IEnumerable<T> collection, Func<T, TValue> valueSelector)
        {
            return MoreLinq.MoreEnumerable.MaxBy(collection, valueSelector);
        }

        public static IEnumerable<T> Minima<T, TValue>(this IEnumerable<T> collection, Func<T, TValue> valueSelector)
        {
            return MoreLinq.MoreEnumerable.MinBy(collection, valueSelector);
        }

        /// <summary>
        /// Returns the item of a collection corresponding to the max of some selected value
        /// </summary>
        public static T MaxWith<T, TValue>(this IEnumerable<T> collection, Func<T, TValue> valueSelector) where TValue : IComparable
        {
            return MoreLinq.MoreEnumerable.MaxBy(collection, valueSelector).Max();
        }

        /// <summary>
        /// Returns the item of a collection corresponding to the mix of some selected value
        /// </summary>
        public static T MinWhere<T, TValue>(this IEnumerable<T> collection, Func<T, TValue> valueSelector) where TValue : IComparable
        {
            return MoreLinq.MoreEnumerable.MinBy(collection, valueSelector).Max();
        }

        /// <summary>
        /// Returns the max of the collection using the selector - or default(TResult)
        /// </summary>
        public static TResult MaxOrDefault<T, TResult>(this IEnumerable<T> collection, Func<T, TResult> selector, TResult defaultValue = default(TResult)) where TResult : IComparable<TResult>
        {
            var maxima = MoreLinq.MoreEnumerable.MaxBy(collection, selector);

            if (maxima.Any())
                return selector(maxima.First());

            else
                return defaultValue;
        }

        /// <summary>
        /// Returns the min of the collection using the selector - or default(TResult)
        /// </summary>
        public static TResult MinOrDefault<T, TResult>(this IEnumerable<T> collection, Func<T, TResult> selector, TResult defaultValue = default(TResult)) where TResult : IComparable<TResult>
        {
            var minima = MoreLinq.MoreEnumerable.MinBy(collection, selector);

            if (minima.Any())
                return selector(minima.First());

            else
                return defaultValue;
        }

        /// <summary>
        /// Selects a random element from the sequence
        /// </summary>
        public static T PickRandom<T>(this IEnumerable<T> collection)
        {
            return collection.Any() ? collection.RandomSubset(1).First() : default(T);
        }

        /// <summary>
        /// Returns random subset of elements from the collection that match the given predicate
        /// </summary>
        public static IEnumerable<T> SelectRandom<T>(this IEnumerable<T> collection, Func<T, bool> predicate, int length)
        {
            if (!collection.Any())
                return collection;

            return collection.Where(predicate).RandomSubset(length);
        }

        /// <summary>
        /// Randomizes the input collection
        /// </summary>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> collection)
        {
            return MoreLinq.MoreEnumerable.Shuffle(collection);
        }
    }
}
