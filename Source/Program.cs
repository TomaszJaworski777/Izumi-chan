namespace Greg
{
    internal class Program
    {
        private static Perft _perft = new();

        static void Main( string[] args )
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Board board = new Board("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1");
            _perft.Execute( 10, board, false );
        }
    }
}
