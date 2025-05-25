using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data.Entity;       
using Microsoft.VisualBasic;      
using lab6;                    
using BaldaGame.Commands;      

namespace BaldaGame.Views
{
    public partial class PageHistory : Page
    {
        private List<GameHistory> _allHistory;

        public PageHistory()
        {
            InitializeComponent();

            CommandBindings.Add(new CommandBinding(DataCommands.Find, Find_Executed, Find_CanExecute));
            CommandBindings.Add(new CommandBinding(DataCommands.Replace, Replace_Executed, Replace_CanExecute));
            CommandBindings.Add(new CommandBinding(DataCommands.Delete, Delete_Executed, Delete_CanExecute));

            LoadHistory();
        }

        private void LoadHistory()
        {
            try
            {
                using (var ctx = new WordsEntities())
                {
                    _allHistory = ctx.GameHistory
                                     .OrderByDescending(g => g.PlayedAt)
                                     .ToList();
                    HistoryGrid.ItemsSource = _allHistory;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при завантаженні історії: " + ex.Message);
            }
        }

        #region Find

        private void Find_CanExecute(object sender, CanExecuteRoutedEventArgs e)
            => e.CanExecute = true;

        private void Find_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string criteria = Interaction.InputBox(
                "Введіть фільтр (дата або ім’я переможця):",
                "Пошук у історії",
                "");

            if (string.IsNullOrWhiteSpace(criteria))
            {
                HistoryGrid.ItemsSource = _allHistory;
            }
            else
            {
                var filtered = _allHistory
                    .Where(h => h.Winner.IndexOf(criteria, StringComparison.OrdinalIgnoreCase) >= 0
                             || h.PlayedAt.ToString().IndexOf(criteria, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();
                HistoryGrid.ItemsSource = filtered;
            }
        }

        #endregion

        #region Replace

        private void Replace_CanExecute(object sender, CanExecuteRoutedEventArgs e)
            => e.CanExecute = HistoryGrid.SelectedItem != null;

        private void Replace_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var selected = HistoryGrid.SelectedItem as GameHistory;
            if (selected == null) return;

            string newWinner = Interaction.InputBox(
                "Введіть нового переможця:",
                "Редагування запису",
                selected.Winner);

            if (string.IsNullOrWhiteSpace(newWinner) || newWinner == selected.Winner)
                return;

            try
            {
                using (var ctx = new WordsEntities())
                {
                    ctx.GameHistory.Attach(selected);
                    selected.Winner = newWinner;
                    ctx.Entry(selected).State = EntityState.Modified;
                    ctx.SaveChanges();
                }
                LoadHistory();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка редагування: " + ex.Message);
            }
        }

        #endregion

        #region Delete

        private void Delete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
            => e.CanExecute = HistoryGrid.SelectedItem != null;

        private void Delete_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var selected = HistoryGrid.SelectedItem as GameHistory;
            if (selected == null) return;

            var result = MessageBox.Show(
                "Ви дійсно хочете видалити цей запис?",
                "Підтвердження видалення",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                using (var ctx = new WordsEntities())
                {
                    ctx.GameHistory.Attach(selected);
                    ctx.GameHistory.Remove(selected);
                    ctx.SaveChanges();
                }
                LoadHistory();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка видалення: " + ex.Message);
            }
        }

        #endregion
    }
}
