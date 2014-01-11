using System;
using CatFileSystem.FS;

namespace CatFileSystem.CLI
{
    /*
     * Класс, управляющий вызовом необходимой команды, введенной с командной строки.
     * Перенаправляет команду тому парсеру, который способен её распознать.
     */
    public class CommandExecuter
    {
        private const string dbFileName = "files.db";
        private readonly FileSystem fileSystem = new FileSystem(dbFileName);
        private readonly ICommand[] commands;

        public CommandExecuter()
        {
            commands = new ICommand[]
            {
                new HelpCommand(),
                new TouchCommand(fileSystem),
                new CatCommand(fileSystem),
                new EchoCommand(fileSystem),
                new RmCommand(fileSystem),
                new TestCommand(dbFileName), 
            };
        }

        public void Execute(string commandString)
        {
            foreach (var command in commands)
            {
                if (command.CanExecute(commandString))
                {
                    command.Execute(commandString);
                    return;
                }
            }
            Console.WriteLine("No such command");
        }
    }
}