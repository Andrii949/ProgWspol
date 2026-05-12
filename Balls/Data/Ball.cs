
namespace ConcurrentProgramming.Data
{
  internal class Ball : IBall
  {
    #region ctor

    internal Ball(Vector initialPosition, Vector initialVelocity, double diameter, double mass)
    {
      PositionBackingField = initialPosition;
      Velocity = initialVelocity;
      Diameter = diameter;
      Mass = mass;
    }

    #endregion ctor

    #region IBall

    public event EventHandler<IVector>? NewPositionNotification;

    public IVector Position
    {
      get => PositionBackingField;
      set => PositionBackingField = value;
    }

    public IVector Velocity { get; set; }
    public double Diameter { get; }
    public double Mass { get; }

    #endregion IBall

    #region private

    private IVector PositionBackingField;

    private void RaiseNewPositionChangeNotification()
    {
      NewPositionNotification?.Invoke(this, Position);
    }

    internal void Move(Vector newPosition, Vector newVelocity)
    {
      PositionBackingField = newPosition;
      Velocity = newVelocity;
      RaiseNewPositionChangeNotification();
    }

    #endregion private
  }
}
