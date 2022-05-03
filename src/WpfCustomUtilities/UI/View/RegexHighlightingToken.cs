using System.Windows.Media;

namespace WpfCustomUtilities.UI.View
{
    public class RegexHighlightingToken
    {
        /// <summary>
        /// Either a wild card or string that is used to search the text to highlight.
        /// </summary>
        public string SearchPattern { get; private set; }

        public Brush HighlightForeground { get; private set; }
        public Brush HighlightBackground { get; private set; }

        public RegexHighlightingToken(string token, Brush foreground, Brush background)
        {
            this.SearchPattern = token;
            this.HighlightForeground = foreground;
            this.HighlightBackground = background;
        }

        public override int GetHashCode()
        {
            return this.SearchPattern.GetHashCode();
        }
    }
}
