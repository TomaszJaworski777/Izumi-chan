using System.Runtime.CompilerServices;

namespace Greg
{
    internal class Evaluation
    {
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public int EvaluatePosition( Board board )
        {
            int result = 0;

            for (int pieceIndex = 0; pieceIndex < 12; pieceIndex++)
            {
                result -= board.Data[pieceIndex].BitCount * EvaluationConfig.PieceValues[pieceIndex % 6];
                if (pieceIndex == 5)
                    result = -result;
            }

            return result * (board.IsWhiteToMove ? 1 : -1);
        }
    }
}
