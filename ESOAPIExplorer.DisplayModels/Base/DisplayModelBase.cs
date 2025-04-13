using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ESOAPIExplorer.DisplayModels;

public partial class DisplayModelBase<T> : INotifyPropertyChanged
{
    private T _value;
    public T Value { get => _value; set { SetProperty(ref _value, value); } }
    public DisplayModelBase() { }

    private bool _isVisible = false;
    public bool IsVisible
    {
        get => _isVisible;
        set => SetProperty(ref _isVisible, value);
    }

    private bool _isSelected = false;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    protected bool SetProperty<U>(ref U backingStore, U value, [CallerMemberName] string propertyName = "", Action onChanged = null)
    {
        backingStore = value;
        onChanged?.Invoke();
        OnPropertyChanged(propertyName);

        return true;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChangedEventHandler changed = PropertyChanged;

        if (changed == null)
        {
            return;
        }

        try
        {
            changed?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        finally { }
    }
}
