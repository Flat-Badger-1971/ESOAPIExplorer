using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ESOAPIExplorer.ViewModels;

public partial class ViewModelBase : INotifyPropertyChanged
{
    public ViewModelBase() { }

    private bool _IsBusy = false;
    public bool IsBusy
    {
        get => _IsBusy;
        set { SetProperty(ref _IsBusy, value); }
    }

    protected bool SetProperty<T>(ref T backingStore, T value,
        [CallerMemberName] string propertyName = "",
        Action onChanged = null)
    {
        //if (EqualityComparer<T>.Default.Equals(backingStore, value))
        //return false;

        backingStore = value;
        onChanged?.Invoke();
        OnPropertyChanged(propertyName);
        return true;
    }


    public virtual Task InitializeAsync(object data)
    {
        return Task.FromResult(false);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        var changed = PropertyChanged;
        if (changed == null)
            return;
        try
        {
            changed?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        finally { }
    }
}
