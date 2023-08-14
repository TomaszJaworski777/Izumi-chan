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
                Console.WriteLine( $"Depth: {i}, Nodes: {PerftInternal( i, board, false )}" + (logger ? "" : $"Nps: {_nodePerSecondTracker.LatestResult}") );
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
    }
}
