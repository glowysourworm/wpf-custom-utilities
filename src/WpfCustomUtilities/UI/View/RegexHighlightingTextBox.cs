using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using WpfCustomUtilities.SimpleCollections.Collection;

namespace WpfCustomUtilities.UI.View
{
    public class RegexHighlightingTextBox : TextBox
    {
        // Set of tokens to search for in the text
        SimpleDictionary<RegexHighlightingToken, RegexHighlightingToken> _tokens;

        // To be rendered to highlight text
        List<Rect> _adorners;

        public RegexHighlightingTextBox()
        {
            _tokens = new SimpleDictionary<RegexHighlightingToken, RegexHighlightingToken>();
            _adorners = new List<Rect>();

            this.Background = Brushes.Transparent;
        }

        public void AddToken(string searchString, Brush foreground, Brush background)
        {
            var token = new RegexHighlightingToken(searchString, foreground, background);

            _tokens.Add(token, token);
        }

        protected virtual void HighlightText()
        {
            _adorners.Clear();

            foreach (var token in _tokens.Keys)
            {
                var matches = Regex.Matches(this.Text, token.SearchPattern);

                foreach (var match in matches)
                {
                    var startIndex = ((Match)match).Index;
                    var length = ((Match)match).Length;
                    var highlightRect = new Rect();

                    for (int index = startIndex; index < startIndex + length; index++)
                    {
                        var charBounds = this.GetRectFromCharacterIndex(index, index != startIndex);

                        if (index == startIndex)
                        {
                            highlightRect.X = charBounds.X;
                            highlightRect.Y = charBounds.Y;
                        }

                        // Update Width / Height
                        highlightRect.Width = charBounds.X - highlightRect.X;
                        highlightRect.Height = Math.Max(charBounds.Height, highlightRect.Height);
                    }

                    _adorners.Add(highlightRect);
                }
            }

            InvalidateVisual();
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);

            HighlightText();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            foreach (var rect in _adorners)
            {
                drawingContext.PushOpacity(0.5);
                drawingContext.DrawRectangle(Brushes.LightBlue, null, rect);
                drawingContext.Pop();
            }
        }
    }
}
