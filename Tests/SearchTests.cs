using Engine.Board;
using Engine.Move;
using Engine.Search;
using Engine.Search.TranspositionTables;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Tests;

[TestClass]
public class SearchTests
{
    [TestMethod]
    public void Search_Test()
    {
        BoardData board = BoardProvider.Create( BoardProvider.StartPosition );
        SearchSystem search = new();
        MoveData bestMove = search.FindBestMove(new SearchParameters( ref board, 3));
        Assert.IsTrue( bestMove.FromSquareIndex != bestMove.ToSquareIndex );
        Assert.IsTrue( bestMove.FromSquareIndex < 64 );
        Assert.IsTrue( bestMove.ToSquareIndex < 64 );
    }

    [TestMethod]
    public void TranspositionMoveOrdering_Test()
    {
        BoardData board = BoardProvider.Create( BoardProvider.StartPosition );
        MoveList moves = new (stackalloc MoveData[218]);
        board.GenerateAllPseudoLegalMoves( ref moves );
        MoveSelector moveSelector = new (moves, stackalloc MoveSelector.ScoredMove[218], ((Span<MoveData>)moves)[12]);

        Assert.IsTrue( moveSelector.GetMoveForIndex( 0 ).Equals( ((Span<MoveData>)moves)[12] ) );
    }

    [TestMethod]
    public void TranspositionAccess_Test()
    {
        TranspositionTable.Push( new TranspositionTableEntry
        {
            BestMove = default,
            Depth = 5,
            Flag = TTFlag.AlphaUnchanged,
            PositionKey = 15,
            Score = 777
        } );

        Assert.IsTrue( TranspositionTable.Probe( 10 ).PositionKey == 0 );
        Assert.IsTrue( TranspositionTable.Probe( 15 ).PositionKey == 15 );
        Assert.IsTrue( TranspositionTable.Probe( 15 ).Score == 777 );
        Assert.IsTrue( TranspositionTable.Probe( 15 ).Depth == 5 );
        Assert.IsTrue( TranspositionTable.Probe( 15 ).Flag == TTFlag.AlphaUnchanged );
        Assert.IsTrue( TranspositionTable.Probe( 15 ).BestMove.IsNull );
    }
}
