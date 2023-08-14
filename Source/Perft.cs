namespace Greg
{
    internal class Perft
    {
        private ulong _currentNps;
        private ulong _latestNps;
        private DateTime _latestTimeStamp;

        public void Execute(int depth, Board board, bool splitPerft )
        {
            if (splitPerft)
                PerftInternal( depth, board, true );

            _latestTimeStamp = DateTime.Now;
            for (int i = 1; i < depth; i++)
            {
                Console.WriteLine( $"Depth: {i}, Nodes: {PerftInternal(i, board, false)}, Nps: {_latestNps}" );
            }
        }

        public ulong PerftInternal( int depth, Board board, bool splitPerft = false )
        {
            if (depth == 0)
            {
                _currentNps++;
                return 1UL;
            }

            ulong count = 0;
            var moves = MoveController.GeneratePseudoLegalMoves( board );

            if ((DateTime.Now - _latestTimeStamp).TotalMilliseconds > 1000)
            {
                _latestTimeStamp = DateTime.Now;
                _latestNps = _currentNps;
                _currentNps = 0;
            }

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
