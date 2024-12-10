using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ESOAPIExplorer.Controls
{
    public partial class HtmlWebView : WebView2
    {
        static bool webviewcore;
        public HtmlWebView()
        {
            Initialise();
        }

        private async void Initialise()
        {
            if (!webviewcore)
            {
                await EnsureCoreWebView2Async();
                if (!string.IsNullOrEmpty(HtmlSource))
                {
                    NavigateToString(HtmlSource);
                }
            }
        }

        public static readonly DependencyProperty HtmlSourceProperty =
        DependencyProperty.Register(nameof(HtmlSource), typeof(string), typeof(HtmlWebView), null);


        public string HtmlSource
        {
            get
            {
                return (string)GetValue(HtmlSourceProperty);
            }
            set
            {
                SetValue(HtmlSourceProperty, value);
                NavigateToString(value);
            }
        }
    }
}
