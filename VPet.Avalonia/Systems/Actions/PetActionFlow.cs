using Avalonia.Threading;
using VPet.Avalonia.Debugging;
using VPet.Avalonia.Interfaces;
using VPet.Avalonia.Mutables;
using VPet.Avalonia.Systems.Actions.Triggers;
using VPet.Avalonia.Systems.Graphics.Queues;
using VPet.Avalonia.Systems.Graphics.Sprites;
using VPet.Avalonia.ViewModels;

namespace VPet.Avalonia.Systems.Actions;

public class PetActionFlow : MutableObject, IPetAction
{
    private GfxSequenceQueue? CurrentQueue
    {
        get => GetSequence();
        set => SetField(ref _currentQueue, value);
    }

    GfxSequenceQueue? IPetAction.CurrentQueue => CurrentQueue;

    private GfxSequenceQueue? _currentQueue;
    
    private readonly Func<SpriteSheetSequenceInfo> _stateIn;
    private readonly Func<SpriteSheetSequenceInfo> _stateWithin;
    private readonly Func<SpriteSheetSequenceInfo> _stateOut;
    private readonly Func<bool> _onLoopEnd;
    private readonly string _name;

    private AnimationState _state;

    private readonly bool _canInterrupted;
    private PetWidgetViewModel? _viewModel;
    private CancellationToken? _cancellationToken;

    private readonly Queue<PetActionTrigger> _activeTriggers = new ();

    private readonly Queue<Action> _onOneLoopEndEventHandlers = new();
    
    public PetActionFlow(string name, bool canInterrupted, Func<SpriteSheetSequenceInfo> stateIn, Func<SpriteSheetSequenceInfo> stateWithin, 
        Func<SpriteSheetSequenceInfo> stateOut, Func<bool> onLoop)
    {
        _name = name;
        _stateIn = stateIn;
        _stateWithin = stateWithin;
        _stateOut = stateOut;
        _onLoopEnd = onLoop;
        _canInterrupted = canInterrupted;
    }

    public void RequestInterrupt()
    {
        if(!_canInterrupted)
            return;

        OnReachEndPrivate();
    }

    public bool CanInterrupted => _canInterrupted;

    void IPetAction.OnAttachedToPet(PetWidgetViewModel vm, CancellationToken cancellationToken)
    {
        _viewModel = vm;
        _cancellationToken = cancellationToken;
    }

    public void Start()
    {
        _state = AnimationState.OnStart;
    }

    public void OnUpdate(TimeSpan elapsed)
    {
        ValidateCancellationAcquiredPrivate();
        
        if(_state == AnimationState.Initial)
            return;
        
        Dispatcher.UIThread.Invoke(() =>
        {
            //OnUpdatePrivate(elapsed);
        });
    }
    public void RequestTransitToEnd()
    {
        CurrentState = AnimationState.OnEnd;
        _currentQueue?.Invalidate();
        BroadcastInvalidatePrivate();
    }

    public AnimationState CurrentState
    {
        get => _state;
        private set
        {
            if(SetField(ref _state, value))
                this.WriteLine(MessageSeverity.Debug, $"Animation ({this}) state: {value}");
        }
    }

    public void InvalidateCurrentQueue()
    {
        CurrentQueue?.Invalidate();
        BroadcastInvalidatePrivate();
    }

    public void PutOneLoopEndEventHandler(Action ev)
    {
        this.WriteLine(MessageSeverity.Debug, $"Register a loop end only event handler {ev}");
        _onOneLoopEndEventHandlers.Enqueue(ev);
    }

    /// <summary>
    /// Detect the view model have been requested to end the session and finalise the actions.
    /// </summary>
    private void ValidateCancellationAcquiredPrivate()
    {
        if((_cancellationToken?.IsCancellationRequested ?? false) == false)
            return;

        switch (CurrentState)
        {
            case AnimationState.Initial:
            case AnimationState.OnEnd:
            default:
                return;
            
            case AnimationState.OnStart:
            case AnimationState.OnLoop:
                CurrentState = AnimationState.OnEnd;
                break;
        }
        
        InvalidateCurrentQueue();
    }
    
    /// <summary>
    /// Check if the current sequence playback is still valid.
    /// </summary>
    /// <returns>It returns true if its valid and still playing.</returns>
    private bool ConfirmValidationCurrentSequenceQueue()
    {
        return _currentQueue?.IsValid ?? false;
    }
    
    internal GfxSequenceQueue GetSequence()
    {
        if (ConfirmValidationCurrentSequenceQueue())
            return _currentQueue!;

        _currentQueue = CreateSequenceQueuePrivate();

        return GetSequence();
    }

    private GfxSequenceQueue CreateSequenceQueuePrivate()
    {
        switch (CurrentState)
        {
            case AnimationState.OnStart:
            {
                return new GfxSequenceQueue
                {
                    Sequence = _stateIn(),
                    OnSequenceComplete = OnSequenceComplete_OnStart
                };
            }
            case AnimationState.OnLoop:
            {
                return new GfxSequenceQueue
                {
                    Sequence = _stateWithin(),
                    OnSequenceComplete = OnSequenceComplete_OnLoop
                };
            }
            case AnimationState.OnEnd:
            {
                return new GfxSequenceQueue
                {
                    Sequence = _stateOut(),
                    OnSequenceComplete = OnSequenceComplete_OnEnd
                };
            }
        }

        throw new ArgumentOutOfRangeException();
    }

    private void OnSequenceComplete_OnStart()
    {
        CurrentState = AnimationState.OnLoop;
    }

    private void OnSequenceComplete_OnLoop()
    {
        while (_onOneLoopEndEventHandlers.TryDequeue(out var ev))
        {
            this.WriteLine(MessageSeverity.Debug, $"Execute registered a loop end event action and remove.");
            ev?.Invoke();
        }
        
        if(_onLoopEnd())
            CurrentState = AnimationState.OnEnd;
    }
    
    private void OnSequenceComplete_OnEnd()
    {
        this.WriteLine(MessageSeverity.Debug, $"Animation ({this}) is end.");
        OnReachEndPrivate();
    }

    private void OnReachEndPrivate()
    {
        _viewModel?.OnActionAnimationSequenceEnd(this);
    }

    private void BroadcastInvalidatePrivate()
    {
        _viewModel?.InvalidateActionAnimationSequence(this);
    }

    void IResettable.Reset()
    {
        _state = AnimationState.Initial;
    }

    public override string ToString()
    {
        return _name;
    }
}