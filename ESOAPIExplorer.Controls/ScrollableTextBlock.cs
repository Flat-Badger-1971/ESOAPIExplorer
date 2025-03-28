using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ESOAPIExplorer.Controls;

public partial class ScrollableTextBlock : UserControl
{
    private Grid _RootGrid;
    private ScrollViewer _ScrollViewer;
    private TextBlock _TextBlock;

    public ScrollableTextBlock()
    {
        DefaultStyleKey = typeof(ScrollableTextBlock);
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        InitialiseComponents();
        UpdateTextBlockPadding();

        _ScrollViewer.ScrollChanged += (s, args) => UpdateTextBlockPadding();
        _TextBlock.SizeChanged += (s, args) => UpdateTextBlockPadding();
    }

    private void InitialiseComponents()
    {
        _RootGrid = new Grid
        {
            Background = new SolidColorBrush(Colors.Black),
            Margin = new Thickness(10, 0, 10, 0),
            // Padding = new Thickness(10)
        };

        _ScrollViewer = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            MaxHeight = 200
        };

        _TextBlock = new TextBlock
        {
            TextWrapping = TextWrapping.NoWrap
        };

        _ScrollViewer.Content = _TextBlock;
        _RootGrid.Children.Add(_ScrollViewer);

        Content = _RootGrid;
    }

    private void UpdateTextBlockPadding()
    {
        bool horizontalVisible = _ScrollViewer.ComputedHorizontalScrollBarVisibility == Visibility.Visible;
        bool verticalVisible = _ScrollViewer.ComputedVerticalScrollBarVisibility == Visibility.Visible;

        if (horizontalVisible || verticalVisible)
        {
            _TextBlock.Padding = new Thickness(0, 0, 0, horizontalVisible ? 20 : 0);
        }
        else
        {
            _TextBlock.Padding = new Thickness(0);
        }
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(ScrollableTextBlock), new PropertyMetadata(string.Empty, OnTextChanged));

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ScrollableTextBlock control && control._TextBlock != null)
        {
            control._TextBlock.Text = e.NewValue as string;
            Thread.Sleep(100);
            control.UpdateTextBlockPadding();
        }
    }
}
