using System.Runtime.CompilerServices;

namespace Greg
{
    internal class ChessEngine
    {
        private const string startFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        private readonly Perft _perft = new();
        private readonly Search _search = new();

        private Board _currentBoard;

        public ChessEngine()
        {
            _currentBoard = new Board( startFen );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void ChangePosition( string fen = startFen ) => _currentBoard = new Board( fen );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void MakeMove( string move ) => _currentBoard.MakeMove( new Move( move, _currentBoard ) );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Perft( int depth, bool logger = false ) => _perft.Execute( depth, _currentBoard, false, logger );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Perft( int depth, string fen, bool logger = false ) => _perft.Execute( depth, new Board( fen ), false, logger );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void SplitPerft( int depth ) => _perft.Execute( depth, _currentBoard, true );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void SplitPerft( int depth, string fen ) => _perft.Execute( depth, new Board( fen ), true );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Search( int depth ) => _search.Execute( _currentBoard, depth );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Search( int wTime, int bTime ) => _search.Execute( _currentBoard, whiteTime: wTime, blackTime: bTime );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Search() => _search.Execute( _currentBoard, infinite: true );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void DrawBoard() => _currentBoard.DrawBoard();
    }
}