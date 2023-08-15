using System.Collections.Concurrent;

namespace Greg
{
    internal class Program
    {
        private static ConcurrentQueue<string> _commands = new();

        private static void Main( string[] args )
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine( UciConfig.Header );

            PieceAttacks.Initizalize();

            Thread engineThread = new Thread(CommandProcessor);
            engineThread.Start();

            ChessEngine chessEngine = new();
            UciCommandManager commandManager = new(chessEngine);

            while (true)
            {
                var command = Console.ReadLine();
                if (command is not null)
                {
                    if (command is "stop" or "quit")
                        Search.CancelationToken = true;
                    _commands.Enqueue( command );
                }
            }
        }

        private static void CommandProcessor()
        {
            ChessEngine chessEngine = new();
            UciCommandManager commandManager = new(chessEngine);

            while (true)
            {
                if (_commands.Count > 0)
                {
                    if (!_commands.TryDequeue( out string? command ))
                        continue;
                    commandManager.ProcessCommand( command );
                }
            }
        }
    }
}
