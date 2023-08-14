namespace Greg
{
    internal class Program
    {
        public static bool StopToken = false;
        public static List<string> Commands = new();

        private static void Main( string[] args )
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Thread engineThread = new Thread(CommandProcessor);
            engineThread.Start();

            while (true)
            {
                var command = Console.ReadLine();
                if (command is not null)
                    Commands.Add( command );

                if (StopToken)
                    break;
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

                if (StopToken)
                    break;
            }
        }
    }
}
