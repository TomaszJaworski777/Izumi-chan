using System.Runtime.CompilerServices;

namespace Greg
{
    internal class ChessEngine
    {
        private const string startFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        private readonly Perft _perft = new();

        private Board _currentBoard;

        public ChessEngine()
        {
            _currentBoard = new Board( startFen );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ChangePosition( string fen = startFen ) => _currentBoard = new Board( fen );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Perft( int depth ) => _perft.PerftInternal( depth, _currentBoard, false );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Perft( int depth, string fen ) => _perft.PerftInternal( depth, new Board(fen), false );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void SplitPerft( int depth ) => _perft.PerftInternal( depth, _currentBoard, true );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void SplitPerft( int depth, string fen ) => _perft.PerftInternal( depth, new Board( fen ), true );
    }
}