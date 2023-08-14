using System.Runtime.CompilerServices;

namespace Greg
{
    internal class Search
    {
        private const int Infinity = 30000;

        private Move _bestRootMove;
        private NodePerSecondTracker _nodePerSecondTracker = new(true);
        private ulong _nodes = 0;

        private readonly Evaluation _evaluation = new();

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

            Console.WriteLine( $"bestmove {_bestRootMove.ToString( board )}" );
        }

        [MethodImpl( MethodImplOptions.AggressiveOptimization )]
        private int NegaMax( Board board, int depth, int alpha, int beta, int movesPlayed )
        {
            _nodePerSecondTracker.Update();

            if (depth <= 0)
            {
                _nodes++;
                _nodePerSecondTracker.AddNode();
                return _evaluation.EvaluatePosition( board );
            }

            ReadOnlySpan<Move> moves = board.GeneratePseudoLegalMoves();
            int value = -Infinity;
            int legalMoveCount = 0;

            for (int moveIndex = 0; moveIndex < moves.Length; moveIndex++)
            {
                Board boardCopy = board;
                Move move = moves[moveIndex];
                if (!boardCopy.MakeMove( move ))
                    continue;
                legalMoveCount++;
                var newValue = -NegaMax(boardCopy, depth - 1, -beta, -alpha, movesPlayed + 1);
                if (newValue > value)
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

            if (legalMoveCount == 0)
                return board.IsKingInCheck( board.IsWhiteToMove ) ? (movesPlayed - Infinity) : 0;

            return value;
        }
    }
}
