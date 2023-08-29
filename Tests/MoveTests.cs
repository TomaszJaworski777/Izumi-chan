using Engine.Board;
using Engine.Data.Enums;
using Engine.Move;
using Engine.Perft;
using Engine.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests;

[TestClass]
public class MoveTests
{
    [TestMethod]
    public void CreateMove_Test()
    {
        BoardData board = BoardProvider.Create(BoardProvider.KiwipetePosition);
        MoveData move = new MoveData("d5e6", board);
        Assert.IsTrue( move.IsCapture );
        Assert.IsTrue( move.MovingPieceType == PieceType.Pawn );
        Assert.IsTrue( move.TargetPieceType == PieceType.Pawn );
        Assert.IsTrue( !move.IsEnPassant );
        Assert.IsTrue( !move.IsCastle );
        Assert.IsTrue( !move.IsPromotion );

        move = new MoveData( "f3d3", board );
        Assert.IsTrue( !move.IsCapture );
        Assert.IsTrue( move.MovingPieceType == PieceType.Queen );
        Assert.IsTrue( move.TargetPieceType == PieceType.None );
        Assert.IsTrue( !move.IsEnPassant );
        Assert.IsTrue( !move.IsCastle );
        Assert.IsTrue( !move.IsPromotion );

        board.SideToMove ^= 1;
        move = new MoveData( "b4c3", board );
        Assert.IsTrue( move.IsCapture );
        Assert.IsTrue( move.MovingPieceType == PieceType.Pawn );
        Assert.IsTrue( move.TargetPieceType == PieceType.Knight );
        Assert.IsTrue( !move.IsEnPassant );
        Assert.IsTrue( !move.IsCastle );
        Assert.IsTrue( !move.IsPromotion );

        move = new MoveData( "b4b1q", board );
        Assert.IsTrue( !move.IsCapture );
        Assert.IsTrue( move.MovingPieceType == PieceType.Pawn );
        Assert.IsTrue( move.TargetPieceType == PieceType.None );
        Assert.IsTrue( move.PromotionPieceType == PieceType.Queen );
        Assert.IsTrue( !move.IsEnPassant );
        Assert.IsTrue( !move.IsCastle );
        Assert.IsTrue( move.IsPromotion );

        move = new MoveData( "e8g8", board );
        Assert.IsTrue( !move.IsCapture );
        Assert.IsTrue( move.MovingPieceType == PieceType.King );
        Assert.IsTrue( move.TargetPieceType == PieceType.None );
        Assert.IsTrue( !move.IsEnPassant );
        Assert.IsTrue( move.IsCastle );
        Assert.IsTrue( !move.IsPromotion );
    }

    [TestMethod]
    public void MakeMove_Test()
    {
        BoardData board = BoardProvider.Create( BoardProvider.KiwipetePosition );
        MoveData move = new MoveData( "f3h3", board );
        board.MakeMove( move );
        Assert.IsTrue( board.SideToMove > 0 );
        Assert.IsTrue( board.GetPieceOnSquare(SquareHelpers.StringToSquareIndex("h3")) == PieceType.Queen );
        Assert.IsTrue( board.GetPieceOnSquare(SquareHelpers.StringToSquareIndex("f3")) == PieceType.None );

        board = BoardProvider.Create( BoardProvider.KiwipetePosition );
        move = new MoveData( "g2g4", board );
        board.MakeMove( move );
        Assert.IsTrue( board.SideToMove > 0 );
        Assert.IsTrue( board.GetPieceOnSquare( SquareHelpers.StringToSquareIndex( "g4" ) ) == PieceType.Pawn );
        Assert.IsTrue( board.GetPieceOnSquare( SquareHelpers.StringToSquareIndex( "g2" ) ) == PieceType.None );
        Assert.IsTrue( board.EnPassantSquareIndex == SquareHelpers.StringToSquareIndex( "g3" ) );

        board = BoardProvider.Create( BoardProvider.KiwipetePosition );
        move = new MoveData( "e1g1", board );
        board.MakeMove( move );
        Assert.IsTrue( board.SideToMove > 0 );
        Assert.IsTrue( board.GetPieceOnSquare( SquareHelpers.StringToSquareIndex( "g1" ) ) == PieceType.King );
        Assert.IsTrue( board.GetPieceOnSquare( SquareHelpers.StringToSquareIndex( "e1" ) ) == PieceType.None );
        Assert.IsTrue( board.GetPieceOnSquare( SquareHelpers.StringToSquareIndex( "f1" ) ) == PieceType.Rook );
        Assert.IsTrue( board.GetPieceOnSquare( SquareHelpers.StringToSquareIndex( "h1" ) ) == PieceType.None );
    }

    [TestMethod]
    public void Perft_Test()
    {
        BoardData board = BoardProvider.Create( BoardProvider.StartPosition );
        PerftTest test = new ( board );
        Assert.IsTrue( test.TestPosition( 4, false ) == 197_281 );

        board = BoardProvider.Create( BoardProvider.KiwipetePosition );
        test = new ( board );
        Assert.IsTrue( test.TestPosition( 3, false ) == 97_862 );

        board = BoardProvider.Create( "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1" );
        test = new( board );
        Assert.IsTrue( test.TestPosition( 5, false ) == 674_624 );

        board = BoardProvider.Create( "r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1" );
        test = new( board );
        Assert.IsTrue( test.TestPosition( 4, false ) == 422_333 );

        board = BoardProvider.Create( "rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8" );
        test = new( board );
        Assert.IsTrue( test.TestPosition( 3, false ) == 62_379 );

        board = BoardProvider.Create( "r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10" );
        test = new( board );
        Assert.IsTrue( test.TestPosition( 3, false ) == 89_890 );
    }
}
