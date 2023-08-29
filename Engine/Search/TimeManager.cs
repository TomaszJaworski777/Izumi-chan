using Engine.Perft;
using System.Runtime.CompilerServices;

namespace Engine.Search
{
    [method: MethodImpl( MethodImplOptions.AggressiveInlining )]
    public struct TimeManager( int time )
    {
        public const int TimeDivider = 20;

        private readonly int _timeRemaning = Math.Clamp(time / TimeDivider, 20, int.MaxValue);
        private readonly DateTime _startTime = DateTime.Now;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Update()
        {
            TimeSpan timeSpan = DateTime.Now - _startTime;
            if(timeSpan.TotalMilliseconds >= _timeRemaning)
            {
                PerftTest.CancellationToken = true;
                SearchSystem.CancellationToken = true;
            }    
        }
    }
}
