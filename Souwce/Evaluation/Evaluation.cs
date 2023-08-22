using System.Runtime.CompilerServices;
using Izumi.Structures;
using Izumi.Structures.Data;

namespace Izumi.EvaluationScripts
{
    internal class Evaluation
    {
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public int EvaluatePosition(Board board)
        {
            (int midgame, int endgame, int phase) = PieceEvaluation(board);
            PieceMobility(board, ref midgame, ref endgame);
            PassedPawnBonus(board, ref midgame, ref endgame);
            DoublePawnsPunishment(board, ref midgame, ref endgame);
            IsolatedPawnsPunishment(board, ref midgame, ref endgame);
            KingEndgameBonus(board, ref endgame);
            return (midgame * (256 - phase) + endgame * phase) / 256 * (board.IsWhiteToMove ? 1 : -1);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private (int, int, int) PieceEvaluation(Board board)
        {
            int totalPhase = 16 * EvaluationConfig.PiecePhase[0] +
                             4 * EvaluationConfig.PiecePhase[1] +
                             4 * EvaluationConfig.PiecePhase[2] +
                             4 * EvaluationConfig.PiecePhase[3] +
                             2 * EvaluationConfig.PiecePhase[4] +
                             2 * EvaluationConfig.PiecePhase[5];
            int midgame = 0;
            int endgame = 0;
            int phase = totalPhase;

            for (int pieceIndex = 0; pieceIndex < 12; pieceIndex++)
            {
                Bitboard bitboard = board.Data[pieceIndex];
                int pieceCount = bitboard.BitCount;
                midgame -= pieceCount * EvaluationConfig.MidgamePieceValues[pieceIndex % 6];
                endgame -= pieceCount * EvaluationConfig.EndgamePieceValues[pieceIndex % 6];
                phase -= pieceCount * EvaluationConfig.PiecePhase[pieceIndex % 6];

                while (bitboard > 0)
                {
                    int squareIndex = bitboard.LsbIndex;
                    bitboard &= bitboard - 1;

                    if (pieceIndex > 5)
                        squareIndex ^= 56;

                    switch ((PieceType)(pieceIndex % 6))
                    {
                        case PieceType.Pawn:
                            midgame -= EvaluationConfig.MidgamePawnTable[squareIndex];
                            endgame -= EvaluationConfig.EndgamePawnTable[squareIndex];
                            break;
                        case PieceType.Knight:
                            midgame -= EvaluationConfig.MidgameKnightTable[squareIndex];
                            endgame -= EvaluationConfig.EndgameKnightTable[squareIndex];
                            break;
                        case PieceType.Bishop:
                            midgame -= EvaluationConfig.MidgameBishopTable[squareIndex];
                            endgame -= EvaluationConfig.EndgameBishopTable[squareIndex];
                            break;
                        case PieceType.Rook:
                            midgame -= EvaluationConfig.MidgameRookTable[squareIndex];
                            endgame -= EvaluationConfig.EndgameRookTable[squareIndex];
                            break;
                        case PieceType.Queen:
                            midgame -= EvaluationConfig.MidgameQueenTable[squareIndex];
                            endgame -= EvaluationConfig.EndgameQueenTable[squareIndex];
                            break;
                        case PieceType.King:
                            midgame -= EvaluationConfig.MidgameKingTable[squareIndex];
                            endgame -= EvaluationConfig.EndgameKingTable[squareIndex];
                            break;
                    }
                }

                if (pieceIndex == 5)
                {
                    midgame = -midgame;
                    endgame = -endgame;
                }
            }

            phase = (phase * 256 + totalPhase / 2) / totalPhase;

            return (midgame, endgame, phase);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void PieceMobility(Board board, ref int midgame, ref int endgame)
        {

        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void PassedPawnBonus(Board board, ref int midgame, ref int endgame)
        {

        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void DoublePawnsPunishment(Board board, ref int midgame, ref int endgame)
        {

        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void IsolatedPawnsPunishment(Board board, ref int midgame, ref int endgame)
        {

        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void KingEndgameBonus(Board board, ref int endgame)
        {

        }

        public void EvaluationTest()
        {
            Console.WriteLine("Stawting (・`w´・) evawuation ^-^ t-t-test...");
            ReadOnlySpan<string> tests = File.ReadLines(@"..\..\..\..\TestData\evaldata.txt").ToArray().AsSpan();

            int index = 0;
            foreach (var test in tests)
            {
                index++;
                Console.WriteLine($"Test {index}/{tests.Length}");
                Board board = new Board(test);
                int result1 = EvaluatePosition(board);
                int result2 = EvaluatePosition(FlipBoard(board));
                if (result1 != result2)
                {
                    Console.WriteLine();
                    Console.WriteLine($"F-F-FEN: {test}");
                    Console.WriteLine($"EXPECTED: {result1}");
                    Console.WriteLine($"WESUWT: {result2}");
                    Console.WriteLine();
                }
            }
            Console.WriteLine($"Donye! ^-^");
        }

        private Board FlipBoard(Board board)
        {
            Board result = new Board();

            foreach (var color in new bool[] { true, false })
            {
                for (int squareIndex = 0; squareIndex < 64; squareIndex++)
                {
                    PieceType type = board.FindPieceTypeOnSquare(squareIndex, color);
                    if (type is PieceType.None)
                        continue;
                    result.Data[(int)type + (color ? 6 : 0)].SetBitToOne(squareIndex ^ 56);
                }
            }

            return result;
        }
    }
}
