using System.Windows;
using System.Linq;
using System.Data.Entity;

namespace lab5_fixed
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            using (var context = new DBTestEntities())
            {
                dgGroups.ItemsSource = context.Group.ToList();

                dgStudents.ItemsSource = context.Students
                    .Include(s => s.Group)
                    .Select(s => new
                    {
                        s.StudentBookNumber,
                        GroupName = s.Group.GroupName,
                        s.Address
                    })
                    .ToList();
                var query = context.Students
                    .Include(s => s.Group)
                    .Where(s => s.Group.GroupName == "ПІ-19")
                    .Select(s => new { s.StudentBookNumber, s.Address })
                    .ToList();

                dgQuery.ItemsSource = query;
            }
            
            }
        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(tbSearchBookNumber.Text, out int bookNumber))
            {
                MessageBox.Show("Введіть, будь ласка, коректний номер залікової книжки.",
                                "Помилка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            using (var context = new DBTestEntities())
            {
                var results = context.Students
                    .Include(s => s.Group)
                    .Where(s => s.StudentBookNumber == bookNumber)
                    .Select(s => new
                    {
                        s.StudentBookNumber,
                        GroupName = s.Group.GroupName,
                        s.Address
                    })
                    .ToList();

                dgSearch.ItemsSource = results;
            }
        }
    }
}
