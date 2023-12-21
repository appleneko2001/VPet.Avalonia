using System.Collections.Immutable;
using VPet.Avalonia.Debugging;
using VPet.Avalonia.Enums;
using VPet.Avalonia.Messages;
using VPet.Avalonia.Modules;
using VPet.Avalonia.Mutables;
using VPet.Avalonia.Systems.Graphics.Queues;
using VPet.Avalonia.Systems.Graphics.Sprites;
using VPet.Avalonia.ViewModels.Interfaces;

namespace VPet.Avalonia.Systems.Actions;

/// <summary>
/// A class that used for managing basic sequences logics (game startup, idle, state change transition, and game shutdown)
/// </summary>
// TODO: refactor and reconstruct code, now it just used for a workaround to make it like VPetSimulator.
public class PetBaseActionFlow : MutableObject
{
    private readonly Dictionary<PetState, IReadOnlyList<ISpriteSheet>> _startupSequences = new ();
    private readonly Dictionary<PetState, IReadOnlyList<ISpriteSheet>> _idleSequences = new ();
    private readonly Dictionary<PetState, IReadOnlyList<ISpriteSheet>> _shutdownSequences = new ();
    private readonly Random _random = new (DateTime.Now.GetHashCode());

    internal GfxSequenceQueue? CurrentQueue
    {
        get => GetSequence();
        private set => SetField(ref _currentQueue, value);
    }
    
    private GfxSequenceQueue? _currentQueue;
    
    private IPetStateLogic? _petStateSource;
    private PetActivityState _currentActionStep;

    internal PetBaseActionFlow(IGfxServiceInterface gfxService)
    {
        InitSequences(gfxService);

        EventBus.Current.Listen<TriggerBasicSequenceMessage>(OnReceiveTrigger);
    }

    private void OnReceiveTrigger(TriggerBasicSequenceMessage obj)
    {
        var nextQueue = CreateSequenceQueuePrivate(obj);
        CurrentQueue?.Invalidate();
        CurrentQueue = nextQueue;
    }

    private void InitSequences(IGfxServiceInterface gfxService)
    {
        foreach (PetState state in Enum.GetValues(typeof(PetState)))
        {
            var startupSeq = InitSequencesPrivate(gfxService, state, PetActivityState.Startup);

            if (!_startupSequences.TryAdd(state, startupSeq))
                PrintErrorLog_InitSequences(state, "Startup", startupSeq);
            
            var idleSeq = InitSequencesPrivate(gfxService, state, PetActivityState.CommonIdle);
            
            if (!_idleSequences.TryAdd(state, idleSeq))
                PrintErrorLog_InitSequences(state, "Common idle", idleSeq);
            
            var shutdownSeq = InitSequencesPrivate(gfxService, state, PetActivityState.Shutdown);
            
            if (!_shutdownSequences.TryAdd(state, shutdownSeq))
                PrintErrorLog_InitSequences(state, "Shutdown", shutdownSeq);
        }
    }

    private IReadOnlyList<ISpriteSheet> InitSequencesPrivate(IGfxServiceInterface gfxService, PetState state,
        PetActivityState activityState)
    {
        return gfxService
            .SearchSequences(a =>
                a.Activity == activityState &&
                a.State == state &&
                a.Transition == GfxAnimationType.Single)
            .ToImmutableArray();
    }

    /// <summary>
    /// Post initialisation procedure.
    /// </summary>
    internal void EndInit(IPetStateLogic stateSource)
    {
        _currentActionStep = PetActivityState.Startup;
        _petStateSource = stateSource;
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
        if (_petStateSource == null)
            throw new NullReferenceException("The game is not initialised completely.");
        
        if (ConfirmValidationCurrentSequenceQueue())
            return _currentQueue!;

        _currentQueue = CreateSequenceQueuePrivate();

        return GetSequence();
    }

    private GfxSequenceQueue CreateSequenceQueuePrivate(TriggerBasicSequenceMessage? msg = null)
    {
        var postAction = msg?.PostSequenceTask ?? delegate { };
        var activity = msg?.TargetState ?? _currentActionStep;
        var state = _petStateSource?.PetState ?? throw new InvalidOperationException();
        
        switch (activity)
        {
            case PetActivityState.Startup:
            {
                var collection = _startupSequences[state];
                
                var max = collection.Count;
                var i = _random.Next(0, max);
                
                // Change state immediately to skip startup sequence if user requested.
                _currentActionStep = PetActivityState.CommonIdle;
                return new GfxSequenceQueue
                {
                    Sequence = collection[i],
                };
            }

            case PetActivityState.CommonIdle:
            {
                var collection = _idleSequences[state];
                
                var max = collection.Count;
                var i = _random.Next(0, max);
                return new GfxSequenceQueue
                {
                    Sequence = collection[i],
                };
            }
            
            case PetActivityState.Shutdown:
            {
                var collection = _shutdownSequences[state];
                
                var max = collection.Count;
                var i = _random.Next(0, max);
                return new GfxSequenceQueue
                {
                    Sequence = collection[i],
                    OnSequenceComplete = postAction
                };
            }
        }

        throw new ArgumentOutOfRangeException();
    }

    internal void OnUpdate()
    {
        
    }

    private void PrintErrorLog_InitSequences(PetState state, string seqName, IReadOnlyList<ISpriteSheet> array)
    {
        this.WriteLine(MessageSeverity.Error, $"Unable to link basic sequences: (PetState: {state}, Name: {seqName} {array.Count} records.)");
    }
}