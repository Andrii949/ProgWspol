
using ConcurrentProgramming.BusinessLogic;

namespace ConcurrentProgramming.BusinessLogic.Test
{
  [TestClass]
  public class BusinessLogicAbstractAPIUnitTest
  {
    [TestMethod]
    public void BusinessLogicConstructorTestMethod()
    {
      BusinessLogicAbstractAPI instance1 = BusinessLogicAbstractAPI.GetBusinessLogicLayer();
      BusinessLogicAbstractAPI instance2 = BusinessLogicAbstractAPI.GetBusinessLogicLayer();
      Assert.AreSame(instance1, instance2);
      instance1.Dispose();
      Assert.ThrowsException<ObjectDisposedException>(() => instance2.Dispose());
    }

    [TestMethod]
    public void GetDimensionsTestMethod()
    {
      Assert.AreEqual<Dimensions>(new(10.0, 10.0, 10.0), BusinessLogicAbstractAPI.GetDimensions);
    }
  }
}