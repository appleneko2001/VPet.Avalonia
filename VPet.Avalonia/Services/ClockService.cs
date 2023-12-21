using System.Diagnostics;
using Avalonia.Animation;
using Avalonia.Threading;
using VPet.Avalonia.Reactive;

namespace VPet.Avalonia.Services;

internal class ClockService : IObservable<TimeSpan>
{
    private readonly ClockObservable _observable;

    private TimeSpan? _previousTime;
    private TimeSpan _internalTime;

    private DispatcherTimer? _dispatcherTimer;
    private Stopwatch? _stopwatch;
    private TimeSpan _interval = TimeSpan.FromMilliseconds(10);

    internal ClockService(TimeSpan? interval = null)
    {
        if (interval.HasValue)
            _interval = interval.Value;
        
        _observable = new ClockObservable
        {
            OnInit = OnInit,
            OnFinalise = OnFinalise
        };
    }

    private void OnFinalise()
    {
        _dispatcherTimer?.Stop();
        _stopwatch?.Stop();
    }

    private void OnInit()
    {
        _dispatcherTimer ??= new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = _interval
        };
        _dispatcherTimer.Tick += OnTick;
        _stopwatch ??= new Stopwatch();
        _stopwatch.Start();
        _dispatcherTimer.Start();
    }

    private void OnTick(object? sender, EventArgs e)
    {
        Pulse(_stopwatch?.Elapsed ?? _interval);
    }

    protected bool HasSubscriptions => _observable.HasSubscriptions;

    public PlayState PlayState { get; set; }

    protected void Pulse(TimeSpan systemTime)
    {
        if (!_previousTime.HasValue)
        {
            _previousTime = systemTime;
            _internalTime = TimeSpan.Zero;
        }
        else
        {
            if (PlayState == PlayState.Pause)
            {
                _previousTime = systemTime;
                return;
            }

            var delta = systemTime - _previousTime;
            _internalTime += delta.Value;
            _previousTime = systemTime;
            
            _observable.Pulse(delta.Value);
        }

        if (PlayState == PlayState.Stop)
        {
            Stop();
        }
    }

    protected virtual void Stop()
    {
    }

    public IDisposable Subscribe(IObserver<TimeSpan> observer)
    {
        return _observable.Subscribe(observer);
    }

    private sealed class ClockObservable : LightweightObservableBase<TimeSpan>
    {
        public bool HasSubscriptions { get; private set; }
        public void Pulse(TimeSpan time) => PublishNext(time);

        public Action? OnInit;
        public Action? OnFinalise;

        protected override void Initialize()
        {
            HasSubscriptions = true;
            OnInit?.Invoke();
        }

        protected override void Deinitialize()
        {
            HasSubscriptions = false;
            OnFinalise?.Invoke();
        }
    }
}