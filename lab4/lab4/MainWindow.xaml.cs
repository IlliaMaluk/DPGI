using System.Data;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml.Linq;
using System;

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
    private void Create_Click(object sender, RoutedEventArgs e)
    {
        db.Insert(int.Parse(tbNum.Text), tbName.Text, tbGroup.Text, tbAddress.Text);
        list.ItemsSource = db.TableLoad().DefaultView;
    }

    private void Update_Click(object sender, RoutedEventArgs e)
    {
        if (list.SelectedItem is DataRowView row)
        {
            int id = (int)row["RecordBookNumber"];
            db.Update(id, tbName.Text, tbGroup.Text, tbAddress.Text);
            list.ItemsSource = db.TableLoad().DefaultView;
        }
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (list.SelectedItem is DataRowView row)
        {
            int id = (int)row["RecordBookNumber"];
            db.Delete(id);
            list.ItemsSource = db.TableLoad().DefaultView;
        }
    }
}
