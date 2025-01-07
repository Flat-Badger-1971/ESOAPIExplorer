using Microsoft.UI.Xaml;

namespace ESOAPIExplorer.Services
{
    public interface IThemeService
    {
        void Initialize(FrameworkElement element);
        void SetTheme(ElementTheme theme);
    }
}