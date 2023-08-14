namespace Greg
{
    internal class UciCommandManager
    {
        private readonly ChessEngine _chessEngine;

        public UciCommandManager(ChessEngine chessEngine )
        {
            _chessEngine = chessEngine;
        }
        
        public void ProcessCommand( string command )
        {
            ReadOnlySpan<string> commandSplit = command.Split(' ');

            switch (commandSplit[0])
            {
                case "uci": UciInitCommand(); break;
                case "isready": IsReadyCommand(); break;
                case "position": SetPositionCommand( commandSplit[1..] ); break;
                case "perft": PerftCommand( commandSplit[1..] ); break;
                case "splitperft": SplitPerftCommand ( commandSplit[1..] ); break;
                case "go": SearchCommand( commandSplit[1..] ); break;
                case "quit": QuitCommand(); break;
            }
        }

        private void UciInitCommand()
        {
            Console.WriteLine( $"id name {UciConfig.Name} v{UciConfig.Version}" );
            Console.WriteLine( $"id author {UciConfig.Author}" );
            Console.WriteLine( "uciok" );
        }

        private void IsReadyCommand()
        {
            Console.WriteLine( "readyok" );
        }

        private void SetPositionCommand( ReadOnlySpan<string> parameters )
        {
            if (parameters.Length == 0)
                return;

            if (parameters[0] == "startpos")
                _chessEngine.ChangePosition();
            else if (parameters[0] == "fen")
                _chessEngine.ChangePosition( parameters[1] + ' ' + parameters[2] + ' ' + parameters[3] + ' ' + parameters[4] + ' ' + parameters[5] + ' ' + parameters[6] );
            else
                return;

            bool hasAdditionalMoves = false;
            for (int parameterIndex = 0; parameterIndex < parameters.Length; parameterIndex++)
            {
                string parameter = parameters[parameterIndex];
                if (parameter == "moves")
                {
                    hasAdditionalMoves = true;
                    continue;
                }

                if (hasAdditionalMoves)
                    _chessEngine.MakeMove( parameter );
            }

            _chessEngine.DrawBoard();
        }

        private void PerftCommand( ReadOnlySpan<string> parameters )
        {
            switch (parameters.Length)
            {
                case 0: 
                    return;
                case 1:
                    _chessEngine.Perft( int.Parse( parameters[0] ) );
                    break;
                case 2:
                    _chessEngine.Perft( int.Parse( parameters[0] ), true );
                    break;
                case 7:
                    _chessEngine.Perft( int.Parse( parameters[0] ), parameters[1] + ' ' + parameters[2] + ' ' + parameters[3] + ' ' + parameters[4] + ' ' + parameters[5] + ' ' + parameters[6] );
                    break;
                case 8:
                    _chessEngine.Perft( int.Parse( parameters[0] ), parameters[1] + ' ' + parameters[2] + ' ' + parameters[3] + ' ' + parameters[4] + ' ' + parameters[5] + ' ' + parameters[6], true );
                    break;
            }
        }

        private void SplitPerftCommand( ReadOnlySpan<string> parameters )
        {
            switch (parameters.Length)
            {
                case 0:
                    return;
                case 1:
                    _chessEngine.SplitPerft( int.Parse( parameters[0] ) );
                    break;
                case 7:
                    _chessEngine.SplitPerft( int.Parse( parameters[0] ), parameters[1] + ' ' + parameters[2] + ' ' + parameters[3] + ' ' + parameters[4] + ' ' + parameters[5] + ' ' + parameters[6] );
                    break;
            }
        }

        private void SearchCommand( ReadOnlySpan<string> parameters )
        {
            bool infinite = false;
            int wTime = 0, bTime = 0;
            int depth = 0;

            for (int parameterIndex = 0; parameterIndex < parameters.Length; parameterIndex++)
            {
                string parameter = parameters[parameterIndex];
                switch (parameter)
                {
                    case "depth":
                        depth = int.Parse( parameters[parameterIndex + 1] );
                        break;
                    case "wtime":
                        wTime = int.Parse( parameters[parameterIndex + 1] );
                        break;
                    case "btime":
                        bTime = int.Parse( parameters[parameterIndex + 1] );
                        break;
                    case "movetime":
                        wTime = int.Parse( parameters[parameterIndex + 1] ) * 25;
                        bTime = int.Parse( parameters[parameterIndex + 1] ) * 25;
                        break;
                    case "infinite":
                        infinite = true;
                        break;
                }
            }

            if (infinite)
                _chessEngine.Search();
            else if (wTime > 0 || bTime > 0)
                _chessEngine.Search( wTime, bTime );
            else if (depth > 0)
                _chessEngine.Search( depth );
        }

        private void QuitCommand()
        {
            Environment.Exit( 0 );
        }
    }
}
