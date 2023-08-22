using System.Runtime.CompilerServices;
using System.Text;
using Izumi.Structures;
using Izumi.EvaluationScripts;

namespace Izumi.SearchScripts
{
    internal class Search
    {
        public static bool CancelationToken = false;

        private const int Infinity = 30000;

        private Move _latestBestMove;
        private Move _bestRootMove;

        private TranspositionTableEntry? _transpositionTableEntry = null;

        private ulong _nodes = 0;
        private NodePerSecondTracker _nodePerSecondTracker = new(true);

        private DateTime _startTime;
        private int _timeRemaning;

        private readonly Evaluation _evaluation = new();

        public void Execute(Board board, int depth = int.MaxValue, int whiteTime = int.MaxValue, int blackTime = int.MaxValue, bool infinite = false)
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
                Console.WriteLine($"info depth {currentDepth} score cp {bestScore} nodes {_nodes} nps {_nodePerSecondTracker.LatestResult} hashfull {TranspositionTable.HashFull} pv {PrintPV(board, currentDepth)}");

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int MoveSortRuleset(Move a, Move b)
        {
            int score = GetMoveValue(b) - GetMoveValue(a);

            if (score is 0)
                return 0;
            return Math.Sign(score);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetMoveValue(Move move)
        {
            if (_transpositionTableEntry != null && move.Equals(_transpositionTableEntry.Value.bestMove))
                return Infinity;
            if (move.IsCapture)
                return ((int)move.TargetPiece + 1) * 100 - (int)move.MovingPiece;
            else if (move.IsCastle || move.IsPromotion)
                return 1;
            return 0;
        }

        #region Search
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private int NegaMax(Board board, int depth, int alpha, int beta, int movesPlayed)
        {
            if (movesPlayed > 0 && (board.IsRepetition() || board.HalfMoves >= 100))
                return 0;

            _transpositionTableEntry = TranspositionTable.Probe(board.ZobristKey);
            if (movesPlayed > 0 && _transpositionTableEntry != null && _transpositionTableEntry.Value.Depth >= depth)
            {
                if (_transpositionTableEntry.Value.Flag == TTeEntryFlag.Exact)
                    return _transpositionTableEntry.Value.Score;
                else if (_transpositionTableEntry.Value.Flag == TTeEntryFlag.Lowerbound)
                    alpha = Math.Max(alpha, _transpositionTableEntry.Value.Score);
                else if (_transpositionTableEntry.Value.Flag == TTeEntryFlag.Upperbound)
                    beta = Math.Min(beta, _transpositionTableEntry.Value.Score);

                if (alpha >= beta)
                    return _transpositionTableEntry.Value.Score;
            }

            if (depth <= 0)
            {
                return QuiesenceSearch(board, alpha, beta);
            }

            if (BreakCondition(_timeRemaning))
                return Infinity;

            Span<Move> moves = board.GeneratePseudoLegalMoves();
            moves.Sort(MoveSortRuleset);
            int value = -Infinity;
            int legalMoveCount = 0;
            int originalAlpha = alpha;
            Move bestMove = new();

            for (int moveIndex = 0; moveIndex < moves.Length; moveIndex++)
            {
                _nodePerSecondTracker.Update();

                Board boardCopy = board;
                Move move = moves[moveIndex];
                if (!boardCopy.MakeMove(move))
                    continue;

                if (_bestRootMove.IsNull)
                    _bestRootMove = move;

                legalMoveCount++;
                _nodes++;
                _nodePerSecondTracker.AddNode();

                var newValue = -NegaMax(boardCopy, depth - 1, -beta, -alpha, movesPlayed + 1);
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
                return board.IsKingInCheck(board.IsWhiteToMove) ? movesPlayed - Infinity : 0;

            TranspositionTableEntry newEntry = new()
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

            TranspositionTable.Add(newEntry);

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

            Span<Move> moves = board.GeneratePseudoLegalPriorityMoves();
            moves.Sort(MoveSortRuleset);

            for (int moveIndex = 0; moveIndex < moves.Length; moveIndex++)
            {
                _nodePerSecondTracker.Update();

                Move move = moves[moveIndex];
                Board boardCopy = board;
                if (!boardCopy.MakeMove(move))
                    continue;

                _nodes++;
                _nodePerSecondTracker.AddNode();

                int score = -QuiesenceSearch(boardCopy, -beta, -alpha);

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
