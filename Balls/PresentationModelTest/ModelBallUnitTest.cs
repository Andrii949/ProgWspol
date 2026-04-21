
using ConcurrentProgramming.BusinessLogic;

namespace ConcurrentProgramming.Presentation.Model.Test
{
  [TestClass]
  public class ModelBallUnitTest
  {
    [TestMethod]
    public void ConstructorTestMethod()
    {
      ModelBall ball = new ModelBall(0.0, 0.0, 20.0, new BusinessLogicIBallFixture());
      Assert.AreEqual<double>(0.0, ball.Top);
      Assert.AreEqual<double>(0.0, ball.Top);
      Assert.AreEqual<double>(20.0, ball.Diameter);
    }

    [TestMethod]
    public void PositionChangeNotificationTestMethod()
    {
      int notificationCounter = 0;
      ModelBall ball = new ModelBall(0, 0.0, 20.0, new BusinessLogicIBallFixture());
      ball.PropertyChanged += (sender, args) => notificationCounter++;
      Assert.AreEqual(0, notificationCounter);
      ball.SetLeft(1.0);
      Assert.AreEqual<int>(1, notificationCounter);
      Assert.AreEqual<double>(1.0, ball.Left);
      Assert.AreEqual<double>(0.0, ball.Top);
      ball.SettTop(1.0);
      Assert.AreEqual(2, notificationCounter);
      Assert.AreEqual<double>(1.0, ball.Left);
      Assert.AreEqual<double>(1.0, ball.Top);
    }

    #region testing instrumentation

    private class BusinessLogicIBallFixture : BusinessLogic.IBall
    {
      public event EventHandler<IPosition>? NewPositionNotification;
    }

    #endregion testing instrumentation
  }
}
