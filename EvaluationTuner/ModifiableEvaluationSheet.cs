﻿using Engine.Evaluation;
using System.Text;

namespace EvaluationTuner;

internal struct ModifiableEvaluationSheet
{
    public ushort BishopPairMidgameBonus;
    public ushort BishopPairEndgameBonus;

    public int DoublePawnMidgamePenalty;
    public int DoublePawnEndgamePenalty;

    public byte[] PiecePhase;

    public ushort[] PieceValues;

    public sbyte[] PstsTable;

    public ModifiableEvaluationSheet()
    {
        BishopPairMidgameBonus = 0;
        BishopPairEndgameBonus = 0;

        DoublePawnMidgamePenalty = 0;
        DoublePawnEndgamePenalty = 0;

        PiecePhase = EvaluationSheet.PiecePhase.ToArray();

        PieceValues = EvaluationSheet.PieceValues.ToArray();

        PstsTable = new sbyte[EvaluationSheet.PstsTable.Length];
    } 

    public void Capture()
    {
        StringBuilder stringBuilder = new();

        stringBuilder.Append( $"BishopPairMidgameBonus = {BishopPairMidgameBonus};\n" );
        stringBuilder.Append( $"BishopPairEndgameBonus = {BishopPairEndgameBonus};\n\n" );

        stringBuilder.Append( $"DoublePawnMidgamePenalty = {DoublePawnMidgamePenalty};\n" );
        stringBuilder.Append( $"DoublePawnEndgamePenalty = {DoublePawnEndgamePenalty};\n\n" );

        stringBuilder.Append( $"PieceValues = [ " );
        for (int i = 0; i < PieceValues.Length; i++)
        {
            stringBuilder.Append( PieceValues[i] );
            if(i != PieceValues.Length - 1)
                stringBuilder.Append( ", " );
        }
        stringBuilder.Append( $" ];\n\n" );

        stringBuilder.Append( $"PstsTable = [ " );
        for (int i = 0; i < PstsTable.Length; i++)
        {
            if(i % 8 == 0)
                stringBuilder.Append( '\n' );

            stringBuilder.Append( PstsTable[i] );
            if (i != PstsTable.Length - 1)
                stringBuilder.Append( ", " );
        }
        stringBuilder.Append( $" ];\n\n" );

        File.WriteAllText(@"../../../output.txt", stringBuilder.ToString() );
    }
}
