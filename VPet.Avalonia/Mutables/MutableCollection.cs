using System.Collections.ObjectModel;

namespace VPet.Avalonia.Mutables
{
    public class MutableCollection<T> : ObservableCollection<T>, IMutableCollection
    {
        public MutableCollection()
        {
            
        }

        public MutableCollection(IEnumerable<T> e) : base (e)
        {
        }

        public event EventHandler? UpdateRequested;

        void IMutableCollection.TryUpdate()
        {
            UpdateRequested?.Invoke(this, System.EventArgs.Empty);
        }
    }
}
