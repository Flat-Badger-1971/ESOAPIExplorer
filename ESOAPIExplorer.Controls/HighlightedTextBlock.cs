using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace ESOAPIExplorer.Controls;

public partial class HighlightedTextBlock : UserControl
{
    private readonly TextBlock _innerTextBlock;
    private List<char> _charactersToMatch;

    public HighlightedTextBlock()
    {
        _innerTextBlock = new TextBlock();
        Content = _innerTextBlock;
        DataContextChanged += (sender, e) => UpdateTextBlock();
        Loaded += (sender, e) => UpdateTextBlock();
    }

    public void UpdateTextBlock()
    {
        if (string.IsNullOrWhiteSpace(Text))
        {
            return;
        }

        _innerTextBlock.TextAlignment = TextAlignment;
        _innerTextBlock.Inlines.Clear();

        if (!string.IsNullOrWhiteSpace(Filter))
        {
            // Initialize characters to match once for the entire text
            _charactersToMatch = Filter?.Select(f => char.ToLower(f))?.ToList() ?? [];

            for (int i = 0; i < Text.Length; i++)
            {
                Run run = new Run();

                while (i < Text.Length)
                {
                    bool isHighlighted = IsMatched(Text[i]);
                    run.Text += Text[i].ToString();

                    if (isHighlighted)
                    {
                        run.Foreground = HighlightForeground;
                        run.FontWeight = HighlightFontWeight;
                    }
                    else
                    {
                        run.Foreground = TextForeground;
                        run.FontWeight = FontWeight;
                    }

                    run.FontSize = FontSize;

                    if (i + 1 < Text.Length && isHighlighted == IsMatched(Text[i + 1], true))
                    {
                        i++;
                    }
                    else
                    {
                        break;
                    }
                }

                _innerTextBlock.Inlines.Add(run);
            }
        }
        else
        {
            Run run = new Run
            {
                Text = Text,
                Foreground = TextForeground,
                FontWeight = FontWeight,
                FontSize = FontSize
            };

            _innerTextBlock.FontSize = FontSize;
            _innerTextBlock.Inlines.Add(run);
        }
    }

    private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HighlightedTextBlock control)
        {
            control.UpdateTextBlock();
        }
    }

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(HighlightedTextBlock),
        new PropertyMetadata(string.Empty, OnPropertyChanged));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly DependencyProperty FilterProperty =
        DependencyProperty.Register(nameof(Filter), typeof(string), typeof(HighlightedTextBlock),
        new PropertyMetadata(string.Empty, OnPropertyChanged));

    public string Filter
    {
        get => (string)GetValue(FilterProperty);
        set => SetValue(FilterProperty, value);
    }

    public static readonly DependencyProperty HighlightForegroundProperty =
        DependencyProperty.Register(nameof(HighlightForeground), typeof(Brush),
        typeof(HighlightedTextBlock), new PropertyMetadata(Brushes.Yellow, OnPropertyChanged));

    public Brush HighlightForeground
    {
        get => (Brush)GetValue(HighlightForegroundProperty);
        set => SetValue(HighlightForegroundProperty, value);
    }

    public static readonly DependencyProperty TextForegroundProperty =
        DependencyProperty.Register(nameof(TextForeground), typeof(Brush),
        typeof(HighlightedTextBlock), new PropertyMetadata(Brushes.Black, OnPropertyChanged));

    public Brush TextForeground
    {
        get => (Brush)GetValue(TextForegroundProperty);
        set => SetValue(TextForegroundProperty, value);
    }

    public static readonly DependencyProperty FontSizeProperty =
        DependencyProperty.Register(nameof(FontSize), typeof(double),
        typeof(HighlightedTextBlock), new PropertyMetadata(12.0, OnPropertyChanged));

    public new double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public new static readonly DependencyProperty FontWeightProperty =
        DependencyProperty.Register(nameof(FontWeight), typeof(FontWeight),
        typeof(HighlightedTextBlock), new PropertyMetadata(FontWeights.Normal, OnPropertyChanged));

    public new FontWeight FontWeight
    {
        get => (FontWeight)GetValue(FontWeightProperty);
        set => SetValue(FontWeightProperty, value);
    }

    public static readonly DependencyProperty HighlightFontWeightProperty =
        DependencyProperty.Register(nameof(HighlightFontWeight), typeof(FontWeight),
        typeof(HighlightedTextBlock), new PropertyMetadata(FontWeights.Bold, OnPropertyChanged));

    public FontWeight HighlightFontWeight
    {
        get => (FontWeight)GetValue(HighlightFontWeightProperty);
        set => SetValue(HighlightFontWeightProperty, value);
    }

    public static readonly DependencyProperty TextAlignmentProperty =
        DependencyProperty.Register(nameof(TextAlignment), typeof(TextAlignment),
        typeof(HighlightedTextBlock), new PropertyMetadata(TextAlignment.Left, OnPropertyChanged));

    public TextAlignment TextAlignment
    {
        get => (TextAlignment)GetValue(TextAlignmentProperty);
        set => SetValue(TextAlignmentProperty, value);
    }

    private bool IsMatched(char character, bool doNotRemove = false)
    {
        if (_charactersToMatch == null || _charactersToMatch.Count == 0)
        {
            return false;
        }

        bool isMatched = false;
        char charToFind = char.ToLower(character);

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

