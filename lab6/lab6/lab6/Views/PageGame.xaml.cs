using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using lab6;                    
using BaldaGame.Commands;    

namespace BaldaGame.Views
{
    public partial class PageGame : Page, INotifyPropertyChanged
    {
        private const int Size = 5;
        private readonly Button[,] _cells = new Button[Size, Size];
        private readonly char[,] _board = new char[Size, Size];
        private bool _isPlayerOne = true;
        private (int Row, int Col)? _lastLetter = null;
        private Button selectedButton = null;
        private List<string> _dictionary = new List<string>();
        private bool isDirty = false;

        public event PropertyChangedEventHandler PropertyChanged;

        public int PlayerOneScore { get; private set; }
        public int PlayerTwoScore { get; private set; }
        public string CurrentPlayer => _isPlayerOne ? "Гравець 1" : "Гравець 2";

        private string _winnerText;
        public string WinnerText
        {
            get => _winnerText;
            set { _winnerText = value; OnPropertyChanged(nameof(WinnerText)); }
        }

        private string _newLetter;
        public string NewLetter
        {
            get => _newLetter;
            set
            {
                _newLetter = value;
                OnPropertyChanged(nameof(NewLetter));
                OnPropertyChanged(nameof(CanPlaceLetter));
            }
        }

        public bool CanPlaceLetter =>
            !string.IsNullOrWhiteSpace(NewLetter) &&
            NewLetter.Length == 1 &&
            selectedButton != null;

        public ICommand PlaceLetterCommand { get; }
        public ICommand ResetGameCommand { get; }

        public PageGame()
        {
            InitializeComponent();
            DataContext = this;

            PlaceLetterCommand = new RelayCommand(_ => PlaceLetter());
            ResetGameCommand = new RelayCommand(_ => StartNewGame());

            CommandBindings.Add(new CommandBinding(DataCommands.Undo, Undo_Executed, Undo_CanExecute));
            CommandBindings.Add(new CommandBinding(DataCommands.New, New_Executed, New_CanExecute));
            CommandBindings.Add(new CommandBinding(DataCommands.Save, Save_Executed, Save_CanExecute));

            LoadDictionaryFromDatabase();
            BuildBoard();
            StartNewGame();
        }

        private void LoadDictionaryFromDatabase()
        {
            try
            {
                using (var context = new WordsEntities())
                {
                    _dictionary = context.Words
                        .Select(w => w.Word.Trim().ToUpper())
                        .Where(w => w.Length >= 3)
                        .Distinct()
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при завантаженні слів з бази: " + ex.Message);
            }
        }

        private void BuildBoard()
        {
            BoardGrid.Children.Clear();
            for (int r = 0; r < Size; r++)
            {
                for (int c = 0; c < Size; c++)
                {
                    var btn = new Button { FontSize = 24, Background = Brushes.LightYellow };
                    btn.Click += Cell_Click;
                    _cells[r, c] = btn;
                    BoardGrid.Children.Add(btn);
                }
            }
        }

        private void StartNewGame()
        {
            Array.Clear(_board, 0, _board.Length);
            PlayerOneScore = PlayerTwoScore = 0;
            _isPlayerOne = true;
            _lastLetter = null;
            selectedButton = null;
            isDirty = false;
            WinnerText = "";

            // Початкове слово "CAT" по центру
            string start = "CAT";
            int mid = Size / 2;
            for (int i = 0; i < start.Length; i++)
                _board[mid, mid - 1 + i] = start[i];

            RefreshBoard();
            NewLetter = "";
            OnPropertyChanged(nameof(PlayerOneScore));
            OnPropertyChanged(nameof(PlayerTwoScore));
            OnPropertyChanged(nameof(CurrentPlayer));
        }

        private void RefreshBoard()
        {
            for (int r = 0; r < Size; r++)
            {
                for (int c = 0; c < Size; c++)
                {
                    _cells[r, c].Content = _board[r, c] == '\0'
                        ? ""
                        : _board[r, c].ToString();
                    _cells[r, c].Background = Brushes.LightYellow;
                }
            }
        }

        private void Cell_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                for (int r = 0; r < Size; r++)
                    for (int c = 0; c < Size; c++)
                        if (_cells[r, c] == btn)
                        {
                            selectedButton = btn;
                            btn.Tag = (r, c);
                            OnPropertyChanged(nameof(CanPlaceLetter));
                            return;
                        }
            }
        }

        private void PlaceLetter()
        {
            if (!CanPlaceLetter || selectedButton?.Tag == null)
                return;

            var (r, c) = ((int, int))selectedButton.Tag;
            if (_board[r, c] != '\0')
            {
                MessageBox.Show("Ця клітинка вже зайнята.");
                return;
            }

            bool hasNeighbor = false;
            for (int dr = -1; dr <= 1; dr++)
                for (int dc = -1; dc <= 1; dc++)
                {
                    int nr = r + dr, nc = c + dc;
                    if (nr >= 0 && nr < Size && nc >= 0 && nc < Size && _board[nr, nc] != '\0')
                        hasNeighbor = true;
                }
            if (!hasNeighbor)
            {
                MessageBox.Show("Літера має прилягати до існуючих.");
                return;
            }

            _board[r, c] = char.ToUpper(NewLetter[0]);
            _lastLetter = (r, c);
            selectedButton = null;
            isDirty = true;
            RefreshBoard();
            NewLetter = "";

            TryScoreWord();

            if (IsBoardFull())
            {
                string winner = PlayerOneScore > PlayerTwoScore ? "Гравець 1"
                                  : PlayerTwoScore > PlayerOneScore ? "Гравець 2"
                                  : "Нічия";

                WinnerText = $"Гру завершено. {winner} переміг!";
                MessageBox.Show($"Гру завершено. {winner} переміг!\nРахунок: {PlayerOneScore} : {PlayerTwoScore}");

                SaveGameToHistory(PlayerOneScore, PlayerTwoScore, winner);
            }
            else
            {
                _isPlayerOne = !_isPlayerOne;
                OnPropertyChanged(nameof(CurrentPlayer));
            }
        }

