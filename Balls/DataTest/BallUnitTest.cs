namespace ConcurrentProgramming.Data.Test
{
    [TestClass]
    public class BallUnitTest
    {
        [TestMethod]
        public void ConstructorTestMethod()
        {
            Vector testinVector = new Vector(0.0, 0.0);
            Ball newInstance = new(1, testinVector, testinVector, 20.0, 2.0);

            Assert.AreEqual(1, newInstance.Id);
            Assert.AreEqual(testinVector, newInstance.Position);
            Assert.AreEqual(testinVector, newInstance.Velocity);
            Assert.AreEqual("Crimson", newInstance.Color);
            Assert.AreEqual(20.0, newInstance.Diameter);
            Assert.AreEqual(2.0, newInstance.Mass);
        }

        [TestMethod]
        public void MoveTestMethod()
        {
            Vector initialPosition = new(10.0, 10.0);
            Ball newInstance = new(1, initialPosition, new Vector(100.0, 50.0), 20.0, 2.0);
            IVector curentPosition = new Vector(0.0, 0.0);
            int numberOfCallBackCalled = 0;
            newInstance.NewPositionNotification += (sender, position) => { Assert.IsNotNull(sender); curentPosition = position; numberOfCallBackCalled++; };

            newInstance.Move(0.02);

            Assert.AreEqual<int>(1, numberOfCallBackCalled);
            Assert.AreEqual(12.0, curentPosition.x, 0.001);
            Assert.AreEqual(11.0, curentPosition.y, 0.001);
        }

        [TestMethod]
        public void MoveSynchronizesPositionWithElapsedTimeTestMethod()
        {
            Ball newInstance = new(1, new Vector(10.0, 10.0), new Vector(100.0, 0.0), 20.0, 2.0);

            newInstance.Move(0.5);

            Assert.AreEqual(60.0, newInstance.Position.x, 0.001);
            Assert.AreEqual(10.0, newInstance.Position.y, 0.001);
        }

        [TestMethod]
        public void ChangeColorTestMethod()
        {
            Ball newInstance = new(0, new Vector(0.0, 0.0), new Vector(0.0, 0.0), 20.0, 2.0);
            Assert.AreEqual("SteelBlue", newInstance.Color);

            newInstance.ChangeColor();

            Assert.AreEqual("Crimson", newInstance.Color);
        }

    }
}
