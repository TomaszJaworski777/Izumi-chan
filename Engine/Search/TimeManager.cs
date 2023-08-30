using Engine.Perft;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Engine.Search
{
    public struct TimeManager
    {
        public const int TimeDivider = 20;

        private readonly int _timeForMove;
        private readonly Stopwatch _stopwatch = new();

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public TimeManager( int time, int increment, int movesToGo = TimeDivider )
        {
            if (movesToGo == 0)
                movesToGo = TimeDivider;

            _timeForMove = Math.Clamp( (time + increment) / movesToGo, 35, time * 8 / 10);
            _stopwatch.Start();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Update()
        {
            if (_stopwatch.ElapsedMilliseconds >= _timeForMove)
            {
                PerftTest.CancellationToken = true;
                SearchSystem.CancellationToken = true;
                _stopwatch.Stop();
            }
        }
    }
}
