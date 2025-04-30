using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BaldaGame
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private const int Size = 5;
        private Button[,] _cells = new Button[Size, Size];
        private char[,] _board = new char[Size, Size];
        private bool _isPlayerOne = true;
        private (int Row, int Col)? _lastLetter = null;
        private Button selectedButton = null;

        private List<string> _dictionary = new List<string>();

        public event PropertyChangedEventHandler PropertyChanged;

        public int PlayerOneScore { get; private set; }
        public int PlayerTwoScore { get; private set; }
        public string CurrentPlayer => _isPlayerOne ? "Гравець 1" : "Гравець 2";

        private string _newLetter;
        public string NewLetter
        {
            get => _newLetter;
            set { _newLetter = value; OnPropertyChanged(nameof(NewLetter)); OnPropertyChanged(nameof(CanPlaceLetter)); }
        }

        public bool CanPlaceLetter => !string.IsNullOrWhiteSpace(NewLetter) && NewLetter.Length == 1 && selectedButton != null;

        public ICommand PlaceLetterCommand { get; }
        public ICommand ResetGameCommand { get; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            LoadDictionary("words.txt");

            PlaceLetterCommand = new RelayCommand(_ => PlaceLetter());
            ResetGameCommand = new RelayCommand(_ => StartNewGame());

            BuildBoard();
            StartNewGame();
        }

        private void LoadDictionary(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    _dictionary = File.ReadLines(path)
                        .Select(word => word.Trim().ToUpper())
                        .Where(word => word.Length >= 3)
                        .Distinct()
                        .ToList();
                }
                else
                {
                    MessageBox.Show("Словник не знайдено за шляхом: " + path);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при читанні словника: " + ex.Message);
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
                for (int c = 0; c < Size; c++)
                {
                    _cells[r, c].Content = _board[r, c] == '\0' ? "" : _board[r, c].ToString();
                    _cells[r, c].Background = Brushes.LightYellow;
                }
        }

        private void Cell_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                for (int r = 0; r < Size; r++)
                {
                    for (int c = 0; c < Size; c++)
                    {
                        if (_cells[r, c] == btn)
                        {
                            selectedButton = btn;
                            btn.Tag = (r, c);
                            OnPropertyChanged(nameof(CanPlaceLetter));
                            return;
                        }
                    }
                }
            }
        }

        private void PlaceLetter()
        {
            if (!CanPlaceLetter || selectedButton == null || selectedButton.Tag == null)
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
            RefreshBoard();
            NewLetter = "";

            TryScoreWord();

            _isPlayerOne = !_isPlayerOne;
            OnPropertyChanged(nameof(CurrentPlayer));
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
                        if (DFS(r, c, word, 0, visited, path, mustInclude)) return path;
                    }
            return null;
        }

        private bool DFS(int r, int c, string word, int index, HashSet<(int, int)> visited, List<(int, int)> path, (int, int) mustInclude)
        {
            if (index >= word.Length || r < 0 || r >= Size || c < 0 || c >= Size || _board[r, c] != word[index] || visited.Contains((r, c)))
                return false;

            visited.Add((r, c));
            path.Add((r, c));

            if (index == word.Length - 1)
                return path.Contains(mustInclude);

            var directions = new (int, int)[] { (1, 0), (-1, 0), (0, 1), (0, -1) };
            foreach (var (dr, dc) in directions)
                if (DFS(r + dr, c + dc, word, index + 1, visited, path, mustInclude))
                    return true;

            visited.Remove((r, c));
            path.RemoveAt(path.Count - 1);
            return false;
        }

        private void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;
        public event EventHandler CanExecuteChanged;
        public RelayCommand(Action<object> exec, Predicate<object> can = null)
        {
            _execute = exec; _canExecute = can;
        }
        public bool CanExecute(object p) => _canExecute?.Invoke(p) ?? true;
        public void Execute(object p) => _execute(p);
    }
}