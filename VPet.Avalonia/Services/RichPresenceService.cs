using System.Reactive.Disposables;
using ReactiveUI;
using VPet.Avalonia.Messages;
using VPet.Avalonia.Services.Interfaces;

namespace VPet.Avalonia.Services;

public class RichPresenceService : ReactiveObject, IApplicationService
{
    private readonly IDisposable? _disposable;
    private IReadOnlyList<IRichPresenceService> _richPresenceSources;
    
    public RichPresenceService(IReadOnlyList<IRichPresenceService> sources)
    {
        var mBus = MessageBus.Current;

        var compositeDisposable = new CompositeDisposable
        {
            mBus.Listen<RichPresenceBroadcastMessage>()
                .Subscribe(OnReceiveRichPresenceBroadcastMessage)
        };

        _disposable = compositeDisposable;

        _richPresenceSources = sources;
    }

    private void OnReceiveRichPresenceBroadcastMessage(RichPresenceBroadcastMessage obj)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        // TODO release managed resources here
        _disposable?.Dispose();
    }
}