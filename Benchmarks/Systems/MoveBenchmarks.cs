using BenchmarkDotNet.Attributes;
using Engine.Board;
using Engine.Data.Enums;
using Engine.Move;

namespace Benchmarks.Systems;

[DisassemblyDiagnoser]
public class MoveBenchmarks
{
    [Benchmark]
    public void CreateMove()
    {
        BoardData board = BoardProvider.Create(BoardProvider.StartPosition);
        for (int i = 0; i < 100000000; i++)
        {
            MoveData move = new MoveData(board,0,0,PieceType.Queen,false,false,false,false);
        }
    }

    [Benchmark]
    public void ReadMove()
    {
        BoardData board = BoardProvider.Create(BoardProvider.StartPosition);
        MoveData move = new MoveData(board,0,0,PieceType.Queen,false,false,false,false);
        for (int i = 0; i < 100000000; i++)
        {
            int from = move.FromSquareIndex;
            int to = move.ToSquareIndex;

            PieceType moveingPiece = move.MovingPieceType;
            PieceType targetPiece = move.TargetPieceType;
            PieceType promotionPiece = move.PromotionPieceType;

            bool isCapture = move.IsCapture;
            bool isCastle = move.IsCastle;
            bool isEnPassant = move.IsEnPassant;
            bool isPromotion = move.IsPromotion;
        }
    }

    [Benchmark]
    public void MakeMove()
    {
        BoardData board = BoardProvider.Create(BoardProvider.KiwipetePosition);
        MoveData move = new MoveData("f3h3", board);
        for (int i = 0; i < 1000000; i++)
        {
            board.MakeMove( move );
            board.UnmakeMove();
        }
    }

    [Benchmark]
    public void GenerateMoves()
    {
        BoardData board = BoardProvider.Create(BoardProvider.KiwipetePosition);
        for (int i = 0; i < 1000000; i++)
        {
            MoveList moves = new(new MoveData[300] );
            board.GenerateAllPseudoLegalMoves( ref moves );
        }
    }

    [Benchmark]
    public void GenerateTacticalMoves()
    {
        BoardData board = BoardProvider.Create(BoardProvider.KiwipetePosition);
        for (int i = 0; i < 1000000; i++)
        {
            MoveList moves = new(new MoveData[300] );
            board.GenerateTacticalPseudoLegalMoves( ref moves );
        }
    }
}