using System.ComponentModel;
using VPet.Avalonia.Systems.Graphics.Queues;
using VPet.Avalonia.ViewModels;

namespace VPet.Avalonia.Systems.Actions;

public interface IPetAction : IBehaviourStateMachine, INotifyPropertyChanged
{
    internal void RequestInterrupt();
    internal bool CanInterrupted { get; }
    internal void OnAttachedToPet(PetWidgetViewModel vm, CancellationToken cancellationToken);
    internal void RequestTransitToEnd();
    
    internal AnimationState CurrentState { get; }
    
    internal GfxSequenceQueue? CurrentQueue { get; }
    internal void InvalidateCurrentQueue();

    /// <summary>
    /// workaround (register once a loop animation ended event action)
    /// </summary>
    /// <param name="ev">The procedure to use into the event (will be used once.)</param>
    internal void PutOneLoopEndEventHandler(Action ev);
}