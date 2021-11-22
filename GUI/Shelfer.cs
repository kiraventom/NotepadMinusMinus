using System;
using System.IO;
using System.Reflection;

namespace GUI
{
    public record Shelf(string FilePath, string Content);
    
    // TODO: Rework shelf to JSON
    public class Shelfer
    {
        public Shelfer(string programName)
        {
            var appPath = 
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    programName);
            Directory.CreateDirectory(appPath);

            const string contentFilename = "content";
            const string filepathFilename = "path";
            _contentPath = Path.Combine(appPath, contentFilename);
            _filepathPath = Path.Combine(appPath, filepathFilename);
        }
        
        private readonly string _contentPath;
        private readonly string _filepathPath;

        public bool IsEmpty() => !File.Exists(_contentPath) && !File.Exists(_filepathPath);

        public void Put(string filepath, string content)
        {
            File.WriteAllText(_filepathPath, filepath);
            File.WriteAllText(_contentPath, content);
        }

        public Shelf Take()
        {
            if (IsEmpty())
            {
                throw new InvalidOperationException();
            }
            
            var shelf = new Shelf(File.ReadAllText(_filepathPath) , File.ReadAllText(_contentPath));
            File.Delete(_filepathPath);
            File.Delete(_contentPath);
            return shelf;
        }
    }
}