namespace ConcurrentProgramming.Data.Test
{
    [TestClass]
    public class DataAbstractAPIUnitTest
    {
        [TestMethod]
        public void ConstructorTestTestMethod()
        {
            DataAbstractAPI instance1 = DataAbstractAPI.GetDataLayer();
            DataAbstractAPI instance2 = DataAbstractAPI.GetDataLayer();
            Assert.AreSame<DataAbstractAPI>(instance1, instance2);
            instance1.Dispose();
            Assert.ThrowsException<ObjectDisposedException>(() => instance2.Dispose());
        }

        [TestMethod]
        public void StopAfterDisposeThrowsTestMethod()
        {
            DataImplementation instance = new DataImplementation();
            instance.Dispose();

            Assert.ThrowsException<ObjectDisposedException>(() => instance.Stop());
        }
    }
}