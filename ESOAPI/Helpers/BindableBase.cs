using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ESOAPI.Helpers;

public class BindableBase : Microsoft.UI.Xaml.DependencyObject
{
    protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
    {
        if (Equals(storage, value)) return false;

        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}