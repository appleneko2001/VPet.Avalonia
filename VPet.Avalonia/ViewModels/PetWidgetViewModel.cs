using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;
using Avalonia;
using ReactiveUI;
using VPet.Avalonia.Debugging;
using VPet.Avalonia.Enums;
using VPet.Avalonia.Messages;
using VPet.Avalonia.Mutables;
using VPet.Avalonia.Systems;
using VPet.Avalonia.Systems.Actions;
using VPet.Avalonia.Systems.Graphics.Queues;
using VPet.Avalonia.Systems.Graphics.Sprites;
using VPet.Avalonia.ViewModels.Commands;
using VPet.Avalonia.ViewModels.Overlays;

namespace VPet.Avalonia.ViewModels;

public class PetWidgetViewModel : ReactiveObject
{
    public ISpriteSheet? Sprites => CurrentQueue?.Sequence;

    public bool IsRepeat => CurrentQueue?.IsInLoop ?? false;
    
    public GfxSequenceQueue? CurrentQueue
    {
        get => _currentQueue;
        set
        {
            //this.RaiseAndSetIfChanged(ref _currentQueue, value);
            _currentQueue = value;
            this.RaisePropertyChanged();
            this.RaisePropertyChanged(nameof(Sprites));
            this.RaisePropertyChanged(nameof(IsRepeat));
            Index = 0;
        }
    }
    
    public int WidgetSize
    {
        get => _widgetSize;
        internal set => this.RaiseAndSetIfChanged(ref _widgetSize, value);
    }
    
    public GamePrepViewModel? GamePrepOverlay
    {
        get => _gamePrepOverlay;
        internal set => this.RaiseAndSetIfChanged(ref _gamePrepOverlay, value);
    }

    public double ToolbarOpacity
    {
        get => _cancellationTokenSource.IsCancellationRequested ? 0 : _toolbarOpacity;
        internal set => this.RaiseAndSetIfChanged(ref _toolbarOpacity, value);
    }

    public bool IsPlaying
    {
        get => _isPlaying;
        set => this.RaiseAndSetIfChanged(ref _isPlaying, value);
    }

    public int Index
    {
        get => _index;
        set => this.RaiseAndSetIfChanged(ref _index, value);
    }

    public bool IsOpenStatusPanel
    {
        get => _isOpenStatusPanel;
        set => this.RaiseAndSetIfChanged(ref _isOpenStatusPanel, value);
    }
    
    public ICommand ExitCommand => GlobalCommands.ExitCommand;

    public PixelPoint WidgetPosition
    {
        get => _widgetPosition;
        internal set => this.RaiseAndSetIfChanged(ref _widgetPosition, value);
    }

    private bool _isOpenStatusPanel;
    private bool _isPlaying;
    
    private PixelPoint _widgetPosition;
    private int _widgetSize;
    private int _index;
    private GamePrepViewModel? _gamePrepOverlay;
    
    private GfxSequenceQueue? _currentQueue;

    private PetBaseActionFlow? _baseAnimationLayer;
    private double _toolbarOpacity;

    private readonly MutableStack<IPetAction> _actionsStack = new ();
    private readonly Queue<GfxSequenceQueue> _queues = new ();

    private CancellationTokenSource _cancellationTokenSource = new ();

    public PetWidgetViewModel()
    {
        _actionsStack.CollectionChanged += OnActionStackChanged;
        
        WidgetSize = 300;
        var mBus = EventBus.Current;

        mBus.Listen<GamePrepareTextMessage>(OnReceiveGamePrepareTextMessage);
        //mBus.Listen<PlaySequenceMessage>(OnReceivePlaySequenceMessage);
        mBus.Listen<StopViewModelMessage>(OnReceiveStopServiceMessage);
        mBus.Listen<ClearNextSequenceMessage>(OnReceiveClearNextSequenceMessage);
        mBus.Listen<TryLetPetDoActionMessage>(OnReceiveTryLetPetDoActionMessage);
        WidgetPosition = PixelPoint.FromPoint(new Point(500, 500), 1);
    }

    public void OnReceiveTryLetPetDoActionMessage(TryLetPetDoActionMessage msg) 
    {
        if (_actionsStack.TryPeek(out var latest) && latest == msg.Action)
        {
            if (latest.CurrentState == AnimationState.OnEnd)
            {
                latest.RequestInterrupt();
                latest.Reset();
            }
            else
                return;
        }

        UnloadInterruptedActionsPrivate(null);
        
        _actionsStack.Push(msg.Action);
        TryPlayNextSequencePrivate();
    }

    private void UnloadInterruptedActionsPrivate(IPetAction? skip)
    {
        IPetAction? prev = default;
        while (_actionsStack.TryPeek(out var last))
        {
            if(skip == last)
                break;
            
            if(!last.CanInterrupted)
                break;
                    
            if(prev == last)
                break;
                    
            last.RequestInterrupt();
            prev = last;
        }
    }

