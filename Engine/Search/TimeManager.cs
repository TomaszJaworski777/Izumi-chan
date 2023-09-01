using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Engine.Options;
using Engine.Perft;

namespace Engine.Search
{
    public readonly struct TimeManager
    {
        public const int TimeDivider = 20;

        private readonly int _timeForMove;
        private readonly Stopwatch _stopwatch = new();

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public TimeManager( int time, int increment, int movesToGo = TimeDivider )
        {
            if (time == int.MaxValue)
            {
                _timeForMove = int.MaxValue;
                return;
            }

            time -= (int)EngineOptions.GetOption( EngineOptions.MoveOverheadKey );
            _timeForMove = Math.Min( Math.Max( (time + increment) / movesToGo, 20 ), time * 8 / 10 );

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
