﻿using System.Runtime.CompilerServices;
using Engine.Board;
using Engine.Data.Bitboards;

namespace Engine.Zobrist;

//hashing method to achive almost unique key for any board state. (https://www.chessprogramming.org/Zobrist_Hashing) Usefull to check for repetition or f.ex. Transposition Tables 
public static class ZobristHashing
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong GetSeed(int pieceIndex, int side, int squareIndex) => ZobristSeeds.Seeds[(pieceIndex + side * 6) * 64 + squareIndex];

    public static ulong GenerateKey( BoardData board )
    {
        ulong result = 0;

        for (int pieceIndex = 0; pieceIndex < 12; pieceIndex++)
        {
            Bitboard bitboard = board.GetPieceBitboard(pieceIndex % 6, pieceIndex / 6);
            while (bitboard > 0)
            {
                int squareIndex = bitboard.LsbIndex;
                bitboard &= bitboard - 1;

                result ^= ZobristSeeds.Seeds[pieceIndex * 64 + squareIndex];
            }
        }

        if (board.SideToMove > 0)
            result ^= ZobristSeeds.Seeds[268];

        if (board.CanWhiteCastleQueenSide > 0)
            result ^= ZobristSeeds.Seeds[269];
        if (board.CanWhiteCastleKingSide > 0)
            result ^= ZobristSeeds.Seeds[270];
        if (board.CanBlackCastleQueenSide > 0)
            result ^= ZobristSeeds.Seeds[271];
        if (board.CanBlackCastleKingSide > 0)
            result ^= ZobristSeeds.Seeds[272];

        if (board.EnPassantSquareIndex > 0)
            result ^= ZobristSeeds.Seeds[273 + board.EnPassantSquareIndex % 8];

        return result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ulong ModifyKey( BoardData currentBoard, BoardData previousBoard )
    {
        ulong result = currentBoard.ZobristKey;

        if (currentBoard.SideToMove != previousBoard.SideToMove)
            result ^= ZobristSeeds.Seeds[268];

        if (previousBoard.EnPassantSquareIndex > 0)
            result ^= ZobristSeeds.Seeds[273 + previousBoard.EnPassantSquareIndex % 8];

        if (currentBoard.EnPassantSquareIndex > 0)
            result ^= ZobristSeeds.Seeds[273 + currentBoard.EnPassantSquareIndex % 8];

        if (previousBoard.CanWhiteCastleQueenSide != currentBoard.CanWhiteCastleQueenSide)
            result ^= ZobristSeeds.Seeds[269];
        if (previousBoard.CanWhiteCastleKingSide != currentBoard.CanWhiteCastleKingSide)
            result ^= ZobristSeeds.Seeds[270];
        if (previousBoard.CanBlackCastleQueenSide != currentBoard.CanBlackCastleQueenSide)
            result ^= ZobristSeeds.Seeds[271];
        if (previousBoard.CanBlackCastleKingSide != currentBoard.CanBlackCastleKingSide)
            result ^= ZobristSeeds.Seeds[272];

        return result;
    }
}
