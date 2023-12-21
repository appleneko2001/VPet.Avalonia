#if USE_OBSOLETE_ANIMATION_SYSTEM

using StateMachineNet;
using VPet.Avalonia.Debugging;
using VPet.Avalonia.Interfaces;
using VPet.Avalonia.Messages;
using VPet.Avalonia.Systems.Actions.Triggers;
using VPet.Avalonia.Systems.Graphics.Sprites;
using VPet.Avalonia.ViewModels;

namespace VPet.Avalonia.Systems.Actions;

// ANTI-TO-DO: FIX THIS SHIT ANIMATOR
// This object have been replaced by PetActionFlow. Use newest object instead.
public class PetActionStateMachine : IPetAction
{
    private readonly Fsm<AnimationState, PetActionTrigger> _animatorStateMachine;

    private readonly Func<SpriteSheetSequenceInfo> _stateIn;
    private readonly Func<SpriteSheetSequenceInfo> _stateWithin;
    private readonly Func<SpriteSheetSequenceInfo> _stateOut;
    private readonly Func<bool> _onLoopEnd;
    private readonly string _name;

    private PetWidgetViewModel? _viewModel;
    private Action? _onUnload;
    private bool _requestedTransitToEnd;
    private bool _canInterrupted;
    
    public PetActionStateMachine(string name, bool canInterrupted, Func<SpriteSheetSequenceInfo> stateIn, Func<SpriteSheetSequenceInfo> stateWithin, 
        Func<SpriteSheetSequenceInfo> stateOut, Func<bool> onLoop)
    {
        _name = name;
        _stateIn = stateIn;
        _stateWithin = stateWithin;
        _stateOut = stateOut;
        _onLoopEnd = onLoop;
        _canInterrupted = canInterrupted;
        
        var builder = Fsm<AnimationState, PetActionTrigger>
            .Builder(AnimationState.Initial);

        builder.State(AnimationState.Initial)
            .TransitionTo(AnimationState.OnStart)
            .On(PetActionTrigger.Start);
        
        builder.State(AnimationState.OnStart)
            .OnEnter(_ => StartState_OnEnter())
            .TransitionTo(AnimationState.OnLoop)
            .On(PetActionTrigger.Loop)
            
            .TransitionTo(AnimationState.Initial)
            .On(PetActionTrigger.Reset);
        
        builder.State(AnimationState.OnLoop)
            .OnEnter(_ => LoopState_OnEnter())
            .TransitionTo(AnimationState.OnEnd)
            .On(PetActionTrigger.End)
            .TransitionTo(AnimationState.Initial)
            .On(PetActionTrigger.Reset);

        builder.State(AnimationState.OnEnd)
            .OnEnter(_ => EndState_OnEnter())
            .TransitionTo(AnimationState.Initial)
            .On(PetActionTrigger.Reset);

        _animatorStateMachine = builder.Build();
    }

    public void RequestInterrupt()
    {
        if(!_canInterrupted)
            return;
        
        _onUnload?.Invoke();
    }

    public bool CanInterrupted => _canInterrupted;

    void IPetAction.OnAttachedToPet(PetWidgetViewModel vm, Action action)
    {
        _viewModel = vm;
        _onUnload = action;
    }

    public void Start()
    {
        _animatorStateMachine.Trigger(PetActionTrigger.Start);
    }

    public void OnUpdate(TimeSpan elapsed)
    {
        if(_animatorStateMachine.Current.Identifier == AnimationState.Initial)
            return;
        
        if (_requestedTransitToEnd)
        {
            _requestedTransitToEnd = false;
            _animatorStateMachine.Trigger(PetActionTrigger.End);
        }
            
        _animatorStateMachine.Update(elapsed);
    }

    void IPetAction.RequestTransitToEnd()
    {
        _requestedTransitToEnd = true;
    }

    public AnimationState CurrentState => _animatorStateMachine.Current.Identifier;

    private void StartState_OnEnter()
    {
        this.WriteLine(MessageSeverity.Debug, $"State machine \"{_name}\" on start state");
        /*
        _viewModel?.OnReceivePlaySequenceMessage(new PlaySequenceMessage
        {
            DoImmediately = true,
            SequenceInfo = _stateIn(),
            OnSequenceComplete = StartState_OnReachToEnd
        });*/
    }

    private void StartState_OnReachToEnd()
    {
        _animatorStateMachine.Trigger(PetActionTrigger.Loop);
    }
    
    private void LoopState_OnEnter()
    {
        this.WriteLine(MessageSeverity.Debug, $"State machine \"{_name}\" on loop state");
        PlaySequence_Loop();
    }

    private void PlaySequence_Loop()
    {
        /*
        _viewModel?.OnReceivePlaySequenceMessage(new PlaySequenceMessage
        {
            SequenceInfo = _stateWithin(),
            OnSequenceComplete = OnLoopAnimationEnd
        });*/
    }
    
    private void EndState_OnEnter()
    {
        this.WriteLine(MessageSeverity.Debug, $"State machine \"{_name}\" on end state");
        /*
        _viewModel?.OnReceivePlaySequenceMessage(new PlaySequenceMessage
        {
            SequenceInfo = _stateOut(),
            OnSequenceComplete = EndState_OnEnd
        });*/
    }

    private void OnLoopAnimationEnd()
    {
        if (!_onLoopEnd())
        {
            PlaySequence_Loop();
            return;
        }
        
        _animatorStateMachine.Trigger(PetActionTrigger.End);
    }

    private void EndState_OnEnd()
    {
        this.WriteLine(MessageSeverity.Debug, $"State machine \"{_name}\" completed");
        
        _onUnload?.Invoke();
        _onUnload = null;
    }

    void IResettable.Reset()
    {
        this.WriteLine(MessageSeverity.Debug, $"State machine \"{_name}\" reset");
        _animatorStateMachine.Trigger(PetActionTrigger.Reset);
    }

    public override string ToString()
    {
        return _name;
    }
}

#endif