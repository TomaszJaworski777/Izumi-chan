﻿using Engine.Board;

namespace Engine.Search
{
    public ref struct SearchParameters( ref BoardData board, int depth = 100, int whiteTime = int.MaxValue, int blackTime = int.MaxValue, int whiteIncrement = 0, int blackIncrement = 0, int movesToGo = TimeManager.TimeDivider )
    {
        public BoardData Board = board;
        public readonly int Depth = depth;
        public readonly int WhiteTime = whiteTime;
        public readonly int BlackTime = blackTime;
        public readonly int WhiteIncrement = whiteIncrement;
        public readonly int BlackIncrement = blackIncrement;
        public readonly int MovesToGo = movesToGo;
    }
}