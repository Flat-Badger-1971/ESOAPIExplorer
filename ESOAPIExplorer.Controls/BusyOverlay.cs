using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace ESOAPIExplorer.Controls;

public class BusyOverlay : UserControl
{
    private readonly Border _overlay;
    private readonly TextBlock _messageText;

    public BusyOverlay()
    {
        _messageText = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.WrapWholeWords,
            MaxWidth = 420,
            Foreground = Application.Current.Resources["ESOOverlayForegroundBrush"] as Brush,
            FontSize = 16
        };

        _overlay = new Border
        {
            Background = Application.Current.Resources["ESOOverlayBackgroundBrush"] as Brush,
            Padding = new Thickness(24),
            Visibility = Visibility.Collapsed,
            Child = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Spacing = 12,
                Children =
                {
                    new ProgressRing
                    {
                        IsActive = true,
                        Width = 56,
                        Height = 56,
                        HorizontalAlignment = HorizontalAlignment.Center
                    },
                    _messageText
                }
            }
        };

        Content = _overlay;
        UpdateState();
    }

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(
        nameof(IsActive),
        typeof(bool),
        typeof(BusyOverlay),
        new PropertyMetadata(false, OnStateChanged));

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
        nameof(Message),
        typeof(string),
        typeof(BusyOverlay),
        new PropertyMetadata(string.Empty, OnStateChanged));

    private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is BusyOverlay overlay)
        {
            overlay.UpdateState();
        }
    }

    private void UpdateState()
    {
        _overlay.Visibility = IsActive ? Visibility.Visible : Visibility.Collapsed;
        IsHitTestVisible = IsActive;
        _messageText.Text = string.IsNullOrWhiteSpace(Message) ? "Working..." : Message;
    }
}
