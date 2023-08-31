namespace Engine.Board
{
    public ref partial struct BoardData
    {
        //draws board if build is in Debug Mode
        public void Draw()
        {
#if DEBUG
            //gathers board data
            Span<byte> pieces = new byte[64];
            for (int pieceIndex = 0; pieceIndex < 12; pieceIndex++)
            {
                PieceType pieceType = (PieceType)(pieceIndex % 6);
                for (int squareIndex = 0; squareIndex < 64; squareIndex++)
                {
                    if (GetPieceBitboard( pieceIndex % 6, pieceIndex / 6 ).GetBitValue( squareIndex ) == 0)
                    {
                        if (pieces[squareIndex] == 0)
                            pieces[squareIndex] = 42;
                        continue;
                    }
                    pieces[squareIndex] = pieceType switch
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

            //draws board from gathered data
            for (int i = 0; i < 64; i++)
            {
                if (i % 8 == 0)
                    Console.WriteLine();

                byte value = pieces[i ^ 56];
                Console.ForegroundColor = value < 'a' ? ConsoleColor.Yellow : ConsoleColor.Blue;
                if (value is 42)
                    Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("{0, 2}", (char)value);
            }
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;

            Console.WriteLine( SideToMove == 0 ? "White to move" : "Black to move" );
            var whiteQueenCastle = CanWhiteCastleQueenSide > 0 ? 'Q' : '-';
            var whiteKingCastle = CanWhiteCastleKingSide > 0  ? 'K' : '-';
            var blackQueenCastle = CanBlackCastleQueenSide > 0  ? 'q' : '-';
            var blackKingCastle = CanBlackCastleKingSide > 0  ? 'k' : '-';
            Console.WriteLine( $"Castle Rights: {whiteKingCastle}{whiteQueenCastle}{blackKingCastle}{blackQueenCastle}" );
            Console.WriteLine( $"Half moves: {HalfMoves}, Moves: {Moves}" );
            var enPassantSquareText = EnPassantSquareIndex is 0 ? "-" : SquareHelpers.SquareIndexToString(EnPassantSquareIndex);
            Console.WriteLine( $"En Passant: {enPassantSquareText}" );
            Console.WriteLine( $"White king in check: {IsWhiteKingInCheck > 0}" );
            Console.WriteLine( $"Black king in check: {IsBlackKingInCheck > 0}" );
            Console.WriteLine( $"Hash: {ZobristKey}" );
            Console.WriteLine( $"Is repeated: {MoveHistory.IsRepetition( ZobristKey )}" );
            Console.WriteLine();
#endif
        }
    }
}
