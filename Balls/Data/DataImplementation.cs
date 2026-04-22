
using System;
using System.Diagnostics;

namespace ConcurrentProgramming.Data
{
  internal class DataImplementation : DataAbstractAPI
  {
    #region ctor

    public DataImplementation()
    {
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
        for (int i = 0; i < numberOfBalls; i++)
        {
          Vector startingPosition = new(
            RandomGenerator.NextDouble() * (TableWidth - BallDiameter),
            RandomGenerator.NextDouble() * (TableHeight - BallDiameter));
          Vector initialVelocity = CreateVelocity();
          Ball newBall = new(startingPosition, initialVelocity);
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
          MoveTimer.Dispose();
          ClearBalls();
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

    private readonly Timer MoveTimer;
    private readonly Random RandomGenerator = new();
    private readonly object BallsLock = new();
    private readonly List<Ball> BallsList = [];

    internal const double BallDiameter = 20.0;
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
      lock (BallsLock)
      {
        foreach (Ball item in BallsList)
        {
          Vector velocity = (Vector)item.Velocity;
          double nextX = item.Position.x + velocity.x;
          double nextY = item.Position.y + velocity.y;
          double correctedVelocityX = velocity.x;
          double correctedVelocityY = velocity.y;

          if (nextX <= 0 || nextX >= TableWidth - BallDiameter)
          {
            correctedVelocityX = -correctedVelocityX;
            nextX = Math.Clamp(nextX, 0, TableWidth - BallDiameter);
          }

          if (nextY <= 0 || nextY >= TableHeight - BallDiameter)
          {
            correctedVelocityY = -correctedVelocityY;
            nextY = Math.Clamp(nextY, 0, TableHeight - BallDiameter);
          }

          item.Move(new Vector(nextX, nextY), new Vector(correctedVelocityX, correctedVelocityY));
        }
      }
    }

    private Vector CreateVelocity()
    {
      double componentX = 0;
      double componentY = 0;

      while (Math.Abs(componentX) < 0.25)
        componentX = (RandomGenerator.NextDouble() - 0.5) * 6.0;

      while (Math.Abs(componentY) < 0.25)
        componentY = (RandomGenerator.NextDouble() - 0.5) * 6.0;

      return new Vector(componentX, componentY);
    }

    #endregion private

    #region TestingInfrastructure

    [Conditional("DEBUG")]
    internal void CheckBallsList(Action<IEnumerable<IBall>> returnBallsList)
    {
      returnBallsList(BallsList);
    }

    [Conditional("DEBUG")]
    internal void CheckNumberOfBalls(Action<int> returnNumberOfBalls)
    {
      returnNumberOfBalls(BallsList.Count);
    }

    [Conditional("DEBUG")]
    internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
    {
      returnInstanceDisposed(Disposed);
    }

    #endregion TestingInfrastructure
  }
}
