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
                Assert.IsNotNull(data);
        }
    }
}
