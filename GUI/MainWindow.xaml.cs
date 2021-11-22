using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GUI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            _builder = new StringBuilder();
            MainTB.TextChanged += MainTBOnTextChanged;
            NewDocBt.Click += NewDocBtOnClick;
            OpenBt.Click += OpenBtOnClick;
            SaveBt.Click += SaveBtOnClick;
            SaveAsBt.Click += SaveAsBtOnClick;
            KeyDown += OnKeyDown;
            Closing += OnClosing;

            _shelfer = new Shelfer(ProgramName);
            if (_shelfer.IsEmpty())
            {
                Reset();
            }
            else
            {
                var (filePath, content) = _shelfer.Take();
                if (!string.IsNullOrWhiteSpace(filePath)) 
                    IO.Open(filePath);

                MainTB.Text = content;
                IsSaved = false;
            }
        }

        private const string ProgramName = "Notepad--";
 
        private readonly StringBuilder _builder;

        private bool _isSaved;

        private readonly Shelfer _shelfer;
        
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

        private void Reset()
        {
            MainTB.Clear();
            _builder.Clear();
            IO.Reset();
            IsSaved = true;
            UpdateTitle();
        }

        private void UpdateTitle()
        {
            Title = $"{ProgramName} : {IO.CurrentFileName} {(IsSaved ? string.Empty : "(not saved)")}";
        }

        private void Open()
        {
            if (!CloseAndCreateNew())
                return;

            if (IO.TryOpen(out var content))
            {
                MainTB.Text = content;
                IsSaved = true;
                UpdateTitle();
            }
        }

        private void Save()
        {
            IO.Save(_builder.ToString());
            IsSaved = true;
        }

        private bool CloseAndCreateNew()
        {
            if (IsSaved)
            {
                Reset();
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
                    Reset();
                    return true;
                case MessageBoxResult.No:
                    Reset();
                    return true;
                default:
                    throw new ArgumentOutOfRangeException();
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

        private void NewDocBtOnClick(object sender, RoutedEventArgs e) => CloseAndCreateNew();

        private void OpenBtOnClick(object sender, RoutedEventArgs e) => Open();

        private void SaveBtOnClick(object sender, RoutedEventArgs e) => Save();

        private void SaveAsBtOnClick(object sender, RoutedEventArgs e) => IO.SaveAs(_builder.ToString());

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
                else if (Keyboard.IsKeyDown(Key.N))
                {
                    CloseAndCreateNew();
                }

                e.Handled = true;
            }
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            // shelve
            if (!_isSaved)
            {
                _shelfer.Put(IO.CurrentFilePath, _builder.ToString());
            }
            else
            {
                if (!_shelfer.IsEmpty())
                    _ = _shelfer.Take(); // discard shelf
            }
        }
    }
}