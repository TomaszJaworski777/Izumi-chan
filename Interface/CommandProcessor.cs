using Engine;

namespace Interface;

internal abstract class CommandProcessor(ChessEngine chessEngine)
{
    protected ChessEngine _chessEngine = chessEngine;

    public abstract void ProcessCommand( string[] commandSplit );
}
