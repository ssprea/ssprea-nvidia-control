using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace sspreaNvidiaControl.Models;

public partial class UserThemeColor : ObservableObject
{
    [ObservableProperty] string _key;
    [ObservableProperty] Color _value;
}