using Engine.Data.Enums;
using Engine.Move;
using Engine.Utils;
using Engine.Zobrist;
using System.Runtime.CompilerServices;

namespace Engine.Board;

public static class BoardProvider
{
    public const string StartPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    public const string KiwipetePosition = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1";

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static BoardData Create( ReadOnlySpan<char> fen )
    {
        BoardData result = new();

        ReadOnlySpan<char> positionFenPart = fen.Split(' ', 0);

        //reads pieces from FEN (https://www.chessprogramming.org/Forsyth-Edwards_Notation) and applies them to board
        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0, index = 0; file < 8; file++, index++)
            {
                int squareIndex = (7 - rank) * 8 + file;
                char currentCharacter = positionFenPart.Split('/', rank)[index];

                if (int.TryParse( currentCharacter.ToString(), out int output ))
                {
                    file += output - 1;
                    continue;
                }

                int isWhite = currentCharacter < 'a' ? 0 : 1;

                switch (currentCharacter)
                {
                    case 'p':
                    case 'P':
                        result.SetPieceOnSquare( PieceType.Pawn, isWhite, squareIndex );
                        break;
                    case 'n':
                    case 'N':
                        result.SetPieceOnSquare( PieceType.Knight, isWhite, squareIndex );
                        break;
                    case 'b':
                    case 'B':
                        result.SetPieceOnSquare( PieceType.Bishop, isWhite, squareIndex );
                        break;
                    case 'r':
                    case 'R':
                        result.SetPieceOnSquare( PieceType.Rook, isWhite, squareIndex );
                        break;
                    case 'q':
                    case 'Q':
                        result.SetPieceOnSquare( PieceType.Queen, isWhite, squareIndex );
                        break;
                    case 'k':
                    case 'K':
                        result.SetPieceOnSquare( PieceType.King, isWhite, squareIndex );
                        break;
                }
            }
        }

        //applies side to move from FEN. I decided to store it as an int for ease of extracting data from arrays.
        result.SideToMove = fen.Split( ' ', 1 )[0] == 'w' ? 0 : 1;

        //applies castle rights from FEN. I set them to one here, but remember that .GetBitValue() from bitboard does not have to return 0 or 1, cuz output is not shifted.
        ReadOnlySpan<char> castleRights = fen.Split( ' ', 2 );
        for (int i = 0; i < castleRights.Length; i++)
        {
            if (castleRights[i] == '-')
                break;
            else if (castleRights[i] == 'Q')
                result.CanWhiteCastleQueenSide = 1;
            else if (castleRights[i] == 'K')
                result.CanWhiteCastleKingSide = 1;
            else if (castleRights[i] == 'q')
                result.CanBlackCastleQueenSide = 1;
            else if (castleRights[i] == 'k')
                result.CanBlackCastleKingSide = 1;
        }

        result.IsWhiteKingInCheck = result.IsSquareAttacked( result.GetPieceBitboard( 5, 0 ).LsbIndex, 0 ) ? 1 : 0;
        result.IsBlackKingInCheck = result.IsSquareAttacked( result.GetPieceBitboard( 5, 1 ).LsbIndex, 1 ) ? 1 : 0;

        //applies en passant square (https://www.chessprogramming.org/En_passant)
        ReadOnlySpan<char> enPassant = fen.Split( ' ', 3 );
        if (enPassant[0] == '-')
            result.EnPassantSquareIndex = 0;
        else
            result.EnPassantSquareIndex = SquareHelpers.StringToSquareIndex( enPassant );

        //applies move counts from FEN
        if (int.TryParse( fen.Split( ' ', 4 ), out int moveCount ))
            result.HalfMoves = moveCount;

        if (int.TryParse( fen.Split( ' ', 5 ), out moveCount ))
            result.Moves = moveCount;

        //generate starting zobrist key for new position
        result.ZobristKey = ZobristHashing.GenerateKey( result );
        MoveHistory.Reset();
        MoveHistory.Add( result.ZobristKey );

        return result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    //simple method to split string spans
    private static ReadOnlySpan<char> Split(this ReadOnlySpan<char> span, char spliter, int index )
    {
        int startIndex = 0;
        int endIndex = span.Length;
        int counter = 0;

        for (int i = 0; i < endIndex; i++)
        {
            if (span[i] != spliter)
                continue;

            counter++;
            if (counter < index)
                continue;

            if (startIndex == 0 && index != 0)
                startIndex = i;
            else
                endIndex = i;
        }

        if (index > 0)
            startIndex++;

        return span[startIndex..endIndex];
    }
}
