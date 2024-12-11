using ESOAPIExplorer.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WinRT.Interop;
using WinRT;

namespace ESPAPIExplorer.Controls
{
    public sealed class BindableHighlighter : DependencyObject
    {

        public static readonly DependencyProperty BindableRangesProperty =
        DependencyProperty.Register(nameof(BindableRanges), typeof(Dictionary<int,int>), typeof(BindableHighlighter), null);


        public Dictionary<int, int> BindableRanges
        {
            get
            {
                return (Dictionary<int, int>)GetValue(BindableRangesProperty);
            }
            set
            {
                SetValue(BindableRangesProperty, value);
                if (value != null)
                {
                    value.Select(r => new TextRange(r.Key, r.Value));
                }
                else
                {
                    SetValue(BindableRangesProperty, new List<TextRange>());
                }
            }
        }

        public static readonly DependencyProperty TextRangesProperty =
        DependencyProperty.Register(nameof(TextRanges), typeof(List<TextRange>), typeof(BindableHighlighter), null);


        public List<TextRange> TextRanges
        {
            get
            {
                return (List<TextRange>)GetValue(TextRangesProperty);
            }
            private set
            {
                SetValue(TextRangesProperty, value);
            }
        }
    }
}
