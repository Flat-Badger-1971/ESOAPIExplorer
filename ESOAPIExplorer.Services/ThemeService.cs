using ESOAPIExplorer.Models;
using System.Windows;

namespace ESOAPIExplorer.Services;

public class ThemeService : IThemeService
{
    private FrameworkElement _element;

    public void Initialize(FrameworkElement element)
    {
        _element = element;
    }

    public void SetTheme(ElementTheme theme)
    {
        if (_element != null)
        {
            // _element.RequestedTheme = theme;
        }
    }
}
