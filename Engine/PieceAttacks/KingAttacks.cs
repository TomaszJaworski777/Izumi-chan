using Engine.Data.Bitboards;
using Engine.Utils;
using System.Runtime.CompilerServices;

namespace Engine.PieceAttacks;

public class KingAttacks
{
    private Array64<Bitboard> _attacksTable = default;

    public KingAttacks()
    {
        //generate knight attacks lookup table
        for (int squareIndex = 0; squareIndex < 64; squareIndex++)
        {
            int squareRank = SquareHelpers.SquareIndexToRank(squareIndex);
            int squareFile = SquareHelpers.SquareIndexToFile(squareIndex);

            //if checks to make sure attacks wont loop to different rows or files, cuz whole board is represented as serioes of 64-bit bitboards
            for (int rank = -1; rank < 2; rank++)
            {
                if (squareRank + rank < 0 || squareRank + rank > 7) continue;
                for (int file = -1; file < 2; file++)
                {
                    if ((rank | file) == 0) continue;
                    if (squareFile + file < 0 || squareFile + file > 7) continue;
                    _attacksTable[squareIndex].SetBitToOne( SquareHelpers.CoordsToSqaureIndex( squareRank + rank, squareFile + file ) );
                }
            }
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    //get king attacks from pawn lookup table
    public Bitboard GetAttacksForSquare( int squareIndex ) => _attacksTable[squareIndex];
}
  