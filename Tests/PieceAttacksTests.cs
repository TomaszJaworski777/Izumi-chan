using Engine.Board;
using Engine.PieceAttacks;
using Engine.PieceAttacks.SlidingPieces;
using Engine.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests;

[TestClass]
public class PieceAttacksTests
{
    [TestMethod]
    public void PawnAttacks_Test()
    {
        PawnAttacks pawnAttacks = new();
        Assert.IsTrue( pawnAttacks.GetAttacksForSquare( 60, 0 ) == 0 );
        Assert.IsTrue( pawnAttacks.GetAttacksForSquare( 7, 1 ) == 0 );
        Assert.IsTrue( pawnAttacks.GetAttacksForSquare( 0, 0 ).GetBitValue( SquareHelpers.StringToSquareIndex( "b2" ) ) > 0 );
        Assert.IsTrue( pawnAttacks.GetAttacksForSquare( 0, 0 ).GetBitValue( SquareHelpers.StringToSquareIndex( "h2" ) ) == 0 );

        Assert.IsTrue( pawnAttacks.GetAttacksForSquare( SquareHelpers.StringToSquareIndex( "e4" ), 0 ).GetBitValue( SquareHelpers.StringToSquareIndex( "d5" ) ) > 0 );
        Assert.IsTrue( pawnAttacks.GetAttacksForSquare( SquareHelpers.StringToSquareIndex( "e4" ), 0 ).GetBitValue( SquareHelpers.StringToSquareIndex( "f5" ) ) > 0 );
        Assert.IsTrue( pawnAttacks.GetAttacksForSquare( SquareHelpers.StringToSquareIndex( "e4" ), 0 ).GetBitValue( SquareHelpers.StringToSquareIndex( "e5" ) ) == 0 );
        Assert.IsTrue( pawnAttacks.GetAttacksForSquare( SquareHelpers.StringToSquareIndex( "e4" ), 0 ).GetBitValue( SquareHelpers.StringToSquareIndex( "d3" ) ) == 0 );
        Assert.IsTrue( pawnAttacks.GetAttacksForSquare( SquareHelpers.StringToSquareIndex( "e4" ), 0 ).GetBitValue( SquareHelpers.StringToSquareIndex( "f3" ) ) == 0 );
        Assert.IsTrue( pawnAttacks.GetAttacksForSquare( SquareHelpers.StringToSquareIndex( "e4" ), 0 ).GetBitValue( SquareHelpers.StringToSquareIndex( "e3" ) ) == 0 );

        Assert.IsTrue( pawnAttacks.GetAttacksForSquare( SquareHelpers.StringToSquareIndex( "e4" ), 1 ).GetBitValue( SquareHelpers.StringToSquareIndex( "d5" ) ) == 0 );
        Assert.IsTrue( pawnAttacks.GetAttacksForSquare( SquareHelpers.StringToSquareIndex( "e4" ), 1 ).GetBitValue( SquareHelpers.StringToSquareIndex( "f5" ) ) == 0 );
        Assert.IsTrue( pawnAttacks.GetAttacksForSquare( SquareHelpers.StringToSquareIndex( "e4" ), 1 ).GetBitValue( SquareHelpers.StringToSquareIndex( "e5" ) ) == 0 );
        Assert.IsTrue( pawnAttacks.GetAttacksForSquare( SquareHelpers.StringToSquareIndex( "e4" ), 1 ).GetBitValue( SquareHelpers.StringToSquareIndex( "d3" ) ) > 0 );
        Assert.IsTrue( pawnAttacks.GetAttacksForSquare( SquareHelpers.StringToSquareIndex( "e4" ), 1 ).GetBitValue( SquareHelpers.StringToSquareIndex( "f3" ) ) > 0 );
        Assert.IsTrue( pawnAttacks.GetAttacksForSquare( SquareHelpers.StringToSquareIndex( "e4" ), 1 ).GetBitValue( SquareHelpers.StringToSquareIndex( "e3" ) ) == 0 );
    }

