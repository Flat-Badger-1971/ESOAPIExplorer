using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ESOAPIExplorer.DisplayModels;

public partial class DisplayModelBase<T> : INotifyPropertyChanged
{
    private T _Value;
    public T Value { get => _Value; set { SetProperty(ref _Value, value); } }
    public DisplayModelBase() { }

    private bool _IsVisible = false;
    public bool IsVisible
    {
        get => _IsVisible;
        set => SetProperty(ref _IsVisible, value);
    }

    private bool _IsSelected = false;
    public bool IsSelected
    {
        get => _IsSelected;
        set => SetProperty(ref _IsSelected, value);
    }

    protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "", Action onChanged = null)
    {
        backingStore = value;
        onChanged?.Invoke();
        OnPropertyChanged(propertyName);

        return true;
    }

    #region INotifyPropertyChanged
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
    #endregion
}
