using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

// TODO: allow newlines at textbox (maybe rich textbox)
// TODO: add new document button
// TODO: add close current document button
// TODO: shelve unsaved documents on app close

namespace GUI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            _builder = new StringBuilder();
            MainTB.TextChanged += MainTBOnTextChanged;
            OpenBt.Click += OpenBtOnClick;
            SaveBt.Click += SaveBtOnClick;
            SaveAsBt.Click += SaveAsBtOnClick;
            KeyDown += OnKeyDown;
            IsSaved = true;
            UpdateTitle();
        }

        private readonly StringBuilder _builder;

        private bool _isSaved;
        private bool IsSaved
        {
            get => _isSaved;
            set
            {
                SaveBt.IsEnabled = !value;
                SaveBt.Content = FindResource(SaveBt.IsEnabled ? "SaveIcon" : "SaveIconDisabled");
                _isSaved = value;
                UpdateTitle();
            }
        }

        private void MainTBOnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not TextBox tb)
                throw new ArgumentException($"{nameof(sender)} is not {nameof(TextBox)}");

            IsSaved = false;
            foreach (var change in e.Changes)
            {
                if (change.AddedLength != 0)
                {
                    string added = tb.Text.Substring(change.Offset, change.AddedLength);
                    _builder.Insert(change.Offset, added);
                }
                else if (change.RemovedLength != 0)
                {
                    _builder.Remove(change.Offset, change.RemovedLength);
                }
            }
        }

        private void UpdateTitle()
        {
            Title = $"Notepad-- : {IO.CurrentFileName} {(IsSaved ? string.Empty : "(not saved)")}";
        }

        private void Save()
        {
            IO.Save(_builder.ToString());
            IsSaved = true;
        }

        private void Open()
        {
            if (!TryCloseCurrentDocument())
                return;

            if (IO.Open(out var content))
            {
                MainTB.Text = content;
                IsSaved = true;
                UpdateTitle();
            }
        }

        private void OpenBtOnClick(object sender, RoutedEventArgs e)
        {
            Open();
        }

        private bool TryCloseCurrentDocument()
        {
            void CloseCurrentDocument()
            {
                MainTB.Clear();
                _builder.Clear();

                IsSaved = false;
                IO.Reset();
            }
            
            if (IsSaved)
            {
                CloseCurrentDocument();
                return true;
            }

            var result = MessageBox.Show(
                "Save current document before closing?",
                "Save on close",
                MessageBoxButton.YesNoCancel);

            switch (result)
            {
                case MessageBoxResult.Cancel:
                    return false;
                case MessageBoxResult.Yes:
                    Save();
                    CloseCurrentDocument();
                    return true;
                case MessageBoxResult.No:
                    CloseCurrentDocument();
                    return true;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SaveBtOnClick(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void SaveAsBtOnClick(object sender, RoutedEventArgs e)
        {
            IO.SaveAs(_builder.ToString());
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                if (Keyboard.IsKeyDown(Key.S))
                {
                    if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                    {
                        IO.SaveAs(_builder.ToString());
                    }
                    else
                    {
                        Save();
                    }
                }
                else if (Keyboard.IsKeyDown(Key.O))
                {
                    Open();
                }

                e.Handled = true;
            }
        }
    }
}