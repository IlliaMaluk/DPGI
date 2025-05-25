using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace BaldaGame.Views
{
    public partial class PageMain : Page
    {
        public PageMain()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mw)
            {
                mw.NavigateTo(e.Uri.ToString());
            }
            e.Handled = true;
        }
    }
}
