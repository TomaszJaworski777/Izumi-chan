using System.Runtime.CompilerServices;

namespace Greg
{
    internal class Search
    {
        private const int Infinity = 30000;

        private SearchData _data;
        private Move _bestRootMove;
        private DateTime _startTime;
        private NodePerSecondTracker _nodePerSecondTracker = new(true);
        private ulong _nodes = 0;
        private bool _stop = false;

        private readonly Evaluation _evaluation = new();
        private readonly Thread _thread;

        public Search()
        {
            _thread = new( ThreadSearch );
            _thread.Start();
        }

        public void Execute( Board board, int depth = int.MaxValue, int whiteTime = int.MaxValue, int blackTime = int.MaxValue, bool infinite = false )
        {
            _nodePerSecondTracker = new( true );
            _bestRootMove = new();
            _nodes = 0;
            _stop = false;
            _startTime = DateTime.Now;

            for (int currentDepth = 1; currentDepth <= depth; currentDepth++)
            {
                _data = new SearchData
                {
                    Board = board,
                    Depth = currentDepth,
                    Done = false
                };
                while (!_data.Done)
                {
                    _nodePerSecondTracker.Update();

                    if (Program.Commands.Contains( "stop" ) || Program.Commands.Contains( "quit" ) || (DateTime.Now - _startTime).TotalMilliseconds > ((board.IsWhiteToMove ? whiteTime : blackTime) / 20))
                    {
                        _stop = true;
                        break;
                    }
                }
                Console.WriteLine( $"info depth {currentDepth} score cp {_data.BestScore} nodes {_nodes}" );

                if (_stop)
                    break;
            }

            Console.WriteLine( $"bestmove {_bestRootMove.ToString( board )}" );
        }

        private void ThreadSearch()
        {
            while (true)
            {
                if (_data.Done) 
                    continue;

                _data.BestScore = NegaMax( _data.Board, _data.Depth, -Infinity, Infinity, 0 );
                _data.Done = true;
            }
        }

        private struct SearchData
        {
            public int Depth;
            public Board Board;
            public bool Done;
            public int BestScore;
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
