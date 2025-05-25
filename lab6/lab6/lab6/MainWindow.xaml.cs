using System;
using System.Windows;
using System.Windows.Navigation;

namespace BaldaGame
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // Викликається з PageMain.xaml → Hyperlink_RequestNavigate
        public void NavigateTo(string relativeUri)
        {
            MainFrame?.Navigate(new Uri(relativeUri, UriKind.Relative));
        }
    }
}
