using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Engine.Board;
using Engine.Evaluation;
using Engine.Move;
using Engine.Search.PVs;
using Engine.Search.TranspositionTables;

namespace Engine.Search;

public class SearchSystem
{
    public static bool CancellationToken;

    private MoveData _bestRootMoveCandidate;
    private MoveData _bestRootMove;
    private ulong _nodes;
    private int _lastBestScore;

    private TimeManager _timeManager;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MoveData FindBestMove(SearchParameters searchParameters)
    {
        CancellationToken = false;

        int time = searchParameters.Board.SideToMove == 0 ? searchParameters.WhiteTime : searchParameters.BlackTime;
        int increment = searchParameters.Board.SideToMove == 0 ? searchParameters.WhiteIncrement : searchParameters.BlackIncrement;
        _timeManager = new TimeManager(time, increment, searchParameters.MovesToGo);
        Stopwatch stopwatch = new();
        stopwatch.Start();

        _nodes = 0;

        //implementation of iterative deepening (https://www.chessprogramming.org/Iterative_Deepening)
        for (int currentDepth = 1; currentDepth <= searchParameters.Depth; currentDepth++)
        {
            long before = GC.GetAllocatedBytesForCurrentThread();

            int bestScore = NegaMax(NodeType.Root, ref searchParameters.Board, currentDepth, -INFINITY, INFINITY, 0);

            ulong totalMiliseconds = (ulong)stopwatch.ElapsedMilliseconds;
            ulong nps = _nodes * 1_000 / Math.Max(totalMiliseconds, 1);

            long allocatedBytes = GC.GetAllocatedBytesForCurrentThread() - before;
            if (allocatedBytes != 0)
            {
                Console.WriteLine($"WARNING: allocated {allocatedBytes}B when running!");
            }

            if (CancellationToken)
            {
                Console.WriteLine($"info depth {currentDepth - 1} score {DisplayScore(_lastBestScore)} nodes {_nodes} time {totalMiliseconds} nps {nps}");
                break;
            }

            Console.WriteLine($"info depth {currentDepth} score {DisplayScore(bestScore)} nodes {_nodes} time {totalMiliseconds} nps {nps}");

            _bestRootMove = _bestRootMoveCandidate;
            _lastBestScore = bestScore;
        }

        return _bestRootMove;
    }