    private void OnActionStackChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        this.WriteLine(MessageSeverity.Debug, $"Pet animator stacks had a changes {_actionsStack.Count}");

        /*
        var triedUnloadInterrupt = false;
        
        void UnloadInterruptOnce(IPetAction? skip)
        {
            if(triedUnloadInterrupt)
                return;
            
            UnloadInterruptedActionsPrivate(skip);
            triedUnloadInterrupt = true;
        }*/
        
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
            {
                foreach (IPetAction queue in e.NewItems ?? throw new InvalidOperationException())
                {
                    queue.OnAttachedToPet(this, _cancellationTokenSource.Token);
                }
                
                _currentQueue?.Invalidate();
                //_actionsStack.Current?.Start();
            } break;

            
            case NotifyCollectionChangedAction.Remove:
            {
            } break;
        }
        
        var current = _actionsStack.Current;
                
        if(current == null)
            return;
                
        //_queues.Clear();
        current.Reset();
        current.Start();
    }

    public void TouchHead()
    {
        EventBus.Current.Post(new InteractToPetMessage
        {
            Action = InteractPetActionKind.TouchHead
        });
    }

    public void TouchBody()
    {
        EventBus.Current.Post(new InteractToPetMessage
        {
            Action = InteractPetActionKind.TouchBody
        });
    }
    
    public void Sleep()
    {
        EventBus.Current.Post(new InteractToPetMessage
        {
            Action = InteractPetActionKind.Sleep
        });
    }

    internal void OnActionAnimationSequenceEnd(PetActionFlow action)
    {
        if(_actionsStack.TryPeek(out var latest) && action != latest)
            return;

        _actionsStack.TryPop(out _);
    }

    internal void InvalidateActionAnimationSequence(PetActionFlow action)
    {
        if(_actionsStack.TryPeek(out var latest) && action != latest)
            return;

        TryPlayNextSequencePrivate();
        //
    }

    private GfxSequenceQueue GetCurrentQueuePrivate()
    {
        var sequence = _actionsStack.Current?.CurrentQueue ??
                       _baseAnimationLayer?.CurrentQueue ??
                       throw new InvalidOperationException("");
        return sequence;
    }

    private void OnReceiveClearNextSequenceMessage(ClearNextSequenceMessage obj)
    {
        _queues.Clear();
    }

    private void OnReceiveStopServiceMessage(StopViewModelMessage obj)
    {
        _cancellationTokenSource?.Cancel();
        this.RaisePropertyChanged(nameof(ToolbarOpacity));
    }

#if USE_OBSOLETE_ANIMATION_SYSTEM

    internal void OnReceivePlaySequenceMessage(PlaySequenceMessage msg)
    {
        var newQueue = new GfxSequenceQueue
        {
            Sequence = msg.SequenceInfo,
            IsInLoop = false,
            OnSequenceComplete = msg.OnSequenceComplete
        };
        this.WriteLine(MessageSeverity.Debug, $"Received message: {newQueue}");
        if (msg.DoImmediately)
        {
            _queues.Clear();
            Index = 0;
            //CurrentQueue = newQueue;
            DoNextSequence(newQueue);
            OnCurrentSpritePlaybackCompleted();
        }
        else
        {
            // Workaround
            if (Sprites == msg.SequenceInfo)
            {
                Index = 0;
            }
            
            DoNextSequence(newQueue);
        }
    }

    private void DoNextSequence(GfxSequenceQueue newQueue)
    {
        _queues.Enqueue(newQueue);
        if(CurrentQueue == null)
            TryPlayNextSequencePrivate();
    }
#endif

    private void OnReceiveGamePrepareTextMessage(GamePrepareTextMessage msg)
    {
        if (msg.IsComplete)
        {
            GamePrepOverlay = null;
            PostInitPrivate(msg.Params);
            return;
        }
        
        GamePrepOverlay ??= new GamePrepViewModel();
        GamePrepOverlay.Text = msg.Text ?? string.Empty;
    }

    private void PostInitPrivate(IReadOnlyList<object> @params)
    {
        _baseAnimationLayer = (PetBaseActionFlow)@params[0];
        _baseAnimationLayer.PropertyChanged += BaseAnimationLayerOnPropertyChanged;
        OnCurrentSpritePlaybackCompleted();
    }

    private void BaseAnimationLayerOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(PetBaseActionFlow.CurrentQueue):
                TryPlayNextSequencePrivate();
                break;
        }
    }

    public void OnCurrentSpritePlaybackCompleted()
    {
        var currentQueue = CurrentQueue;
        currentQueue?.Invalidate();
        currentQueue?.OnSequenceComplete?.Invoke();

        TryPlayNextSequencePrivate();
    }

    private void TryPlayNextSequencePrivate()
    {
        var nextQueue = !_queues.TryDequeue(out var next) ? GetCurrentQueuePrivate() : next;
        this.WriteLine(MessageSeverity.Debug, $"Play next sequence {nextQueue?.Sequence}");
        CurrentQueue = nextQueue;
    }
}