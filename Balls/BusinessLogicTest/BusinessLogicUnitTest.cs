using ConcurrentProgramming.Data;

namespace ConcurrentProgramming.BusinessLogic.Test
{
    [TestClass]
    public class BusinessLogicImplementationUnitTest
    {
        [TestMethod]
        public void ConstructorTestMethod()
        {
            using (BusinessLogicImplementation newInstance = new(new DataLayerConstructorFixcure()))
            {
                bool newInstanceDisposed = true;
                newInstance.CheckObjectDisposed(x => newInstanceDisposed = x);
                Assert.IsFalse(newInstanceDisposed);
            }
        }

        [TestMethod]
        public void DisposeTestMethod()
        {
            DataLayerDisposeFixcure dataLayerFixcure = new DataLayerDisposeFixcure();
            BusinessLogicImplementation newInstance = new(dataLayerFixcure);
            Assert.IsFalse(dataLayerFixcure.Disposed);
            bool newInstanceDisposed = true;
            newInstance.CheckObjectDisposed(x => newInstanceDisposed = x);
            Assert.IsFalse(newInstanceDisposed);
            newInstance.Dispose();
            newInstance.CheckObjectDisposed(x => newInstanceDisposed = x);
            Assert.IsTrue(newInstanceDisposed);
            Assert.ThrowsException<ObjectDisposedException>(() => newInstance.Dispose());
            Assert.ThrowsException<ObjectDisposedException>(() => newInstance.Start(0, (position, ball) => { }));
            Assert.IsTrue(dataLayerFixcure.Disposed);
        }

        [TestMethod]
        public void StartTestMethod()
        {
            DataLayerStartFixcure dataLayerFixcure = new();
            using (BusinessLogicImplementation newInstance = new(dataLayerFixcure))
            {
                int called = 0;
                int numberOfBalls2Create = 10;
                newInstance.Start(
                  numberOfBalls2Create,
                  (startingPosition, ball) =>
                  {
                      called++;
                      Assert.IsNotNull(startingPosition);
                      Assert.IsNotNull(ball);
                      Assert.AreEqual(12.0, startingPosition.x);
                      Assert.AreEqual(34.0, startingPosition.y);
                  });
                Assert.AreEqual<int>(1, called);
                Assert.IsTrue(dataLayerFixcure.StartCalled);
                Assert.AreEqual<int>(numberOfBalls2Create, dataLayerFixcure.NumberOfBallseCreated);
            }
        }

        [TestMethod]
        public void StopTestMethod()
        {
            DataLayerStopFixture dataLayerFixture = new();

            using (BusinessLogicImplementation newInstance = new(dataLayerFixture))
            {
                newInstance.Stop();

                Assert.IsTrue(dataLayerFixture.StopCalled);
            }
        }

        [TestMethod]
        public void ElasticCollisionEqualMassHeadOnTestMethod()
        {
            CollisionBallFixture first = new(new CollisionVectorFixture { x = 100.0, y = 100.0 }, new CollisionVectorFixture { x = 2.0, y = 0.0 }, 20.0, 1.0);
            CollisionBallFixture second = new(new CollisionVectorFixture { x = 118.0, y = 100.0 }, new CollisionVectorFixture { x = -2.0, y = 0.0 }, 20.0, 1.0);

            BusinessLogicImplementation.ResolveCollision(first, second);

            Assert.AreEqual(-2.0, first.Velocity.x, 0.001);
            Assert.AreEqual(2.0, second.Velocity.x, 0.001);
        }

        [TestMethod]
        public void ElasticCollisionDifferentMassesTestMethod()
        {
            CollisionBallFixture first = new(new CollisionVectorFixture { x = 100.0, y = 100.0 }, new CollisionVectorFixture { x = 3.0, y = 0.0 }, 20.0, 2.0);
            CollisionBallFixture second = new(new CollisionVectorFixture { x = 118.0, y = 100.0 }, new CollisionVectorFixture { x = 0.0, y = 0.0 }, 20.0, 1.0);

            BusinessLogicImplementation.ResolveCollision(first, second);

            Assert.AreEqual(1.0, first.Velocity.x, 0.001);
            Assert.AreEqual(4.0, second.Velocity.x, 0.001);
        }

