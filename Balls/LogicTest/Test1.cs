using Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogicTest
{
    [TestClass]
    public sealed class Test1
    {
        [TestMethod]
        public void SayHi_ReturnCorrectly()
        {
            LogicClass test_logic = new LogicClass();
            string result = test_logic.SayHi("World");

            Assert.AreEqual("Hi, World!", result);
        }
    }
}