    [TestMethod]
    public void KnightAttacks_Test()
    {
        KnightAttacks knightAttacks = new();
        int squareIndex = SquareHelpers.StringToSquareIndex( "g4" );

        Assert.IsTrue( knightAttacks.GetAttacksForSquare( squareIndex ).GetBitValue( SquareHelpers.StringToSquareIndex( "h2" ) ) > 0 );
        Assert.IsTrue( knightAttacks.GetAttacksForSquare( squareIndex ).GetBitValue( SquareHelpers.StringToSquareIndex( "f2" ) ) > 0 );
        Assert.IsTrue( knightAttacks.GetAttacksForSquare( squareIndex ).GetBitValue( SquareHelpers.StringToSquareIndex( "h6" ) ) > 0 );
        Assert.IsTrue( knightAttacks.GetAttacksForSquare( squareIndex ).GetBitValue( SquareHelpers.StringToSquareIndex( "f6" ) ) > 0 );
        Assert.IsTrue( knightAttacks.GetAttacksForSquare( squareIndex ).GetBitValue( SquareHelpers.StringToSquareIndex( "e5" ) ) > 0 );
        Assert.IsTrue( knightAttacks.GetAttacksForSquare( squareIndex ).GetBitValue( SquareHelpers.StringToSquareIndex( "e3" ) ) > 0 );
        Assert.IsTrue( knightAttacks.GetAttacksForSquare( squareIndex ).GetBitValue( SquareHelpers.StringToSquareIndex( "a5" ) ) == 0 );
        Assert.IsTrue( knightAttacks.GetAttacksForSquare( squareIndex ).GetBitValue( SquareHelpers.StringToSquareIndex( "a3" ) ) == 0 );
        Assert.IsTrue( knightAttacks.GetAttacksForSquare( squareIndex ).GetBitValue( SquareHelpers.StringToSquareIndex( "g2" ) ) == 0 );
        Assert.IsTrue( knightAttacks.GetAttacksForSquare( squareIndex ).GetBitValue( SquareHelpers.StringToSquareIndex( "f3" ) ) == 0 );
        Assert.IsTrue( knightAttacks.GetAttacksForSquare( squareIndex ).GetBitValue( SquareHelpers.StringToSquareIndex( "g6" ) ) == 0 );
        Assert.IsTrue( knightAttacks.GetAttacksForSquare( squareIndex ).GetBitValue( SquareHelpers.StringToSquareIndex( "f5" ) ) == 0 );
    }

    [TestMethod]
    public void BishopAttacks_Test()
    {
        MagicNumbers magicNumbers = new();
        BishopAttacks bishopAttacks = new(magicNumbers);
        ulong blocker = BoardProvider.Create(BoardProvider.KiwipetePosition).GetPiecesBitboardForSide(0);

        Assert.IsTrue( bishopAttacks.GetAttacksForSquare( SquareHelpers.StringToSquareIndex( "e4" ), blocker ) == 36099337564455936 );
        Assert.IsTrue( bishopAttacks.GetAttacksForSquare( SquareHelpers.StringToSquareIndex( "a4" ), blocker ) == 1155177711057110016 );
        Assert.IsTrue( bishopAttacks.GetAttacksForSquare( SquareHelpers.StringToSquareIndex( "h8" ), blocker ) == 18049651601047552 );
        Assert.IsTrue( bishopAttacks.GetAttacksForSquare( SquareHelpers.StringToSquareIndex( "c2" ), blocker ) == 285868042 );
        Assert.IsTrue( bishopAttacks.GetAttacksForSquare( SquareHelpers.StringToSquareIndex( "g6" ), blocker ) == 1197958188343754752 );
    }