    //implementation of NegaMax algorithm (https://www.chessprogramming.org/Negamax)
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private unsafe int NegaMax(NodeType nodeType, ref BoardData board, int depth, int alpha, int beta, int movesPlayed )
    {
        bool isPV = nodeType != NodeType.NonPV;
        bool isRoot = nodeType == NodeType.Root;

        //returns 0 if position is drawn through repetition or 50-move. Here it returns 0 even when position is repeated just once, to make sure it won't miss a draw just because of a depth.
        if (movesPlayed > 0 && (MoveHistory.IsRepetition( board.ZobristKey ) || board.HalfMoves >= 100))
            return 0;

        //We are trying to get entry from Transposition Table (https://www.chessprogramming.org/Transposition_Table)
        ref TranspositionTableEntry entry = ref TranspositionTable.Probe(board.ZobristKey);

        //We only want to get Transposition entry, when we are NOT in root node.
        //Entry that we get can be for different position that our current one due to the way we extract TT index, so we have to make the check
        if (movesPlayed > 0 && entry.PositionKey == board.ZobristKey && entry.Depth >= depth)
        {
            //if last time alpha changed, then stored move in the entry is the best move, so we can return it
            if (entry.Flag == TTFlag.AlphaChanged)
                return entry.Score;
            //if we had a beta cutoff in the loop that the entry is from, then we can set our alpha to value from that entity to make sure that cutoff is respected and we wont get worse move
            else if (entry.Flag == TTFlag.BetaCutoff)
                alpha = Math.Max( alpha, entry.Score );
            //if alpha did not change, then we set beta to best move found in entry loop to increase chance that this time alpha will change
            else if (entry.Flag == TTFlag.AlphaUnchanged)
                beta = Math.Min( beta, entry.Score );

            //if after applying values from entry we find out that move is good enough we can return it as TT cutoff
            if (alpha >= beta)
                return entry.Score;
        }

        //returns eval when at finishing node
        if (depth <= 0)
        {
            return QuiesenceSearch(isPV ? NodeType.PV : NodeType.NonPV, ref board, alpha, beta);
        }

        //every 10k nodes updates the timer to check if engine run out of search time.
        //Same check will appear in Quiesence Search.
        if (_nodes % 10_000 == 0)
            _timeManager.Update();

        //breaks the search if cancelation token was invoked (stop command or out of time).
        //Same check will appear in Quiesence Search.
        if (CancellationToken)
            return -INFINITY;

        //gets list of all pseudo moves
        MoveList moveList = new(stackalloc MoveData[218]);
        board.GenerateAllPseudoLegalMoves( ref moveList );
        MoveSelector selector = new(moveList, stackalloc MoveSelector.ScoredMove[218], entry.PositionKey == board.ZobristKey ? entry.BestMove : default);

        //setup variables for search loop
        int bestValue = -INFINITY;
        bool foundLegalMove = false;
        int localMoveCount = 0;
        int originalAlpha = alpha;
        MoveData bestLocalMove = default;

        //loops through all moves
        for (int moveIndex = 0; moveIndex < selector.Length; moveIndex++)
        {
            //makes move and skips if move was illegal
            BoardData boardCopy = board;
            MoveData move = selector.GetMoveForIndex(moveIndex);
            if (!boardCopy.MakeMove( move ))
                continue;

            _nodes++;

            //counts legal moves for checkmate/stalemate check
            foundLegalMove = true;

            // We must give some random initial value to newValue, because the compiler doesn't see
            // that newValue will always be assigned when we are in PV node and playedMoves > 0

            int newValue = -INFINITY;

            // If we are either in a non pv node, or in a pv node where we have played at least one move, do a zero window search
            if (!isPV || localMoveCount > 0)
                newValue = -NegaMax(NodeType.NonPV, ref boardCopy, depth - 1, -alpha-1, -alpha, movesPlayed + 1);

            // If we are in a PV node and we haven't played any move yet, or the zero window search hit alpha
            // do a search again with normal bounds
            if (isPV && (localMoveCount == 0 || newValue > alpha))
                newValue = -NegaMax(NodeType.PV, ref boardCopy, depth - 1, -beta, -alpha, movesPlayed + 1);


            //when we come back to this point after recursive negamax, we want to unmake the move,
            //because if was just theoretical move for the sake of search loop
            boardCopy.UnmakeMove();

            localMoveCount++;

            //if new value is better than current value..
            if (newValue > bestValue)
            {
                //then override if 
                bestValue = newValue;

                //if it's first loop then we want to override our move candidate with best move found
                if (movesPlayed == 0)
                    _bestRootMoveCandidate = move;

                //and because from our POV we want to maximize alpha then we incease out current alpha 
                if (bestValue > alpha)
                {
                    bestLocalMove = move;

                    //if current move is "too good" and opponent has a way to avoid it or this move is not a PV move (integrated zwSearch)
                    //(https://www.chessprogramming.org/Principal_Variation_Search#-Quote%20by%20Dennis:~:text=int%20zwSearch(int,hard%2C%20return%20alpha%0A%7D), 
                    //there is no need to look further, so we fail high and break (beta cutoff)
                    if (!isPV || bestValue >= beta)
                        return bestValue; // TODO: tt save

                    alpha = bestValue;
                }
            }
        }

        //if there was no legal moves, then its either checkmate or stalemate
        if (!foundLegalMove)
        {
            if (board.IsStmInCheck)
                //we pass moves played here to make sure engine will always follow the quickest way to mate
                return movesPlayed - MATE_SCORE;

            return 0;
        }

        // Push to TT
      /*  TranspositionTable.Push( new TranspositionTableEntry
        {
            PositionKey = board.ZobristKey,
            BestMove = bestLocalMove,
            Score = bestValue,
            Depth = (byte)depth,
            Flag = (bestValue >= beta ? TTFlag.BetaCutoff : (bestValue > originalAlpha ? TTFlag.AlphaChanged : TTFlag.AlphaUnchanged))
        } );*/

        return bestValue;
    }

