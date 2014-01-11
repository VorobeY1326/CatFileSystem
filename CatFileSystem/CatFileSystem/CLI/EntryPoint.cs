using System;

namespace CatFileSystem.CLI
{
    /*
     * Точка входа для консольной программы.
     * Консольная программа читает команды, пока не встретит exit. Все команды она пытается выполнить на нашей файловой системе.
     */
    public static class EntryPoint
    {
        public static void Main()
        {
            var commandExecuter = new CommandExecuter();

            commandExecuter.Execute("help");

            while (true)
            {
                var command = Console.ReadLine();
                
                if (command != null && command.ToLower().Trim() == "exit")
                    break;

                commandExecuter.Execute(command);
            }
        }
    }
}