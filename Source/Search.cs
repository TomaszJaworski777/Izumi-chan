namespace Greg
{
    internal class Search
    {
        private const int Infinity = 30000;

        private Move _bestRootMove;
        private NodePerSecondTracker _nodePerSecondTracker = new(true);
        private ulong _nodes = 0;

        public void Execute( Board board, int depth = int.MaxValue, int whiteTime = int.MaxValue, int blackTime = int.MaxValue, bool infinite = false )
        {
            _nodePerSecondTracker = new( true );
            _bestRootMove = new();
            _nodes = 0;

            for (int currentDepth = 1; currentDepth <= depth; currentDepth++)
            {
                int bestScore = NegaMax(board, currentDepth, -Infinity, Infinity, 0);
                Console.WriteLine( $"info depth {currentDepth} score cp {bestScore} nodes {_nodes}" );
            }

            Console.WriteLine( $"bestmove {_bestRootMove.ToString(board)}" );
        }

        private int NegaMax( Board board, int depth, int alpha, int beta, int movesPlayed )
        {
            _nodePerSecondTracker.Update();

            if(depth <= 0)
            {
                _nodes++;
                _nodePerSecondTracker.AddNode();
                return 0; //eval
            }

            ReadOnlySpan<Move> moves = board.GeneratePseudoLegalMoves();
            int value = -Infinity;

            for (int moveIndex = 0; moveIndex < moves.Length; moveIndex++)
            {
                Board boardCopy = board;
                Move move = moves[moveIndex];
                if (!boardCopy.MakeMove( move ))
                    continue;
                var newValue = -NegaMax(boardCopy, depth - 1, -beta, -alpha, movesPlayed + 1);
                if(newValue > value)
                {
                    value = newValue;

                    if (value > alpha)
                        alpha = value;

                    if (movesPlayed == 0)
                        _bestRootMove = move;

                    if (alpha >= beta)
                        break;
                }
            }

            if (moves.Length == 0)
                return board.IsKingInCheck( board.IsWhiteToMove ) ? -Infinity + movesPlayed : 0; 

            return value;
        }
    }
}
