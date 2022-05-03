using System.IO;
using System.Text.RegularExpressions;

namespace WpfCustomUtilities.Utility
{
    public static class TextUtility
    {
        public static string CamelCaseToTitleCase(string str)
        {
            return Regex.Replace(str, "(\\B[A-Z])", " $1");
        }

        public static bool ValidateFileName(string str)
        {
            return str.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
        }
    }
}
