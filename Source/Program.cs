namespace Greg
{
    internal class Program
    {
        public static List<string> Commands = new();

        private static void Main( string[] args )
        {
            Console.ForegroundColor = ConsoleColor.Gray;

            PieceAttacks.Initizalize();

            Thread engineThread = new Thread(CommandProcessor);
            engineThread.Start();

            while (true)
            {
                var command = Console.ReadLine();
                if (command is not null)
                    Commands.Add( command );
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
                    string command = Commands.First();
                    Commands.RemoveAt( 0 );
                    commandManager.ProcessCommand( command );
                }
            }
        }
    }
}
