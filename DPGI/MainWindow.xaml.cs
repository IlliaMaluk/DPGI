using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace DPGI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var openBinding = new CommandBinding(ApplicationCommands.Open, Execute_Open, CanExecute_Open);
            var closeBinding = new CommandBinding(ApplicationCommands.Close, Execute_Close, CanExecute_Close);
            var saveBinding = new CommandBinding(ApplicationCommands.Save, Execute_Save, CanExecute_Save);

            CommandBindings.Add(openBinding);
            CommandBindings.Add(closeBinding);
            CommandBindings.Add(saveBinding);
        }

        private void CanExecute_Save(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = MainTextBox.Text.Trim().Length > 0;
        }

        private void Execute_Save(object sender, ExecutedRoutedEventArgs e)
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string filePath = System.IO.Path.Combine(desktopPath, "DPGI.txt");

            File.WriteAllText(filePath, MainTextBox.Text);
            MessageBox.Show("File saved!");
        }
        private void CanExecute_Open(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = string.IsNullOrWhiteSpace(MainTextBox.Text);
        }

        private void Execute_Open(object sender, ExecutedRoutedEventArgs e)
        {
            const string filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

            var openDialog = new OpenFileDialog()
            {
                InitialDirectory = System.AppDomain.CurrentDomain.BaseDirectory,
                Filter = filter
            };

            if (openDialog.ShowDialog() == true)
            {
                MainTextBox.Text = File.ReadAllText(openDialog.FileName);
            }
        }

        private void CanExecute_Close(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = MainTextBox.Text.Length > 0;
        }

        private void Execute_Close(object sender, ExecutedRoutedEventArgs e)
        {
            MainTextBox.Text = "";
        }
    }
}
