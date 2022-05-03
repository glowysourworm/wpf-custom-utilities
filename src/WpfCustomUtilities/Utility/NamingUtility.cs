using System.Collections.Generic;
using System.Linq;

namespace WpfCustomUtilities.Utility
{
    public static class NamingUtility
    {
        /// <summary>
        /// Returns a unique name - defaulting to "prefix". Example:  prefix = "My Name", names = { "My Name", "Something Else" }
        /// would return "My Name (1)"
        /// </summary>
        public static string GenerateUniqueName(IEnumerable<string> names, string prefix)
        {
            var ctr = 1;
            var name = prefix;
            while (names.Contains(prefix))
                prefix = name + " (" + ctr++.ToString() + ")";

            return prefix;
        }
    }
}
