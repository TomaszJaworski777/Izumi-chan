using System.Runtime.CompilerServices;

namespace Greg
{
    internal struct Board
    {
        public GameBoard Data = default;

        public Board(string fen) //low impact on overall performance
        {
            string[] fenParts = fen.Split(' ');
            string[] positionSegments = fenParts[0].Split('/');

            for (int rank = 0; rank < positionSegments.Length; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    int squareIndex = (7 - rank) * 8 + file;
                    char currentCharacter = positionSegments[rank][file];

                    if (int.TryParse(currentCharacter.ToString(), out int output))
                    {
                        file += output - 1;
                        continue;
                    }

                    bool isWhite = currentCharacter < 'a';

                    switch (currentCharacter)
                    {
                        case 'p':
                        case 'P':
                            SetPieceOnSquare(PieceType.Pawn, isWhite, squareIndex);
                            break;
                        case 'n':
                        case 'N':
                            SetPieceOnSquare(PieceType.Knight, isWhite, squareIndex);
                            break;
                        case 'b':
                        case 'B':
                            SetPieceOnSquare(PieceType.Bishop, isWhite, squareIndex);
                            break;
                        case 'r':
                        case 'R':
                            SetPieceOnSquare(PieceType.Rook, isWhite, squareIndex);
                            break;
                        case 'q':
                        case 'Q':
                            SetPieceOnSquare(PieceType.Queen, isWhite, squareIndex);
                            break;
                        case 'k':
                        case 'K':
                            SetPieceOnSquare(PieceType.King, isWhite, squareIndex);
                            break;
                    }
                }
            }

            DrawBoard();
        }

        public unsafe Board(Board other)
        {
            Data = other.Data;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Bitboard GetBitboardForPiece(PieceType type, bool isWhite) => Data[(int)type + (isWhite ? 0 : 6)];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPieceOnSquare(PieceType type, bool isWhite, int squareIndex) => Data[(int)type + (isWhite ? 0 : 6)].SetBitToOne(squareIndex);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemovePieceOnSquare(PieceType type, bool isWhite, int squareIndex) => Data[(int)type + (isWhite ? 0 : 6)].SetBitToZero(squareIndex);

        public void DrawBoard() //low impact on overall performance
        {
#if DEBUG
            Array64<byte> pieces = new();
            for (int pieceIndex = 0; pieceIndex < 12; pieceIndex++)
            {
                PieceType pieceType = (PieceType)(pieceIndex > 5 ? pieceIndex - 6 : pieceIndex);
                for (int i = 0; i < 64; i++)
                {
                    if (Data[pieceIndex].GetBitValue( i ) == 0)
                    {
                        if(pieces[i] == 0)
                            pieces[i] = 42;
                        continue;
                    }
                    pieces[i] = pieceType switch
                    {
                        PieceType.Pawn => pieceIndex > 5 ? (byte)'p' : (byte)'P',
                        PieceType.Knight => pieceIndex > 5 ? (byte)'n' : (byte)'N',
                        PieceType.Bishop => pieceIndex > 5 ? (byte)'b' : (byte)'B',
                        PieceType.Rook => pieceIndex > 5 ? (byte)'r' : (byte)'R',
                        PieceType.Queen => pieceIndex > 5 ? (byte)'q' : (byte)'Q',
                        PieceType.King => pieceIndex > 5 ? (byte)'k' : (byte)'K',
                    };
                }
            }

            for (int i = 0; i < 64; i++)
            {
                if (i % 8 == 0)
                    Console.WriteLine();

                byte value = pieces[i ^ 56];
                Console.ForegroundColor = value < 'a' ? ConsoleColor.Yellow : ConsoleColor.Blue;
                if(value is 42)
                    Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write( string.Format( "{0, 2}", (char)value ) );
            }
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;
#endif
        }
    }

    [InlineArray(16)]
    internal struct GameBoard
    {
        private Bitboard _value;
        /* 0 - 5 ==> white piece tables
         * 6 - 11 ==> black piece tables
         * 12 ==> white pieces table (all pieces on one bitboard)
         * 13 ==> black pieces table (all pieces on one bitboard)
         * 14 ==> white attack table
         * 15 ==> black attack table
         * 16 ==> misc data
         *      Reserved bits in misc from right to left (Total 21/64 reserved):
         *          4 bits - castle {bk}{bq}[wK][kQ]
         *          1 bit - side to move {0 - white to move}
         *          7 bits - half moves
         *          9 bits - moves
         */
    }
}
