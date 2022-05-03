using System.Windows.Media;

namespace WpfCustomUtilities.SyntaxHighlighting
{
    public struct HighlightRule
    {
        public string SearchPattern { get; set; }
        public Color Color { get; set; }

        public HighlightRule(string searchPattern, Color color)
        {
            this.SearchPattern = searchPattern;
            this.Color = color;
        }
    }
}
