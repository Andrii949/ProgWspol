using System;
using System.ComponentModel;

namespace ConcurrentProgramming.Presentation.Model
{
    public interface IBall : INotifyPropertyChanged
    {
        double Top { get; }
        double Left { get; }
        double Diameter { get; }
        string Color { get; }
    }

    public abstract class ModelAbstractApi : IObservable<IBall>, IDisposable
    {
        public static ModelAbstractApi CreateModel()
        {
            return modelInstance.Value;
        }

        public abstract double BoardHeight { get; }

        public abstract double BoardWidth { get; }

        public abstract void Start(int numberOfBalls);

        public abstract void Stop();

        public abstract void SetDrawingArea(double drawingAreaWidth, double drawingAreaHeight);

        #region IObservable 

        public abstract IDisposable Subscribe(IObserver<IBall> observer);

        #endregion IObservable 

        #region IDisposable 

        public abstract void Dispose();

        #endregion IDisposable 

        #region private 

        private static Lazy<ModelAbstractApi> modelInstance = new Lazy<ModelAbstractApi>(() => new ModelImplementation());

        #endregion private 
    }
}
