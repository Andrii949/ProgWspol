using Data;

namespace DataTest
{
    [TestClass]
    public sealed class Test1
    {
        [TestMethod]
        public void DataClass_created_successfully()
        {
            DataClass data = new DataClass();

            data.X = 5;
            data.Y = 10;

            Assert.AreEqual(5, data.X);
            Assert.AreEqual(10, data.Y);
        }
    }
}
