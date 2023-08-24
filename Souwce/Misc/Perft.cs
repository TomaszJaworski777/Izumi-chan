using Izumi.Structures;
using Izumi.Systems;

namespace Izumi.Misc
{
    internal class Perft
    {
        private NodePerSecondTracker _nodePerSecondTracker = new(false);
        private Dictionary<(ulong, int), TranspositionTableEntry> _perftTT = new();

        public void Execute(int depth, Board board, bool splitPerft, bool logger = false)
        {
            _nodePerSecondTracker = new(logger);
            _perftTT.Clear();

            if (splitPerft)
            {
                PerftInternal(depth, board, true);
                return;
            }

            for (int i = 1; i < depth + 1; i++)
            {
                Console.WriteLine($"Depth: {i}, Nyodies: {PerftInternal(i, board, false)}" + (logger ? "" : $", Nps: {_nodePerSecondTracker.LatestResult}"));
            }
        }

        private ulong PerftInternal(int depth, Board board, bool splitPerft = false)
        {
            if (depth == 0)
            {
                _nodePerSecondTracker.AddNode();
                return 1UL;
            }

            _nodePerSecondTracker.Update();

            if(_perftTT.TryGetValue((board.ZobristKey, depth), out TranspositionTableEntry entry ))
            {
                for (ulong i = 0; i < entry.PositionKey; i++)
                {
                    _nodePerSecondTracker.AddNode();
                }

                _nodePerSecondTracker.Update();
                return entry.PositionKey;
            }

            ulong count = 0;
            var moves = MoveController.GeneratePseudoLegalMoves(board);

            Board copy;
            for (int i = 0; i < moves.Length; i++)
            {
                copy = board;
                if (!copy.MakeMove(moves[i]))
                    continue;
                ulong result = PerftInternal(depth - 1, copy);
                MoveHistory.RemoveLast();
                count += result;
                if (splitPerft)
                    Console.WriteLine($"{moves[i]} - {result}");
            }

            _perftTT.Add( (board.ZobristKey, depth), new TranspositionTableEntry()
            {
                PositionKey = count
            } );

            return count;
        }

        public void PerftTest()
        {
            _nodePerSecondTracker = new(false);
            ulong fullNodes = 0;

            Console.WriteLine("Stawting (・`w´・) p-p-pewft t-t-test...");
            ReadOnlySpan<string> tests = File.ReadLines(@"..\..\..\..\TestData\perftsuite.epd").ToArray().AsSpan();

            int index = 0;
            foreach (var test in tests)
            {
                index++;
                var split = test.Split(' ');
                ulong nodes = ulong.Parse(split[15]);
                Console.WriteLine($"Test {index}/{tests.Length}, Expected value: {nodes}");
                fullNodes += nodes;
                string fen = split[0] + ' ' + split[1] + ' ' + split[2] + ' ' + split[3] + ' ' + split[4] + ' ' + split[5];
                ulong result = PerftInternal(5, new Board(fen), false);
                if (result != nodes)
                {
                    Console.WriteLine();
                    Console.WriteLine($"F-F-FEN: {fen}");
                    Console.WriteLine($"EXPECTED: {nodes}");
                    Console.WriteLine($"WESUWT: {result}");
                    Console.WriteLine();
                }

            }
            Console.WriteLine($"Donye! ^-^ Nyodies searched: {fullNodes}, Nps: {_nodePerSecondTracker.LatestResult}");
        }
    }
}
