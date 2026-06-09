
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using ConcurrentProgramming.Presentation.Model;
using ModelIBall = ConcurrentProgramming.Presentation.Model.IBall;

namespace ConcurrentProgramming.Presentation.ViewModel.Test
{
    [TestClass]
    public class MainWindowViewModelUnitTest
    {
        [TestMethod]
        public void ConstructorTest()
        {
            ModelNullFixture nullModelFixture = new();
            Assert.AreEqual<int>(0, nullModelFixture.Disposed);
            Assert.AreEqual<int>(0, nullModelFixture.Started);
            Assert.AreEqual<int>(0, nullModelFixture.Subscribed);
            using (MainWindowViewModel viewModel = new(nullModelFixture))
            {
                Random random = new Random();
                int numberOfBalls = random.Next(1, 10);
                Assert.AreEqual(820.0, viewModel.BoardWidth, 0.001);
                Assert.AreEqual(492.0, viewModel.BoardHeight, 0.001);
                viewModel.Start(numberOfBalls);
                Assert.IsNotNull(viewModel.Balls);
                Assert.AreEqual<int>(0, nullModelFixture.Disposed);
                Assert.AreEqual<int>(numberOfBalls, nullModelFixture.Started);
                Assert.AreEqual<int>(1, nullModelFixture.Subscribed);
                Assert.IsTrue(viewModel.IsStarted);
            }
            Assert.AreEqual<int>(1, nullModelFixture.Disposed);
        }

        [TestMethod]
        public void BehaviorTestMethod()
        {
            ModelSimulatorFixture modelSimulator = new();
            MainWindowViewModel viewModel = new(modelSimulator);
            Assert.IsNotNull(viewModel.Balls);
            Assert.AreEqual<int>(0, viewModel.Balls.Count);
            Random random = new Random();
            int numberOfBalls = random.Next(1, 10);
            viewModel.Start(numberOfBalls);
            Assert.AreEqual<int>(numberOfBalls, viewModel.Balls.Count);
            viewModel.Dispose();
            Assert.IsTrue(modelSimulator.Disposed);
            Assert.AreEqual<int>(0, viewModel.Balls.Count);
        }

        [TestMethod]
        public void StartCommandTestMethod()
        {
            ModelNullFixture model = new();
            using (MainWindowViewModel viewModel = new(model))
            {
                viewModel.NumberOfBalls = 3;
                ICommand command = viewModel.StartCommand;
                Assert.IsTrue(command.CanExecute(null));
                command.Execute(null);
                Assert.AreEqual(3, model.Started);
                Assert.IsFalse(command.CanExecute(null));
            }
        }

        [TestMethod]
        public void StopCommandTestMethod()
        {
            ModelNullFixture model = new();

            using (MainWindowViewModel viewModel = new(model))
            {
                viewModel.NumberOfBalls = 3;
                viewModel.StartCommand.Execute(null);
                ICommand stopCommand = viewModel.StopCommand;

                Assert.IsTrue(stopCommand.CanExecute(null));
                stopCommand.Execute(null);

                Assert.AreEqual(1, model.Stopped);
                Assert.IsFalse(viewModel.IsStarted);
            }
        }

        #region testing infrastructure

        private class ModelNullFixture : ModelAbstractApi
        {
            #region Test

            internal int Disposed = 0;
            internal int Started = 0;
            internal int Subscribed = 0;
            internal int Stopped = 0;

            #endregion Test

            #region ModelAbstractApi

            public override double BoardHeight => 492.0;

            public override double BoardWidth => 820.0;

            public override void Dispose()
            {
                Disposed++;
            }

            public override void Start(int numberOfBalls)
            {
                Started = numberOfBalls;
            }

            public override void Stop()
            {
                Stopped++;
            }

            public override void SetDrawingArea(double drawingAreaWidth, double drawingAreaHeight)
            { }

            public override IDisposable Subscribe(IObserver<ModelIBall> observer)
            {
                Subscribed++;
                return new NullDisposable();
            }

            #endregion ModelAbstractApi

            #region private

            private class NullDisposable : IDisposable
            {
                public void Dispose()
                { }
            }

            #endregion private
        }

        private class ModelSimulatorFixture : ModelAbstractApi
        {
            #region Testing indicators

            internal bool Disposed = false;
            internal bool Stopped = false;

            #endregion Testing indicators

            #region ctor

            public ModelSimulatorFixture()
            {
                eventObservable = Observable.FromEventPattern<BallChaneEventArgs>(this, "BallChanged");
            }

            #endregion ctor

            #region ModelAbstractApi fixture

            public override IDisposable? Subscribe(IObserver<ModelIBall> observer)
            {
                return eventObservable?.Subscribe(x => observer.OnNext(x.EventArgs.Ball), ex => observer.OnError(ex), () => observer.OnCompleted());
            }

            public override double BoardHeight => 492.0;

            public override double BoardWidth => 820.0;

            public override void Start(int numberOfBalls)
            {
                for (int i = 0; i < numberOfBalls; i++)
                {
                    ModelBall newBall = new ModelBall();
                    BallChanged?.Invoke(this, new BallChaneEventArgs() { Ball = newBall });
                }
            }

            public override void Dispose()
            {
                Disposed = true;
            }

            public override void Stop()
            {
                Stopped = true;
            }

            public override void SetDrawingArea(double drawingAreaWidth, double drawingAreaHeight)
            { }

            #endregion ModelAbstractApi

            #region API

            public event EventHandler<BallChaneEventArgs> BallChanged;

            #endregion API

            #region private

            private IObservable<EventPattern<BallChaneEventArgs>>? eventObservable = null;

            private class ModelBall : ModelIBall
            {
                #region IBall

                public double Diameter => 20.0;

                public double Top => 0.0;

                public double Left => 0.0;

                public string Color => "SteelBlue";

                #region INotifyPropertyChanged

                public event PropertyChangedEventHandler? PropertyChanged;

                #endregion INotifyPropertyChanged

                #endregion IBall
            }

            #endregion private
        }

        #endregion testing infrastructure
    }
}
