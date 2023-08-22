using System.Runtime.CompilerServices;

namespace Izumi.Structures.Data
{
    internal struct Square
    {
        private BitboardInt _bitboard;
        /* Reserved bits from LSB (Total 12/32 reserved):
         *      6 bits - square index
         *      3 bits - rank
         *      3 bits - file
         */

        public Square(int index)
        {
            var file = index % 8;
            var rank = (index - file) / 8;
            InitializeBitboard(rank, file);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square(string signature) => InitializeBitboard(int.Parse(signature[1].ToString()) - 1, signature[0] - 'a');

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square(int rank, int file) => InitializeBitboard(rank, file);

        private void InitializeBitboard(int rank, int file)
        {
            SquareIndex = rank * 8 + file;
            Rank = rank;
            File = file;
        }

        public int SquareIndex
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _bitboard.GetValueChunk(0, 63);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private set => _bitboard.SetValueChunk(0, 63, value);
        }

        public int Rank
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _bitboard.GetValueChunk(6, 7);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private set => _bitboard.SetValueChunk(6, 7, value);
        }

        public int File
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _bitboard.GetValueChunk(9, 7);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private set => _bitboard.SetValueChunk(9, 7, value);
        }

        public override string ToString() => $"{(char)('a' + File)}{Rank + 1}";

        public static implicit operator int(Square square) => square.SquareIndex;
    }
}
