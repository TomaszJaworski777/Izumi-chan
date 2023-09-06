using System;
using System.Runtime.CompilerServices;
using System.Text;
using Engine.Board;
using Engine.Data.Enums;
using Engine.Utils;

namespace Engine.Move;

//not in a single Bitboard32, because in this struct size doesn't matter, the most important thing here is speed of accessing data
public readonly struct MoveData
{
    public int FromSquareIndex { get; }

    public int ToSquareIndex { get; }

    public PieceType MovingPieceType { get; }

    public PieceType TargetPieceType { get; }

    public PieceType PromotionPieceType { get; }

    public bool IsCapture { get; }

    public bool IsCastle { get; }

    public bool IsEnPassant { get; }

    public bool IsPromotion { get; }

    public bool IsNull => (FromSquareIndex | ToSquareIndex) == 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MoveData( BoardData board, int from, int to, PieceType promotionPiece, bool isCapture, bool isCastle, bool isEnPassant, bool isPromotion )
    {
        FromSquareIndex = from;
        ToSquareIndex = to;

        MovingPieceType = board.GetPieceOnSquare( from );
        TargetPieceType = board.GetPieceOnSquare( to );
        PromotionPieceType = promotionPiece;

        IsCapture = isCapture;
        IsCastle = isCastle;
        IsEnPassant = isEnPassant;
        IsPromotion = isPromotion;

        if(IsEnPassant)
            TargetPieceType = PieceType.Pawn;
    }

    //generates move from string
    public MoveData( ReadOnlySpan<char> signature, BoardData board )
    {
        FromSquareIndex = SquareHelpers.StringToSquareIndex( signature[..2] );
        ToSquareIndex = SquareHelpers.StringToSquareIndex( signature.Slice( 2, 2 ) );

        //get moving piece from piece lookup table
        MovingPieceType = board.GetPieceOnSquare( FromSquareIndex );
        //get target piece from piece lookup table
        TargetPieceType = board.GetPieceOnSquare( ToSquareIndex );

        //checks capture, if move has a target then it has to be a capture
        IsCapture = TargetPieceType != PieceType.None;
        //if king moves 2 squares to the side then it has to be castle
        IsCastle = MovingPieceType is PieceType.King &&
            Math.Abs( SquareHelpers.SquareIndexToFile( FromSquareIndex ) - SquareHelpers.SquareIndexToFile( ToSquareIndex ) ) is 2;
        //if pawn is moving to en passant square (https://www.chessprogramming.org/En_passant), and it is in correct rank, then it has to be en passant
        IsEnPassant = MovingPieceType is PieceType.Pawn && ToSquareIndex == board.EnPassantSquareIndex && SquareHelpers.SquareIndexToRank( FromSquareIndex ) == 4 - board.SideToMove;
        if (IsEnPassant)
        {
            //if its en passant then it also has to be capture
            IsCapture = true;
            TargetPieceType = PieceType.Pawn;
        }
        //move with 5 characters has to be a promotion
        IsPromotion = signature.Length is 5;

        if (IsPromotion)
        {
            //translate 5th character into piece
            switch (signature[4])
            {
                case 'n':
                case 'N':
                    PromotionPieceType = PieceType.Knight;
                    break;
                case 'b':
                case 'B':
                    PromotionPieceType = PieceType.Bishop;
                    break;
                case 'r':
                case 'R':
                    PromotionPieceType = PieceType.Rook;
                    break;
                case 'q':
                case 'Q':
                    PromotionPieceType = PieceType.Queen;
                    break;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString()
    {
        StringBuilder result = new();
        result.Append( SquareHelpers.SquareIndexToString( FromSquareIndex ) );
        result.Append( SquareHelpers.SquareIndexToString( ToSquareIndex ) );
        if (!IsPromotion) return result.ToString();
        char pieceChar = PromotionPieceType switch
        {
            PieceType.Knight => 'N',
            PieceType.Bishop => 'B',
            PieceType.Rook => 'R',
            PieceType.Queen => 'Q',
            _ => ' '
        };
        if (FromSquareIndex > 55)
            pieceChar += (char)32;
        result.Append( pieceChar );
        return result.ToString();
    }   
}
