using System.Runtime.CompilerServices;
using System.Text;

namespace Izumi.Structures.Data
{
    internal struct Move : IEquatable<Move>
    {
        private int _fromSquareIndex;
        private int _toSquareIndex;
        private PieceType _movingPieceType;
        private PieceType _targetPieceType;
        private PieceType _promotionPieceType;
        private bool _isCapture;
        private bool _isCastles;
        private bool _isEnPassant;
        private bool _isPromotion;

        public int From
        {
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            get => _fromSquareIndex;
        }

        public int To
        {
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            get => _toSquareIndex;
        }

        public PieceType MovingPiece
        {
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            get => _movingPieceType;
        }

        public PieceType TargetPiece
        {
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            get => _targetPieceType;
        }

        public PieceType PromotionPiece
        {
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            get => _promotionPieceType;
        }

        public bool IsCapture
        {
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            get => _isCapture;
        }

        public bool IsCastle
        {
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            get => _isCastles;
        }

        public bool IsEnPassant
        {
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            get => _isEnPassant;
        }

        public bool IsPromotion
        {
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            get => _isPromotion;
        }

        public bool IsNull => (From | To) == 0;

        public Move( int from, int to, PieceType movingPiece, PieceType targetPiece, PieceType promotionPiece, bool isCastle, bool isCapture, bool isEnPassant, bool isPromotion )
        {
            _fromSquareIndex = from;
            _toSquareIndex = to;
            _movingPieceType = movingPiece;
            _targetPieceType = targetPiece;
            _promotionPieceType = promotionPiece;
            _isCapture = isCapture;
            _isCastles = isCastle;
            _isEnPassant = isEnPassant;
            _isPromotion = isPromotion;
        }

        public Move( string signature, Board board )
        {
            ReadOnlySpan<char> signatureSpan = signature.AsSpan();

            Square fromSquare = new Square(signatureSpan.Slice(0, 2).ToString());
            Square toSquare = new Square(signatureSpan.Slice(2, 2).ToString());

            _fromSquareIndex = fromSquare.SquareIndex;
            _toSquareIndex = toSquare.SquareIndex;

            bool fromSet = false, toSet = false;
            for (int i = 0; i < 12; i++)
            {
                var pieceBitboard = board.GetPieceBitboard((PieceType)(i % 6), i > 5);

                if (!fromSet && pieceBitboard.GetBitValue( From ) > 0)
                {
                    fromSet = true;
                    _movingPieceType = (PieceType)(i % 6);
                }

                if (!toSet && pieceBitboard.GetBitValue( To ) > 0)
                {
                    toSet = true;
                    _targetPieceType = (PieceType)(i % 6);
                }
            }

            _isCapture = fromSet && toSet;
            _isCastles = MovingPiece is PieceType.King && Math.Abs( fromSquare.File - toSquare.File ) is 2;
            _isEnPassant = MovingPiece is PieceType.Pawn && To == board.EnPassantSquareIndex && fromSquare.Rank == (board.IsWhiteToMove ? 4 : 3);
            if (IsEnPassant)
            {
                _isCapture = true;
                _targetPieceType = PieceType.Pawn;
            }
            _isPromotion = signatureSpan.Length is 5;

            if (IsPromotion)
            {
                switch (signatureSpan[4])
                {
                    case 'n':
                    case 'N':
                        _promotionPieceType = PieceType.Knight;
                        break;
                    case 'b':
                    case 'B':
                        _promotionPieceType = PieceType.Bishop;
                        break;
                    case 'r':
                    case 'R':
                        _promotionPieceType = PieceType.Rook;
                        break;
                    case 'q':
                    case 'Q':
                        _promotionPieceType = PieceType.Queen;
                        break;
                }
            }
        }

        public override string ToString()
        {
            StringBuilder moveString = new();
            moveString.Append( new Square( From ).ToString() );
            moveString.Append( new Square( To ).ToString() );
            if (!IsPromotion)
                return moveString.ToString();

            bool isWhiteToMove = To > 55;

            moveString.Append( PromotionPiece switch
            {
                PieceType.Pawn => isWhiteToMove ? 'P' : 'p',
                PieceType.Knight => isWhiteToMove ? 'N' : 'n',
                PieceType.Bishop => isWhiteToMove ? 'B' : 'b',
                PieceType.Rook => isWhiteToMove ? 'R' : 'r',
                PieceType.Queen => isWhiteToMove ? 'Q' : 'q',
                PieceType.King => isWhiteToMove ? 'K' : 'k',
                _ => "",
            } );
            return moveString.ToString();
        }

        public bool Equals( Move other ) =>
            _fromSquareIndex == other._fromSquareIndex &&
            _toSquareIndex == other._toSquareIndex &&
            _movingPieceType == other._movingPieceType &&
            _targetPieceType == other._targetPieceType;
    }
}
