using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace ESOAPIExplorer.Controls;

public partial class ScrollableTextBlock : UserControl
{
    private Grid _rootGrid;
    private ScrollViewer _scrollViewer;
    private TextBlock _textBlock;

    public ScrollableTextBlock()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        _rootGrid = new Grid
        {
            Margin = new Thickness(10, 0, 10, 0)
        };

        _scrollViewer = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            MaxHeight = 200
        };

        _textBlock = new TextBlock
        {
            TextWrapping = TextWrapping.NoWrap,
            Foreground = Foreground
        };

        _scrollViewer.Content = _textBlock;
        _rootGrid.Children.Add(_scrollViewer);

        Content = _rootGrid;

        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateTextBlockPadding();

        _scrollViewer.ScrollChanged += (s, args) => UpdateTextBlockPadding();
        _textBlock.SizeChanged += (s, args) => UpdateTextBlockPadding();

        // Apply the current Text value if it was set before loading
        if (!string.IsNullOrEmpty(Text))
        {
            _textBlock.Text = Text;

            // Schedule update after layout is complete
            Dispatcher.BeginInvoke(DispatcherPriority.Render, UpdateTextBlockPadding);
        }
    }

    private void UpdateTextBlockPadding()
    {
        if (_scrollViewer == null || _textBlock == null)
        {
            return;
        }

        bool horizontalVisible = _scrollViewer.ComputedHorizontalScrollBarVisibility == Visibility.Visible;
        bool verticalVisible = _scrollViewer.ComputedVerticalScrollBarVisibility == Visibility.Visible;

        if (horizontalVisible || verticalVisible)
        {
            _textBlock.Padding = new Thickness(0, 0, 0, horizontalVisible ? 20 : 0);
        }
        else
        {
            _textBlock.Padding = new Thickness(0);
        }
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string),
        typeof(ScrollableTextBlock),
        new PropertyMetadata(string.Empty, OnTextChanged));

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ScrollableTextBlock control && control._textBlock != null)
        {
            control._textBlock.Text = e.NewValue as string;

            // Use BeginInvoke to update after layout rather than blocking with Thread.Sleep
            control.Dispatcher.BeginInvoke(DispatcherPriority.Render,
                new System.Action(control.UpdateTextBlockPadding));
        }
    }

    // Add this to inherit background correctly
    public new static readonly DependencyProperty BackgroundProperty =
        DependencyProperty.Register(nameof(Background), typeof(Brush),
        typeof(ScrollableTextBlock),
        new PropertyMetadata(null, OnBackgroundChanged));

    private static void OnBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ScrollableTextBlock control && control._rootGrid != null)
        {
            control._rootGrid.Background = e.NewValue as Brush;
        }
    }

    // Add this for TextBlock foreground
    public new static readonly DependencyProperty ForegroundProperty =
        DependencyProperty.Register(nameof(Foreground), typeof(Brush),
        typeof(ScrollableTextBlock),
        new PropertyMetadata(Brushes.White, OnForegroundChanged));

    private static void OnForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ScrollableTextBlock control && control._textBlock != null)
        {
            control._textBlock.Foreground = e.NewValue as Brush;
        }
    }
}
