using Engine.Board;
using Engine.Move;
using Engine.Search;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests;

[TestClass]
public class SearchTests
{
    [TestMethod]
    public void SymmetricalEval_Test()
    {
        BoardData board = BoardProvider.Create( BoardProvider.StartPosition );
        SearchSystem search = new();
        MoveData bestMove = search.FindBestMove(new SearchParameters(board, 3));
        Assert.IsTrue( bestMove.FromSquareIndex != bestMove.ToSquareIndex );
        Assert.IsTrue( bestMove.FromSquareIndex < 64 );
        Assert.IsTrue( bestMove.ToSquareIndex < 64 );
    }
}
