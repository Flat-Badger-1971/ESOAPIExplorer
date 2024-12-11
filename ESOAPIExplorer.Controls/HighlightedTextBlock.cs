using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using Microsoft.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.ComponentModel.DataAnnotations;
using Windows.UI.Text;

namespace ESOAPIExplorer.Controls
{
    public partial class HighlightedTextBlock : Grid
    {
        public HighlightedTextBlock()
        {
            this.DataContextChanged += (sender, e) => UpdateTextBlock();
        }

        public void UpdateTextBlock()
        {
            if (string.IsNullOrEmpty( Text))
            {
                return;
            }
            TextBlock textBlock = new TextBlock();
            textBlock.TextAlignment = TextAlignment;
            if (!string.IsNullOrEmpty(Filter))
            {
                for (int i = 0; i < Text.Length; i++)
                {
                    Run run = new Run();
                    bool isHighlighted = Filter.ToLower().Contains(Text[i].ToString().ToLower());
                    while (i < Text.Length && isHighlighted == Filter.ToLower().Contains(Text[i].ToString().ToLower()))
                    {
                        run.Text += Text[i].ToString();
                        if (isHighlighted)
                        {
                            run.Foreground = HighlightForeground;
                            run.FontWeight = HighlightFontWeight;
                        }
                        else
                        {
                            run.Foreground = Foreground;
                            run.FontWeight = FontWeight;
                        }
                        run.FontSize = FontSize;

                        if (i+1 < Text.Length && isHighlighted == Filter.ToLower().Contains(Text[i + 1].ToString().ToLower()))
                        {
                            i++;
                        } else
                        {
                            break;
                        }
                    }
                    textBlock.Inlines.Add(run);
                }
            }
            else
            {
                Run run = new Run();
                run.Text = Text;
                run.Foreground = Foreground;
                run.FontWeight = FontWeight;
                run.FontSize = FontSize;
                textBlock.Inlines.Add(run);
            }

            this.Children.Clear();
            this.Children.Add(textBlock);
        }


        public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(HighlightedTextBlock), new PropertyMetadata(string.Empty));


        public string Text
        {
            get
            {
                return (string)GetValue(TextProperty);
            }
            set
            {
                SetValue(TextProperty, value);
                UpdateTextBlock();
            }
        }


        public static readonly DependencyProperty FilterProperty =
        DependencyProperty.Register(nameof(Filter), typeof(string), typeof(HighlightedTextBlock), new PropertyMetadata(string.Empty));


        public string Filter
        {
            get
            {
                return (string)GetValue(FilterProperty);
            }
            set
            {
                SetValue(FilterProperty, value);
                UpdateTextBlock();
            }
        }

        public static readonly DependencyProperty HighlightForegroundProperty =
        DependencyProperty.Register(nameof(HighlightForeground), typeof(SolidColorBrush), typeof(HighlightedTextBlock), new PropertyMetadata(new SolidColorBrush(Colors.Black)));

        public SolidColorBrush HighlightForeground
        {
            get
            {
                return (SolidColorBrush)GetValue(HighlightForegroundProperty);
            }
            set
            {
                SetValue(HighlightForegroundProperty, value);
            }
        }

        public static readonly DependencyProperty ForegroundProperty =
        DependencyProperty.Register(nameof(Foreground), typeof(SolidColorBrush), typeof(HighlightedTextBlock), new PropertyMetadata(new SolidColorBrush(Colors.Black)));

        public SolidColorBrush Foreground
        {
            get
            {
                return (SolidColorBrush)GetValue(ForegroundProperty);
            }
            set
            {
                SetValue(ForegroundProperty, value);
            }
        }

        //public static readonly DependencyProperty MarginProperty =
        //DependencyProperty.Register(nameof(Margin), typeof(Thickness), typeof(HighlightedTextBlock), new PropertyMetadata(new Thickness(0)));

        //public Thickness Margin
        //{
        //    get
        //    {
        //        return (Thickness)GetValue(MarginProperty);
        //    }
        //    set
        //    {
        //        SetValue(MarginProperty, value);
        //    }
        //}

        public static readonly DependencyProperty FontSizeProperty =
        DependencyProperty.Register(nameof(FontSize), typeof(double), typeof(HighlightedTextBlock), new PropertyMetadata(12d));

        public double FontSize
        {
            get
            {
                return (double)GetValue(FontSizeProperty);
            }
            set
            {
                SetValue(FontSizeProperty, value);
            }
        }

        public static readonly DependencyProperty FontWeightProperty =
        DependencyProperty.Register(nameof(FontWeight), typeof(FontWeight), typeof(HighlightedTextBlock), new PropertyMetadata(new FontWeight(0)));

        public FontWeight FontWeight
        {
            get
            {
                return (FontWeight)GetValue(FontWeightProperty);
            }
            set
            {
                SetValue(FontWeightProperty, value);
            }
        }

        public static readonly DependencyProperty HighlightFontWeightProperty =
        DependencyProperty.Register(nameof(HighlightFontWeight), typeof(FontWeight), typeof(HighlightedTextBlock), new PropertyMetadata(new FontWeight(0)));

        public FontWeight HighlightFontWeight
        {
            get
            {
                return (FontWeight)GetValue(HighlightFontWeightProperty);
            }
            set
            {
                SetValue(HighlightFontWeightProperty, value);
            }
        }

        public static readonly DependencyProperty TextAlignmentProperty =
        DependencyProperty.Register(nameof(TextAlignment), typeof(TextAlignment), typeof(HighlightedTextBlock), new PropertyMetadata(TextAlignment.Left));

        public TextAlignment TextAlignment
        {
            get
            {
                return (TextAlignment)GetValue(TextAlignmentProperty);
            }
            set
            {
                SetValue(TextAlignmentProperty, value);
            }
        }

    }

}

