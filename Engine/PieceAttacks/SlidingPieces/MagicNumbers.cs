using Engine.Data.Bitboards;
using Engine.Utils;

namespace Engine.PieceAttacks.SlidingPieces;

//magic numbers used to generate sliding pieces attacks (https://www.chessprogramming.org/Magic_Bitboards)
public class MagicNumbers
{
    public ulong[] BishopMagicNumbers =
        {
            18298089890717764,
            620197606720032,
            4592136120991812,
            10953954962744476672,
            11338438886294528,
            4759478007813506048,
            2306969476133814274,
            5837233565811607564,
            6971642628454746210,
            162252740616538248,
            2324147687450484752,
            2273807906144257,
            6759815213031424,
            4899918671717344836,
            9304445935612596304,
            9263904579668673536,
            4582906786742529,
            1226105831843189760,
            4630263435677941768,
            2315976211523701786,
            145241691436107904,
            144258300687417860,
            72198417761769476,
            1161986020208676864,
            13956672846154490113,
            149190843531002016,
            9234668446967660608,
            4665733629186474016,
            582099185436729344,
            4613940017400677456,
            4648278177784005120,
            756752072006722064,
            577692241835852416,
            9223534782373171328,
            9511638250223043616,
            9223407359203213440,
            2269426915475488,
            567383434987544,
            13876224341865399296,
            2324042679729132288,
            601234967703330848,
            77687660776784912,
            2816949360919568,
            36064395066869764,
            2260600268800067,
            486397560416668680,
            446001533009264768,
            288552576117379589,
            397077698108983296,
            2414000053155823616,
            1152932508867821585,
            577589968472903712,
            2341995025119977536,
            4611897133386581008,
            9801244596582285440,
            2321658541242384416,
            2378466028249424000,
            2314861241186750552,
            583216710661115936,
            22545503111741969,
            144396665506365952,
            19141965134842368,
            2311824361629305360,
            4504776583286944
        };
    public ulong[] RookMagicNumbers =
        {
            9259401108760043536,
            18014535952633856,
            2341880671047254144,
            9547635642440159233,
            324261922083963904,
            144154806198534656,
            612491748362420352,
            144128393036701762,
            576601490338103304,
            633337219653633,
            576601627231780992,
            2305983781070180352,
            360991812250763392,
            108649362753716232,
            4621256176225789953,
            4756364158085137924,
            599783601356800,
            297238400045416480,
            315252524217012224,
            576602039681814528,
            217299231910659200,
            288239177613709344,
            2311016215717609984,
            577588851241982020,
            10394378310862774406,
            7521011938204073992,
            4755871721277391368,
            4769329599719407744,
            1317303994812731400,
            2380156803259433088,
            776581629347088,
            148649857496679427,
            36099166308401280,
            1216613944664576,
            18155204759265280,
            140806216222720,
            18586155301733376,
            162133986787721728,
            6070853981389721602,
            1197962037473312897,
            9224638812762832896,
            9024792514543648,
            145273275940928,
            2326109242165067904,
            75444089918619776,
            9241456821825700112,
            9656280723119800404,
            288793618171428897,
            4539097574146560,
            18647752254718720,
            5188219339036197376,
            11673347835473955072,
            1127549308601472,
            4992521679098348288,
            2424203245510525056,
            14412082308385604096,
            563500784034058,
            18014540247629841,
            4611721207095234625,
            293021017050187809,
            1198238984581435397,
            4900479625920516098,
            88134910476548,
            4039729175542664194
        };

    public void GenerateNewMagicValues( Array64<int> bishopRelevantBitCount, Array64<int> rookRelevantBitCount )
    {
        BishopAttacks bishopAttacks = new(this);
        RookAttacks rookAttacks = new(this);

        for (int i = 0; i < 64; i++)
        {
            BishopMagicNumbers[i] = FindMagicNumber( i, bishopRelevantBitCount[i], true, bishopAttacks, rookAttacks );
            RookMagicNumbers[i] = FindMagicNumber( i, rookRelevantBitCount[i], false, bishopAttacks, rookAttacks );
        }
    }

    private Bitboard FindMagicNumber( int squareIndex, int relevantBitsCount, bool forBishop, BishopAttacks bishopAttacks, RookAttacks rookAttacks )
    {
        Span<Bitboard> occupancyPatterns = stackalloc Bitboard[4096];
        Span<Bitboard> attacks = stackalloc Bitboard[4096];
        Span<Bitboard> checkedAttacks = stackalloc Bitboard[4096];

        Bitboard attackMask = forBishop ? bishopAttacks.GetBishopRelevantBits(squareIndex) : rookAttacks.GetRookRelevantBits(squareIndex);

        int occupancyIndexCount = 1 << relevantBitsCount;

        for (int index = 0; index < occupancyIndexCount; index++)
        {
            occupancyPatterns[index] = bishopAttacks.SetOccupancy( index, attackMask );
            attacks[index] = forBishop ? bishopAttacks.GetFullBishopAttackPattern( squareIndex, occupancyPatterns[index] ) : rookAttacks.GetFullRookAttackPattern( squareIndex, occupancyPatterns[index] );
        }

        for (int iterationCount = 0; iterationCount < 1000000000; iterationCount++)
        {
            ulong magicNumber = GetRandom64() & GetRandom64() & GetRandom64();

            if (((Bitboard)(attackMask * magicNumber & 0xFF00000000000000)).BitCount < 6)
                continue;

            checkedAttacks.Clear();

            bool incorrectNumber = false;
            for (int index = 0; !incorrectNumber && index < occupancyIndexCount; index++)
            {
                int magicIndex = (int)(occupancyPatterns[index] * magicNumber >> 64 - relevantBitsCount);

                if (checkedAttacks[magicIndex] == 0)
                    checkedAttacks[magicIndex] = attacks[index];
                else if (checkedAttacks[magicIndex] != attacks[index])
                    incorrectNumber = true;
            }

            if (!incorrectNumber)
                return magicNumber;
        }

        return new();
    }

    private ulong GetRandom64()
    {
        Random random = new();

        ulong u1, u2, u3, u4;
        u1 = (ulong)random.Next() & 0xFFFF; u2 = (ulong)random.Next() & 0xFFFF;
        u3 = (ulong)random.Next() & 0xFFFF; u4 = (ulong)random.Next() & 0xFFFF;
        return u1 | u2 << 16 | u3 << 32 | u4 << 48;
    }
}
