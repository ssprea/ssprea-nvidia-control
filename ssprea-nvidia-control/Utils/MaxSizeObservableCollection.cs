using System.Collections.ObjectModel;

namespace ssprea_nvidia_control.Utils;

public class MaxSizeObservableCollection<T> : ObservableCollection<T>
{
    private readonly int _maxSize;

    public MaxSizeObservableCollection(int maxSize)
    {
        _maxSize = maxSize;
    }

    protected override void InsertItem(int index, T item)
    {
        base.InsertItem(index, item);

        if (Count > _maxSize)
        {
            RemoveAt(0);
        }
    }
}