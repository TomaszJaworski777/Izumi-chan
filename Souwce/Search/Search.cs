using System.Runtime.CompilerServices;
using System.Text;
using Izumi.Structures;
using Izumi.EvaluationScripts;
using Izumi.Structures.Data;

namespace Izumi.SearchScripts
{
    internal class Search
    {
        public static bool CancelationToken = false;

        private const int Infinity = 30000;

        private Move _latestBestMove;
        private Move _bestRootMove;

        private ulong _nodes = 0;
        private NodePerSecondTracker _nodePerSecondTracker = new(true);

        private DateTime _startTime;
        private int _timeRemaning;

        private readonly Evaluation _evaluation = new();

        public void Execute(Board board, int depth = 64, int whiteTime = int.MaxValue, int blackTime = int.MaxValue, bool infinite = false)
        {
            CancelationToken = false;

            _nodePerSecondTracker = new(true);
            _bestRootMove = new();
            _latestBestMove = new();
            _startTime = DateTime.Now;
            _timeRemaning = board.IsWhiteToMove ? whiteTime : blackTime;

            for (int currentDepth = 1; currentDepth <= depth; currentDepth++)
            {
                _nodes = 0;

                int bestScore = NegaMax(board, currentDepth, -Infinity, Infinity, 0);
                if (BreakCondition(_timeRemaning))
                    break;
                Console.WriteLine($"info depth {currentDepth} score cp {bestScore} nodes {_nodes} nps {_nodePerSecondTracker.LatestResult} hashfull {TranspositionTable.HashFull} best {_bestRootMove}");

                _latestBestMove = _bestRootMove;
            }

            Console.WriteLine($"bestmove {_latestBestMove}");
        }

        private string PrintPV(Board board, int depth)
        {
            StringBuilder pvLine = new();

            for (int i = 0; i < depth; i++)
            {
                ulong key = board.ZobristKey;
                TranspositionTableEntry? entry = TranspositionTable.Probe(key);
                if (entry is null)
                    break;
                Move move = entry.Value.bestMove;
                pvLine.Append($"{move} ");
                board.MakeMove(move);
            }

            return pvLine.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool BreakCondition(int timeRemaining) => CancelationToken || (DateTime.Now - _startTime).TotalMilliseconds > timeRemaining / 20;

        #region Search
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private int NegaMax(Board board, int depth, int alpha, int beta, int movesPlayed)
        {
            if (movesPlayed > 0 && (board.IsRepetition() || board.HalfMoves >= 100))
            {
                return 0;
            }

/*            TranspositionTableEntry? transpositionTableEntry = TranspositionTable.Probe(board.ZobristKey);
            if (movesPlayed > 0 && transpositionTableEntry != null && transpositionTableEntry.Value.Depth >= depth)
            {
                if (transpositionTableEntry.Value.Flag == TTeEntryFlag.Exact)
                    return transpositionTableEntry.Value.Score;
                else if (transpositionTableEntry.Value.Flag == TTeEntryFlag.Lowerbound)
                    alpha = Math.Max( alpha, transpositionTableEntry.Value.Score );
                else if (transpositionTableEntry.Value.Flag == TTeEntryFlag.Upperbound)
                    beta = Math.Min( beta, transpositionTableEntry.Value.Score );

                if (alpha >= beta)
                    return transpositionTableEntry.Value.Score;
            }*/

            if (depth <= 0)
            {
                return QuiesenceSearch(board, alpha, beta);
            }

            if (BreakCondition(_timeRemaning))
                return Infinity;

            MoveSelector selector = new(board.GeneratePseudoLegalMoves(), null);
            int value = -Infinity;
            int legalMoveCount = 0;
            int originalAlpha = alpha;
            Move bestMove = new();

            for (int moveIndex = 0; moveIndex < selector.Length; moveIndex++)
            {
                _nodePerSecondTracker.Update();

                Board boardCopy = board;
                Move move = selector.GetMoveForIndex(moveIndex);
                if (!boardCopy.MakeMove(move))
                    continue;

                legalMoveCount++;
                _nodes++;
                _nodePerSecondTracker.AddNode();

                var newValue = -NegaMax(boardCopy, depth - 1, -beta, -alpha, movesPlayed + 1);
                MoveHistory.RemoveLast();

                if (newValue > value)
                {
                    value = newValue;
                    bestMove = move;

                    if (value > alpha)
                        alpha = value;

                    if (movesPlayed == 0)
                        _bestRootMove = move;

                    if (alpha >= beta)
                        break;
                }
            }

            if (legalMoveCount == 0)
            {
                if (board.SideToMoveKingInCheck)
                    return movesPlayed - Infinity;
                else
                    return 0;
            }

/*            TranspositionTableEntry newEntry = new()
            {
                Depth = (byte)depth,
                PositionKey = board.ZobristKey,
                Score = (short)value,
                bestMove = bestMove
            };

            if (value <= originalAlpha)
                newEntry.Flag = TTeEntryFlag.Upperbound;
            else if (value >= beta)
                newEntry.Flag = TTeEntryFlag.Lowerbound;
            else
                newEntry.Flag = TTeEntryFlag.Exact;

            TranspositionTable.Add( newEntry );*/

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private int QuiesenceSearch(Board board, int alpha, int beta)
        {
            int eval = _evaluation.EvaluatePosition(board);

            if (eval >= beta)
                return beta;
            if (alpha < eval)
                alpha = eval;

            if (board.HalfMoves >= 100)
                return 0;

            if (BreakCondition(_timeRemaning))
                return Infinity;

            MoveSelector selector = new(board.GeneratePseudoLegalTacticalMoves(), null);

            for (int moveIndex = 0; moveIndex < selector.Length; moveIndex++)
            {
                _nodePerSecondTracker.Update();

                Move move = selector.GetMoveForIndex(moveIndex);
                Board boardCopy = board;
                if (!boardCopy.MakeMove(move))
                    continue;

                _nodes++;
                _nodePerSecondTracker.AddNode();

                int score = -QuiesenceSearch(boardCopy, -beta, -alpha);
                MoveHistory.RemoveLast();

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
