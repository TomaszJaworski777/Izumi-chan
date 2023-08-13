using System.Diagnostics;

namespace Greg
{
    internal class Program
    {
        private static DateTime _startTime;
        private static ulong _nps;

        static void Main( string[] args )
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Board board = new Board("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");

            _startTime = DateTime.Now;
            for (int i = 1; i < 8; i++)
            {
                ulong result = Perft( i, board, false );
                Console.WriteLine($"Depth {i}, nodes {result}");
            }

/*            Console.WriteLine( moves.Length );
            for (int i = 0; i < moves.Length; i++)
            {
                Console.WriteLine( moves[i].ToString(board) );
            }*/

            while (true)
            {
                var move = Console.ReadLine()!;
            }
        }

        private static ulong Perft( int depth, Board board, bool first )
        {
            if (depth == 0)
            {
                _nps++;
                return 1UL;
            }

            if ((DateTime.Now - _startTime).TotalMilliseconds > 1000)
            {
                _startTime = DateTime.Now;
                Console.WriteLine( $"nps {_nps}" );
                _nps = 0;
            }

            ulong count = 0;
            var moves = MoveController.GeneratePseudoLegalMoves( board );

            if((DateTime.Now - _startTime).TotalMilliseconds > 1000)
            {
                _startTime = DateTime.Now;
                Console.WriteLine( $"nps {_nps}" );
                _nps = 0;
            }

            for (int i = 0; i < moves.Length; i++)
            {
                Board copy = board;
                if (!copy.MakeMove( moves[i] ))
                    continue;
                ulong result = Perft( depth - 1, copy, false );
                count += result;
                if (first)
                    Console.WriteLine( $"{moves[i].ToString(board)} - {result}" );
            }

            return count;
        }
    }
}
