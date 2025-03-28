using ESOAPIExplorer.Models;
using System.Windows;

namespace ESOAPIExplorer.Services
{
    public interface IThemeService
    {
        void Initialize(FrameworkElement element);
        void SetTheme(ElementTheme theme);
    }
}