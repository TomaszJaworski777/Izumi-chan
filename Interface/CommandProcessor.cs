using Engine;

namespace Interface;

internal abstract class CommandProcessor
{
    protected ChessEngine _chessEngine;

    public CommandProcessor( ChessEngine chessEngine )
    {
        _chessEngine = chessEngine;
    }

    public abstract void ProcessCommand( string[] commandSplit );
}
