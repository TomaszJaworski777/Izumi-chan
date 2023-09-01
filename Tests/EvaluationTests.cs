using Engine.Board;
using Engine.Data.Enums;
using Engine.Evaluation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests;

[TestClass]
public class EvaluationTests
{
    [TestMethod]
    public void SymmetricalEval_Test()
    {
        BoardData board = BoardProvider.Create(BoardProvider.StartPosition);
        BoardData flippedBoard = FlipBoard( board );
        Assert.IsTrue( EvaluationSystem.EvaluatePosition( ref board ) == EvaluationSystem.EvaluatePosition( ref flippedBoard ) );

        board = BoardProvider.Create(BoardProvider.KiwipetePosition);
        flippedBoard = FlipBoard( board );
        Assert.IsTrue( EvaluationSystem.EvaluatePosition( ref board ) == EvaluationSystem.EvaluatePosition( ref flippedBoard ) );

        board = BoardProvider.Create( "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1" );
        flippedBoard = FlipBoard( board );
        Assert.IsTrue( EvaluationSystem.EvaluatePosition( ref board ) == EvaluationSystem.EvaluatePosition( ref flippedBoard ) );

        board = BoardProvider.Create( "r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1" );
        flippedBoard = FlipBoard( board );
        Assert.AreEqual( EvaluationSystem.EvaluatePosition( ref board ), EvaluationSystem.EvaluatePosition( ref flippedBoard ) );

        board = BoardProvider.Create( "rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8" );
        flippedBoard = FlipBoard( board );
        Assert.IsTrue( EvaluationSystem.EvaluatePosition( ref board ) == EvaluationSystem.EvaluatePosition( ref flippedBoard ) );

        board = BoardProvider.Create( "r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10" );
        flippedBoard = FlipBoard( board );
        Assert.IsTrue( EvaluationSystem.EvaluatePosition( ref board ) == EvaluationSystem.EvaluatePosition( ref flippedBoard ) );
    }

    private BoardData FlipBoard( BoardData board )
    {
        BoardData result = new();

        foreach (int color in new[] { 0, 1 })
        {
            for (int squareIndex = 0; squareIndex < 64; squareIndex++)
            {
                PieceType type = board.GetPieceOnSquare(squareIndex, color);
                if (type is PieceType.None)
                    continue;
                result.SetPieceOnSquare( type, color ^ 1, squareIndex ^ 56 );
            }
        }

        result.SideToMove ^= 1;

        return result;
    }
}
