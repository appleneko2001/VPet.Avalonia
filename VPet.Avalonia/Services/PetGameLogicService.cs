using System.Collections.Immutable;
using System.Diagnostics;
using Avalonia.Threading;
using ReactiveUI;
using VPet.Avalonia.Debugging;
using VPet.Avalonia.Enums;
using VPet.Avalonia.Messages;
using VPet.Avalonia.Modules;
using VPet.Avalonia.Options;
using VPet.Avalonia.Services.Interfaces;
using VPet.Avalonia.Systems;
using VPet.Avalonia.Systems.Actions;
using VPet.Avalonia.Systems.Graphics.Sprites;
using VPet.Avalonia.ViewModels.Interfaces;

namespace VPet.Avalonia.Services;

public class PetGameLogicService : ReactiveObject, IApplicationService, IPetStateLogic
{
    public PetState PetState
    {
        get => _state;
        private set => this.RaiseAndSetIfChanged(ref _state, value);
    }
    
    private readonly Thread _logicThread;
    private readonly Queue<Action> _queues;
    private readonly CancellationTokenSource _cancellationTokenSource = new ();
    private readonly Stopwatch _stopwatch;
    
    private readonly CoreModuleService _coreModuleService;
    private readonly Random _random = new ();
    private int _updateRateMilli = 1000 / 60;

    private readonly Dictionary<InteractPetActionKind, IPetAction> _petActions = new ();

    /// <summary>
    /// TODO: This field should not be here, move it to VPet.Avalonia.Providers.VPetSimulator after refactor is complete.
    /// </summary>
    private readonly Dictionary<PetState, IReadOnlyList<SpriteSheetSequenceInfo>> _petDragMovesAnimations = new();

    private PetBaseActionFlow? _baseLayer;

    private bool _petHeadRemains;
    private bool _touchBodyRemains;
    private bool _petDragging;
    private bool _petDragMoving;

    private bool _petSleeping;

    private PetState _state = PetState.Normal;
    
    private IGfxServiceInterface? _gfxService;
    
    public PetGameLogicService()
    {
        _coreModuleService = new CoreModuleService();
        _stopwatch = new Stopwatch();
        _queues = new Queue<Action>();
        _logicThread = new Thread(OnThreadLoop)
        {
            Name = "GameLogic"
        };
        
        // TODO: make the message bus usable internal only.

        //MessageBus.Current.Listen<RequestPlaySequenceMessage>()
        //    .Subscribe(OnReceiveRequestPlaySequenceMessage);
        EventBus.Current.Listen<StopServiceMessage>(OnReceiveStopServiceMessage);
        EventBus.Current.Listen<InteractToPetMessage>(OnReceiveInteractToPetMessage);

        PetState = PetState.Happy;
    }

    private void OnReceiveInteractToPetMessage(InteractToPetMessage msg)
    {
        // TODO: Refactor
        switch (msg.Action)
        {
            case InteractPetActionKind.Sleep:
                if (!_petSleeping)
                {
                    EventBus.Current.Post(new TryLetPetDoActionMessage
                    {
                        Action = _petActions[InteractPetActionKind.Sleep]
                    });
                    _petSleeping = true;
                }
                else
                {
                    _petActions[InteractPetActionKind.Sleep]?.RequestTransitToEnd();
                    _petActions[InteractPetActionKind.Sleep]?.InvalidateCurrentQueue();
                    _petSleeping = false;
                }
                break;
            
            case InteractPetActionKind.TouchHead:
                EventBus.Current.Post(new TryLetPetDoActionMessage
                {
                    Action = _petActions[InteractPetActionKind.TouchHead]
                });
                _petHeadRemains = true;
                break;
            
            
            case InteractPetActionKind.TouchBody:
                EventBus.Current.Post(new TryLetPetDoActionMessage
                {
                    Action = _petActions[InteractPetActionKind.TouchBody]
                });
                _touchBodyRemains = true;
                break;

            case InteractPetActionKind.StartDrag:
            {
                var moving = (bool)(msg.Params ??
                                    throw new ArgumentNullException(nameof(InteractToPetMessage.Params)));
                var action = _petActions[InteractPetActionKind.StartDrag];
                
                if (_petDragMoving == false && moving)
                {
                    // Change animation to dragging with self movement
                    if (action.CurrentState != AnimationState.Initial)
                    {
                        _petDragMoving = true;
                        action.InvalidateCurrentQueue();
                        action.PutOneLoopEndEventHandler(() =>
                        {
                            // Change state once a loop animation ended.
                            if (_petDragMoving)
                                _petDragMoving = false;
                        });
                    }
                }
                    
                EventBus.Current.Post(new TryLetPetDoActionMessage
                {
                    Action = action
                });
                
                _petDragging = true;
            }break;
            
            case InteractPetActionKind.EndDrag:
                _petActions[InteractPetActionKind.StartDrag].RequestTransitToEnd();
                _petDragging = false;
                _petDragMoving = false;
                break;
        }
    }
    
