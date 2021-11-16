using System;
using System.IO;
using Microsoft.Win32;

namespace GUI
{
    public static class IO
    {
        private const string Filter = "Text files | *.txt";
        private static string _currentPath;

        public static string CurrentFileName => _currentPath is not null ? Path.GetFileName(_currentPath) : "empty document";

        public static void Reset() => _currentPath = null;
        public static bool Open(out string contents)
        {
            var ofd = new OpenFileDialog()
            {
                Filter = Filter
            };

            var result = ofd.ShowDialog().GetValueOrDefault();
            contents = result ? File.ReadAllText(ofd.FileName) : null;
            _currentPath = result ? ofd.FileName : _currentPath;
            
            return contents is not null;
        }
        
        public static void Save(string content)
        {
            bool saved = SaveToCurrent(content);
            if (!saved)
            {
                SaveToNew(content, true);
            }
        }
        
        public static void SaveAs(string content) => SaveToNew(content, false);

        private static bool SaveToCurrent(string content)
        {
            if (_currentPath is null) 
                return false;
            
            File.WriteAllText(_currentPath, content);
            return true;
        }

        private static void SaveToNew(string content, bool rememberPath)
        {
            SaveFileDialog sfd = new()
            {
                Filter = Filter,
                FileName = DateTime.Now.ToFileTimeUtc().ToString(),
            };
            
            var result = sfd.ShowDialog().GetValueOrDefault();
            if (!result) 
                return;
            
            File.WriteAllText(sfd.FileName, content);
            
            if (rememberPath)
            {
                _currentPath = sfd.FileName;
            }
        }
    }
}