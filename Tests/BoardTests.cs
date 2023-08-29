using Engine.Board;
using Engine.Data.Enums;
using Engine.Move;
using Engine.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class BoardTests
    {
        [TestMethod]
        public void SetPieceOnSquare_Test()
        {
            BoardData board = new();
            Assert.IsTrue( board.GetPieceBitboard( PieceType.Queen, 0 ) == 0 );
            Assert.IsTrue( board.GetPieceOnSquare( 7, 0 ) == PieceType.None );
            board.SetPieceOnSquare( PieceType.Queen, 0, 7 );
            Assert.IsTrue( board.GetPieceBitboard( PieceType.Queen, 0 ) > 0 );
            Assert.IsTrue( board.GetPieceOnSquare( 7, 0 ) == PieceType.Queen );
            Assert.IsTrue( board.GetPieceBitboard( PieceType.Queen, 1 ) == 0 );
            Assert.IsTrue( board.GetPieceOnSquare( 8, 0 ) == PieceType.None );
        }

        [TestMethod]
        public void RemovePieceOnSquare_Test()
        {
            BoardData board = new();
            board.SetPieceOnSquare( PieceType.Queen, 0, 7 );
            board.SetPieceOnSquare( PieceType.Queen, 0, 14 );
            board.SetPieceOnSquare( PieceType.Queen, 0, 21 );
            board.SetPieceOnSquare( PieceType.Queen, 0, 28 );
            board.SetPieceOnSquare( PieceType.Queen, 0, 35 );
            Assert.IsTrue( board.GetPieceOnSquare( 7, 0 ) == PieceType.Queen );
            Assert.IsTrue( board.GetPieceOnSquare( 14, 0 ) == PieceType.Queen );
            Assert.IsTrue( board.GetPieceOnSquare( 21, 0 ) == PieceType.Queen );
            Assert.IsTrue( board.GetPieceOnSquare( 28, 0 ) == PieceType.Queen );
            Assert.IsTrue( board.GetPieceOnSquare( 35, 0 ) == PieceType.Queen );
            board.RemovePieceOnSquare( PieceType.Queen, 0, 21 );
            Assert.IsTrue( board.GetPieceOnSquare( 21, 0 ) == PieceType.None );
            Assert.IsTrue( board.GetPieceOnSquare( 21, 1 ) == PieceType.None );
            Assert.IsTrue( board.GetPieceOnSquare( 20, 0 ) == PieceType.None );
            Assert.IsTrue( board.GetPieceOnSquare( 22, 0 ) == PieceType.None );
        }

        [TestMethod]
        public void CreateBoard_Test()
        {
            BoardData board = BoardProvider.Create(BoardProvider.StartPosition);
            Assert.IsTrue( board.SideToMove == 0 );
            Assert.IsTrue( board.CanWhiteCastleQueenSide > 0 );
            Assert.IsTrue( board.GetPieceOnSquare(SquareHelpers.StringToSquareIndex("e1"), 0) == PieceType.King );
            Assert.IsTrue( board.GetPieceOnSquare(SquareHelpers.StringToSquareIndex("e1"), 1) == PieceType.None );
            Assert.IsTrue( board.GetPieceOnSquare(SquareHelpers.StringToSquareIndex("e3"), 1) == PieceType.None );
            Assert.IsTrue( board.GetPieceOnSquare(SquareHelpers.StringToSquareIndex("e8"), 1) == PieceType.King );
            Assert.IsTrue( board.GetPieceOnSquare(SquareHelpers.StringToSquareIndex("e2"), 1) == PieceType.None );
            Assert.IsTrue( board.GetPieceOnSquare(SquareHelpers.StringToSquareIndex("e2"), 0) == PieceType.Pawn );
            Assert.IsTrue( board.GetPieceOnSquare(SquareHelpers.StringToSquareIndex("a1"), 0) == PieceType.Rook );
            Assert.IsTrue( board.GetPieceOnSquare(SquareHelpers.StringToSquareIndex("g8"), 1) == PieceType.Knight );

            board = BoardProvider.Create(BoardProvider.KiwipetePosition);
            Assert.IsTrue( board.GetPieceOnSquare( SquareHelpers.StringToSquareIndex( "b4" ), 1 ) == PieceType.Pawn );
            Assert.IsTrue( board.GetPieceOnSquare( SquareHelpers.StringToSquareIndex( "d5" ), 0 ) == PieceType.Pawn );
        }

        [TestMethod]
        public void ZobristKey_Test()
        {
            BoardData board = BoardProvider.Create(BoardProvider.StartPosition);
            ulong zobrist = board.ZobristKey;
            board.MakeMove( new MoveData( "g1f3", board ) );
            Assert.IsTrue(board.ZobristKey != zobrist);
            board.MakeMove( new MoveData( "g8f6", board ) );
            board.MakeMove( new MoveData( "f3g1", board ) );
            board.MakeMove( new MoveData( "f6g8", board ) );
            Assert.IsTrue( board.ZobristKey == zobrist );
        }

        [TestMethod]
        public void Repetition_Test()
        {
            BoardData board = BoardProvider.Create(BoardProvider.StartPosition);
            Assert.IsTrue( !MoveHistory.IsRepetition(board.ZobristKey) );
            board.MakeMove( new MoveData( "g1f3", board ) );
            Assert.IsTrue( !MoveHistory.IsRepetition(board.ZobristKey) );
            board.MakeMove( new MoveData( "g8f6", board ) );
            board.MakeMove( new MoveData( "f3g1", board ) );
            board.MakeMove( new MoveData( "f6g8", board ) );
            Assert.IsTrue( MoveHistory.IsRepetition(board.ZobristKey) );

            board.MakeMove( new MoveData( "e2e4", board ) );
            Assert.IsTrue( !MoveHistory.IsRepetition( board.ZobristKey ) );
            board.MakeMove( new MoveData( "e7e5", board ) );
            board.MakeMove( new MoveData( "e4e2", board ) );
            board.MakeMove( new MoveData( "e5e7", board ) );
            Assert.IsTrue( !MoveHistory.IsRepetition( board.ZobristKey ) );
        }
    }
}