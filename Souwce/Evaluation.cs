using System.Runtime.CompilerServices;

namespace Izumi
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

        public void EvaluationTest()
        {
            Console.WriteLine( "Stawting (・`w´・) evawuation ^-^ t-t-test..." );
            ReadOnlySpan<string> tests =  File.ReadLines( @"..\..\..\..\TestData\evaldata.txt" ).ToArray().AsSpan();

            int index = 0;
            foreach (var test in tests)
            {
                index++;
                Console.WriteLine( $"Test {index}/{tests.Length}" );
                Board board = new Board(test);
                int result1 = EvaluatePosition(board);
                int result2 = EvaluatePosition(FlipBoard(board));
                if (result1 != result2)
                {
                    Console.WriteLine();
                    Console.WriteLine( $"F-F-FEN: {test}" );
                    Console.WriteLine( $"EXPECTED: {result1}" );
                    Console.WriteLine( $"WESUWT: {result2}" );
                    Console.WriteLine();
                }
            }
            Console.WriteLine( $"Donye! ^-^" );
        }

        private Board FlipBoard( Board board )
        {
            Board result = new Board();

            foreach (var color in new bool[] { true, false })
            {
                for (int squareIndex = 0; squareIndex < 64; squareIndex++)
                {
                    PieceType type = board.FindPieceTypeOnSquare(squareIndex, color);
                    if (type is PieceType.None)
                        continue;
                    result.Data[(int)type + (color ? 6 : 0)].SetBitToOne( squareIndex ^ 56 );
                }
            }

            return result;
        }
    }
}
