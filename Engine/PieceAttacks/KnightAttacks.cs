using Engine.Data.Bitboards;
using Engine.Utils;
using System.Runtime.CompilerServices;

namespace Engine.PieceAttacks;

public class KnightAttacks
{
    private Array64<Bitboard> _attackTable = default;

    public KnightAttacks()
    {
        //generate knight attacks lookup table
        for (int squareIndex = 0; squareIndex < 64; squareIndex++)
        {
            int rank = SquareHelpers.SquareIndexToRank(squareIndex);
            int file = SquareHelpers.SquareIndexToFile(squareIndex);

            //if checks to make sure attacks wont loop to different rows or files, cuz whole board is represented as serioes of 64-bit bitboards
            if (file > 0 && rank < 6)
            {
                _attackTable[squareIndex].SetBitToOne( squareIndex + 15 );
            }
            if (file < 7 && rank < 6)
            {
                _attackTable[squareIndex].SetBitToOne( squareIndex + 17 );
            }
            if (file > 1 && rank < 7)
            {
                _attackTable[squareIndex].SetBitToOne( squareIndex + 6 );
            }
            if (file < 6 && rank < 7)
            {
                _attackTable[squareIndex].SetBitToOne( squareIndex + 10 );
            }
            if (file > 0 && rank > 1)
            {
                _attackTable[squareIndex].SetBitToOne( squareIndex - 17 );
            }
            if (file < 7 && rank > 1)
            {
                _attackTable[squareIndex].SetBitToOne( squareIndex - 15 );
            }
            if (file > 1 && rank > 0)
            {
                _attackTable[squareIndex].SetBitToOne( squareIndex - 10 );
            }
            if (file < 6 && rank > 0)
            {
                _attackTable[squareIndex].SetBitToOne( squareIndex - 6 );
            }
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    //get knight attacks from pawn lookup table
    public Bitboard GetAttacksForSquare( int squareIndex ) => _attackTable[squareIndex];
}
