using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using VPet.Avalonia.Debugging;

namespace VPet.Avalonia.Mutables;

internal class MutableStack<T> : Stack<T>, INotifyCollectionChanged, INotifyPropertyChanged
{
    public T? Current
    {
        get => _current;
        private set
        {
            var old = _current;
            if(ReferenceEquals(old, value))
                return;

            _current = value;
            OnPropertyChanged(nameof(Current));
        }
    }

    private T? _current;
    
    #region Constructors

    public MutableStack()
    {
        
    }

    public MutableStack(IEnumerable<T> collection) : base(collection)
    {
        
    }

    public MutableStack(int capacity) : base(capacity)
    {
        
    }

    #endregion

    #region Overrides

    public new virtual T Pop()
    {
        var item = base.Pop();
        Current = TryPeek(out var i) ? i : default;
        OnCollectionChanged(NotifyCollectionChangedAction.Remove, item);
        
        return item;
    }

    public new bool TryPop([MaybeNullWhen(false)] out T result)
    {
        result = default;
        try
        {
            result = Pop();
            return true;
        }
        catch (Exception e)
        {
            this.WriteLine(MessageSeverity.Error, e.Message);
        }

        return false;
    }

    public new virtual void Push(T item)
    {
        base.Push(item);
        Current = item;
        OnCollectionChanged(NotifyCollectionChangedAction.Add, item);
    }

    public new virtual void Clear()
    {
        base.Clear();
        Current = default;
        OnCollectionChanged(NotifyCollectionChangedAction.Reset, default!);
    }

    #endregion

    #region CollectionChanged

    public virtual event NotifyCollectionChangedEventHandler? CollectionChanged;

    protected virtual void OnCollectionChanged(NotifyCollectionChangedAction action, T item)
    {
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(
            action
            , item
            , item == null ? -1 : 0)
        );

        OnPropertyChanged(nameof(Count));
    }

    #endregion

    #region PropertyChanged

    public virtual event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
}