        private bool IsBoardFull()
        {
            for (int r = 0; r < Size; r++)
                for (int c = 0; c < Size; c++)
                    if (_board[r, c] == '\0') return false;
            return true;
        }

        private void TryScoreWord()
        {
            if (_lastLetter == null) return;
            var usedWords = new HashSet<string>();
            foreach (string word in _dictionary)
            {
                var path = FindWordPath(word, _lastLetter.Value);
                if (path != null && !usedWords.Contains(word))
                {
                    foreach (var (r, c) in path)
                        _cells[r, c].Background = Brushes.LightGreen;

                    if (_isPlayerOne) PlayerOneScore += word.Length;
                    else PlayerTwoScore += word.Length;

                    OnPropertyChanged(nameof(PlayerOneScore));
                    OnPropertyChanged(nameof(PlayerTwoScore));
                    return;
                }
            }
        }

        private List<(int, int)> FindWordPath(string word, (int Row, int Col) mustInclude)
        {
            for (int r = 0; r < Size; r++)
                for (int c = 0; c < Size; c++)
                    if (_board[r, c] == word[0])
                    {
                        var visited = new HashSet<(int, int)>();
                        var path = new List<(int, int)>();
                        if (DFS(r, c, word, 0, visited, path, mustInclude))
                            return path;
                    }

            return null;
        }

        private bool DFS(int r, int c, string word, int index,
                         HashSet<(int, int)> visited,
                         List<(int, int)> path,
                         (int, int) mustInclude)
        {
            if (index >= word.Length ||
                r < 0 || r >= Size || c < 0 || c >= Size ||
                _board[r, c] != word[index] ||
                visited.Contains((r, c)))
                return false;

            visited.Add((r, c));
            path.Add((r, c));

            if (index == word.Length - 1)
                return path.Contains(mustInclude);

            var dirs = new (int, int)[] { (1, 0), (-1, 0), (0, 1), (0, -1) };
            foreach (var (dr, dc) in dirs)
                if (DFS(r + dr, c + dc, word, index + 1, visited, path, mustInclude))
                    return true;

            visited.Remove((r, c));
            path.RemoveAt(path.Count - 1);
            return false;
        }

        private void SaveGameToHistory(int score1, int score2, string winner)
        {
            try
            {
                using (var context = new WordsEntities())
                {
                    var history = new GameHistory
                    {
                        PlayerOneScore = score1,
                        PlayerTwoScore = score2,
                        Winner = winner,
                        PlayedAt = DateTime.Now
                    };
                    context.GameHistory.Add(history);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при збереженні історії: " + ex.Message);
            }
        }

        #region CommandBindings

        private void Undo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
            => e.CanExecute = isDirty;

        private void Undo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (_lastLetter != null)
            {
                var (r, c) = _lastLetter.Value;
                _board[r, c] = '\0';
                RefreshBoard();
                isDirty = false;
            }
        }

        private void New_CanExecute(object sender, CanExecuteRoutedEventArgs e)
            => e.CanExecute = true;

        private void New_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            StartNewGame();
            isDirty = false;
        }

        private void Replace_CanExecute(object sender, CanExecuteRoutedEventArgs e)
            => e.CanExecute = false;

        private void Replace_Executed(object sender, ExecutedRoutedEventArgs e)
            => MessageBox.Show("Редагування не реалізовано");

        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
            => e.CanExecute = (PlayerOneScore + PlayerTwoScore) > 0;

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string winner = PlayerOneScore > PlayerTwoScore ? "Гравець 1"
                              : PlayerTwoScore > PlayerOneScore ? "Гравець 2"
                              : "Нічия";
            SaveGameToHistory(PlayerOneScore, PlayerTwoScore, winner);
            MessageBox.Show("Гру збережено в історію");
        }

        private void Find_CanExecute(object sender, CanExecuteRoutedEventArgs e)
            => e.CanExecute = false;

        private void Find_Executed(object sender, ExecutedRoutedEventArgs e)
            => MessageBox.Show("Пошук не реалізовано");

        private void Delete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
            => e.CanExecute = false;

        private void Delete_Executed(object sender, ExecutedRoutedEventArgs e)
            => MessageBox.Show("Видалення не реалізовано");

        #endregion

        private void OnPropertyChanged(string propName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;
        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
            => _canExecute?.Invoke(parameter) ?? true;

        public void Execute(object parameter)
            => _execute(parameter);
    }
}