    [TestMethod]
    public void RookAttacks_Test()
    {
        MagicNumbers magicNumbers = new();
        RookAttacks rookAttacks = new(magicNumbers);
        ulong blocker = BoardProvider.Create(BoardProvider.KiwipetePosition).GetPiecesBitboardForSide(0);

        Assert.IsTrue( rookAttacks.GetAttacksForSquare( SquareHelpers.StringToSquareIndex( "e4" ), blocker ) == 72730284032 );
        Assert.IsTrue( rookAttacks.GetAttacksForSquare( SquareHelpers.StringToSquareIndex( "a4" ), blocker ) == 72340173324615936 );
        Assert.IsTrue( rookAttacks.GetAttacksForSquare( SquareHelpers.StringToSquareIndex( "h8" ), blocker ) == 9187484529235886080 );
        Assert.IsTrue( rookAttacks.GetAttacksForSquare( SquareHelpers.StringToSquareIndex( "c2" ), blocker ) == 264708 );
        Assert.IsTrue( rookAttacks.GetAttacksForSquare( SquareHelpers.StringToSquareIndex( "g6" ), blocker ) == 4629910699613634560 );
    }

    [TestMethod]
    public void KingtAttacks_Test()
    {
        KingAttacks kingAttacks = new();
        int squareIndex = SquareHelpers.StringToSquareIndex( "h4" );

        Assert.IsTrue( kingAttacks.GetAttacksForSquare( squareIndex ).GetBitValue( SquareHelpers.StringToSquareIndex( "h3" ) ) > 0 );
        Assert.IsTrue( kingAttacks.GetAttacksForSquare( squareIndex ).GetBitValue( SquareHelpers.StringToSquareIndex( "h5" ) ) > 0 );
        Assert.IsTrue( kingAttacks.GetAttacksForSquare( squareIndex ).GetBitValue( SquareHelpers.StringToSquareIndex( "g3" ) ) > 0 );
        Assert.IsTrue( kingAttacks.GetAttacksForSquare( squareIndex ).GetBitValue( SquareHelpers.StringToSquareIndex( "g4" ) ) > 0 );
        Assert.IsTrue( kingAttacks.GetAttacksForSquare( squareIndex ).GetBitValue( SquareHelpers.StringToSquareIndex( "g5" ) ) > 0 );
        Assert.IsTrue( kingAttacks.GetAttacksForSquare( squareIndex ).GetBitValue( SquareHelpers.StringToSquareIndex( "a3" ) ) == 0 );
        Assert.IsTrue( kingAttacks.GetAttacksForSquare( squareIndex ).GetBitValue( SquareHelpers.StringToSquareIndex( "a4" ) ) == 0 );
        Assert.IsTrue( kingAttacks.GetAttacksForSquare( squareIndex ).GetBitValue( SquareHelpers.StringToSquareIndex( "a5" ) ) == 0 );
    }

    [TestMethod]
    public void IsSquareAttacked_Test()
    {
        BoardData board = BoardProvider.Create(BoardProvider.KiwipetePosition);

        Assert.IsTrue( board.IsSquareAttacked( SquareHelpers.StringToSquareIndex( "d5" ), 0 ) );
        Assert.IsTrue( board.IsSquareAttacked( SquareHelpers.StringToSquareIndex( "b5" ), 0 ) );
        Assert.IsTrue( board.IsSquareAttacked( SquareHelpers.StringToSquareIndex( "c4" ), 0 ) );
        Assert.IsTrue( board.IsSquareAttacked( SquareHelpers.StringToSquareIndex( "a4" ), 0 ) );

        Assert.IsTrue( board.IsSquareAttacked( SquareHelpers.StringToSquareIndex( "d7" ), 1 ) );
        Assert.IsTrue( board.IsSquareAttacked( SquareHelpers.StringToSquareIndex( "b5" ), 1 ) );
        Assert.IsTrue( board.IsSquareAttacked( SquareHelpers.StringToSquareIndex( "f6" ), 1 ) );
        Assert.IsTrue( board.IsSquareAttacked( SquareHelpers.StringToSquareIndex( "f4" ), 1 ) );

        Assert.IsTrue( !board.IsSquareAttacked( SquareHelpers.StringToSquareIndex( "b3" ), 0 ) );
        Assert.IsTrue( !board.IsSquareAttacked( SquareHelpers.StringToSquareIndex( "e5" ), 0 ) );

        Assert.IsTrue( !board.IsSquareAttacked( SquareHelpers.StringToSquareIndex( "h4" ), 1 ) );
        Assert.IsTrue( !board.IsSquareAttacked( SquareHelpers.StringToSquareIndex( "d6" ), 1 ) );
    }
}