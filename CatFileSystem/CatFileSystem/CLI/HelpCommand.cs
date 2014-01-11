using System;
using System.Text.RegularExpressions;

namespace CatFileSystem.CLI
{
    /*
     * Парсер команды help, показывающей справку.
     */
    public class HelpCommand : ICommand
    {
        private readonly Regex helpCommandRegex = new Regex(@"^\s*help\s*$");

        public bool CanExecute(string command)
        {
            return helpCommandRegex.IsMatch(command);
        }

        public void Execute(string command)
        {
            Console.WriteLine("Help - list of commands:");
            Console.WriteLine("help : help");
            Console.WriteLine("cat filename : read file");
            Console.WriteLine("echo text > filename : write to file");
            Console.WriteLine("touch filename : create file");
            Console.WriteLine("rm filename : REMOVES file");
            Console.WriteLine("test : start many threads test");
        }
    }
}