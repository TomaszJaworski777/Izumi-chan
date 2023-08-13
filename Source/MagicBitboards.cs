namespace Greg
{
    internal static class MagicBitboards
    {
        public static Array64<ulong> BishopMagicNumbers = default; 
        public static Array64<ulong> RookMagicNumbers = default; 

        static MagicBitboards()
        {
            for (int i = 0; i < 64; i++)
            {
                BishopMagicNumbers[i] = PieceAttack.FindMagicNumber( i, PieceAttack.BishopRelevantBitCountForSquare[i], true ).Value;
                RookMagicNumbers[i] = PieceAttack.FindMagicNumber( i, PieceAttack.RookRelevantBitCountForSquare[i], false ).Value;
            }
        }
    }
}
