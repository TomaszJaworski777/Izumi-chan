using Engine.Data.Bitboards;
using Engine.Utils;
using System.Runtime.CompilerServices;

namespace Engine.PieceAttacks
{
    public class PawnAttacks
    {
        private Array64<Bitboard> _whiteAttackTable = default;
        private Array64<Bitboard> _blackAttackTable = default;

        public PawnAttacks()
        {
            //generate pawn attacks lookup table
            for (int squareIndex = 0; squareIndex < 64; squareIndex++)
            {
                int rank = SquareHelpers.SquareIndexToRank(squareIndex);
                int file = SquareHelpers.SquareIndexToFile(squareIndex);

                //if checks to make sure attacks wont loop to different rows or files, cuz whole board is represented as serioes of 64-bit bitboards
                if (file > 0 && rank < 7)
                    _whiteAttackTable[squareIndex].SetBitToOne( squareIndex + 7 );
                if (file < 7 && rank < 7)
                    _whiteAttackTable[squareIndex].SetBitToOne( squareIndex + 9 );
                if (file < 7 && rank > 0)
                    _blackAttackTable[squareIndex].SetBitToOne( squareIndex - 7 );
                if (file > 0 && rank > 0)
                    _blackAttackTable[squareIndex].SetBitToOne( squareIndex - 9 );
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //get pawn attacks from pawn lookup table
        public Bitboard GetAttacksForSquare(int squareIndex, int color ) => color == 0 ? _whiteAttackTable[squareIndex] : _blackAttackTable[squareIndex];
    }
}