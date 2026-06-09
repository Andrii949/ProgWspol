
namespace ConcurrentProgramming.Data
{
  internal class Ball : IBall
  {
    #region ctor

    internal Ball(int id, Vector initialPosition, Vector initialVelocity, double diameter, double mass)
    {
      Id = id;
      PositionBackingField = initialPosition;
      VelocityBackingField = initialVelocity;
      ColorIndex = id % BallColors.Length;
      Diameter = diameter;
      Mass = mass;
    }

    #endregion ctor

    #region IBall

    public event EventHandler<IVector>? NewPositionNotification;

    public IVector Position
    {
      get
      {
        lock (StateLock)
          return PositionBackingField;
      }
      set
      {
        lock (StateLock)
          PositionBackingField = value;
      }
    }

    public IVector Velocity
    {
      get
      {
        lock (StateLock)
          return VelocityBackingField;
      }
      set
      {
        lock (StateLock)
          VelocityBackingField = value;
      }
    }

    public string Color
    {
      get
      {
        lock (StateLock)
          return BallColors[ColorIndex];
      }
    }

    public int Id { get; }
    public double Diameter { get; }
    public double Mass { get; }

    #endregion IBall

    #region private

    private readonly object StateLock = new();
    private static readonly string[] BallColors = ["SteelBlue", "Crimson", "DarkOrange", "SeaGreen"];
    private IVector PositionBackingField;
    private IVector VelocityBackingField;
    private int ColorIndex;

    internal void ChangeColor()
    {
      lock (StateLock)
        ColorIndex = (ColorIndex + 1) % BallColors.Length;
    }

    internal void Move(double elapsedSeconds)
    {
      if (elapsedSeconds <= 0.0)
        throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));

      IVector newPosition;
      lock (StateLock)
      {
        newPosition = new Vector(
          PositionBackingField.x + VelocityBackingField.x * elapsedSeconds,
          PositionBackingField.y + VelocityBackingField.y * elapsedSeconds);
        PositionBackingField = newPosition;
      }

      NewPositionNotification?.Invoke(this, newPosition);
    }

    #endregion private
  }
}
