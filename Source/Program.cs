using System.Collections.Concurrent;

namespace Greg
{
    internal class Program
    {
        public static ConcurrentQueue<string> Commands = new();

        private static void Main( string[] args )
        {
            Console.ForegroundColor = ConsoleColor.Gray;

            PieceAttacks.Initizalize();

            Thread engineThread = new Thread(CommandProcessor);
            engineThread.Start();

            ChessEngine chessEngine = new();
            UciCommandManager commandManager = new(chessEngine);

            while (true)
            {
                var command = Console.ReadLine();
                if (command is not null)
                    Commands.Enqueue( command );
            }
        }

        private static void CommandProcessor()
        {
            ChessEngine chessEngine = new();
            UciCommandManager commandManager = new(chessEngine);

            while (true)
            {
                if (Commands.Count > 0)
                {
                    if (!Commands.TryDequeue( out string? command ))
                        continue;
                    commandManager.ProcessCommand( command );
                }
            }
        }
    }
}
