using System.Collections;

namespace Izumi
{
    internal class Perft
    {
        private NodePerSecondTracker _nodePerSecondTracker = new(false);
        private Dictionary<(ulong, int), ulong> _perftTable = new();

        public void Execute( int depth, Board board, bool splitPerft, bool logger = false )
        {
            _nodePerSecondTracker = new( logger );
            _perftTable.Clear();

            if (splitPerft)
            {
                PerftInternal( depth, board, true );
                return;
            }

            for (int i = 1; i < depth + 1; i++)
            {
                Console.WriteLine( $"Depth: {i}, Nyodies: {PerftInternal( i, board, false )}" + (logger ? "" : $", Nps: {_nodePerSecondTracker.LatestResult}") );
            }
        }

        private ulong PerftInternal( int depth, Board board, bool splitPerft = false )
        {
            _nodePerSecondTracker.Update();

            if (depth == 0)
            {
                _nodePerSecondTracker.AddNode();
                return 1UL;
            }

            if (_perftTable.TryGetValue( (board.ZobristKey, depth), out ulong storedCount ))
            {
                for (ulong i = 0; i < storedCount; i++)
                    _nodePerSecondTracker.AddNode();

                return storedCount;
            }

            ulong count = 0;
            var moves = MoveController.GeneratePseudoLegalMoves( board );

            Board copy;
            for (int i = 0; i < moves.Length; i++)
            {
                copy = board;
                if (!copy.MakeMove( moves[i] ))
                    continue;
                ulong result = PerftInternal( depth - 1, copy );
                count += result;
                if (splitPerft)
                    Console.WriteLine( $"{moves[i]} - {result}" );
            }

            _perftTable.Add( (board.ZobristKey, depth), count );

            return count;
        }

        public void PerftTest()
        {
            _nodePerSecondTracker = new( false );
            _perftTable.Clear();
            ulong fullNodes = 0;

            Console.WriteLine( "Stawting (・`w´・) p-p-pewft t-t-test..." );
            ReadOnlySpan<string> tests =  File.ReadLines( @"..\..\..\..\TestData\perftsuite.epd" ).ToArray().AsSpan();

            int index = 0;
            foreach (var test in tests)
            {
                index++;
                var split = test.Split( ' ' );
                ulong nodes = ulong.Parse( split[ 15 ] );
                Console.WriteLine($"Test {index}/{tests.Length}, Expected value: {nodes}");
                fullNodes += nodes;
                string fen = split[0] + ' ' + split[1] + ' ' + split[2] + ' ' + split[3] + ' ' + split[4] + ' ' + split[5];
                ulong result = PerftInternal(5, new Board(fen), false);
                if (result != nodes)
                {
                    Console.WriteLine();
                    Console.WriteLine( $"F-F-FEN: {fen}" );
                    Console.WriteLine( $"EXPECTED: {nodes}" );
                    Console.WriteLine( $"WESUWT: {result}" );
                    Console.WriteLine();
                }

            }
            Console.WriteLine( $"Donye! ^-^ Nyodies searched: {fullNodes}, Nps: {_nodePerSecondTracker.LatestResult}" );
        }
    }
}
