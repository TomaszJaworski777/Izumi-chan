using System.Runtime.CompilerServices;
using Engine.Data.Bitboards;
using Engine.Data.Enums;
using Engine.PieceAttacks;
using Engine.PieceAttacks.SlidingPieces;

namespace Engine.Board
{
    public partial struct BoardData
    {
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        //checks if square is attacked, by applying every piece move from this square and masking it with opponent's pieces. (https://www.chessprogramming.org/Square_Attacked_By)
        public bool IsSquareAttacked(int squareIndex, int squareColor)
        {
            Bitboard attackers = 0;

            // pawn attacks
            attackers |= PieceAttackProvider.GetPawnAttacks(squareIndex, squareColor) & GetPieceBitboard(PieceType.Pawn);

            // knight attacks
            attackers |= PieceAttackProvider.GetKnightAttacks(squareIndex) & GetPieceBitboard(PieceType.Knight);

            // bishop and queen attacks
            attackers |= PieceAttackProvider.GetBishopAttacks(squareIndex, GetAllPieces()) 
                & (GetPieceBitboard(PieceType.Bishop) | GetPieceBitboard(PieceType.Queen));

            // rook and queen attacks
            attackers |= PieceAttackProvider.GetRookAttacks(squareIndex, GetAllPieces())
                & (GetPieceBitboard(PieceType.Rook) | GetPieceBitboard(PieceType.Queen));

            attackers |= PieceAttackProvider.GetKingAttacks(squareIndex) & GetPieceBitboard(PieceType.King);

            // We have calculated attackers to the square for both sides. Now filter
            attackers &= GetPiecesBitboardForSide(squareColor ^ 1);

            return attackers != 0;
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
