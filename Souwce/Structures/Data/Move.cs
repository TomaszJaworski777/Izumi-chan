using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace Izumi.Structures.Data
{
    internal struct Move : IEquatable<Move>
    {
        private BitboardInt _bitboard;
        /* Reserved bits from LSB (Total 25/32 reserved):
 *      6 bits - from square index
 *      6 bits - to square index
 *      3 bits - moving piece type
 *      3 bits - target piece type
 *      3 bits - promotion piece type
 *      1 bit - capture
 *      1 bit - castles
 *      1 bit - en passant
 *      1 bit - promotion
 */
        public int From
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _bitboard.GetValueChunk(0, 63);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private set => _bitboard.SetValueChunk(0, 63, value);
        }

        public int To
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _bitboard.GetValueChunk(6, 63);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private set => _bitboard.SetValueChunk(6, 63, value);
        }

        public PieceType MovingPiece
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (PieceType)_bitboard.GetValueChunk(12, 7);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private set => _bitboard.SetValueChunk(12, 7, (int)value);
        }

        public PieceType TargetPiece
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (PieceType)_bitboard.GetValueChunk(15, 7);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private set => _bitboard.SetValueChunk(15, 7, (int)value);
        }

        public PieceType PromotionPiece
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (PieceType)_bitboard.GetValueChunk(18, 7);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private set => _bitboard.SetValueChunk(18, 7, (int)value);
        }

        public bool IsCapture
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _bitboard.GetBitValue(21) > 0;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private set => _bitboard.SetValueChunk(21, 1, value ? 1 : 0);
        }

        public bool IsCastle
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _bitboard.GetBitValue(22) > 0;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private set => _bitboard.SetValueChunk(22, 1, value ? 1 : 0);
        }

        public bool IsEnPassant
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _bitboard.GetBitValue(23) > 0;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private set => _bitboard.SetValueChunk(23, 1, value ? 1 : 0);
        }

        public bool IsPromotion
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _bitboard.GetBitValue(24) > 0;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private set => _bitboard.SetValueChunk(24, 1, value ? 1 : 0);
        }

        public bool IsNull => (From | To) == 0;

        public Move(int from, int to, PieceType movingPiece, PieceType targetPiece, PieceType promotionPiece, bool isCastle, bool isCapture, bool isEnPassant, bool isPromotion)
        {
            From = from;
            To = to;
            MovingPiece = movingPiece;
            TargetPiece = targetPiece;
            PromotionPiece = promotionPiece;
            IsCapture = isCapture;
            IsCastle = isCastle;
            IsEnPassant = isEnPassant;
            IsPromotion = isPromotion;
        }

        public Move(string signature, Board board)
        {
            ReadOnlySpan<char> signatureSpan = signature.AsSpan();

            Square fromSquare = new Square(signatureSpan.Slice(0, 2).ToString());
            Square toSquare = new Square(signatureSpan.Slice(2, 2).ToString());

            From = fromSquare.SquareIndex;
            To = toSquare.SquareIndex;

            bool fromSet = false, toSet = false;
            for (int i = 0; i < 12; i++)
            {
                var pieceBitboard = board.GetBitboardForPiece((PieceType)(i % 6), i > 5);

                if (!fromSet && pieceBitboard.GetBitValue(From) > 0)
                {
                    fromSet = true;
                    MovingPiece = (PieceType)(i % 6);
                }

                if (!toSet && pieceBitboard.GetBitValue(To) > 0)
                {
                    toSet = true;
                    TargetPiece = (PieceType)(i % 6);
                }
            }

            IsCapture = fromSet && toSet;
            IsCastle = MovingPiece is PieceType.King && Math.Abs(fromSquare.File - toSquare.File) is 2;
            IsEnPassant = MovingPiece is PieceType.Pawn && To == (int)board.EnPassantSquareIndex && fromSquare.Rank == (board.IsWhiteToMove ? 4 : 3);
            if (IsEnPassant)
            {
                IsCapture = true;
                TargetPiece = PieceType.Pawn;
            }
            IsPromotion = signatureSpan.Length is 5;

            if (IsPromotion)
            {
                switch (signatureSpan[4])
                {
                    case 'n':
                    case 'N':
                        PromotionPiece = PieceType.Knight;
                        break;
                    case 'b':
                    case 'B':
                        PromotionPiece = PieceType.Bishop;
                        break;
                    case 'r':
                    case 'R':
                        PromotionPiece = PieceType.Rook;
                        break;
                    case 'q':
                    case 'Q':
                        PromotionPiece = PieceType.Queen;
                        break;
                }
            }
        }

        public override string ToString()
        {
            StringBuilder moveString = new();
            moveString.Append(new Square(From).ToString());
            moveString.Append(new Square(To).ToString());
            if (!IsPromotion)
                return moveString.ToString();

            bool isWhiteToMove = To > 55;

            moveString.Append(PromotionPiece switch
            {
                PieceType.Pawn => isWhiteToMove ? 'P' : 'p',
                PieceType.Knight => isWhiteToMove ? 'N' : 'n',
                PieceType.Bishop => isWhiteToMove ? 'B' : 'b',
                PieceType.Rook => isWhiteToMove ? 'R' : 'r',
                PieceType.Queen => isWhiteToMove ? 'Q' : 'q',
                PieceType.King => isWhiteToMove ? 'K' : 'k',
                _ => "",
            });
            return moveString.ToString();
        }

        public bool Equals(Move other) => _bitboard.Value == other._bitboard.Value;
    }
}
