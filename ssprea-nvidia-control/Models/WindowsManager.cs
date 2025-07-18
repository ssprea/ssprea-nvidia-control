using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using ssprea_nvidia_control.Views;

namespace ssprea_nvidia_control.Models;

public static class WindowsManager
{
    public static List<Window> AllWindows {private set; get;} = new List<Window>() ;

    public static void ApplyMainWindowCustomGui()
    {
        var mainWindow = (MainWindow)AllWindows.First(x => x.Name == "MainOcWindow");
        mainWindow.LoadGuiGrid();
    }
}