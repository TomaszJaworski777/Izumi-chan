using System;
using System.Runtime.CompilerServices;
using Engine.Board;
using Engine.Move;

namespace Engine.Perft
{
    [method: MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ref struct PerftTest( ref BoardData testBoard )
    {
        public static bool CancellationToken;

        private BoardData _board = testBoard;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ulong TestPosition( int depth, bool divide )
        {
            CancellationToken = false;
            return InternalTestPosition( ref _board, depth, divide );
        }

        //executes perft test based on the given position (https://www.chessprogramming.org/Perft)
        private unsafe ulong InternalTestPosition(ref BoardData board, int depth, bool divide)
        {
            //counts nodes for final results
            if (depth <= 0)
                return 1UL;
            ulong result = 0;

            //break if cancelation token
            if (CancellationToken)
                return 0;

            //gets list of all pseudo moves
            MoveList moveList = new(stackalloc MoveData[218]);
            board.GenerateAllPseudoLegalMoves( ref moveList );

            //iterates through moves
            foreach (ref MoveData move in (Span<MoveData>)moveList)
            {
                //makes move and if its illegal then skips the iteration
                BoardData boardCopy = board;
                if (!boardCopy.MakeMove( move ))
                    continue;
                //get perft value of current move and then unmakes the move
                ulong newResult = InternalTestPosition(ref boardCopy, depth - 1, false );
                boardCopy.UnmakeMove();
                //prints divided perft if chosen (https://www.chessprogramming.org/Perft#Divide)
                if (divide)
                    Console.WriteLine( $"{move} - {newResult}" );
                //adds current move perft to the overall result
                result += newResult;
            }

            return result;  
        }
    }
}
