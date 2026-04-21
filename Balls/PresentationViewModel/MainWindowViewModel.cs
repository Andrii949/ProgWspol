
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
    #region ctor

    public MainWindowViewModel() : this(null)
    { }

    internal MainWindowViewModel(ModelAbstractApi modelLayerAPI)
    {
      ModelLayer = modelLayerAPI == null ? ModelAbstractApi.CreateModel() : modelLayerAPI;
      StartCommand = new RelayCommand(StartRequested, CanStart);
      Observer = ModelLayer.Subscribe<ModelIBall>(x => Balls.Add(x));
      NumberOfBalls = 5;
    }

    #endregion ctor

    #region public API

    public void Start(int numberOfBalls)
    {
      if (Disposed)
        throw new ObjectDisposedException(nameof(MainWindowViewModel));
      if (Started)
        return;

      Balls.Clear();
      ModelLayer.Start(numberOfBalls);
      Started = true;
      RaisePropertyChanged(nameof(IsStarted));
      StartRelayCommand.RaiseCanExecuteChanged();
    }

    public ObservableCollection<ModelIBall> Balls { get; } = new ObservableCollection<ModelIBall>();
    public ICommand StartCommand { get; }

    public double BoardHeight => BusinessLogicAbstractAPI.GetDimensions.TableHeight;
    public double BoardWidth => BusinessLogicAbstractAPI.GetDimensions.TableWidth;

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

    private bool CanStart()
    {
      return !Disposed && !Started && NumberOfBalls > 0;
    }

    private void StartRequested()
    {
      Start(NumberOfBalls);
    }

    #endregion private
  }
}
