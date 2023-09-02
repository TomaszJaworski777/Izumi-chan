using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Engine.Board;
using Engine.Evaluation;
using Engine.Move;

namespace Engine.Search;

public class SearchSystem
{
    public static bool CancellationToken;

    //definition of intinity used in the scope of search
    private const int Infinity = 777_777;
    private const int MateScore = 777_700;

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
        _timeManager = new TimeManager( time, increment, searchParameters.MovesToGo );
        Stopwatch stopwatch = new();

        //implementation of iterative deepening (https://www.chessprogramming.org/Iterative_Deepening)
        for (int currentDepth = 1; currentDepth <= searchParameters.Depth; currentDepth++)
        {
            _nodes = 0;
            long before = GC.GetAllocatedBytesForCurrentThread();
            stopwatch.Restart();

            int bestScore = NegaMax(ref searchParameters.Board, currentDepth, -Infinity, Infinity, 0);

            ulong totalMiliseconds = (ulong)stopwatch.ElapsedMilliseconds;
            ulong nps = _nodes * 1_000 / Math.Clamp(totalMiliseconds, 1, ulong.MaxValue);

            long allocatedBytes = GC.GetAllocatedBytesForCurrentThread() - before;
            if (allocatedBytes != 0)
            {
                Console.WriteLine($"WARNING: allocated {allocatedBytes}B when running!");
            }

            if (CancellationToken)
            {
                Console.WriteLine( $"info depth {currentDepth-1} score {DisplayScore( _lastBestScore )} nodes {_nodes} time {totalMiliseconds} nps {nps}" );
                break;
            }

            Console.WriteLine( $"info depth {currentDepth} score {DisplayScore(bestScore)} nodes {_nodes} time {totalMiliseconds} nps {nps}" );

            _bestRootMove = _bestRootMoveCandidate;
            _lastBestScore = bestScore;
        }

        return _bestRootMove;
    }

    //implementation of NegaMax algorithm (https://www.chessprogramming.org/Negamax)
    private unsafe int NegaMax( ref BoardData board, int depth, int alpha, int beta, int movesPlayed )
    {
        //returns 0 if position is drawn through repetition or 50-move. Here it returns 0 even when position is repeated just once, to make sure it won't miss a draw just because of a depth.
        if (movesPlayed > 0 && (MoveHistory.IsRepetition( board.ZobristKey ) || board.HalfMoves >= 100))
        {
            _nodes++;
            return 0;
        }

        //returns eval when at finishing node
        if (depth <= 0)
        {
            return QuiesenceSearch(ref board, alpha, beta);
        }

        //every 10k nodes updates the timer to check if engine run out of search time.
        //Same check will appear in Quiesence Search.
        if (_nodes % 10_000 == 0)
            _timeManager.Update();

        //breaks the search if cancelation token was invoked (stop command or out of time).
        //Same check will appear in Quiesence Search.
        if (CancellationToken)
            return -Infinity;

        //gets list of all pseudo moves
        MoveList moveList = new(stackalloc MoveData[218]);
        board.GenerateAllPseudoLegalMoves( ref moveList );
        MoveSelector selector = new(moveList, stackalloc MoveSelector.ScoredMove[218]);

        //setup variables for search loop
        int value = -Infinity;
        int legalMoveCount = 0;

        //loops through all moves
        for (int moveIndex = 0; moveIndex < selector.Length; moveIndex++)
        {
            //makes move and skips if move was illegal
            BoardData boardCopy = board;
            MoveData move = selector.GetMoveForIndex(moveIndex);
            if (!boardCopy.MakeMove( move ))
                continue;

            //counts legal moves for checkmate/stalemate check
            legalMoveCount++;

            //recursive negamax search. We pass -beta as alpha and -alpha as beta and also take negative score, because Negamax
            //always works from the POV of the side to move, so we have to reverse everything every time we switch side.
            var newValue = -NegaMax(ref boardCopy, depth - 1, -beta, -alpha, movesPlayed + 1);

            //when we come back to this point after recursive negamax, we want to unmake the move,
            //because if was just theoretical move for the sake of search loop
            boardCopy.UnmakeMove();

            //if new value is better than current value..
            if (newValue > value)
            {
                //then override if 
                value = newValue;

                //and because from our POV we want to maximize alpha then we incease out current alpha 
                if (value > alpha)
                    alpha = value;

                //if it's first loop then we want to override our move candidate with best move found
                if (movesPlayed == 0)
                    _bestRootMoveCandidate = move;

                //beta means the score of the current strongest move for the opponent,
                //if current move is stronger than opponent move, then there is no need to look further, so we break
                if (alpha >= beta)
                    break;
            }
        }

        //if there was no legal moves, then its either checkmate or stalemate
        if (legalMoveCount == 0)
        {
            if ((board.SideToMove == 0 ? board.IsWhiteKingInCheck : board.IsBlackKingInCheck) == 1)
                //we pass moves played here to make sure engine will always follow the quickest way to mate
                return movesPlayed - MateScore;
            return 0;
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //search method created to extend search for capture moves and remove horizon effect.
    //(https://www.chessprogramming.org/Quiescence_Search) (https://www.chessprogramming.org/Horizon_Effect)
    private unsafe int QuiesenceSearch(ref BoardData board, int alpha, int beta)
    {
        //get evaluation fo the position
        int eval = EvaluationSystem.EvaluatePosition(ref board);
        _nodes++;

        //if position is better than current opponent best move, the we perform beta cut-off, similar to what we did in negamax
        if (eval >= beta)
            return beta;
        //if that's the best move so far, we update alpha, same as in negamax, we need to do it,
        //because if there is no capture moves from this position we have to return this move value
        //and eval saved in alpha will help with some cutoffs in a loop
        if (eval > alpha)
            alpha = eval;

        //every 10k nodes updates the timer to check if engine run out of search time.
        //Same check will appear in Negamax.
        if (_nodes % 10_000 == 0)
            _timeManager.Update();

        //breaks the search if cancelation token was invoked (stop command or out of time).
        //Same check will appear in Negamax.
        if (CancellationToken)
            return -Infinity;

        //gets list of only tactical pseudo moves
        MoveList moveList = new(stackalloc MoveData[256]);
        board.GenerateTacticalPseudoLegalMoves( ref moveList );
        MoveSelector selector = new(moveList, stackalloc MoveSelector.ScoredMove[256]);

        //loops through all moves
        for (int moveIndex = 0; moveIndex < selector.Length; moveIndex++)
        {
            //makes move and skips if move was illegal
            BoardData boardCopy = board;
            MoveData move = selector.GetMoveForIndex(moveIndex);
            if (!boardCopy.MakeMove( move ))
                continue;

            //we check cpatures recursivly, and again it's from side's POV, so we have to flip it
            var score = -QuiesenceSearch(ref boardCopy, -beta, -alpha);

            //when we come back to this point after recursive negamax, we want to unmake the move,
            //because if was just theoretical move for the sake of search loop
            boardCopy.UnmakeMove();

            //we do the same, what we did in negamax, and in qsearch above
            if (score >= beta)
                return beta;
            //we override alpha in the same way
            if (score > alpha)
                alpha = score;
        }

        //when all moves checked then return score of the best one, which is stored in alpha
        return alpha;
    }

    private string DisplayScore(int score )
    {
        string scoreString = $"cp {score}";
        if (Math.Abs( score ) > MateScore / 2)
            scoreString = $"mate {Math.Ceiling( (MateScore - Math.Abs( score )) / 2f ) * Math.Sign( score )}";
        return scoreString;
    }
}
