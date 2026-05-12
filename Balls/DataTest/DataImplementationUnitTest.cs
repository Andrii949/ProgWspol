
namespace ConcurrentProgramming.Data.Test
{
    [TestClass]
    public class DataImplementationUnitTest
    {
        [TestMethod]
        public void ConstructorTestMethod()
        {
            using (DataImplementation newInstance = new DataImplementation())
            {
                IEnumerable<IBall>? ballsList = null;
                newInstance.CheckBallsList(x => ballsList = x);
                Assert.IsNotNull(ballsList);
                int numberOfBalls = 0;
                newInstance.CheckNumberOfBalls(x => numberOfBalls = x);
                Assert.AreEqual<int>(0, numberOfBalls);
            }
        }

        [TestMethod]
        public void DisposeTestMethod()
        {
            DataImplementation newInstance = new DataImplementation();
            bool newInstanceDisposed = false;
            newInstance.CheckObjectDisposed(x => newInstanceDisposed = x);
            Assert.IsFalse(newInstanceDisposed);
            newInstance.Dispose();
            newInstance.CheckObjectDisposed(x => newInstanceDisposed = x);
            Assert.IsTrue(newInstanceDisposed);
            IEnumerable<IBall>? ballsList = null;
            newInstance.CheckBallsList(x => ballsList = x);
            Assert.IsNotNull(ballsList);
            newInstance.CheckNumberOfBalls(x => Assert.AreEqual<int>(0, x));
            Assert.ThrowsException<ObjectDisposedException>(() => newInstance.Dispose());
            Assert.ThrowsException<ObjectDisposedException>(() => newInstance.Start(0, (position, ball) => { }));
        }

        [TestMethod]
        public void StartTestMethod()
        {
            using (DataImplementation newInstance = new DataImplementation())
            {
                int numberOfCallbackInvoked = 0;
                int numberOfBalls2Create = 10;
                newInstance.Start(
                  numberOfBalls2Create,
                  (startingPosition, ball) =>
                  {
                      numberOfCallbackInvoked++;
                      Assert.IsTrue(startingPosition.x >= 0);
                      Assert.IsTrue(startingPosition.y >= 0);
                      Assert.IsTrue(startingPosition.x <= DataImplementation.TableWidth - DataImplementation.BallDiameter);
                      Assert.IsTrue(startingPosition.y <= DataImplementation.TableHeight - DataImplementation.BallDiameter);
                      Assert.IsNotNull(ball);
                      Assert.AreEqual(DataImplementation.BallDiameter, ball.Diameter);
                      Assert.AreEqual(DataImplementation.BallMass, ball.Mass);
                  });
                Assert.AreEqual<int>(numberOfBalls2Create, numberOfCallbackInvoked);
                newInstance.CheckNumberOfBalls(x => Assert.AreEqual<int>(10, x));
            }
        }

        [TestMethod]
        public void StopTestMethod()
        {
            using (DataImplementation newInstance = new DataImplementation())
            {
                newInstance.Start(5, (startingPosition, ball) => { });
                newInstance.CheckNumberOfBalls(x => Assert.AreEqual(5, x));

                newInstance.Stop();

                newInstance.CheckNumberOfBalls(x => Assert.AreEqual(0, x));
            }
        }

        [TestMethod]
        public void MoveAcceptsAnyVectorImplementationAsVelocityTestMethod()
        {
            using (DataImplementation newInstance = new DataImplementation())
            {
                newInstance.Start(
                  1,
                  (startingPosition, ball) =>
                  {
                      ball.Velocity = new ExternalVector(1.0, 1.0);
                  });

                typeof(DataImplementation)
                  .GetMethod("Move", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                  ?.Invoke(newInstance, new object?[] { null });
            }
        }

        private sealed record ExternalVector(double x, double y) : IVector;
    }
}
