
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using ConcurrentProgramming.BusinessLogic;
using LogicIBall = ConcurrentProgramming.BusinessLogic.IBall;

namespace ConcurrentProgramming.Presentation.Model
{
  internal class ModelBall : IBall
  {
    public ModelBall(double top, double left, double diameter, LogicIBall underneathBall, double scaleFactor = 1.0)
    {
      rawTop = top;
      rawLeft = left;
      rawDiameter = diameter;
      ApplyScale(scaleFactor);
      underneathBall.NewPositionNotification += NewPositionNotification;
    }

    #region IBall

    public double Top
    {
      get { return TopBackingField; }
      private set
      {
        if (TopBackingField == value)
          return;
        TopBackingField = value;
        RaisePropertyChanged();
      }
    }

    public double Left
    {
      get { return LeftBackingField; }
      private set
      {
        if (LeftBackingField == value)
          return;
        LeftBackingField = value;
        RaisePropertyChanged();
      }
    }

    public double Diameter
    {
      get { return DiameterBackingField; }
      private set
      {
        if (DiameterBackingField == value)
          return;
        DiameterBackingField = value;
        RaisePropertyChanged();
      }
    }

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion INotifyPropertyChanged

    #endregion IBall

    #region private

    private double TopBackingField;
    private double LeftBackingField;
    private double DiameterBackingField;
    private double currentScaleFactor = 1.0;
    private double rawTop;
    private double rawLeft;
    private double rawDiameter;

    private void NewPositionNotification(object sender, IPosition e)
    {
      rawTop = e.y;
      rawLeft = e.x;
      ApplyScale(currentScaleFactor);
    }

    internal void Rescale(double scaleFactor)
    {
      ApplyScale(scaleFactor);
    }

    private void ApplyScale(double scaleFactor)
    {
      currentScaleFactor = scaleFactor;
      Top = rawTop * scaleFactor;
      Left = rawLeft * scaleFactor;
      Diameter = rawDiameter * scaleFactor;
    }

    private void RaisePropertyChanged([CallerMemberName] string propertyName = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion private

    #region testing instrumentation

    [Conditional("DEBUG")]
    internal void SetLeft(double x)
    {
      rawLeft = x;
      ApplyScale(currentScaleFactor);
    }

    [Conditional("DEBUG")]
    internal void SettTop(double x)
    {
      rawTop = x;
      ApplyScale(currentScaleFactor);
    }

    #endregion testing instrumentation
  }
}
