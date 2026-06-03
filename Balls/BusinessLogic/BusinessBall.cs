
namespace ConcurrentProgramming.BusinessLogic
{
  internal class Ball : IBall
  {
    public Ball(Data.IBall ball)
    {
      UnderneathBall = ball;
      UnderneathBall.NewPositionNotification += RaisePositionChangeEvent;
    }

    #region IBall

    public event EventHandler<IPosition>? NewPositionNotification;

    #endregion IBall

    #region private

    private void RaisePositionChangeEvent(object? sender, Data.IVector e)
    {
      NewPositionNotification?.Invoke(this, new Position(UnderneathBall.Position.x, UnderneathBall.Position.y));
    }

    private readonly Data.IBall UnderneathBall;

    #endregion private
  }
}
