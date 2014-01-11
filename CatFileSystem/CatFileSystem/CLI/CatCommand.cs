using System;
using System.Text;
using System.Text.RegularExpressions;
using CatFileSystem.FS;

namespace CatFileSystem.CLI
{
    /*
     * Парсер команды cat, показывающей содержимое файла.
     */
    public class CatCommand : ICommand
    {
        public CatCommand(FileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        private readonly FileSystem fileSystem;
        private readonly Regex touchCommandRegex = new Regex(@"^\s*cat\s+(\S+)\s*$");

        public bool CanExecute(string command)
        {
            return touchCommandRegex.IsMatch(command);
        }

        public void Execute(string command)
        {
            var match = touchCommandRegex.Match(command);

            if (match.Success)
            {
                var filename = match.Groups[1];
                var file = fileSystem.ReadFile(filename.Value);
                Console.WriteLine(null != file ? Encoding.UTF8.GetString(file) : "Sorry, no such file found");
            }
            else
                Console.WriteLine("Bad cat command: " + command);
        }
    }
}