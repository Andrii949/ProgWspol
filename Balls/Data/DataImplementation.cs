
using System;
using System.Diagnostics;

namespace ConcurrentProgramming.Data
{
    internal class DataImplementation : DataAbstractAPI
    {
        #region ctor

        public DataImplementation()
          : this(
              new FileDiagnosticLogger(Path.Combine(AppContext.BaseDirectory, "ball-diagnostics.log")),
              new StopwatchElapsedTimeProvider(),
              startTimer: true)
        { }

        internal DataImplementation(IDiagnosticLogger diagnosticLogger, IElapsedTimeProvider elapsedTimeProvider, bool startTimer)
        {
            DiagnosticLogger = diagnosticLogger ?? throw new ArgumentNullException(nameof(diagnosticLogger));
            ElapsedTimeProvider = elapsedTimeProvider ?? throw new ArgumentNullException(nameof(elapsedTimeProvider));
            if (startTimer)
                MoveTimer = new Timer(Move, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(20));
        }

        #endregion ctor

        #region DataAbstractAPI

        public override void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler)
        {
            ThrowIfDisposed();
            if (numberOfBalls < 0)
                throw new ArgumentOutOfRangeException(nameof(numberOfBalls));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));

            lock (BallsLock)
            {
                ClearBalls();
                ElapsedTimeProvider.Reset();
                for (int i = 0; i < numberOfBalls; i++)
                {
                    Vector startingPosition = new(
                      RandomGenerator.NextDouble() * (TableWidth - BallDiameter),
                      RandomGenerator.NextDouble() * (TableHeight - BallDiameter));
                    Vector initialVelocity = CreateVelocity();
                    Ball newBall = new(i, startingPosition, initialVelocity, BallDiameter, BallMass);
                    BallsList.Add(newBall);
                    upperLayerHandler(startingPosition, newBall);
                }
            }
        }

        public override void Stop()
        {
            ThrowIfDisposed();

            lock (BallsLock)
            {
                ClearBalls();
            }
        }

        #endregion DataAbstractAPI

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    MoveTimer?.Dispose();
                    lock (BallsLock)
                    {
                        ClearBalls();
                    }
                    DiagnosticLogger.Dispose();
                }
                Disposed = true;
            }
            else
                throw new ObjectDisposedException(nameof(DataImplementation));
        }

        public override void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable

        #region private

        private bool Disposed = false;

        private readonly Timer? MoveTimer;
        private readonly Random RandomGenerator = new();
        private readonly object BallsLock = new();
        private readonly List<Ball> BallsList = [];
        private readonly IDiagnosticLogger DiagnosticLogger;
        private readonly IElapsedTimeProvider ElapsedTimeProvider;

        internal const double BallDiameter = 20.0;
        internal const double BallMass = 1.0;
        internal const double TableHeight = 420.0;
        internal const double TableWidth = 700.0;

        private void ClearBalls()
        {
            BallsList.Clear();
        }

        private void ThrowIfDisposed()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataImplementation));
        }

        private void Move(object? x)
        {
            double elapsedSeconds = ElapsedTimeProvider.GetElapsedSeconds();
            if (elapsedSeconds <= 0.0)
                return;

            lock (BallsLock)
            {
                foreach (Ball item in BallsList)
                {
                    IVector velocity = item.Velocity;
                    double nextX = item.Position.x + velocity.x * elapsedSeconds;
                    double nextY = item.Position.y + velocity.y * elapsedSeconds;

                    item.Move(new Vector(nextX, nextY), velocity);
                    DiagnosticLogger.TryLog(new DiagnosticRecord(
                      DateTime.UtcNow.Ticks,
                      item.Id,
                      item.Position.x,
                      item.Position.y,
                      item.Velocity.x,
                      item.Velocity.y));
                }
            }
        }

        private Vector CreateVelocity()
        {
            double componentX = 0;
            double componentY = 0;

            while (Math.Abs(componentX) < 12.5)
                componentX = (RandomGenerator.NextDouble() - 0.5) * 300.0;

            while (Math.Abs(componentY) < 12.5)
                componentY = (RandomGenerator.NextDouble() - 0.5) * 300.0;

            return new Vector(componentX, componentY);
        }

        #endregion private

        #region TestingInfrastructure

        [Conditional("DEBUG")]
        internal void CheckBallsList(Action<IEnumerable<IBall>> returnBallsList)
        {
            lock (BallsLock)
            {
                returnBallsList(BallsList.Cast<IBall>().ToArray());
            }
        }

        [Conditional("DEBUG")]
        internal void CheckNumberOfBalls(Action<int> returnNumberOfBalls)
        {
            lock (BallsLock)
            {
                returnNumberOfBalls(BallsList.Count);
            }
        }

        [Conditional("DEBUG")]
        internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
        {
            returnInstanceDisposed(Disposed);
        }

        #endregion TestingInfrastructure
    }
}