    private void OnReceiveStopServiceMessage(StopServiceMessage msg)
    {
        this.WriteLine(MessageSeverity.Info, "Received stop service signal, finalise services...");
        _cancellationTokenSource.Cancel();
    }

    private void OnThreadLoop()
    {
        try
        {
            GamePrepTask();
        }
        catch (TaskCanceledException)
        {
            Process.GetCurrentProcess().Kill();
        }
        catch (Exception e)
        {
            using var @lock = new ManualResetEventSlim();
            
            EventBus.Current.Post(new ShowMessageBoxMessage
            {
                Title = "Error",
                SupportingText = e.ToString(),
                OnDialogClosed = () =>
                {
                    @lock.Set();
                }
            });

            @lock.Wait();
            
            // Terminate main thread because the game preparation task is fail
            Dispatcher.UIThread.Invoke(() => throw new AggregateException(e));
            throw;
        }
        
        _stopwatch.Start();
        
        var prev = TimeSpan.Zero;

        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            while (_queues.TryDequeue(out var action))
            {
                try
                {
                    action?.Invoke();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            try
            {
                foreach (var pair in _petActions)
                {
                    pair.Value.OnUpdate(prev);
                }
            }
            catch (Exception e)
            {
                this.WriteLine(MessageSeverity.Error, e.ToString());
            }

            OnUpdate_BaseLayer();

            var current = _stopwatch.Elapsed;
            var delta = current - prev;
            prev = current;

            if (!(delta.TotalMilliseconds < _updateRateMilli))
                continue;
            
            var sleep = _updateRateMilli - delta.TotalMilliseconds;
            Thread.Sleep((int)sleep);
        }
        
        _stopwatch.Stop();
    }

    private void OnUpdate_BaseLayer()
    {
        try
        {
            _baseLayer?.OnUpdate();
        }
        catch (Exception e)
        {
            this.WriteLine(MessageSeverity.Error, e.ToString());
        }
    } 

    public void Init()
    {
        _logicThread.Start();
    }

    internal void Post(Action action) => _queues.Enqueue(action);

    /// <summary>
    /// Game prepare procedure. It loads all possible relevant assets and link them together to use.
    /// </summary>
    /// <exception cref="DirectoryNotFoundException"></exception>
    private void GamePrepTask()
    {
        var spriteCellSize = 500u;
        
        BroadcastGamePrepLoading("Loading core modules...");
        _coreModuleService.LoadModules(PetApp.ApplicationRootPath);
        
        BroadcastGamePrepLoading("Loading assets...");
        
        WorkDirService.CreateCacheFolderIfNotExist();
        var cacheFolderPath = WorkDirService.CachePath;
        
        _coreModuleService.InitAssetsProviders(new ApplicationGfxOptions
        {
            CacheFolderPath = cacheFolderPath
        }, a => BroadcastGamePrepLoading($"Loading assets... {a * 100:F}%"));
        _gfxService = _coreModuleService.GetGfxService();
        
        //BroadcastGamePrepLoading("Validating cache...");
        _gfxService.CreateAnimationSequencesInfo(spriteCellSize);
        
        BroadcastGamePrepLoading("Loading sprites...");
        _gfxService.PreloadAllGfxSequences();
        
        BroadcastGamePrepLoading("Constructing animation sequences...");
        // Create common animation sequences collection. They will be used as common animations source and management.
        // It contains animations: "Startup", "Common idle", "State changing transition", "Shutdown"
        // Additional animations should be provided by "PetActionStateMachine" or "PetActionFlow".
        var baseLayer = new PetBaseActionFlow(_gfxService);
        Prep_PetDragMovesAnimations();
        
        BroadcastGamePrepLoading("Loading game logic...");
        PrepareStateSystem();
        
        baseLayer.EndInit(this);
        BroadcastGamePrepReady(baseLayer);

        _baseLayer = baseLayer;

        // Broadcast and push the first sequences and common idle sequences loop.
        // its known buggy thing rn, gonna reconstruct and create a new class to manage it correctly.
        /*
        var defaultSeq = _gfxService.SearchSequences(a =>
                a.Activity == PetActivityState.Default &&
                a.State == _state)
            ?.FirstOrDefault();

        var startupSeq = GetActionGfxSequencePrivate(PetActivityState.Startup,
            GfxAnimationType.Single, true);

        if (startupSeq != null)
        {
            PushSequences(startupSeq, onComplete: () =>
            {
                if (defaultSeq == null)
                    return;

                var queue = new GfxSequenceQueue
                {
                    OnSequenceComplete = OnDefaultSequenceComplete,
                    Sequence = defaultSeq,
                    IsInLoop = true
                };

                PostMessageOnUiThread(new SetDefaultSequenceMessage
                {
                    Sequence = queue
                });
                PushSequences(defaultSeq);
            });
        }*/
    }

    private void PrepareStateSystem()
    {
        // Those thing should be within progress "construct animation sequences".
        // they are requires refactor work, too.
        _petActions.Add(InteractPetActionKind.TouchHead, 
            new PetActionFlow("Pet head", true, 
                () => GetActionGfxSequencePrivate(PetActivityState.TouchHead, GfxAnimationType.Start, false)!,
                () => GetActionGfxSequencePrivate(PetActivityState.TouchHead, GfxAnimationType.Loop, false)!,
                () => GetActionGfxSequencePrivate(PetActivityState.TouchHead, GfxAnimationType.End, false)!,
                () =>
                {
                    if (!_petHeadRemains)
                        return true;
                    
                    _petHeadRemains = false;
                    return false;

                }));
        
        _petActions.Add(InteractPetActionKind.StartDrag, 
            new PetActionFlow("Dragging pet", true, 
                () => GetActionGfxSequencePrivate(PetActivityState.RaisedStatic, GfxAnimationType.Start, false)!,
                () =>
                {
                    if (_petDragMoving == false)
                        return GetActionGfxSequencePrivate(PetActivityState.RaisedStatic, GfxAnimationType.Loop,
                            false)!;
                    
                    var arr = _petDragMovesAnimations[PetState];
                    return arr[_random.Next(0, arr.Count)];
                },
                () => GetActionGfxSequencePrivate(PetActivityState.RaisedStatic, GfxAnimationType.End, false)!,
                () => !_petDragging));
        
        _petActions.Add(InteractPetActionKind.Sleep,
            new PetActionFlow("Sleep", false,
                () => GetActionGfxSequencePrivate(PetActivityState.Sleep, GfxAnimationType.Start, true)!,
                () => GetActionGfxSequencePrivate(PetActivityState.Sleep, GfxAnimationType.Loop, true)!,
                () => GetActionGfxSequencePrivate(PetActivityState.Sleep, GfxAnimationType.End, true)!,
            () => !_petSleeping));
        
        _petActions.Add(InteractPetActionKind.TouchBody,
            new PetActionFlow("Touch body", true,
                () => GetActionGfxSequencePrivate(PetActivityState.TouchBody, GfxAnimationType.Start, true)!,
                () => GetActionGfxSequencePrivate(PetActivityState.TouchBody, GfxAnimationType.Loop, true)!,
                () => GetActionGfxSequencePrivate(PetActivityState.TouchBody, GfxAnimationType.End, true)!,
                () =>
                {
                    if (!_touchBodyRemains)
                        return true;
                    
                    _touchBodyRemains = false;
                    return false;
                }));
    }

    // Move it to other project with PetDragMovesAnimations
    private void Prep_PetDragMovesAnimations()
    {
        foreach (PetState state in Enum.GetValues(typeof(PetState)))
        {
            var loopAnimations = _gfxService!
                .SearchSequences(a =>
                    a.Activity == PetActivityState.RaisedDynamic &&
                    a.State == state &&
                    a.Transition == GfxAnimationType.Single);

            _petDragMovesAnimations.Add(state, loopAnimations.ToImmutableArray());
        }
    }
    
    private SpriteSheetSequenceInfo? GetActionGfxSequencePrivate(PetActivityState activity, 
        GfxAnimationType animationType, bool random)
    {
        var groupEnumerable = _gfxService!
            .SearchSequences(
                a => a.Activity == activity &&
                     a.State == _state &&
                     a.Transition == animationType);
        
        if(!random)
            return groupEnumerable.FirstOrDefault();

        var group = groupEnumerable.ToImmutableArray();
        var i = _random.Next(0, group.Length);
        return group[i];
    }

    private void BroadcastGamePrepReady(PetBaseActionFlow baseLayer) => PostMessageOnUiThread(new GamePrepareTextMessage
    {
        IsComplete = true,
        Params = new object[]
        {
            baseLayer
        }
    });
    
    private void BroadcastGamePrepLoading(string text) => PostMessageOnUiThread(new GamePrepareTextMessage
    {
        Text = text
    });

    private void PostMessageOnUiThread<T>(T msg)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            EventBus.Current.Post(msg);
        }, DispatcherPriority.Background);
    }
    
    public void Dispose()
    {
        // TODO release managed resources here
        _cancellationTokenSource?.Cancel();
    }
}