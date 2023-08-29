using Engine.Perft;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Engine.Search
{
    public struct TimeManager
    {
        public const int TimeDivider = 20;

        private readonly int _timeRemaning;
        private readonly Stopwatch _stopwatch = new();

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public TimeManager( int time )
        {
            _timeRemaning = Math.Clamp( time / TimeDivider, 15, int.MaxValue );
            _stopwatch.Start();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Update()
        {
            if(_stopwatch.ElapsedMilliseconds >= _timeRemaning)
            {
                PerftTest.CancellationToken = true;
                SearchSystem.CancellationToken = true;
                _stopwatch.Stop();
            }    
        }
    }
}
