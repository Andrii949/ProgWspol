namespace ConcurrentProgramming.Data.Test
{
    [TestClass]
    public class BallUnitTest
    {
        [TestMethod]
        public void ConstructorTestMethod()
        {
            Vector testinVector = new Vector(0.0, 0.0);
            Ball newInstance = new(testinVector, testinVector, 20.0, 2.0);

            Assert.AreEqual(testinVector, newInstance.Position);
            Assert.AreEqual(testinVector, newInstance.Velocity);
            Assert.AreEqual(20.0, newInstance.Diameter);
            Assert.AreEqual(2.0, newInstance.Mass);
        }

        [TestMethod]
        public void MoveTestMethod()
        {
            Vector initialPosition = new(10.0, 10.0);
            Ball newInstance = new(initialPosition, new Vector(0.0, 0.0), 20.0, 2.0);
            Vector updatedVelocity = new(1.0, 1.0);
            IVector curentPosition = new Vector(0.0, 0.0);
            int numberOfCallBackCalled = 0;
            newInstance.NewPositionNotification += (sender, position) => { Assert.IsNotNull(sender); curentPosition = position; numberOfCallBackCalled++; };
            newInstance.Move(new Vector(0.0, 0.0), updatedVelocity);
            Assert.AreEqual<int>(1, numberOfCallBackCalled);
            Assert.AreEqual<IVector>(new Vector(0.0, 0.0), curentPosition);
            Assert.AreEqual<IVector>(updatedVelocity, newInstance.Velocity);
        }
    }
}
