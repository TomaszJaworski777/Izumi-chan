using System.Runtime.CompilerServices;

namespace Greg
{
    internal class Search
    {
        private const int Infinity = 30000;

        public static bool CancelationToken = false;

        private Move _bestRootMove;
        private Move _latestBestMove;
        private DateTime _startTime;
        private NodePerSecondTracker _nodePerSecondTracker = new(true);
        private ulong _nodes = 0;

        private int _timeRemaning;

        private readonly Evaluation _evaluation = new();

        public void Execute( Board board, int depth = int.MaxValue, int whiteTime = int.MaxValue, int blackTime = int.MaxValue, bool infinite = false )
        {
            CancelationToken = false;

            _nodePerSecondTracker = new( true );
            _bestRootMove = new();
            _latestBestMove = new();
            _nodes = 0;
            _startTime = DateTime.Now;
            _timeRemaning = board.IsWhiteToMove ? whiteTime : blackTime;

            for (int currentDepth = 1; currentDepth <= depth; currentDepth++)
            {
                int bestScore = NegaMax(board, currentDepth, -Infinity, Infinity, 0);
                if (BreakCondition( _timeRemaning ))
                    break;
                Console.WriteLine( $"info depth {currentDepth} score cp {bestScore} nodes {_nodes}" );

                _latestBestMove = _bestRootMove;
            }

            Console.WriteLine( $"bestmove {_latestBestMove.ToString( board )}" ); //still makes illegal moves
        }

        [MethodImpl( MethodImplOptions.AggressiveOptimization )]
        private int NegaMax( Board board, int depth, int alpha, int beta, int movesPlayed )
        {
            if (depth <= 0)
            {
                _nodes++;
                _nodePerSecondTracker.AddNode();
                return _evaluation.EvaluatePosition( board );
            }

            if (BreakCondition( _timeRemaning ))
                return Infinity;

            ReadOnlySpan<Move> moves = board.GeneratePseudoLegalMoves();
            int value = -Infinity;
            int legalMoveCount = 0;

            for (int moveIndex = 0; moveIndex < moves.Length; moveIndex++)
            {
                _nodePerSecondTracker.Update();

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

        private bool BreakCondition( int timeRemaining ) => CancelationToken || (DateTime.Now - _startTime).TotalMilliseconds > (timeRemaining / 20);
    }
}
