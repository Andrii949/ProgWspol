
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using UnderneathLayerAPI = ConcurrentProgramming.BusinessLogic.BusinessLogicAbstractAPI;

namespace ConcurrentProgramming.Presentation.Model
{
    internal class ModelImplementation : ModelAbstractApi
    {
        internal ModelImplementation() : this(null)
        { }

        internal ModelImplementation(UnderneathLayerAPI underneathLayer)
        {
            layerBellow = underneathLayer == null ? UnderneathLayerAPI.GetBusinessLogicLayer() : underneathLayer;
            eventObservable = Observable.FromEventPattern<BallChaneEventArgs>(this, "BallChanged");
            boardWidth = UnderneathLayerAPI.GetDimensions.TableWidth;
            boardHeight = UnderneathLayerAPI.GetDimensions.TableHeight;
        }

        #region ModelAbstractApi

        public override double BoardHeight => boardHeight;

        public override double BoardWidth => boardWidth;

        public override void Dispose()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(ModelImplementation));
            trackedBalls.Clear();
            layerBellow.Dispose();
            Disposed = true;
        }

        public override IDisposable Subscribe(IObserver<IBall> observer)
        {
            return eventObservable.Subscribe(x => observer.OnNext(x.EventArgs.Ball), ex => observer.OnError(ex), () => observer.OnCompleted());
        }

        public override void Start(int numberOfBalls)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(ModelImplementation));
            trackedBalls.Clear();
            layerBellow.Start(numberOfBalls, StartHandler);
        }

        public override void Stop()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(ModelImplementation));

            trackedBalls.Clear();
            layerBellow.Stop();
        }

        public override void SetDrawingArea(double drawingAreaWidth, double drawingAreaHeight)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(ModelImplementation));
            if (drawingAreaWidth <= 0 || drawingAreaHeight <= 0)
                return;

            double horizontalScale = drawingAreaWidth / UnderneathLayerAPI.GetDimensions.TableWidth;
            double verticalScale = drawingAreaHeight / UnderneathLayerAPI.GetDimensions.TableHeight;
            scaleFactor = Math.Min(horizontalScale, verticalScale);
            boardWidth = UnderneathLayerAPI.GetDimensions.TableWidth * scaleFactor;
            boardHeight = UnderneathLayerAPI.GetDimensions.TableHeight * scaleFactor;

            foreach (ModelBall ball in trackedBalls)
                ball.Rescale(scaleFactor);
        }

        #endregion ModelAbstractApi

        #region API

        public event EventHandler<BallChaneEventArgs> BallChanged;

        #endregion API

        #region private

        private bool Disposed = false;
        private double boardHeight;
        private double boardWidth;
        private double scaleFactor = 1.0;
        private readonly IObservable<EventPattern<BallChaneEventArgs>> eventObservable = null;
        private readonly UnderneathLayerAPI layerBellow = null;
        private readonly List<ModelBall> trackedBalls = [];

        private void StartHandler(BusinessLogic.IPosition position, BusinessLogic.IBall ball)
        {
            ModelBall newBall = new ModelBall(position.y, position.x, UnderneathLayerAPI.GetDimensions.BallDimension, ball, scaleFactor);
            trackedBalls.Add(newBall);
            BallChanged?.Invoke(this, new BallChaneEventArgs() { Ball = newBall });
        }

        #endregion private

        #region TestingInfrastructure

        [Conditional("DEBUG")]
        internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
        {
            returnInstanceDisposed(Disposed);
        }

        [Conditional("DEBUG")]
        internal void CheckUnderneathLayerAPI(Action<UnderneathLayerAPI> returnNumberOfBalls)
        {
            returnNumberOfBalls(layerBellow);
        }

        [Conditional("DEBUG")]
        internal void CheckBallChangedEvent(Action<bool> returnBallChangedIsNull)
        {
            returnBallChangedIsNull(BallChanged == null);
        }

        #endregion TestingInfrastructure
    }

    public class BallChaneEventArgs : EventArgs
    {
        public IBall Ball { get; init; }
    }
}
