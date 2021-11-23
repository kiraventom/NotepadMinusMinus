using System;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace GUI
{
    public record ShelvedRecord(string FilePath, string Content);
    
    public class Shelf
    {
        public Shelf(string programName)
        {
            var appPath = 
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    programName);
            Directory.CreateDirectory(appPath);

            const string shelfFilename = "shelf.json";
            _shelfPath = Path.Combine(appPath, shelfFilename);
        }
        
        private readonly string _shelfPath;

        public bool IsEmpty() => !File.Exists(_shelfPath);

        public void Put(string filepath, string content)
        {
            string json = JsonSerializer.Serialize(new ShelvedRecord(filepath, content));
            File.WriteAllText(_shelfPath, json);
        }

        public ShelvedRecord Take()
        {
            if (IsEmpty())
            {
                throw new InvalidOperationException();
            }

            string json = File.ReadAllText(_shelfPath);
            var shelved = JsonSerializer.Deserialize<ShelvedRecord>(json);
            
            File.Delete(_shelfPath);
            return shelved;
        }
    }
}