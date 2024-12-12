using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Text;

namespace ESOAPIExplorer.Controls;

public partial class HighlightedTextBlock : Grid
{
    private List<char> _charactersToMatch = null;

    public HighlightedTextBlock()
    {
        DataContextChanged += (sender, e) => UpdateTextBlock();
    }

    public void UpdateTextBlock()
    {
        if (string.IsNullOrWhiteSpace(Text))
        {
            return;
        }

        TextBlock textBlock = new TextBlock
        {
            TextAlignment = TextAlignment
        };

        if (!string.IsNullOrWhiteSpace(Filter))
        {
            for (int i = 0; i < Text.Length; i++)
            {
                Run run = new Run();

                while (i < Text.Length)
                {
                    bool isHighlighted = IsMatched(Filter, Text[i]);
                    run.Text += Text[i].ToString();

                    if (isHighlighted)
                    {
                        run.Foreground = HighlightForeground;
                        run.FontWeight = HighlightFontWeight;
                    }
                    else
                    {
                        run.Foreground = Foreground;
                        run.FontWeight = FontWeight;
                    }

                    run.FontSize = FontSize;

                    if (i + 1 < Text.Length && isHighlighted == IsMatched(Filter, Text[i + 1], true))
                    {
                        i++;
                    }
                    else
                    {
                        break;
                    }
                }

                textBlock.Inlines.Add(run);
            }
        }
        else
        {
            Run run = new Run
            {
                Text = Text,
                Foreground = Foreground,
                FontWeight = FontWeight,
                FontSize = FontSize
            }; 

            textBlock.FontSize = FontSize;
            textBlock.Inlines.Add(run);
        }

        Children.Clear();
        Children.Add(textBlock);
    }

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(HighlightedTextBlock), new PropertyMetadata(string.Empty));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set
        {
            SetValue(TextProperty, value);
            UpdateTextBlock();
        }
    }

    public static readonly DependencyProperty FilterProperty =
    DependencyProperty.Register(nameof(Filter), typeof(string), typeof(HighlightedTextBlock), new PropertyMetadata(string.Empty));

    public string Filter
    {
        get => (string)GetValue(FilterProperty);
        set
        {
            SetValue(FilterProperty, value);
            UpdateTextBlock();
        }
    }

    public static readonly DependencyProperty HighlightForegroundProperty =
    DependencyProperty.Register(nameof(HighlightForeground), typeof(SolidColorBrush), typeof(HighlightedTextBlock), new PropertyMetadata(new SolidColorBrush(Colors.Black)));

    public SolidColorBrush HighlightForeground
    {
        get => (SolidColorBrush)GetValue(HighlightForegroundProperty);
        set => SetValue(HighlightForegroundProperty, value);
    }

    public static readonly DependencyProperty ForegroundProperty =
    DependencyProperty.Register(nameof(Foreground), typeof(SolidColorBrush), typeof(HighlightedTextBlock), new PropertyMetadata(new SolidColorBrush(Colors.Black)));

    public SolidColorBrush Foreground
    {
        get => (SolidColorBrush)GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    public static readonly DependencyProperty FontSizeProperty =
    DependencyProperty.Register(nameof(FontSize), typeof(double), typeof(HighlightedTextBlock), new PropertyMetadata((double)11));

    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public static readonly DependencyProperty FontWeightProperty =
    DependencyProperty.Register(nameof(FontWeight), typeof(FontWeight), typeof(HighlightedTextBlock), new PropertyMetadata(default(FontWeight)));

    public FontWeight FontWeight
    {
        get => (FontWeight)GetValue(FontWeightProperty);
        set => SetValue(FontWeightProperty, value);
    }

    public static readonly DependencyProperty HighlightFontWeightProperty =
    DependencyProperty.Register(nameof(HighlightFontWeight), typeof(FontWeight), typeof(HighlightedTextBlock), new PropertyMetadata(default(FontWeight)));

    public FontWeight HighlightFontWeight
    {
        get => (FontWeight)GetValue(HighlightFontWeightProperty);
        set => SetValue(HighlightFontWeightProperty, value);
    }

    public static readonly DependencyProperty TextAlignmentProperty =
    DependencyProperty.Register(nameof(TextAlignment), typeof(TextAlignment), typeof(HighlightedTextBlock), new PropertyMetadata(TextAlignment.Left));

    public TextAlignment TextAlignment
    {
        get => (TextAlignment)GetValue(TextAlignmentProperty);
        set => SetValue(TextAlignmentProperty, value);
    }

    private bool IsMatched(string filter, char character, bool doNotRemove = false)
    {
        bool isMatched = false;
        char charToFind = char.ToLower(character);

        _charactersToMatch ??= filter.ToLower().ToList();

        if (_charactersToMatch.Contains(charToFind))
        {
            isMatched = true;

            if (!doNotRemove)
            {
                _charactersToMatch.Remove(charToFind);
            }
        }

        return isMatched;
    }
}

