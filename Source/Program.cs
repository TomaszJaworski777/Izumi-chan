namespace Greg
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Board board = new Board("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
            var move = new Move("d2d8Q", board);
            Console.WriteLine( $"Move: {move.ToString( board )}, moving piece: {move.MovingPiece}, target piece: {move.TargetPiece}, ispromotion: {move.IsPromotion}" );
        }
    }
}
