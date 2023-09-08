using System.Runtime.CompilerServices;
using Engine.Data.Bitboards;
using Engine.Data.Enums;
using Engine.Zobrist;

namespace Engine.Board;

public partial struct BoardData
{
    private PieceBoard _pieces; //piece bitboards ot represent board state (https://www.chessprogramming.org/Bitboards). I used denser board solution to reduce size and sped up copy process (https://www.chessprogramming.org/Bitboard_Board-Definition#Denser_Board)
    private PieceLookup _pieceLookup; //mailbox tyle 8x8 array of pieces for easy lookup (https://www.chessprogramming.org/Mailbox)
    
    #region Proporties
    public int EnPassantSquareIndex
    {
        get; set;
    }
    public int HalfMoves
    {
        get; set;
    }
    public int Moves
    {
        get; set;
    }
    public bool IsStmInCheck
    {
        get; set;
    }
    public int SideToMove
    {
        get; set;
    }
    public bool CanWhiteCastleQueenSide
    {
        get; set;
    }
    public bool CanWhiteCastleKingSide
    {
        get; set;
    }
    public bool CanBlackCastleQueenSide
    {
        get; set;
    }
    public bool CanBlackCastleKingSide
    {
        get; set;
    }
    public ulong ZobristKey
    {
        get; set;
    }

    #endregion

    public BoardData()
    {
        _pieces = default;
        _pieceLookup = default;
        ZobristKey = 0;

        foreach (ref PieceType piece in _pieceLookup)
            piece = PieceType.None;
    }

    #region Basic Methods

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bitboard GetAllPieces() => _pieces[6] | _pieces[7];

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Bitboard GetPiecesBitboardForSide( int side ) => _pieces[6 + side];

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Bitboard GetPieceBitboard( int pieceIndex ) => _pieces[pieceIndex];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bitboard GetPieceBitboard(PieceType pieceType) => _pieces[(int) pieceType];

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Bitboard GetPieceBitboard( PieceType pieceType, int side ) => GetPieceBitboard( (int)pieceType, side );

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Bitboard GetPieceBitboard( int pieceIndex, int side ) => _pieces[pieceIndex] & GetPiecesBitboardForSide( side );

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public PieceType GetPieceOnSquare( int squareIndex ) => _pieceLookup[squareIndex];

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public PieceType GetPieceOnSquare( int squareIndex, int side )
    {
        if (_pieceLookup[squareIndex] != PieceType.None && GetPiecesBitboardForSide( side ).GetBitValue( squareIndex ) > 0)
            return _pieceLookup[squareIndex];
        return PieceType.None;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void SetPieceOnSquare( PieceType pieceType, int side, int squareIndex ) => SetPieceOnSquare( (int)pieceType, side, squareIndex );

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void SetPieceOnSquare( int pieceIndex, int side, int squareIndex )
    {
        _pieces[pieceIndex].SetBitToOne( squareIndex );
        _pieces[6 + side].SetBitToOne( squareIndex );
        _pieceLookup[squareIndex] = (PieceType)pieceIndex;
        ZobristKey ^= ZobristHashing.GetSeed( pieceIndex, side, squareIndex );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void RemovePieceOnSquare( PieceType pieceType, int side, int squareIndex ) => RemovePieceOnSquare( (int)pieceType, side, squareIndex );

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void RemovePieceOnSquare( int pieceIndex, int side, int squareIndex )
    {
        _pieces[pieceIndex].SetBitToZero( squareIndex );
        _pieces[6 + side].SetBitToZero( squareIndex );
        _pieceLookup[squareIndex] = PieceType.None;
        ZobristKey ^= ZobristHashing.GetSeed( pieceIndex, side, squareIndex );
    }
    #endregion
}

[InlineArray( 8 )]
internal struct PieceBoard
{
    private Bitboard _value;

    /* 0 - 5 ==> piece tables 
     * 6 ==> white pieces table (all pieces on one bitboard)
     * 7 ==> black pieces table (all pieces on one bitboard)
     */
}

[InlineArray( 64 )]
internal struct PieceLookup
{
    private PieceType _value;
}