
namespace ConcurrentProgramming.Data
{
  internal record Vector : IVector
  {
    #region IVector

    public double x { get; init; }
    public double y { get; init; }

    #endregion IVector

    public Vector(double XComponent, double YComponent)
    {
      x = XComponent;
      y = YComponent;
    }
  }
}