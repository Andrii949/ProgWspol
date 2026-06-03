
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

        [TestMethod]
        public void MoveUsesElapsedTimeToUpdatePositionTestMethod()
        {
            TestDiagnosticLogger logger = new();
            using (DataImplementation newInstance = new DataImplementation(logger, new FixedElapsedTimeProvider(0.5), startTimer: false))
            {
                IBall? createdBall = null;
                IVector? startingPosition = null;
                newInstance.Start(
                  1,
                  (position, ball) =>
                  {
                      startingPosition = position;
                      createdBall = ball;
                      ball.Velocity = new ExternalVector(100.0, 0.0);
                  });

                InvokeMove(newInstance);

                Assert.IsNotNull(createdBall);
                Assert.IsNotNull(startingPosition);
                Assert.AreEqual(startingPosition.x + 50.0, createdBall.Position.x, 0.001);
                Assert.AreEqual(startingPosition.y, createdBall.Position.y, 0.001);
            }
        }

        [TestMethod]
        public void DiagnosticLoggerReceivesRecordWhenBallMovesTestMethod()
        {
            TestDiagnosticLogger logger = new();
            using (DataImplementation newInstance = new DataImplementation(logger, new FixedElapsedTimeProvider(0.02), startTimer: false))
            {
                newInstance.Start(1, (position, ball) => { });

                InvokeMove(newInstance);

                Assert.AreEqual(1, logger.Records.Count);
                Assert.AreEqual(0, logger.DroppedRecords);
            }
        }

        [TestMethod]
        public void FullDiagnosticLoggerDoesNotStopBallMovementTestMethod()
        {
            TestDiagnosticLogger logger = new() { AcceptRecords = false };
            using (DataImplementation newInstance = new DataImplementation(logger, new FixedElapsedTimeProvider(1.0), startTimer: false))
            {
                IBall? createdBall = null;
                IVector? startingPosition = null;
                newInstance.Start(
                  1,
                  (position, ball) =>
                  {
                      startingPosition = position;
                      createdBall = ball;
                      ball.Velocity = new ExternalVector(10.0, 0.0);
                  });

                InvokeMove(newInstance);

                Assert.IsNotNull(createdBall);
                Assert.IsNotNull(startingPosition);
                Assert.AreEqual(startingPosition.x + 10.0, createdBall.Position.x, 0.001);
                Assert.AreEqual(1, logger.DroppedRecords);
            }
        }

        [TestMethod]
        public void DiagnosticRecordSerializesToAsciiTextTestMethod()
        {
            DiagnosticRecord record = new(1, 7, 10.5, 20.25, -3.0, 4.0);

            string serialized = DiagnosticLogSerializer.Serialize(record);

            Assert.IsTrue(serialized.All(character => character <= 127));
            StringAssert.Contains(serialized, "7");
            StringAssert.Contains(serialized, "10.500");
        }

        [TestMethod]
        public void FileDiagnosticLoggerWritesAsciiTextFileTestMethod()
        {
            string filePath = Path.Combine(Path.GetTempPath(), $"balls-diagnostics-{Guid.NewGuid():N}.log");
            try
            {
                using (FileDiagnosticLogger logger = new(filePath, capacity: 8))
                {
                    Assert.IsTrue(logger.TryLog(new DiagnosticRecord(1, 2, 3.0, 4.0, 5.0, 6.0)));
                }

                string content = File.ReadAllText(filePath, System.Text.Encoding.ASCII);

                Assert.IsTrue(content.All(character => character <= 127));
                StringAssert.Contains(content, "2;3.000;4.000;5.000;6.000");
            }
            finally
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
        }

        private static void InvokeMove(DataImplementation instance)
        {
            typeof(DataImplementation)
              .GetMethod("Move", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
              ?.Invoke(instance, new object?[] { null });
        }

        private sealed record ExternalVector(double x, double y) : IVector;

        private sealed class FixedElapsedTimeProvider : IElapsedTimeProvider
        {
            private readonly double elapsedSeconds;

            internal FixedElapsedTimeProvider(double elapsedSeconds)
            {
                this.elapsedSeconds = elapsedSeconds;
            }

            public double GetElapsedSeconds()
            {
                return elapsedSeconds;
            }

            public void Reset()
            { }
        }

        private sealed class TestDiagnosticLogger : IDiagnosticLogger
        {
            internal bool AcceptRecords = true;
            internal List<DiagnosticRecord> Records { get; } = [];
            public int DroppedRecords { get; private set; }

            public bool TryLog(DiagnosticRecord record)
            {
                if (!AcceptRecords)
                {
                    DroppedRecords++;
                    return false;
                }

                Records.Add(record);
                return true;
            }

            public void Dispose()
            { }
        }
    }
}
