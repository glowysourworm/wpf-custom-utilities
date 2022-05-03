using System;

namespace WpfCustomUtilities.Extensions.Collection
{
    public static class ArrayExtension
    {
        /// <summary>
        /// Allocates array and copies data using the supplied selector
        /// </summary>
        public static TResult[] Transform<T, TResult>(this T[] array, Func<T, TResult> selector)
        {
            var result = new TResult[array.Length];

            for (int index = 0; index < array.Length; index++)
                result[index] = selector(array[index]);

            return result;
        }

        /// <summary>
        /// Returns an aggregate of values in the array up until.
        /// </summary>
        public static TResult AggregateUntil<T, TResult>(this T[] array,
                                                           int exclusiveStopIndex,
                                                           TResult initialAggregate,
                                                           Func<T, TResult> selector,
                                                           Func<TResult, TResult, TResult> aggregator)
        {
            for (int index = 0; index < exclusiveStopIndex; index++)
            {
                initialAggregate = aggregator(initialAggregate, selector(array[index]));
            }

            return initialAggregate;
        }

        /// <summary>
        /// Iterates a for-loop for the provided array and uses the callback action
        /// </summary>
        public static void ForEach<T>(this T[] array, Action<T> action)
        {
            for (int index = 0; index < array.Length; index++)
                action(array[index]);
        }
    }
}
