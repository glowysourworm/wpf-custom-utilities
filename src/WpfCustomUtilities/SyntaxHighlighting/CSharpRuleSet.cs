using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WpfCustomUtilities.SyntaxHighlighting
{
    public static class CSharpRuleSet
    {
        public static IEnumerable<HighlightRule> GetRules()
        {
            return new List<HighlightRule>()
            {
                new HighlightRule("public", Colors.Blue),
                new HighlightRule("protected", Colors.Blue),
                new HighlightRule("private", Colors.Blue),
                new HighlightRule("virtual", Colors.Blue),
                new HighlightRule("override", Colors.Blue),
                new HighlightRule("static", Colors.Blue),
                new HighlightRule("new", Colors.Blue),
                new HighlightRule("return", Colors.Blue)
            };
        }
    }
}
