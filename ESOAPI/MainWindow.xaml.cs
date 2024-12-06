using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ESOAPI.Models;

namespace ESOAPI
{
    public sealed partial class MainWindow : Window
    {
        private MainViewModel ViewModel { get; }

        public MainWindow()
        {
            ViewModel = new MainViewModel();
            this.InitializeComponent();

            RootGrid.DataContext = ViewModel;
        }

        private async void RootGrid_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.InitialiseAsync();
            EventsRadioButton.IsChecked = true;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (ListBox == null)
            {
                return;
            }

            RadioButton radioButton = sender as RadioButton;
            if (radioButton != null)
            {
                switch (radioButton.Content.ToString())
                {
                    case "Events":
                        ListBox.ItemsSource = ViewModel.Events;
                        break;
                    case "Functions":
                        ListBox.ItemsSource = ViewModel.Functions;
                        break;
                    case "Constants":
                        ListBox.ItemsSource = ViewModel.Constants;
                        break;
                }
            }
        }
    }
}
