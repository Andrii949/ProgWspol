
using System.Diagnostics;
using System.Timers;

namespace ConcurrentProgramming.Data
{
    internal class DataImplementation : DataAbstractAPI
    {
        #region ctor

        public DataImplementation()
          : this(
              new FileDiagnosticLogger(Path.Combine(AppContext.BaseDirectory, "ball-diagnostics.log")),
              enableMovementTimer: true)
        { }

        internal DataImplementation(IDiagnosticLogger diagnosticLogger, bool enableMovementTimer)
        {
            DiagnosticLogger = diagnosticLogger ?? throw new ArgumentNullException(nameof(diagnosticLogger));
            EnableMovementTimer = enableMovementTimer;
            MovementTimer = new System.Timers.Timer(MovementIntervalMilliseconds)
            {
                AutoReset = true
            };
            MovementTimer.Elapsed += MovementTimerElapsed;
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

            lock (MovementLock)
            {
                MovementTimer.Stop();
                lock (BallsLock)
                {
                    ClearBalls();
                    for (int i = 0; i < numberOfBalls; i++)
                    {
                        Vector startingPosition = new(
                          RandomGenerator.NextDouble() * (TableWidth - BallDiameter),
                          RandomGenerator.NextDouble() * (TableHeight - BallDiameter));
                        Vector initialVelocity = CreateVelocity();
                        Ball newBall = new(i, startingPosition, initialVelocity, BallDiameter, BallMass);
                        newBall.NewPositionNotification += LogBallPosition;
                        BallsList.Add(newBall);
                        upperLayerHandler(startingPosition, newBall);
                    }
                }
                LastMovementTime = DateTime.Now;
                if (EnableMovementTimer)
                    MovementTimer.Start();
            }
        }

        public override void Stop()
        {
            ThrowIfDisposed();

            lock (MovementLock)
            {
                MovementTimer.Stop();
                lock (BallsLock)
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
                    lock (MovementLock)
                    {
                        MovementTimer.Stop();
                        MovementTimer.Elapsed -= MovementTimerElapsed;
                        MovementTimer.Dispose();
                        lock (BallsLock)
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

        private readonly Random RandomGenerator = new();
        private readonly object BallsLock = new();
        private readonly object MovementLock = new();
        private readonly List<Ball> BallsList = [];
        private readonly IDiagnosticLogger DiagnosticLogger;
        private readonly bool EnableMovementTimer;
        private readonly System.Timers.Timer MovementTimer;
        private DateTime LastMovementTime;

        internal const double BallDiameter = 20.0;
        internal const double BallMass = 1.0;
        internal const double MovementIntervalMilliseconds = 20.0;
        internal const double TableHeight = 420.0;
        internal const double TableWidth = 700.0;

        private void ClearBalls()
        {
            foreach (Ball ball in BallsList)
                ball.NewPositionNotification -= LogBallPosition;
            BallsList.Clear();
        }

        private void ThrowIfDisposed()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataImplementation));
        }

        private void LogBallPosition(object? sender, IVector position)
        {
            if (sender is not Ball ball)
                return;

            IVector velocity = ball.Velocity;
            DiagnosticLogger.TryLog(new DiagnosticRecord(
              DateTime.UtcNow.Ticks,
              ball.Id,
              position.x,
              position.y,
              velocity.x,
              velocity.y));
        }

        private void MovementTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            lock (MovementLock)
            {
                double elapsedSeconds = (e.SignalTime - LastMovementTime).TotalSeconds;
                LastMovementTime = e.SignalTime;
                if (elapsedSeconds <= 0.0)
                    return;

                Ball[] ballsSnapshot;
                lock (BallsLock)
                    ballsSnapshot = BallsList.ToArray();

                Parallel.ForEach(ballsSnapshot, ball => ball.Move(elapsedSeconds));
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
