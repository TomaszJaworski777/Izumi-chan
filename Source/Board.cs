using System.Runtime.CompilerServices;

namespace Greg
{
    internal struct Board
    {
        public GameBoard Data = default;

        public bool IsWhiteToMove
        {
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            get => Data[16].GetBitValue( GameBoard.SideIndex ) > 0;
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            set => Data[16].SetValueChunk( GameBoard.SideIndex, 1, value ? 1UL : 0UL );
        }
        public ulong HalfMoves
        {
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            get => Data[16].GetValueChunk( GameBoard.HalfMovesIndex, GameBoard.HalfMovesMask );
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            set => Data[16].SetValueChunk( GameBoard.HalfMovesIndex, GameBoard.HalfMovesMask, value );
        }
        public ulong Moves
        {
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            get => Data[16].GetValueChunk( GameBoard.MovesIndex, GameBoard.MovesMask );
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            set => Data[16].SetValueChunk( GameBoard.MovesIndex, GameBoard.MovesMask, value );
        }
        public ulong EnPassantSquareIndex
        {
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            get => Data[16].GetValueChunk( GameBoard.EnPassantIndex, GameBoard.EnPassantMask );
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            set => Data[16].SetValueChunk( GameBoard.EnPassantIndex, GameBoard.EnPassantMask, value );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Board( string fen ) => FenSystem.CreateBoard( ref this, fen );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public unsafe Board( Board other ) => Data = other.Data;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Bitboard GetBitboardForPiece( PieceType type, bool isWhite ) => Data[(int)type + (isWhite ? 0 : 6)];

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void SetPieceOnSquare( PieceType type, bool isWhite, int squareIndex ) => Data[(int)type + (isWhite ? 0 : 6)].SetBitToOne( squareIndex );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void RemovePieceOnSquare( PieceType type, bool isWhite, int squareIndex ) => Data[(int)type + (isWhite ? 0 : 6)].SetBitToZero( squareIndex );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool MakeMove( Move move ) => MoveController.MakeMove( ref this, move );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ReadOnlySpan<Move> GeneratePseudoLegalMoves() => MoveController.GeneratePseudoLegalMoves( this );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ReadOnlySpan<Move> GeneratePseudoLegalPriorityMoves() => MoveController.GeneratePseudoLegalPriorityMoves( this );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool IsKingInCheck( bool isWhite ) => IsSquareAttacked( GetBitboardForPiece( PieceType.King, isWhite ).LsbIndex, isWhite );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool IsSquareAttacked( int squareIndex, bool isSquareWhite ) => PieceAttacks.IsSquareAttacked( squareIndex, isSquareWhite, this );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public PieceType FindPieceTypeOnSquare( int squareIndex, bool forWhite )
        {
            for (int pieceIndex = 0; pieceIndex < 6; pieceIndex++)
            {
                if (Data[pieceIndex + (forWhite ? 0 : 6)].GetBitValue( squareIndex ) > 0)
                    return (PieceType)pieceIndex;
            }
            return PieceType.None;
        }

        public void DrawBoard()
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
                        if (pieces[i] == 0)
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
                        _ => 42,
                    };
                }
            }

            for (int i = 0; i < 64; i++)
            {
                if (i % 8 == 0)
                    Console.WriteLine();

                byte value = pieces[i ^ 56];
                Console.ForegroundColor = value < 'a' ? ConsoleColor.Yellow : ConsoleColor.Blue;
                if (value is 42)
                    Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write( string.Format( "{0, 2}", (char)value ) );
            }
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;

            Console.WriteLine( IsWhiteToMove ? "White to move" : "Black to move" );
            var whiteQueenCastle = Data[16].GetBitValue(0) > 0 ? 'Q' : '-';
            var whiteKingCastle = Data[16].GetBitValue(1) > 0 ? 'K' : '-';
            var blackQueenCastle = Data[16].GetBitValue(2) > 0 ? 'q' : '-';
            var blackKingCastle = Data[16].GetBitValue(3) > 0 ? 'k' : '-';
            Console.WriteLine( $"Castle Rights: {whiteKingCastle}{whiteQueenCastle}{blackKingCastle}{blackQueenCastle}" );
            Console.WriteLine( $"Half moves: {HalfMoves}, Moves: {Moves}" );
            var enPassantSquareText = EnPassantSquareIndex is 0 ? "-" : new Square((int)EnPassantSquareIndex).ToString();
            Console.WriteLine( $"En Passant: {enPassantSquareText}" );
            Console.WriteLine( $"White king in check: {IsKingInCheck(true)}" );
            Console.WriteLine( $"Black king in check: {IsKingInCheck(false)}" );
#endif
        }
    }

    [InlineArray( 17 )]
    internal struct GameBoard
    {
        public const ulong HalfMovesMask = 127;
        public const ulong MovesMask = 1023;
        public const ulong EnPassantMask = 63;

        public const int SideIndex = 4;
        public const int HalfMovesIndex = 5;
        public const int MovesIndex = 12;
        public const int EnPassantIndex = 22;

        private Bitboard _value;
        /* 0 - 5 ==> white piece tables
         * 6 - 11 ==> black piece tables
         * 12 ==> white pieces table (all pieces on one bitboard)
         * 13 ==> black pieces table (all pieces on one bitboard)
         * 14 ==> all pieces table (white |= black)
         * 15 ==> 
         * 16 ==> misc data
         *      Reserved bits in misc from LSB (Total 27/64 reserved):
         *          4 bits - castle {bk}{bq}[wK][wQ] 3 2 1 0
         *          1 bit - side to move {0 - white to move}
         *          7 bits - half moves 
         *          10 bits - moves
         *          6 bits - en passant
         */
    }
}