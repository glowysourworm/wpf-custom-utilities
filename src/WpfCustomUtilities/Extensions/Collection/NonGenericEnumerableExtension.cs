using System;
using System.Collections;
using System.Collections.Generic;

namespace WpfCustomUtilities.Extensions.Collection
{
    public static class NonGenericEnumerableExtension
    {
        /// <summary>
        /// Performs collection search using casting and the provided predicate
        /// </summary>
        public static T First<T>(this IEnumerable collection, Func<T, bool> predicate)
        {
            foreach (var item in collection)
            {
                if (predicate((T)item))
                    return (T)item;
            }

            throw new Exception("No item found in the collection matching predicate:  NonGenericEnumerableExtension");
        }

        /// <summary>
        /// Performs collection query using casting and the provided predicate
        /// </summary>
        public static bool Any<T>(this IEnumerable collection, Func<T, bool> predicate)
        {
            foreach (var item in collection)
            {
                if (predicate((T)item))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Performs collection query using casting and the provided predicate
        /// </summary>
        public static IEnumerable<TResult> Select<T, TResult>(this IEnumerable collection, Func<T, TResult> selector)
        {
            var result = new List<TResult>();

            foreach (var item in collection)
            {
                result.Add(selector((T)item));
            }

            return result;
        }
    }
}
