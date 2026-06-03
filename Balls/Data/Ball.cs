
namespace ConcurrentProgramming.Data
{
  internal class Ball : IBall
  {
    #region ctor

    internal Ball(int id, Vector initialPosition, Vector initialVelocity, double diameter, double mass)
    {
      Id = id;
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
    public int Id { get; }
    public double Diameter { get; }
    public double Mass { get; }

    #endregion IBall

    #region private

    private IVector PositionBackingField;

    private void RaiseNewPositionChangeNotification()
    {
      NewPositionNotification?.Invoke(this, Position);
    }

    internal void Move(IVector newPosition, IVector newVelocity)
    {
      PositionBackingField = newPosition;
      Velocity = newVelocity;
      RaiseNewPositionChangeNotification();
    }

    #endregion private
  }
}
