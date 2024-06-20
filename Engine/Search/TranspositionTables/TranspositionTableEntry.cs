using Engine.Move;
using System.Runtime.InteropServices;

namespace Engine.Search.TranspositionTables;

[StructLayout(LayoutKind.Sequential)]
public struct TranspositionTableEntry
{
    public ulong PositionKey;
    public MoveData BestMove;
    public byte Depth;
    public int Score;
    public TTFlag Flag;
}

public enum TTFlag : byte
{
    AlphaChanged,
    BetaCutoff,
    AlphaUnchanged
}
