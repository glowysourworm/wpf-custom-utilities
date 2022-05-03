﻿using System;
using System.Collections.Generic;
using System.Linq;

using WpfCustomUtilities.SimpleCollections.Collection;

namespace WpfCustomUtilities.Extensions.Collection
{
    /// <summary>
    /// Enumerable extension that uses some home-grown methods with other libraries (to hide the
    /// external namespaces) to provide more linq extensions
    /// </summary>
    public static class EnumerableExtension
    {
        /// <summary>
        /// Selects all distinct pairs of elements from both collections - based on a common key - ignoring both ordering, and duplicates. 
        /// Example:  { 1, 2, 3 } X { 1, 3, 5 }  produces { (1, 3), (1, 5), (2, 1), (2, 3), (5, 2), (5, 3) }
        /// </summary>
        public static IEnumerable<TResult> Pairs<T1, T2, TKey, TResult>(this IEnumerable<T1> collection1,
                                                                       IEnumerable<T2> collection2,
                                                                       Func<T1, TKey> keySelector1,
                                                                       Func<T2, TKey> keySelector2,
                                                                       Func<T1, T2, TResult> resultSelector)
        {
            var result = new List<TResult>();
            var lookup = new SimpleDictionary<TKey, SimpleDictionary<TKey, TKey>>();

            foreach (var item1 in collection1)
            {
                foreach (var item2 in collection2)
                {
                    var key1 = keySelector1(item1);
                    var key2 = keySelector2(item2);

                    // Ignore equal items
                    if (key1.Equals(key2))
                        continue;

                    // Ignore duplicate pairs (item1, item2)
                    if (lookup.ContainsKey(key1) &&
                        lookup[key1].ContainsKey(key2))
                        continue;

                    // Ignore duplicate pairs (item2, item1)
                    if (lookup.ContainsKey(key2) &&
                        lookup[key2].ContainsKey(key1))
                        continue;

                    else
                    {
                        // RESULT
                        result.Add(resultSelector(item1, item2));

                        // Store lookup 1 -> 2
                        if (lookup.ContainsKey(key1))
                            lookup[key1].Add(key2, key2);

                        else
                            lookup.Add(key1, new SimpleDictionary<TKey, TKey>() { { key2, key2 } });

                        // Store lookup 2 -> 1
                        if (lookup.ContainsKey(key2))
                            lookup[key2].Add(key1, key1);

                        else
                            lookup.Add(key2, new SimpleDictionary<TKey, TKey>() { { key1, key1 } });
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Implementation of Except for IEnumerable using Where the provided equalityComparer
        /// </summary>
        public static IEnumerable<T> Except<T>(this IEnumerable<T> first, IEnumerable<T> second, Func<T, T, bool> equalityComparer)
        {
            return first.Where(element1 => !second.Any(element2 => equalityComparer(element1, element2)));
        }

        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var element in collection)
                action(element);
        }

        public static IEnumerable<T> Union<T>(this IEnumerable<T> collection, params T[] items)
        {
            return collection.Union(items);
        }

        /// <summary>
        /// Repeats a transform on an object to create a collection of transformed items. (Somewhat like a "Clone Many")
        /// </summary>
        /// <typeparam name="T">input type</typeparam>
        /// <typeparam name="TTransform">transform type</typeparam>
        /// <param name="item">the source item</param>
        /// <param name="transform">the transform method</param>
        /// <param name="repeatCount">the number of times to repeat the transform</param>
        /// <returns>New collection of type TTransform</returns>
        public static ICollection<TTransform> TransformMany<T, TTransform>(this T item, Func<T, TTransform> transform, int repeatCount)
        {
            var result = new List<TTransform>();
            for (int i = 0; i < repeatCount; i++)
                result.Add(transform(item));

            return result;
        }

        /// <summary>
        /// Removes some elements of a list and return those elements
        /// </summary>
        public static IEnumerable<T> Remove<T>(this IList<T> list, Func<T, bool> predicate)
        {
            var result = new List<T>();

            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (predicate(list[i]))
                {
                    // Store with results
                    result.Add(list[i]);

                    // Remove from the list
                    list.RemoveAt(i);
                }
            }

            return result;
        }

        /// <summary>
        /// Removes specified elements from the list using ReferenceEquals(..., ...)
        /// </summary>
        public static void Remove<T>(this IList<T> list, IEnumerable<T> listElements)
        {
            // Foreach item-to-remove
            foreach (var item in listElements)
            {
                // Iterate BACKWARDS and remove ALL INSTANCES
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    if (ReferenceEquals(list[i], item))
                        list.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Forces actualization of Lazy enumerables
        /// </summary>
        public static IEnumerable<T> Actualize<T>(this IEnumerable<T> collection)
        {
            return collection.ToList();
        }

        /// <summary>
        /// Copies element references into a new collection
        /// </summary>
        public static IEnumerable<T> Copy<T>(this IEnumerable<T> collection)
        {
            var result = new List<T>(collection.Count());

            foreach (var item in collection)
                result.Add(item);

            return result;
        }

        public static bool None<T>(this IEnumerable<T> collection)
        {
            return !collection.Any();
        }

        public static bool None<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
        {
            return !collection.Any(x => predicate(x));
        }

        /// <summary>
        /// Returns elements that have a non-unique property
        /// </summary>
        public static IEnumerable<T> NonUnique<T, K>(this IEnumerable<T> collection, Func<T, K> selector)
        {
            var counts = collection.Select(x => new
            {
                Item = x,
                Count = collection.Where(z => selector(z).Equals(selector(x))).Count()
            });

            return counts.Where(x => x.Count > 1).Select(x => x.Item);
        }

        /// <summary>
        /// Returns all elements in the collection that match the given item using the provided key selector. This
        /// avoids some extra Linq calls to GroupBy to create the collection.
        /// </summary>
        public static IEnumerable<T> Gather<T, K>(this IEnumerable<T> collection, T item, Func<T, K> keySelector)
        {
            return collection.Where(x => keySelector(x).Equals(keySelector(item)));
        }

        /// <summary>
        /// Returns elements that are distinct up to the selected key (property). This would be the first
        /// such element in a grouping by the provided key selector
        /// </summary>
        public static IEnumerable<T> DistinctByKey<T, K>(this IEnumerable<T> collection, Func<T, K> keySelector)
        {
            return collection.GroupBy(x => keySelector(x)).Select(x => x.First());
        }

        /// <summary>
        /// Returns elements that are distinct using the specified equality comparer. NOTE*** Relies on object.Equals() to compare
        /// references (or values).
        /// </summary>
        public static IList<T> DistinctWith<T>(this IList<T> list, Func<T, T, bool> equalityComparer)
        {
            // Iterate backwards to avoid problems when removing. STOP AT 1.
            for (int index1 = list.Count - 1; index1 >= 1; index1--)
            {
                // Iterate starting behind index1 -> 0. Remove at index2 when duplicate(s) found
                for (int index2 = index1 - 1; index2 >= 0; index2--)
                {
                    if (equalityComparer(list[index1], list[index2]))
                    {
                        // Remove at index2
                        list.RemoveAt(index2);

                        // Decrement index1 by 1 to compensate
                        index1--;
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Returns minimum of a collection by a given selector
        /// </summary>
        public static T MinBy<T, V>(this IEnumerable<T> collection, Func<T, V> selector) where V : IComparable
        {
            var minValue = default(V);
            var minItem = default(T);
            var first = true;

            foreach (var item in collection)
            {
                var itemValue = selector(item);

                if (first)
                {
                    minValue = itemValue;
                    minItem = item;
                    first = false;
                }

                else if (minValue.CompareTo(itemValue) > 0)
                {
                    minValue = itemValue;
                    minItem = item;
                }
            }

            return minItem;
        }

        /// <summary>
        /// Returns maximum of a collection by a given selector
        /// </summary>
        public static T MaxBy<T, V>(this IEnumerable<T> collection, Func<T, V> selector) where V : IComparable
        {
            var maxValue = default(V);
            var maxItem = default(T);
            var first = true;

            foreach (var item in collection)
            {
                var itemValue = selector(item);

                if (first)
                {
                    maxValue = itemValue;
                    maxItem = item;
                    first = false;
                }

                else if (maxValue.CompareTo(itemValue) < 0)
                {
                    maxValue = itemValue;
                    maxItem = item;
                }
            }

            return maxItem;
        }

        /// <summary>
        /// Returns the item before (or default) the provided item by reference
        /// </summary>
        /// <param name="loopEnd">Set to true to enable looping back to the last item</param>
        public static T BeforeOrDefault<T>(this IEnumerable<T> collection, T item, bool loopEnd = true) where T : class
        {
            var found = false;
            var first = true;
            T previous = default(T);

            foreach (var otherItem in collection)
            {
                if (ReferenceEquals(item, otherItem))
                {
                    found = true;
                    break;
                }

                previous = otherItem;
                first = false;
            }

            if (found)
            {
                // Was the first item -> Loop end only
                if (first && loopEnd)
                    return collection.LastOrDefault();

                else
                    return previous;
            }
            else
            {
                return default(T);
            }
        }

        /// <summary>
        /// Returns the item after (or default) the provided item by reference
        /// </summary>
        /// <param name="loopEnd">Set to true to enable looping back to the first item</param>
        public static T AfterOrDefault<T>(this IEnumerable<T> collection, T item, bool loopBegin = true) where T : class
        {
            var found = false;

            foreach (var otherItem in collection)
            {
                if (found)
                    return otherItem;

                if (ReferenceEquals(item, otherItem))
                    found = true;
            }

            // Found as last item -> only for loop beginning
            if (found && loopBegin)
                return collection.FirstOrDefault();

            else
                return default(T);
        }

        /// <summary>
        /// Synchronizes a souce collection with a destination collection using the provided: equality comparer,
        /// constructor, and updater Func's. This will also apply a hard-constraint on ordering (using the source
        /// ordering as a guide)
        /// </summary>
        /// <param name="equalityComparer">Anonymous method that provides a bool for the source / dest inputs</param>
        /// <param name="construct">Anonymous method that constructs a TDest object</param>
        /// <param name="update">Anonymous method that updates a destination object from the source</param>
        public static void SynchronizeFrom<TSource, TDest>(
                            this IList<TDest> destCollection,
                            IEnumerable<TSource> sourceCollection,
                            Func<TSource, TDest, bool> equalityComparer,
                            Func<TSource, TDest> construct,
                            Action<TSource, TDest> update)
        {
            // Use an index to apply proper ordering
            var index = 0;

            // Start from the beginning of the source
            foreach (var item in sourceCollection)
            {
                // Get Destination Item to compare
                var destItem = destCollection.ElementAtOrDefault(index);

                // Add
                if (destItem == null)
                    destCollection.Add(construct(item));

                // Compare for Equality (FALSE) => Replace
                else if (!equalityComparer(item, destItem))
                {
                    destCollection.RemoveAt(index);
                    destCollection.Insert(index, construct(item));
                }

                // Compare for Equality (TRUE) => Update
                else if (equalityComparer(item, destItem))
                    update(item, destItem);

                // Error Handling: improper use of equality comparer
                else
                    throw new Exception("Improper use of equality comparer in SynchronizeFrom");

                index++;
            }

            // Continue on to remove additional items from the destination collection
            for (int i = destCollection.Count - 1; i >= index; i--)
                destCollection.RemoveAt(i);
        }

        /// <summary>
        /// Returns the next item in the collection after the specified one. If it's the last item - then 
        /// the first item is returned
        /// </summary>
        public static T Next<T>(this IEnumerable<T> collection, T item) where T : class
        {
            var found = false;

            foreach (var otherItem in collection)
            {
                if (found)
                    return otherItem;

                else if (otherItem == item)
                    found = true;
            }

            if (found)
                return collection.First();

            else
                throw new Exception("Item in collection not found EnumerableExtension.Next()");
        }

        public static IEnumerable<V> OfType<T, V>(this IEnumerable<T> collection)
        {
            return collection.Where(item => item is V).Cast<V>();
        }

        public static SimpleDictionary<KResult, VResult> ToSimpleDictionary<T, KResult, VResult>(this IEnumerable<T> collection,
                                                                                                 Func<T, KResult> keySelector,
                                                                                                 Func<T, VResult> valueSelector)
        {
            var result = new SimpleDictionary<KResult, VResult>();

            foreach (var element in collection)
                result.Add(keySelector(element), valueSelector(element));

            return result;
        }

        public static SimpleDictionary<KResult, VResult> ToSimpleDictionary<T, KResult, VResult>(this ICollection<T> collection,
                                                                                         Func<T, KResult> keySelector,
                                                                                         Func<T, VResult> valueSelector)
        {
            var result = new SimpleDictionary<KResult, VResult>(collection.Count);

            foreach (var element in collection)
                result.Add(keySelector(element), valueSelector(element));

            return result;
        }
    }
}
