using System.Collections;

namespace WpfCustomUtilities.RecursiveSerializer.Shared
{
    /// <summary>
    /// Creates unique hash codes using the SHA-256 algorithm
    /// </summary>
    public static class RecursiveSerializerHashGenerator
    {
        const int LARGE_PRIME = 5999471;

        /// <summary>
        /// Implements standard method to calculate hash codes. Calls object.GetHashCode(..) from each
        /// object parameter. Uses these to create the final hash. ORDER OF PROPERTIES MATTERS! SUPPORTS IENUMERABLE
        /// </summary>
        public static int CreateSimpleHash(params object[] propertiesToHash)
        {
            var hash = LARGE_PRIME;
            var found = false;

            for (int index = 0; index < propertiesToHash.Length; index++)
            {
                // IEnumerable
                if (propertiesToHash[index] is IEnumerable)
                {
                    // 1.87% foreach (IEnumerable) cast
                    var enumerable = (IEnumerable)propertiesToHash[index];

                    // Extend for each item in the collection
                    foreach (var propertyItem in enumerable)
                    {
                        if (propertyItem != null)
                        {
                            hash = (hash * LARGE_PRIME) ^ propertyItem.GetHashCode();
                            found = true;
                        }
                    }
                }
                else
                {
                    if (propertiesToHash[index] != null)
                    {
                        hash = (hash * LARGE_PRIME) ^ propertiesToHash[index].GetHashCode();
                        found = true;
                    }
                }
            }

            // Make sure to indicate that there are no properties to hash
            //
            if (found)
                return hash;

            else
                return 0;
        }
    }
}
