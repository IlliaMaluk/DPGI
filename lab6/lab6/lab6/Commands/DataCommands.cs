using System.Windows.Input;

namespace BaldaGame.Commands
{
    public static class DataCommands
    {
        public static RoutedCommand Undo { get; }
        public static RoutedCommand New { get; }
        public static RoutedCommand Replace { get; }
        public static RoutedCommand Save { get; }
        public static RoutedCommand Find { get; }
        public static RoutedCommand Delete { get; }

        static DataCommands()
        {
            InputGestureCollection gestures;

            gestures = new InputGestureCollection { new KeyGesture(Key.Z, ModifierKeys.Control) };
            Undo = new RoutedCommand("Undo", typeof(DataCommands), gestures);

            gestures = new InputGestureCollection { new KeyGesture(Key.N, ModifierKeys.Control) };
            New = new RoutedCommand("New", typeof(DataCommands), gestures);

            gestures = new InputGestureCollection { new KeyGesture(Key.E, ModifierKeys.Control) };
            Replace = new RoutedCommand("Replace", typeof(DataCommands), gestures);

            gestures = new InputGestureCollection { new KeyGesture(Key.S, ModifierKeys.Control) };
            Save = new RoutedCommand("Save", typeof(DataCommands), gestures);

            gestures = new InputGestureCollection { new KeyGesture(Key.F, ModifierKeys.Control) };
            Find = new RoutedCommand("Find", typeof(DataCommands), gestures);

            gestures = new InputGestureCollection { new KeyGesture(Key.D, ModifierKeys.Control) };
            Delete = new RoutedCommand("Delete", typeof(DataCommands), gestures);
        }
    }
}
