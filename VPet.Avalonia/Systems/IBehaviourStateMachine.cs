using VPet.Avalonia.Interfaces;

namespace VPet.Avalonia.Systems;

public interface IBehaviourStateMachine : IResettable
{
    internal void Start();
    
    internal void OnUpdate(TimeSpan elapsed);   
}