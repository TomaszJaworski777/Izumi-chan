using System.Collections;

namespace Greg
{
    internal class Perft
    {
        private NodePerSecondTracker _nodePerSecondTracker = new(false);

        public void Execute( int depth, Board board, bool splitPerft, bool logger = false )
        {
            _nodePerSecondTracker = new( logger );

            if (splitPerft)
            {
                PerftInternal( depth, board, true );
                return;
            }

            for (int i = 1; i < depth + 1; i++)
            {
                Console.WriteLine( $"Depth: {i}, Nodes: {PerftInternal( i, board, false )}" + (logger ? "" : $", Nps: {_nodePerSecondTracker.LatestResult}") );
            }
        }

        private ulong PerftInternal( int depth, Board board, bool splitPerft = false )
        {
            if (depth == 0)
            {
                _nodePerSecondTracker.AddNode();
                return 1UL;
            }

            _nodePerSecondTracker.Update();

            ulong count = 0;
            var moves = MoveController.GeneratePseudoLegalMoves( board );

            for (int i = 0; i < moves.Length; i++)
            {
                Board copy = board;
                if (!copy.MakeMove( moves[i] ))
                    continue;
                ulong result = PerftInternal( depth - 1, copy );
                count += result;
                if (splitPerft)
                    Console.WriteLine( $"{moves[i].ToString( board )} - {result}" );
            }

            return count;
        }

        public void PerftTest()
        {
            _nodePerSecondTracker = new( false );
            ulong fullNodes = 0;

            Console.WriteLine("Starting perft test...");
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
                    Console.WriteLine( $"FEN: {fen}" );
                    Console.WriteLine( $"EXPECTED: {nodes}" );
                    Console.WriteLine( $"RESULT: {result}" );
                    Console.WriteLine();
                }

            }
            Console.WriteLine( $"Done! Nodes searched: {fullNodes}, Nps: {_nodePerSecondTracker.LatestResult}" );
        }
    }
}
