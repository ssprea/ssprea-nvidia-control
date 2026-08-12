using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using sspreaNvidiaControl.Views;

namespace sspreaNvidiaControl.Models;

public static class WindowsManager
{
    public static List<Window> AllWindows {private set; get;} = new List<Window>() ;

    public static void ApplyMainWindowCustomGui()
    {
        var mainWindow = (MainWindow)AllWindows.First(x => x.Name == "MainOcWindow");
        mainWindow.LoadGuiGrid();
    }
}