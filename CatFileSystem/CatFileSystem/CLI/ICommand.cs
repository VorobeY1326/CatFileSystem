namespace CatFileSystem.CLI
{
    /*
     * Интерфейс для всех парсеров команд.
     */
    public interface ICommand
    {
        bool CanExecute(string command);
        void Execute(string command);
    }
}