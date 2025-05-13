using System.Data;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml.Linq;
using System;
using System.Windows.Controls;

namespace lab4
{
    public partial class MainWindow : Window
    {
        AdoAssistant db = new AdoAssistant();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            list.ItemsSource = db.TableLoad().DefaultView;
        }

        private void List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (list.SelectedItem is DataRowView row)
            {
                tbNum.Text = row["RecordBookNumber"].ToString();
                tbName.Text = row["FullName"].ToString();
                tbGroup.Text = row["GroupName"].ToString();
                tbAddress.Text = row["Address"].ToString();
            }
            else
            {
                ClearInputs();
            }
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateNumber(tbNum.Text, out int number)) return;
            if (!ValidateNonEmpty(tbName.Text, "ПІБ студента", tbName)) return;

            try
            {
                db.Insert(number, tbName.Text, tbGroup.Text, tbAddress.Text);
            }
            catch (Exception ex)
            {
                ShowError("Не вдалося додати запис до бази", ex.Message);
                return;
            }

            RefreshList();
            ShowInfo("Запис додано");
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            if (!(list.SelectedItem is DataRowView row))
            {
                ShowWarning("Оберіть запис для оновлення");
                return;
            }

            int id = (int)row["RecordBookNumber"];
            if (!ValidateNonEmpty(tbName.Text, "ПІБ студента", tbName)) return;

            try
            {
                db.Update(id, tbName.Text, tbGroup.Text, tbAddress.Text);
            }
            catch (Exception ex)
            {
                ShowError("Не вдалося оновити запис", ex.Message);
                return;
            }

            RefreshList();
            ShowInfo("Запис оновлено");
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (!(list.SelectedItem is DataRowView row))
            {
                ShowWarning("Оберіть запис для видалення");
                return;
            }

            var result = MessageBox.Show(
                "Ви впевнені, що хочете видалити вибраний запис?",
                "Підтвердження",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            int id = (int)row["RecordBookNumber"];
            try
            {
                db.Delete(id);
            }
            catch (Exception ex)
            {
                ShowError("Не вдалося видалити запис", ex.Message);
                return;
            }

            RefreshList();
            ShowInfo("Запис видалено");
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            list.SelectedIndex = -1;
            ClearInputs();
        }

        // Вспомогательные методы
        private bool ValidateNumber(string text, out int number)
        {
            if (!int.TryParse(text, out number))
            {
                ShowWarning("Поле «Номер залікової книги» повинно містити тільки цифри!");
                tbNum.Focus();
                return false;
            }
            return true;
        }

        private bool ValidateNonEmpty(string text, string fieldName, Control control)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                ShowWarning($"Введіть, будь ласка, {fieldName}.");
                control.Focus();
                return false;
            }
            return true;
        }

        private void RefreshList()
        {
            db.ResetDataTable();
            list.ItemsSource = db.TableLoad().DefaultView;
            list.SelectedIndex = -1;
            ClearInputs();
        }

        private void ClearInputs()
        {
            tbNum.Clear();
            tbName.Clear();
            tbGroup.Clear();
            tbAddress.Clear();
            tbNum.Focus();
        }

        private void ShowWarning(string message)
            => MessageBox.Show(message, "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);

        private void ShowError(string title, string details)
            => MessageBox.Show(details, title, MessageBoxButton.OK, MessageBoxImage.Error);

        private void ShowInfo(string message)
            => MessageBox.Show(message, "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}