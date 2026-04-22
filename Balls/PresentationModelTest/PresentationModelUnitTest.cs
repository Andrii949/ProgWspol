
using ConcurrentProgramming.BusinessLogic;

namespace ConcurrentProgramming.Presentation.Model.Test
{
  [TestClass]
  public class PresentationModelUnitTest
  {
    [TestMethod]
    public void DisposeTestMethod()
    {
      UnderneathLayerFixture underneathLayerFixture = new UnderneathLayerFixture();
      ModelImplementation? newInstance = null;
      using (newInstance = new(underneathLayerFixture))
      {
        newInstance.CheckObjectDisposed(x => Assert.IsFalse(x));
        newInstance.CheckUnderneathLayerAPI(x => Assert.AreSame(underneathLayerFixture, x));
        newInstance.CheckBallChangedEvent(x => Assert.IsTrue(x));
        Assert.IsFalse(underneathLayerFixture.Disposed);
      }
      newInstance.CheckObjectDisposed(x => Assert.IsTrue(x));
      newInstance.CheckUnderneathLayerAPI(x => Assert.AreSame(underneathLayerFixture, x));
      Assert.IsTrue(underneathLayerFixture.Disposed);
      Assert.ThrowsException<ObjectDisposedException>(() => newInstance.Dispose());
    }

    [TestMethod]
    public void StartTestMethod()
    {
      UnderneathLayerFixture underneathLayerFixture = new UnderneathLayerFixture();
      using (ModelImplementation newInstance = new(underneathLayerFixture))
      {
        newInstance.CheckBallChangedEvent(x => Assert.IsTrue(x));
        IDisposable subscription = newInstance.Subscribe(x => { });
        newInstance.CheckBallChangedEvent(x => Assert.IsFalse(x));
        newInstance.Start(10);
        Assert.AreEqual<int>(10, underneathLayerFixture.NumberOfBalls);
        subscription.Dispose();
        newInstance.CheckBallChangedEvent(x => Assert.IsTrue(x));
      }
    }

    [TestMethod]
    public void StopTestMethod()
    {
      UnderneathLayerFixture underneathLayerFixture = new UnderneathLayerFixture();

      using (ModelImplementation newInstance = new(underneathLayerFixture))
      {
        newInstance.Stop();

        Assert.IsTrue(underneathLayerFixture.StopCalled);
      }
    }

    [TestMethod]
    public void SetDrawingAreaScalesBoardTestMethod()
    {
      using (ModelImplementation newInstance = new(new UnderneathLayerFixture()))
      {
        newInstance.SetDrawingArea(350.0, 210.0);

        Assert.AreEqual(350.0, newInstance.BoardWidth, 0.001);
        Assert.AreEqual(210.0, newInstance.BoardHeight, 0.001);
      }
    }

    #region testing instrumentation

    private class UnderneathLayerFixture : BusinessLogicAbstractAPI
    {
      #region testing instrumentation

      internal bool Disposed = false;
      internal int NumberOfBalls = 0;
      internal bool StopCalled = false;

      #endregion testing instrumentation

      #region BusinessLogicAbstractAPI

      public override void Dispose()
      {
        Disposed = true;
      }

      public override void Stop()
      {
        StopCalled = true;
      }

      public override void Start(int numberOfBalls, Action<IPosition, BusinessLogic.IBall> upperLayerHandler)
      {
        NumberOfBalls = numberOfBalls;
        Assert.IsNotNull(upperLayerHandler);
      }

      #endregion BusinessLogicAbstractAPI
    }

    #endregion testing instrumentation
  }
}
