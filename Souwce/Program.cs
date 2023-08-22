using Izumi.Helpers;
using Izumi.SearchScripts;
using Izumi.UCI;

namespace Izumi.Core
{
    internal class Program
    {

        private static void Main( string[] args )
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine( UciConfig.Header );

            PieceAttacks.Initizalize();

            ChessEngine chessEngine = new();
            UciCommandManager commandManager = new(chessEngine);

            while (true)
            {
                var command = Console.ReadLine();
                if (command is not null)
                {
                    if (command is "stop" or "quit")
                    {
                        Search.CancelationToken = true;
                    }
                    commandManager.ProcessCommand( command );
                }
            }
        }
    }
}