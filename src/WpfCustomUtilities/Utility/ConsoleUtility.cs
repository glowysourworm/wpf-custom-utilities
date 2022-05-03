using System;
using System.Collections.Generic;

namespace WpfCustomUtilities.Utility
{
    public static class ConsoleUtility
    {
        /// <summary>
        /// Searches parameter list for option key and tries to convert the option value to type T
        /// </summary>
        /// <returns>Returns the option value or default(T)</returns>
        public static T GetCommandLineOption<T>(List<string> parameters, string optionKey, T defaultValue)
        {
            try
            {
                var index = parameters.IndexOf(optionKey);

                if (index < parameters.Count - 1 && index >= 0)
                    return (T)Convert.ChangeType(parameters[index + 1], typeof(T));

                else
                    return defaultValue;
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Searches parameter list for option key
        /// </summary>
        public static bool HasCommandLineOption<T>(List<string> parameters, string optionKey)
        {
            var index = parameters.IndexOf(optionKey);

            if (index < parameters.Count &&
                index >= 0)
            {
                return !ReferenceEquals(GetCommandLineOption<T>(parameters, optionKey, default(T)), default(T));
            }

            else
                return false;
        }

        /// <summary>
        /// Searches parameter list for option key
        /// </summary>
        public static bool HasSingleCommandLineOption(List<string> parameters, string optionKey)
        {
            var index = parameters.IndexOf(optionKey);

            if (index < parameters.Count &&
                index >= 0)
            {
                return true;
            }

            else
                return false;
        }
    }
}
