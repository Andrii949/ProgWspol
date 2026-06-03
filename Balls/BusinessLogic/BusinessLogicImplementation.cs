using System.Diagnostics;
using UnderneathLayerAPI = ConcurrentProgramming.Data.DataAbstractAPI;

namespace ConcurrentProgramming.BusinessLogic
{
    internal class BusinessLogicImplementation : BusinessLogicAbstractAPI
    {
        #region ctor 

        public BusinessLogicImplementation() : this(null)
        { }

        internal BusinessLogicImplementation(UnderneathLayerAPI? underneathLayer)
        {
            layerBellow = underneathLayer == null ? UnderneathLayerAPI.GetDataLayer() : underneathLayer;
        }

        #endregion ctor 

        #region BusinessLogicAbstractAPI 

        public override void Dispose()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
            lock (TrackedBallsLock)
            {
                TrackedBalls.Clear();
            }
            layerBellow.Dispose();
            Disposed = true;
        }

        public override void Start(int numberOfBalls, Action<IPosition, IBall> upperLayerHandler)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
            if (numberOfBalls < 0)
                throw new ArgumentOutOfRangeException(nameof(numberOfBalls));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));
            lock (TrackedBallsLock)
            {
                TrackedBalls.Clear();
            }
            layerBellow.Start(
              numberOfBalls,
              (startingPosition, databall) =>
              {
                  lock (TrackedBallsLock)
                  {
                      TrackedBalls.Add(databall);
                  }
                  databall.NewPositionNotification += HandleBallPositionChanged;
                  upperLayerHandler(new Position(startingPosition.x, startingPosition.y), new Ball(databall));
              });
        }

        public override void Stop()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(BusinessLogicImplementation));

            lock (TrackedBallsLock)
            {
                TrackedBalls.Clear();
            }
            layerBellow.Stop();
        }

        #endregion BusinessLogicAbstractAPI 

        #region private 

        private bool Disposed = false;

        private readonly UnderneathLayerAPI layerBellow;
        private readonly object TrackedBallsLock = new();
        private readonly List<Data.IBall> TrackedBalls = [];

        private void HandleBallPositionChanged(object? sender, Data.IVector position)
        {
            lock (TrackedBallsLock)
            {
                for (int i = 0; i < TrackedBalls.Count; i++)
                {
                    for (int j = i + 1; j < TrackedBalls.Count; j++)
                    {
                        ResolveCollision(TrackedBalls[i], TrackedBalls[j]);
                    }
                }
            }
        }

        internal static void ResolveCollision(Data.IBall first, Data.IBall second)
        {
            double firstCenterX = first.Position.x + first.Diameter / 2.0;
            double firstCenterY = first.Position.y + first.Diameter / 2.0;
            double secondCenterX = second.Position.x + second.Diameter / 2.0;
            double secondCenterY = second.Position.y + second.Diameter / 2.0;

            double dx = secondCenterX - firstCenterX;
            double dy = secondCenterY - firstCenterY;
            double distanceSquared = dx * dx + dy * dy;
            double minimumDistance = (first.Diameter + second.Diameter) / 2.0;
            double minimumDistanceSquared = minimumDistance * minimumDistance;

            if (distanceSquared > minimumDistanceSquared || distanceSquared == 0.0)
                return;

            double distance = Math.Sqrt(distanceSquared);
            double normalX = dx / distance;
            double normalY = dy / distance;
            double overlap = minimumDistance - distance;

            if (overlap > 0.0)
            {
                double totalMass = first.Mass + second.Mass;
                double firstCorrection = overlap * (second.Mass / totalMass);
                double secondCorrection = overlap * (first.Mass / totalMass);

                first.Position = new LogicVector(
                    first.Position.x - normalX * firstCorrection,
                    first.Position.y - normalY * firstCorrection);
                second.Position = new LogicVector(
                    second.Position.x + normalX * secondCorrection,
                    second.Position.y + normalY * secondCorrection);
            }

            double firstVelocityNormal = first.Velocity.x * normalX + first.Velocity.y * normalY;
            double secondVelocityNormal = second.Velocity.x * normalX + second.Velocity.y * normalY;

            if (secondVelocityNormal - firstVelocityNormal >= 0.0)
                return;

            double firstVelocityTangentX = first.Velocity.x - firstVelocityNormal * normalX;
            double firstVelocityTangentY = first.Velocity.y - firstVelocityNormal * normalY;
            double secondVelocityTangentX = second.Velocity.x - secondVelocityNormal * normalX;
            double secondVelocityTangentY = second.Velocity.y - secondVelocityNormal * normalY;

            double firstUpdatedNormal =
                (firstVelocityNormal * (first.Mass - second.Mass) + 2.0 * second.Mass * secondVelocityNormal)
                / (first.Mass + second.Mass);
            double secondUpdatedNormal =
                (secondVelocityNormal * (second.Mass - first.Mass) + 2.0 * first.Mass * firstVelocityNormal)
                / (first.Mass + second.Mass);

            first.Velocity = new LogicVector(
                firstVelocityTangentX + firstUpdatedNormal * normalX,
                firstVelocityTangentY + firstUpdatedNormal * normalY);
            second.Velocity = new LogicVector(
                secondVelocityTangentX + secondUpdatedNormal * normalX,
                secondVelocityTangentY + secondUpdatedNormal * normalY);
        }

        private sealed record LogicVector(double x, double y) : Data.IVector;

        #endregion private 

        #region TestingInfrastructure 

        [Conditional("DEBUG")]
        internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
        {
            returnInstanceDisposed(Disposed);
        }

        #endregion TestingInfrastructure 
    }
}
