using Engine.Data.Bitboards;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class BitboardTests
    {
        [TestMethod]
        public void GetBitValue_Test()
        {
            Bitboard bitboard = 246441;

            Assert.IsTrue( bitboard.GetBitValue( 0 ) > 0 );
            Assert.IsTrue( bitboard.GetBitValue( 1 ) == 0 );
            Assert.IsTrue( bitboard.GetBitValue( 3 ) > 0 );
            Assert.IsTrue( bitboard.GetBitValue( 7 ) > 0 );
            Assert.IsTrue( bitboard.GetBitValue( 8 ) == 0 );
            Assert.IsTrue( bitboard.GetBitValue( 13 ) == 0 );
            Assert.IsTrue( bitboard.GetBitValue( 17 ) > 0 );
        }

        [TestMethod]
        public void SetBitValue_Test()
        {
            Bitboard bitboard = 0;
            bitboard.SetBitToOne( 7 );
            Assert.IsTrue( bitboard.GetBitValue( 7 ) > 0 );
            bitboard.SetBitToOne( 47 );
            Assert.IsTrue( bitboard.GetBitValue( 47 ) > 0 );
            bitboard.SetBitToOne( 63 );
            Assert.IsTrue( bitboard.GetBitValue( 63 ) > 0 );
            bitboard.SetBitToZero( 7 );
            Assert.IsTrue( bitboard.GetBitValue( 7 ) == 0 );

            BitboardShort bitShort = 0;
            bitShort.SetBitToOne( 7 );
            Assert.IsTrue( bitShort.GetBitValue( 7 ) > 0 );
            bitShort.SetBitToOne( 15 );
            Assert.IsTrue( bitShort.GetBitValue( 15 ) > 0 );
            bitShort.SetBitToOne( 6 );
            Assert.IsTrue( bitShort.GetBitValue( 6 ) > 0 );
            bitShort.SetBitToZero( 7 );
            Assert.IsTrue( bitShort.GetBitValue( 7 ) == 0 );
        }

        [TestMethod]
        public void GetValueChunk_Test()
        {
            Bitboard bitboard = 246441;
            Assert.IsTrue( bitboard.GetValueChunk( 14, 7 ) == 7 );
            Assert.IsTrue( bitboard.GetValueChunk( 3, 127 ) == 85 );

            BitboardShort bitShort = 617;
            Assert.IsTrue( bitShort.GetValueChunk( 5, 3 ) == 3 );
            Assert.IsTrue( bitShort.GetValueChunk( 0, 15 ) == 9 );
        }

        [TestMethod]
        public void SetValueChunk_Test()
        {
            Bitboard bitboard = 246441;
            Assert.IsTrue( bitboard.GetValueChunk( 14, 7 ) == 7 );
            bitboard.SetValueChunk( 14, 7, 5);
            Assert.IsTrue( bitboard.GetValueChunk( 14, 7 ) == 5 );
            bitboard.SetValueChunk( 3, 127, 127 );
            bitboard.SetBitToZero( 10 );
            Assert.IsTrue( bitboard.GetValueChunk( 4, 127 ) == 63 );

            BitboardShort bitShort = 617;
            bitShort.SetValueChunk( 10, 7, 5 );
            Assert.IsTrue( bitShort.GetValueChunk( 10, 7 ) == 5 );
        }
    }
}