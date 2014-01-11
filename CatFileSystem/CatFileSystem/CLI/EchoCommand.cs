using System;
using System.Text;
using System.Text.RegularExpressions;
using CatFileSystem.FS;

namespace CatFileSystem.CLI
{
    /*
     * Парсер команды echo, записывающей строку в файл.
     */
    public class EchoCommand : ICommand
    {
        public EchoCommand(FileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        private readonly FileSystem fileSystem;
        private readonly Regex echoCommandRegex = new Regex(@"^\s*echo\s+(\S+)\s*>\s*(\S+)\s*$");

        public bool CanExecute(string command)
        {
            return echoCommandRegex.IsMatch(command);
        }

        public void Execute(string command)
        {
            var match = echoCommandRegex.Match(command);

            if (match.Success)
            {
                var content = match.Groups[1];
                var filename = match.Groups[2];
                fileSystem.WriteFile(filename.Value, Encoding.UTF8.GetBytes(content.Value));
            }
            else
                Console.WriteLine("Bad ECHO command: " + command);
        }
    }
}