    //search method created to extend search for capture moves and remove horizon effect.
    //(https://www.chessprogramming.org/Quiescence_Search) (https://www.chessprogramming.org/Horizon_Effect)
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private unsafe int QuiesenceSearch(NodeType nodeType, ref BoardData board, int alpha, int beta)
    {
        bool isPV = nodeType != NodeType.NonPV;

        //We only want to get Transpostion entry, when we are NOT in root node
        if (false) // TODO enable TT back
        {
            //We are trying to get entry from Transposition Table (https://www.chessprogramming.org/Transposition_Table)
            ref TranspositionTableEntry entry = ref TranspositionTable.Probe(board.ZobristKey);

            //Entry that we get can be for different position that our current one due to the way we extract TT index, so we have to make the check
            //Based on result of the search that current TT entry is from we can check if our current search can be interrupted to avoind unecessary iterations
            if (entry.PositionKey == board.ZobristKey &&
                ( entry.Flag == TTFlag.AlphaChanged ||
                entry.Flag == TTFlag.BetaCutoff && entry.Score >= beta ||
                entry.Flag == TTFlag.AlphaUnchanged && entry.Score <= alpha ))
            {
                return entry.Score;
            }
        }

        // TODO
        int bestValue = -INFINITY;


        //get evaluation fo the position
        // FIXME can't do standpat when in check
        int eval = bestValue = EvaluationSystem.EvaluatePosition(ref board);

        //if position is better than current opponent best move, the we perform beta cut-off, similar to what we did in negamax
        if (bestValue >= beta)
            return bestValue;
        //if that's the best move so far, we update alpha, same as in negamax, we need to do it,
        //because if there is no capture moves from this position we have to return this move value
        //and eval saved in alpha will help with some cutoffs in a loop
        if (bestValue > alpha)
            alpha = bestValue;

        //every 10k nodes updates the timer to check if engine run out of search time.
        //Same check will appear in Negamax.
        if (_nodes % 10_000 == 0)
            _timeManager.Update();

        //breaks the search if cancelation token was invoked (stop command or out of time).
        //Same check will appear in Negamax.
        if (CancellationToken)
            return -INFINITY;

        //gets list of only tactical pseudo moves
        MoveList moveList = new(stackalloc MoveData[256]);
        board.GenerateTacticalPseudoLegalMoves( ref moveList );
        MoveSelector selector = new(moveList, stackalloc MoveSelector.ScoredMove[256], default);

        //loops through all moves
        for (int moveIndex = 0; moveIndex < selector.Length; moveIndex++)
        {
            //makes move and skips if move was illegal
            BoardData boardCopy = board;
            MoveData move = selector.GetMoveForIndex(moveIndex);
            if (!boardCopy.MakeMove( move ))
                continue;

            _nodes++;

            //we check cpatures recursivly, and again it's from side's POV, so we have to flip it
            var value = -QuiesenceSearch(nodeType, ref boardCopy, -beta, -alpha);

            //when we come back to this point after recursive negamax, we want to unmake the move,
            //because if was just theoretical move for the sake of search loop
            boardCopy.UnmakeMove();

            //we look for the best value from possible tactical moves
            if (value >= bestValue) {
                bestValue = value;

                //similarly to NegaMax we seek for alpha improvements
                if (bestValue > alpha)
                {
                    //and if they fail high we want to return value, because it's pointless to search more, because we have beta cutoff
                    if (!isPV || bestValue >= beta)
                        return bestValue; // TODO tt save

                    alpha = bestValue;
                }
            }
        }

        //when all moves checked then return score of the best one, fail soft approach
        return bestValue;
    }

    private string DisplayScore(int score )
    {
        string scoreString = $"cp {score}";
        if (Math.Abs( score ) > MATE_SCORE / 2)
            scoreString = $"mate {Math.Ceiling( (MATE_SCORE - Math.Abs( score )) / 2f ) * Math.Sign( score )}";
        return scoreString;
    }
}
