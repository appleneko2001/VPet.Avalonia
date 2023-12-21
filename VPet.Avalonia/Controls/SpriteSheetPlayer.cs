using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using VPet.Avalonia.Debugging;
using VPet.Avalonia.Interfaces;
using VPet.Avalonia.Services;
using VPet.Avalonia.Systems.Graphics.Sprites;

namespace VPet.Avalonia.Controls;

/// <summary>
/// Sprite sheet sequence player.
/// </summary>
public class SpriteSheetPlayer : Control
{
    public static readonly DirectPropertyBase<ISpriteSheet?> SpriteSheetProperty =
        AvaloniaProperty.RegisterDirect<SpriteSheetPlayer, ISpriteSheet?>(nameof(SpriteSheet),
            player => player.SpriteSheet, (player, queue) => player.SpriteSheet = queue);
    
    public static readonly DirectPropertyBase<double> SpeedMultiplierProperty =
        AvaloniaProperty.RegisterDirect<SpriteSheetPlayer, double>(nameof(SpeedMultiplier),
            player => player.SpeedMultiplier, (player, v) => player.SpeedMultiplier = v);

    public static readonly DirectPropertyBase<bool> IsPlayingProperty =
        AvaloniaProperty.RegisterDirect<SpriteSheetPlayer, bool>(nameof(IsPlaying),
            player => player.IsPlaying, (player, v) => player.IsPlaying = v);
    
    public static readonly DirectPropertyBase<bool> IsRepeatProperty =
        AvaloniaProperty.RegisterDirect<SpriteSheetPlayer, bool>(nameof(IsRepeat),
            player => player.IsRepeat, (player, v) => player.IsRepeat = v);
    
    public static readonly DirectPropertyBase<int> IndexProperty =
        AvaloniaProperty.RegisterDirect<SpriteSheetPlayer, int>(nameof(Index),
            player => player.Index, (player, v) => player.Index = v);

    /// <summary>
    /// Get or set sprite sheet instance that will be used for play.
    /// </summary>
    public ISpriteSheet? SpriteSheet
    {
        get => _spriteSheet;
        set => SetAndRaise(SpriteSheetProperty, ref _spriteSheet, value);
    }

    /// <summary>
    /// Playback speed multiplier
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public double SpeedMultiplier
    {
        get => _speedMultiplier;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(SpeedMultiplier),
                    "Speed multiplier shouldn't be lower than zero.");
            
            SetAndRaise(SpeedMultiplierProperty, ref _speedMultiplier, value);
        }
    }

    /// <summary>
    /// Get or set playback is playing or paused
    /// </summary>
    public bool IsPlaying
    {
        get => _isPlaying;
        set => SetAndRaise(IsPlayingProperty, ref _isPlaying, value);
    }
    
    /// <summary>
    /// A switch used for control player should or not to repeat play the current sprite sheet. 
    /// </summary>
    public bool IsRepeat
    {
        get => _isRepeat;
        set => SetAndRaise(IsRepeatProperty, ref _isRepeat, value);
    }
    
    /// <summary>
    /// Get or set the current cell index of current sprite sheet
    /// </summary>
    public int Index
    {
        get => _index;
        set => SetAndRaise(IndexProperty, ref _index, value);
    }

    /// <summary>
    /// Event handler that will be used once current sprite sheet is complete (regardless IsRepeat is true or false).
    /// </summary>
    public event EventHandler? ReachToEnd;

    private int _index;
    private bool _isPlaying;
    private bool _isRepeat;
    private ISpriteSheet? _spriteSheet;
    private double _speedMultiplier = 1.0;
    private double _totalWaitMs;
    
    /// <summary>
    /// Get a clock service that will be used for playback loop.
    /// </summary>
    private static ClockService ClockService => _clockServiceInst ??=
        new ClockService(TimeSpan.FromMilliseconds(25));
    
    private static ClockService? _clockServiceInst;

    static SpriteSheetPlayer()
    {
        SpriteSheetProperty.Changed
            .Subscribe(OnSpriteSheetPropertyChanged);
        
        AffectsRender<SpriteSheetPlayer>(SpriteSheetProperty,
            IndexProperty, IsPlayingProperty, IndexProperty, IsRepeatProperty);
    }
    
    protected override void OnInitialized()
    {
        base.OnInitialized();
        Dispatcher.UIThread.Invoke(() => ClockService.Subscribe(OnRenderUpdatePassPrivate));
    }

    private void OnRenderUpdatePassPrivate(TimeSpan delta)
    {
        OnRenderUpdatePrivate(delta);
    }

    private void OnRenderUpdatePrivate(TimeSpan delta)
    {
        if(!IsPlaying)
            return;
        
        var dMilli = delta.TotalMilliseconds;

        if (dMilli > 1000)
        {
            this.WriteLine(MessageSeverity.Warn, $"Detected severe delay {dMilli}");
            return;
        }

        _totalWaitMs += dMilli;
        var index = Index;

        var spriteSheet = SpriteSheet;
        SpriteSheetFrameInfo? frame = null;
        if(!spriteSheet?.TryGetFrame(index, out frame) ?? false)
            return;

        var wait = frame?.DelayMilliseconds;

        if (!(_totalWaitMs >= wait))
            return;
        
        _totalWaitMs -= wait.Value;
        index++;

        if (index >= spriteSheet?.CellCounts)
        {
            this.WriteLine(MessageSeverity.Debug, $"The sequence {spriteSheet} is end.");
            try
            {
                ReachToEnd?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                this.WriteLine(MessageSeverity.Warn, $"An error occurred while updating player state: {e}");
            }
            
            if (IsRepeat)
            {
                Index = 0;
                return;
            }

            _totalWaitMs = 0;
        }
        else
        {
            Index = index;
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var spriteSheet = SpriteSheet;
        var index = Index;

        if (spriteSheet == null)
            return;

        var bitmap = spriteSheet.Sprite;
        
        if(!spriteSheet.TryGetFrame(index, out var frame))
            return;
        
        if(bitmap != null)
            context.DrawImage(bitmap.Instance, frame!.Area, Bounds.WithX(0).WithY(0));
    }

    private static void OnSpriteSheetPropertyChanged(AvaloniaPropertyChangedEventArgs<ISpriteSheet?> args)
    {
        if(args.Sender is not SpriteSheetPlayer player)
            return;
        
        if(args.OldValue.Value is { Sprite: IHotLoadable unload })
            unload.Unload();

        player.OnSpriteSheetPropertyChangedPrivate(args.NewValue.Value);
    }

    private void OnSpriteSheetPropertyChangedPrivate(ISpriteSheet? spriteSheet)
    {
        _totalWaitMs = 0;
        Index = 0;
        IsPlaying = true;
    }
}