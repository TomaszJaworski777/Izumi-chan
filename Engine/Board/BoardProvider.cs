using System;
using System.Runtime.CompilerServices;
using Engine.Data.Enums;
using Engine.Move;
using Engine.Utils;
using Engine.Zobrist;

namespace Engine.Board;

public static class BoardProvider
{
    public const string StartPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    public const string KiwipetePosition = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1";

    public static BoardData Create( ReadOnlySpan<char> fen )
    {
        BoardData result = new();

        Span<Range> split = stackalloc Range[7];
        split = split[..fen.Split(split, ' ')];

        //reads pieces from FEN (https://www.chessprogramming.org/Forsyth-Edwards_Notation) and applies them to board
        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0, index = 0; file < 8; file++, index++)
            {
                int squareIndex = (7 - rank) * 8 + file;
                char currentCharacter = fen[split[0]].Split('/', rank)[index];

                if (int.TryParse( new ReadOnlySpan<char>(in currentCharacter), out int output ))
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
        result.SideToMove = fen[split[1]][0] == 'w' ? WHITE : BLACK;

        //applies castle rights from FEN. I set them to one here, but remember that .GetBitValue() from bitboard does not have to return 0 or 1, cuz output is not shifted.
        ReadOnlySpan<char> castleRights = fen[split[2]];
        for (int i = 0; i < castleRights.Length; i++)
        {
            if (castleRights[i] == '-')
                break;

            if (castleRights[i] == 'Q')
                result.CanWhiteCastleQueenSide = true;
            else if (castleRights[i] == 'K')
                result.CanWhiteCastleKingSide = true;
            else if (castleRights[i] == 'q')
                result.CanBlackCastleQueenSide = true;
            else if (castleRights[i] == 'k')
                result.CanBlackCastleKingSide = true;
        }

        result.IsSideToMoveInCheck = result.IsSquareAttacked(result.GetPieceBitboard(5, result.SideToMove).LsbIndex, result.SideToMove);

        //applies en passant square (https://www.chessprogramming.org/En_passant)
        ReadOnlySpan<char> enPassant = fen[split[3]];
        result.EnPassantSquareIndex = enPassant[0] == '-' ? 0 : SquareHelpers.StringToSquareIndex( enPassant );

        //applies move counts from FEN
        if (int.TryParse( fen[split[4]], out int moveCount ))
            result.HalfMoves = moveCount;

        if (int.TryParse( fen[split[5]], out moveCount ))
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
        Span<Range> ranges = stackalloc Range[index + 2];
        int count = span.Split(ranges, spliter);
        return span[ranges[..count][index]];
    }
}
