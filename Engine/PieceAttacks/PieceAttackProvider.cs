using System.Runtime.CompilerServices;
using Engine.Data.Bitboards;
using Engine.Data.Enums;
using Engine.PieceAttacks;
using Engine.PieceAttacks.SlidingPieces;

namespace Engine.Board
{
    public partial struct BoardData
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //checks if square is attacked, by applying every piece move from this square and masking it with opponent's pieces. (https://www.chessprogramming.org/Square_Attacked_By)
        public bool IsSquareAttacked( int squareIndex, int squareColor )
        {
            Bitboard colorPawnAttack = PieceAttackProvider.GetPawnAttacks(squareIndex, squareColor);
            Bitboard attackerPieces = GetPieceBitboard(PieceType.Pawn, squareColor ^ 1);

            //pawn attacks
            if ((colorPawnAttack & attackerPieces) > 0)
                return true;

            //knight attacks
            attackerPieces = GetPieceBitboard( PieceType.Knight, squareColor ^ 1 );
            if ((PieceAttackProvider.GetKnightAttacks( squareIndex ) & attackerPieces) > 0)
                return true;

            //bishop attacks
            attackerPieces = GetPieceBitboard( PieceType.Bishop, squareColor ^ 1 );
            Bitboard bishopAttacks = PieceAttackProvider.GetBishopAttacks( squareIndex, GetPiecesBitboardForSide(0) | GetPiecesBitboardForSide(1) );
            if ((bishopAttacks & attackerPieces) > 0)
                return true;

            //rook attacks
            attackerPieces = GetPieceBitboard( PieceType.Rook, squareColor ^ 1 );
            Bitboard rookAttacks = PieceAttackProvider.GetRookAttacks( squareIndex, GetPiecesBitboardForSide(0) | GetPiecesBitboardForSide(1) );
            if ((rookAttacks & attackerPieces) > 0)
                return true;

            //queen attacks, created on the fly based on bishop and rook results to increase performance
            attackerPieces = GetPieceBitboard( PieceType.Queen, squareColor ^ 1 );
            if (((bishopAttacks | rookAttacks) & attackerPieces) > 0)
                return true;

            //king attacks
            attackerPieces = GetPieceBitboard( PieceType.King, squareColor ^ 1 );
            return (PieceAttackProvider.GetKingAttacks( squareIndex ) & attackerPieces) > 0;
        }
    }
}

namespace Engine.PieceAttacks
{
    public static class PieceAttackProvider
    {
        private static readonly PawnAttacks _pawnAttacks = new();
        private static readonly KnightAttacks _knightAttacks = new();
        private static readonly KingAttacks _kingAttacks = new();
        private static readonly BishopAttacks _bishopAttacks = new();
        private static readonly RookAttacks _rookAttacks = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard GetPawnAttacks( int squareIndex, int color ) => _pawnAttacks.GetAttacksForSquare( squareIndex, color );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Bitboard GetKnightAttacks( int squareIndex ) => _knightAttacks.GetAttacksForSquare( squareIndex );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Bitboard GetBishopAttacks( int squareIndex, ulong blocker ) => _bishopAttacks.GetAttacksForSquare( squareIndex, blocker );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Bitboard GetRookAttacks( int squareIndex, ulong blocker ) => _rookAttacks.GetAttacksForSquare( squareIndex, blocker );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Bitboard GetQueenAttacks( int squareIndex, ulong blocker ) => 
            _bishopAttacks.GetAttacksForSquare( squareIndex, blocker ) | _rookAttacks.GetAttacksForSquare( squareIndex, blocker );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Bitboard GetKingAttacks( int squareIndex ) => _kingAttacks.GetAttacksForSquare( squareIndex );
    }
}
