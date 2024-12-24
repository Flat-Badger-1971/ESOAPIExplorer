using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;

namespace ESOAPIExplorer.Controls;

public sealed partial class StatusBar : UserControl
{
    public StatusBar()
    {
        // Create the Grid
        Grid grid = new Grid
        {
            Background = new SolidColorBrush(Colors.Black),
            Height = 30
        };

        Grid.SetRow(grid, 1);
        Grid.SetColumnSpan(grid, 2);

        // Create the StackPanel
        StackPanel stackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal
        };

        // Add TextBlocks to the StackPanel
        stackPanel.Children.Add(CreateTextBlock("API Version: ", "Status.APIVersion"));
        stackPanel.Children.Add(CreateTextBlock("Total Items: ", "Status.APIItems"));
        stackPanel.Children.Add(CreateTextBlock("C Functions: ", "Status.CFunctionItems"));
        stackPanel.Children.Add(CreateTextBlock("Functions: ", "Status.FunctionItems"));
        stackPanel.Children.Add(CreateTextBlock("Enum Types: ", "Status.EnumTypes"));
        stackPanel.Children.Add(CreateTextBlock("Enum Constants: ", "Status.EnumConstants"));
        stackPanel.Children.Add(CreateTextBlock("Events: ", "Status.EventItems"));
        stackPanel.Children.Add(CreateTextBlock("Constants: ", "Status.ConstantItems"));
        stackPanel.Children.Add(CreateTextBlock("Globals: ", "Status.GlobalItems"));

        // Add the StackPanel to the Grid
        grid.Children.Add(stackPanel);

        // Add the Grid to the UserControl
        Content = grid;
    }

    private static TextBlock CreateTextBlock(string labelText, string bindingPath)
    {
        var textBlock = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 0, 0),
            FontSize = 10
        };

        var runLabel = new Run
        {
            Text = labelText,
            Foreground = (Brush)Application.Current.Resources["SystemColorActiveCaptionColor"]
        };

        var runValue = new Run();
        runValue.SetBinding(Run.TextProperty, new Binding
        {
            Path = new PropertyPath(bindingPath),
            Mode = BindingMode.OneWay
        });
        runValue.Foreground = (Brush)Application.Current.Resources["SystemAccentColorDark1"];

        textBlock.Inlines.Add(runLabel);
        textBlock.Inlines.Add(runValue);

        return textBlock;
    }
}
