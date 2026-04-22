
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ConcurrentProgramming.BusinessLogic;
using ConcurrentProgramming.Presentation.Model;
using ConcurrentProgramming.Presentation.ViewModel.MVVMLight;
using ModelIBall = ConcurrentProgramming.Presentation.Model.IBall;

namespace ConcurrentProgramming.Presentation.ViewModel
{
  public class MainWindowViewModel : ViewModelBase, IDisposable
  {
    private const double InitialDrawingAreaWidth = 820.0;
    private const double InitialDrawingAreaHeight = 520.0;

    #region ctor

    public MainWindowViewModel() : this(null)
    { }

    internal MainWindowViewModel(ModelAbstractApi modelLayerAPI)
    {
      ModelLayer = modelLayerAPI == null ? ModelAbstractApi.CreateModel() : modelLayerAPI;
      ModelLayer.SetDrawingArea(InitialDrawingAreaWidth, InitialDrawingAreaHeight);
      StartCommand = new RelayCommand(StartRequested, CanStart);
      StopCommand = new RelayCommand(StopRequested, CanStop);
      Observer = ModelLayer.Subscribe<ModelIBall>(x => Balls.Add(x));
      NumberOfBalls = 5;
    }

    #endregion ctor

    #region public API

    public void Start(int numberOfBalls)
    {
      if (Disposed)
        throw new ObjectDisposedException(nameof(MainWindowViewModel));
      if (numberOfBalls <= 0)
        throw new ArgumentOutOfRangeException(nameof(numberOfBalls));
      if (Started)
        return;

      Balls.Clear();
      ModelLayer.Start(numberOfBalls);
      Started = true;
      RaisePropertyChanged(nameof(IsStarted));
      StartRelayCommand.RaiseCanExecuteChanged();
      StopRelayCommand.RaiseCanExecuteChanged();
    }

    public void Stop()
    {
      if (Disposed)
        throw new ObjectDisposedException(nameof(MainWindowViewModel));
      if (!Started)
        return;

      ModelLayer.Stop();
      Balls.Clear();
      Started = false;
      RaisePropertyChanged(nameof(IsStarted));
      StartRelayCommand.RaiseCanExecuteChanged();
      StopRelayCommand.RaiseCanExecuteChanged();
    }

    public ObservableCollection<ModelIBall> Balls { get; } = new ObservableCollection<ModelIBall>();
    public ICommand StartCommand { get; }
    public ICommand StopCommand { get; }

    public double BoardHeight => ModelLayer.BoardHeight;
    public double BoardWidth => ModelLayer.BoardWidth;

    public bool IsStarted => Started;

    public int NumberOfBalls
    {
      get => NumberOfBallsBackingField;
      set
      {
        if (NumberOfBallsBackingField == value)
          return;
        NumberOfBallsBackingField = value;
        RaisePropertyChanged();
        StartRelayCommand.RaiseCanExecuteChanged();
      }
    }

    #endregion public API

    #region IDisposable

    protected virtual void Dispose(bool disposing)
    {
      if (!Disposed)
      {
        if (disposing)
        {
          if (Started)
            ModelLayer.Stop();
          Balls.Clear();
          Observer.Dispose();
          ModelLayer.Dispose();
        }

        Disposed = true;
      }
    }

    public void Dispose()
    {
      if (Disposed)
        throw new ObjectDisposedException(nameof(MainWindowViewModel));
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }

    #endregion IDisposable

    #region private

    private IDisposable Observer = null;
    private ModelAbstractApi ModelLayer;
    private bool Disposed = false;
    private bool Started = false;
    private int NumberOfBallsBackingField;

    private RelayCommand StartRelayCommand => (RelayCommand)StartCommand;
    private RelayCommand StopRelayCommand => (RelayCommand)StopCommand;

    private bool CanStart()
    {
      return !Disposed && !Started && NumberOfBalls > 0;
    }

    private bool CanStop()
    {
      return !Disposed && Started;
    }

    private void StartRequested()
    {
      Start(NumberOfBalls);
    }

    private void StopRequested()
    {
      Stop();
    }

    #endregion private
  }
}