        [TestMethod]
        public void ElasticCollisionSeparatedBallsLeaveVelocityUnchangedTestMethod()
        {
            CollisionBallFixture first = new(new CollisionVectorFixture { x = 100.0, y = 100.0 }, new CollisionVectorFixture { x = 3.0, y = 0.0 }, 20.0, 2.0);
            CollisionBallFixture second = new(new CollisionVectorFixture { x = 200.0, y = 100.0 }, new CollisionVectorFixture { x = 0.0, y = 0.0 }, 20.0, 1.0);

            BusinessLogicImplementation.ResolveCollision(first, second);

            Assert.AreEqual(3.0, first.Velocity.x, 0.001);
            Assert.AreEqual(0.0, second.Velocity.x, 0.001);
        }

        #region testing instrumentation

        private class DataLayerConstructorFixcure : Data.DataAbstractAPI
        {
            public override void Dispose()
            { }

            public override void Stop()
            { }

            public override void Start(int numberOfBalls, Action<IVector, Data.IBall> upperLayerHandler)
            {
                throw new NotImplementedException();
            }
        }

        private class DataLayerDisposeFixcure : Data.DataAbstractAPI
        {
            internal bool Disposed = false;

            public override void Dispose()
            {
                Disposed = true;
            }

            public override void Stop()
            { }

            public override void Start(int numberOfBalls, Action<IVector, Data.IBall> upperLayerHandler)
            {
                throw new NotImplementedException();
            }
        }

        private class DataLayerStartFixcure : Data.DataAbstractAPI
        {
            internal bool StartCalled = false;
            internal int NumberOfBallseCreated = -1;

            public override void Dispose()
            { }

            public override void Stop()
            { }

            public override void Start(int numberOfBalls, Action<IVector, Data.IBall> upperLayerHandler)
            {
                StartCalled = true;
                NumberOfBallseCreated = numberOfBalls;
                upperLayerHandler(new DataVectorFixture() { x = 12.0, y = 34.0 }, new DataBallFixture());
            }

            private record DataVectorFixture : Data.IVector
            {
                public double x { get; init; }
                public double y { get; init; }
            }

            private class DataBallFixture : Data.IBall
            {
                public DataBallFixture()
                  : this(new DataVectorFixture(), new DataVectorFixture(), 20.0, 1.0)
                { }

                public DataBallFixture(IVector position, IVector velocity, double diameter, double mass)
                {
                    Position = position;
                    Velocity = velocity;
                    Diameter = diameter;
                    Mass = mass;
                }

                public IVector Position { get; set; }
                public IVector Velocity { get; set; }
                public double Diameter { get; }
                public double Mass { get; }

                public event EventHandler<IVector>? NewPositionNotification = null;
            }
        }

        private class DataLayerStopFixture : Data.DataAbstractAPI
        {
            internal bool StopCalled = false;

            public override void Dispose()
            { }

            public override void Stop()
            {
                StopCalled = true;
            }

            public override void Start(int numberOfBalls, Action<IVector, Data.IBall> upperLayerHandler)
            {
                throw new NotImplementedException();
            }
        }

        private record CollisionVectorFixture : Data.IVector
        {
            public double x { get; init; }
            public double y { get; init; }
        }

        private class CollisionBallFixture : Data.IBall
        {
            public CollisionBallFixture(IVector position, IVector velocity, double diameter, double mass)
            {
                Position = position;
                Velocity = velocity;
                Diameter = diameter;
                Mass = mass;
            }

            public IVector Position { get; set; }
            public IVector Velocity { get; set; }
            public double Diameter { get; }
            public double Mass { get; }
            public event EventHandler<IVector>? NewPositionNotification = null;
        }

        #endregion testing instrumentation
    }
}
