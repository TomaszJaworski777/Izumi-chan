using System.Runtime.CompilerServices;

namespace Izumi
{
    internal struct NodePerSecondTracker
    {
        private ulong _currentNps;
        private ulong _latestNps;
        private DateTime _latestTimeStamp;
        private bool logger;

        public ulong LatestResult => _latestNps;

        public NodePerSecondTracker(bool logger)
        {
            this.logger = logger;
            _currentNps = 0;
            _latestNps = 0;
            _latestTimeStamp = DateTime.Now;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Reset()
        {
            _currentNps = 0;
            _latestNps = 0;
            _latestTimeStamp = DateTime.Now;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void AddNode() => _currentNps++;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update()
        {
            if ((DateTime.Now - _latestTimeStamp).TotalMilliseconds >= 1000)
            {
                _latestTimeStamp = DateTime.Now;
                _latestNps = _currentNps;
                _currentNps = 0;
                if (logger)
                    Console.WriteLine( $"info nps {_latestNps}" );
            }
        }
    }
}
