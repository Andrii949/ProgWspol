using System.Diagnostics;

namespace ConcurrentProgramming.Data
{
    internal interface IElapsedTimeProvider
    {
        double GetElapsedSeconds();

        void Reset();
    }

    internal sealed class StopwatchElapsedTimeProvider : IElapsedTimeProvider
    {
        internal StopwatchElapsedTimeProvider()
        {
            Stopwatch.Start();
        }

        public double GetElapsedSeconds()
        {
            double elapsedSeconds = Stopwatch.Elapsed.TotalSeconds;
            Stopwatch.Restart();
            return elapsedSeconds;
        }

        public void Reset()
        {
            Stopwatch.Restart();
        }

        private readonly Stopwatch Stopwatch = new();
    }
}
