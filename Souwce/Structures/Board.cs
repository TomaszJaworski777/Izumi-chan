using System.ComponentModel;
using System.Runtime.CompilerServices;
using Izumi.Helpers;
using Izumi.Structures.Data;
using Izumi.Systems;

namespace Izumi.Structures
{
    internal struct Board
    {
        private GameBoard _pieces = default;
        private BitboardInt _miscData; // {moves - 10} {half moves - 7} {en passant - 6} {castle - 4} {side to move - 1} {white king in check - 1} {black king in check - 1} = 30 bit
        private ulong _zobristKey;

        public bool BlackKingInCheck
        {
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            get => (_miscData & 1) > 0;
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            set
            {
                if (value)
                    _miscData |= 1;
                else
                    _miscData &= 1073741822;
            }
        }
        public bool WhiteKingInCheck
        {
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            get => (_miscData & 2) > 0;
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            set
            {
                if (value)
                    _miscData |= 2;
                else
                    _miscData &= 1073741821;
            }
        }
        public bool SideToMoveKingInCheck
        {
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            get => IsWhiteToMove ? WhiteKingInCheck : BlackKingInCheck;
        }
        public bool IsWhiteToMove
        {
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            get => (_miscData & 4) > 0;
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            set {
                if (value)
                    _miscData |= 4;
                else
                    _miscData &= 1073741819;
            }
        }
        public bool CanWhiteCastleQueenSide
        {
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            get => (_miscData & 8) > 0;
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            set
            {
                if (value)
                    _miscData |= 8;
                else
                    _miscData &= 1073741815;
            }
        }
        public bool CanWhiteCastleKingSide
        {
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            get => (_miscData & 16) > 0;
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            set
            {
                if (value)
                    _miscData |= 16;
                else
                    _miscData &= 1073741807;
            }
        }
        public bool CanBlackCastleQueenSide
        {
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            get => (_miscData & 32) > 0;
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            set
            {
                if (value)
                    _miscData |= 32;
                else
                    _miscData &= 1073741791;
            }
        }
        public bool CanBlackCastleKingSide
        {
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            get => (_miscData & 64) > 0;
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            set
            {
                if (value)
                    _miscData |= 64;
                else
                    _miscData &= 1073741759;
            }
        }
        public bool CanCurrentSideCastleQueenSide
        {
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            get => IsWhiteToMove ? CanWhiteCastleQueenSide : CanBlackCastleQueenSide;
        }
        public bool CanCurrentSideCastleKingSide
        {
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            get => IsWhiteToMove ? CanWhiteCastleKingSide : CanBlackCastleKingSide;
        }
        public int EnPassantSquareIndex
        {
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            get => _miscData.GetValueChunk( 7, 8064 );
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            set => _miscData.SetValueChunk( 7, 8064, value );
        }
        public int HalfMoves
        {
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            get => _miscData.GetValueChunk( 13, 1040384 );
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            set => _miscData.SetValueChunk( 13, 1040384, value );
        }
        public int Moves
        {
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            get => _miscData.GetValueChunk( 20, 1072693248 );
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            set => _miscData.SetValueChunk( 20, 1072693248, value );
        }
        public ulong ZobristKey
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _zobristKey;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _zobristKey = value;
        }
        public Bitboard WhitePieces
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _pieces[6];
        }
        public Bitboard BlackPieces
        {
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            get => _pieces[7];
        }
        public Bitboard SideToMovePieces
        {
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            get => IsWhiteToMove ? WhitePieces : BlackPieces;
        }
        public Bitboard OpponentPieces
        {
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            get => IsWhiteToMove ? BlackPieces : WhitePieces;
        }
        public Bitboard AllPieces
        {
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            get => WhitePieces | BlackPieces;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Board(string fen)
        {
            MoveHistory.Reset();
            FenSystem.CreateBoard(ref this, fen);
            MoveHistory.Add( ZobristKey );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Board(Board other)
        {
            _pieces = other._pieces;
            _miscData = other._miscData;
            _zobristKey = other._zobristKey;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Bitboard GetPieceBitboard( PieceType type, bool isWhite ) => GetPieceBitboard( (int)type, isWhite );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Bitboard GetPieceBitboard( int type, bool isWhite ) => _pieces[type] & (isWhite ? _pieces[6] : _pieces[7]);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPieceOnSquare(PieceType type, bool isWhite, int squareIndex)
        {
            _pieces[(int)type].SetBitToOne( squareIndex );
            if(isWhite)
                _pieces[6].SetBitToOne( squareIndex );
            else
                _pieces[7].SetBitToOne( squareIndex );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void RemovePieceOnSquare( PieceType type, bool isWhite, int squareIndex ) 
        {
            _pieces[(int)type].SetBitToZero( squareIndex );
            if (isWhite)
                _pieces[6].SetBitToZero( squareIndex );
            else
                _pieces[7].SetBitToZero( squareIndex );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MakeMove(Move move)
        {
            if (!MoveController.MakeMove(ref this, move))
                return false;
            MoveHistory.Add(ZobristKey);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<Move> GeneratePseudoLegalMoves() => MoveController.GeneratePseudoLegalMoves(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<Move> GeneratePseudoLegalTacticalMoves() => MoveController.GeneratePseudoLegalTacticalMoves(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsKingInCheck(bool isWhite) => IsSquareAttacked(GetPieceBitboard(PieceType.King, isWhite).LsbIndex, isWhite);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSquareAttacked(int squareIndex, bool isSquareWhite) => PieceAttacks.IsSquareAttacked(squareIndex, isSquareWhite, this);

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool IsRepetition() => MoveHistory.IsRepetition(ZobristKey);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PieceType FindPieceTypeOnSquare(int squareIndex, bool forWhite)
        {
            for (int pieceIndex = 0; pieceIndex < 6; pieceIndex++)
            {
                if (GetPieceBitboard( (PieceType)pieceIndex, forWhite ).GetBitValue(squareIndex) > 0)
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
                PieceType pieceType = (PieceType)(pieceIndex % 6);
                for (int squareIndex = 0; squareIndex < 64; squareIndex++)
                {
                    if (GetPieceBitboard(pieceType, pieceIndex < 6).GetBitValue( squareIndex ) == 0)
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
            var whiteQueenCastle = CanWhiteCastleQueenSide ? 'Q' : '-';
            var whiteKingCastle = CanWhiteCastleKingSide ? 'K' : '-';
            var blackQueenCastle = CanBlackCastleQueenSide ? 'q' : '-';
            var blackKingCastle = CanBlackCastleKingSide ? 'k' : '-';
            Console.WriteLine( $"Castle Rights: {whiteKingCastle}{whiteQueenCastle}{blackKingCastle}{blackQueenCastle}" );
            Console.WriteLine( $"Half moves: {HalfMoves}, Moves: {Moves}" );
            var enPassantSquareText = EnPassantSquareIndex is 0 ? "-" : new Square((int)EnPassantSquareIndex).ToString();
            Console.WriteLine( $"En Passant: {enPassantSquareText}" );
            Console.WriteLine( $"White king in check: {IsKingInCheck( true )}" );
            Console.WriteLine( $"Black king in check: {IsKingInCheck( false )}" );
            Console.WriteLine( $"Hash: {ZobristKey}" );
            Console.WriteLine( $"Is repeated: {IsRepetition()}" );
            Console.WriteLine();
#endif
        }
    }

    [InlineArray(8)]
    internal struct GameBoard
    {
        private Bitboard _value;

        /* 0 - 5 ==> piece tables 
         * 6 ==> white pieces table (all pieces on one bitboard)
         * 7 ==> black pieces table (all pieces on one bitboard)
         */
    }
}