using System;
using System.Collections.Generic;

namespace WpfCustomUtilities.Extensions.Collection
{
    public static class ListExtension
    {
        /// <summary>
        /// Removes and returns items that match the given predicate.
        /// </summary>
        public static ICollection<T> Remove<T>(this ICollection<T> collection, Func<T, bool> predicate)
        {
            var removedItems = new List<T>();

            foreach (var item in collection)
            {
                if (predicate(item))
                    removedItems.Add(item);
            }

            foreach (var item in removedItems)
                collection.Remove(item);

            return removedItems;
        }
    }
}
