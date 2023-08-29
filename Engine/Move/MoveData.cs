using Engine.Board;
using Engine.Data.Enums;
using Engine.Utils;
using System.Runtime.CompilerServices;
using System.Text;

namespace Engine.Move;

//not in a single Bitboard32, because in this struct size doesn't matter, the most important thing here is speed of accessing data
public readonly struct MoveData
{
    private readonly int _fromSquareIndex;
    private readonly int _toSquareIndex;

    private readonly PieceType _movingPieceType;
    private readonly PieceType _targetPieceType;
    private readonly PieceType _promotionPieceType;

    private readonly bool _isCapture;
    private readonly bool _isCastle;
    private readonly bool _isEnPassant;
    private readonly bool _isPromotion;

    public int FromSquareIndex => _fromSquareIndex;
    public int ToSquareIndex => _toSquareIndex;
    public PieceType MovingPieceType => _movingPieceType;
    public PieceType TargetPieceType => _targetPieceType;
    public PieceType PromotionPieceType => _promotionPieceType;
    public bool IsCapture => _isCapture;
    public bool IsCastle => _isCastle;
    public bool IsEnPassant => _isEnPassant;
    public bool IsPromotion => _isPromotion;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MoveData( BoardData board, int from, int to, PieceType promotionPiece, bool isCapture, bool isCastle, bool isEnPassant, bool isPromotion )
    {
        _fromSquareIndex = from;
        _toSquareIndex = to;

        _movingPieceType = board.GetPieceOnSquare( from );
        _targetPieceType = board.GetPieceOnSquare( to );
        _promotionPieceType = promotionPiece;

        _isCapture = isCapture;
        _isCastle = isCastle;
        _isEnPassant = isEnPassant;
        _isPromotion = isPromotion;

        if(_isEnPassant)
            _targetPieceType = PieceType.Pawn;
    }

    [MethodImpl( MethodImplOptions.AggressiveOptimization )]
    //generates move from string
    public MoveData( ReadOnlySpan<char> signature, BoardData board )
    {
        _fromSquareIndex = SquareHelpers.StringToSquareIndex( signature[..2] );
        _toSquareIndex = SquareHelpers.StringToSquareIndex( signature.Slice( 2, 2 ) );

        //get moving piece from piece lookup table
        _movingPieceType = board.GetPieceOnSquare( _fromSquareIndex );
        //get target piece from piece lookup table
        _targetPieceType = board.GetPieceOnSquare( _toSquareIndex );

        //checks capture, if move has a target then it has to be a capture
        _isCapture = _targetPieceType != PieceType.None;
        //if king moves 2 squares to the side then it has to be castle
        _isCastle = _movingPieceType is PieceType.King &&
            Math.Abs( SquareHelpers.SquareIndexToFile( _fromSquareIndex ) - SquareHelpers.SquareIndexToFile( _toSquareIndex ) ) is 2;
        //if pawn is moving to en passant square (https://www.chessprogramming.org/En_passant), and it is in correct rank, then it has to be en passant
        _isEnPassant = _movingPieceType is PieceType.Pawn && _toSquareIndex == board.EnPassantSquareIndex && SquareHelpers.SquareIndexToRank( _fromSquareIndex ) == (4 - board.SideToMove);
        if (IsEnPassant)
        {
            //if its en passant then it also has to be capture
            _isCapture = true;
            _targetPieceType = PieceType.Pawn;
        }
        //move with 5 characters has to be a promotion
        _isPromotion = signature.Length is 5;

        if (_isPromotion)
        {
            //translate 5th character into piece
            switch (signature[4])
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString()
    {
        StringBuilder result = new();
        result.Append( SquareHelpers.SquareIndexToString( _fromSquareIndex ) );
        result.Append( SquareHelpers.SquareIndexToString( _toSquareIndex ) );
        if (!_isPromotion) return result.ToString();
        char pieceChar = PromotionPieceType switch
        {
            PieceType.Knight => 'N',
            PieceType.Bishop => 'B',
            PieceType.Rook => 'R',
            PieceType.Queen => 'Q',
            _ => ' '
        };
        if (_fromSquareIndex > 55)
            pieceChar += (char)32;
        result.Append( pieceChar );
        return result.ToString();
    }   
}
