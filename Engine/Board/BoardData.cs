using Engine.Data.Bitboards;
using Engine.Data.Enums;
using Engine.Zobrist;
using System.Runtime.CompilerServices;

namespace Engine.Board;

public ref partial struct BoardData
{
    private PieceBoard _pieces; //piece bitboards ot represent board state (https://www.chessprogramming.org/Bitboards). I used denser board solution to reduce size and sped up copy process (https://www.chessprogramming.org/Bitboard_Board-Definition#Denser_Board)
    private PieceLookup _pieceLookup; //mailbox tyle 8x8 array of pieces for easy lookup (https://www.chessprogramming.org/Mailbox)
    private BitboardInt _miscData; // {castle - 4} {side to move - 1} {white king in check - 1} {black king in check - 1} {moves - 10} {half moves - 7} {en passant - 6} = 30 bit
    private ulong _zobristKey; //almost unique position key (https://www.chessprogramming.org/Zobrist_Hashing)

    #region Proporties
    public int EnPassantSquareIndex
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        get => _miscData.GetValueChunk( 0, 63 );
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        set => _miscData.SetValueChunk( 0, 63, value );
    }
    public int HalfMoves
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        get => _miscData.GetValueChunk( 6, 127 );
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        set => _miscData.SetValueChunk( 6, 127, value );
    }
    public int Moves
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        get => _miscData.GetValueChunk( 13, 1023 );
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        set => _miscData.SetValueChunk( 13, 1023, value );
    }
    public int IsBlackKingInCheck
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        get => _miscData.GetBitValue( 23 ) >> 23;
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        set { if (value > 0) _miscData.SetBitToOne( 23 ); else _miscData.SetBitToZero( 23 ); }
    }
    public int IsWhiteKingInCheck
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        get => _miscData.GetBitValue( 24 ) >> 24;
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        set { if (value > 0) _miscData.SetBitToOne( 24 ); else _miscData.SetBitToZero( 24 ); }
    }
    public int SideToMove
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        get => _miscData.GetBitValue( 25 ) >> 25;
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        set { if (value > 0) _miscData.SetBitToOne( 25 ); else _miscData.SetBitToZero( 25 ); }
    }
    public int CanWhiteCastleQueenSide
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        get => _miscData.GetBitValue( 26 ) >> 26;
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        set { if (value > 0) _miscData.SetBitToOne( 26 ); else _miscData.SetBitToZero( 26 ); }
    }
    public int CanWhiteCastleKingSide
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        get => _miscData.GetBitValue( 27 ) >> 27;
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        set { if (value > 0) _miscData.SetBitToOne( 27 ); else _miscData.SetBitToZero( 27 ); }
    }
    public int CanBlackCastleQueenSide
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        get => _miscData.GetBitValue( 28 ) >> 28;
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        set { if (value > 0) _miscData.SetBitToOne( 28 ); else _miscData.SetBitToZero( 28 ); }
    }
    public int CanBlackCastleKingSide
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        get => _miscData.GetBitValue( 29 ) >> 29;
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        set { if (value > 0) _miscData.SetBitToOne( 29 ); else _miscData.SetBitToZero( 29 ); }
    }
    public ulong ZobristKey
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _zobristKey;
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        set => _zobristKey = value;
    }
    #endregion

    public BoardData()
    {
        _pieces = default;
        _pieceLookup = default;
        _miscData = default;
        _zobristKey = 0;

        for (int squareIndex = 0; squareIndex < 64; squareIndex++)
            _pieceLookup[squareIndex] = PieceType.None;
    }

    #region Basic Methods
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Bitboard GetPiecesBitboardForSide( int side ) => _pieces[6 + side];

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Bitboard GetPieceBitboard( int pieceIndex ) => _pieces[pieceIndex];

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
    public void SetPieceOnSquare( PieceType pieceType, int side, int sqaureIndex ) => SetPieceOnSquare( (int)pieceType, side, sqaureIndex );

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void SetPieceOnSquare( int pieceIndex, int side, int sqaureIndex )
    {
        _pieces[pieceIndex].SetBitToOne( sqaureIndex );
        _pieces[6 + side].SetBitToOne( sqaureIndex );
        _pieceLookup[sqaureIndex] = (PieceType)pieceIndex;
        _zobristKey ^= ZobristHashing.GetSeed( pieceIndex, side, sqaureIndex );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void RemovePieceOnSquare( PieceType pieceType, int side, int sqaureIndex ) => RemovePieceOnSquare( (int)pieceType, side, sqaureIndex );

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void RemovePieceOnSquare( int pieceIndex, int side, int sqaureIndex )
    {
        _pieces[pieceIndex].SetBitToZero( sqaureIndex );
        _pieces[6 + side].SetBitToZero( sqaureIndex );
        _pieceLookup[sqaureIndex] = PieceType.None;
        _zobristKey ^= ZobristHashing.GetSeed( pieceIndex, side, sqaureIndex );
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