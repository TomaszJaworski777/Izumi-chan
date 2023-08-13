namespace Greg
{
    internal class Program
    {
        static void Main( string[] args )
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Board board = new Board("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");

            while (true)
            {
                var move = Console.ReadLine()!;
                board.MakeMove( new Move( move, board ) );
                board.DrawBoard();
                board.Data[12].Draw();
                board.Data[13].Draw();
                board.Data[14].Draw();
            }
        }
    }
}
