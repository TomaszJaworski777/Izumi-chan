using Engine.Board;

namespace Engine.Search
{
    internal ref struct SearchParameters( BoardData board, int depth = 100, int whiteTime = int.MaxValue, int blackTime = int.MaxValue )
    {
        public readonly BoardData Board = board;
        public readonly int Depth = depth;
        public readonly int WhiteTime = whiteTime;
        public readonly int BlackTime = blackTime;
    }
}