using System.Runtime.CompilerServices;

namespace Izumi
{
    internal class Search
    {
        private const int Infinity = 30000;

        public static bool CancelationToken = false;

        private Move _latestBestMove;
        private Move _bestRootMove;
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

            Console.WriteLine( $"bestmove {_latestBestMove.ToString()}" );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private bool BreakCondition( int timeRemaining ) => CancelationToken || (DateTime.Now - _startTime).TotalMilliseconds > (timeRemaining / 20);

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private static int MoveSortRuleset( Move a, Move b )
        {
            int score = GetMoveValue(b) - GetMoveValue(a);

            if (score is 0)
                return 0;
            return Math.Sign( score );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private static int GetMoveValue( Move move )
        {
            if (move.IsCapture)
                return ((int)move.TargetPiece + 1) * 100 - (int)move.MovingPiece;
            else if (move.IsCastle || move.IsPromotion)
                return 1;
            return 0;
        }

        #region Search
        [MethodImpl( MethodImplOptions.AggressiveOptimization )]
        private int NegaMax( Board board, int depth, int alpha, int beta, int movesPlayed )
        {
            if (depth <= 0)
            {
                return QuiesenceSearch( board, alpha, beta );
            }

            if (BreakCondition( _timeRemaning ))
                return Infinity;

            Span<Move> moves = board.GeneratePseudoLegalMoves();
            moves.Sort( MoveSortRuleset );
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
                _nodes++;
                _nodePerSecondTracker.AddNode();

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

        [MethodImpl( MethodImplOptions.AggressiveOptimization )]
        private int QuiesenceSearch( Board board, int alpha, int beta )
        {
            int eval = _evaluation.EvaluatePosition( board );

            if (eval >= beta)
                return beta;
            if (alpha < eval)
                alpha = eval;

            if (BreakCondition( _timeRemaning ))
                return Infinity;

            Span<Move> moves = board.GeneratePseudoLegalPriorityMoves();
            moves.Sort( MoveSortRuleset );

            for (int moveIndex = 0; moveIndex < moves.Length; moveIndex++)
            {
                _nodePerSecondTracker.Update();

                Board boardCopy = board;
                Move move = moves[moveIndex];
                if (!boardCopy.MakeMove( move ))
                    continue;

                _nodes++;
                _nodePerSecondTracker.AddNode();

                int score = -QuiesenceSearch( boardCopy, -beta, -alpha );

                if (score >= beta)
                    return beta;
                if (score > alpha)
                    alpha = score;
            }
            return alpha;
        }
        #endregion
    }
}
