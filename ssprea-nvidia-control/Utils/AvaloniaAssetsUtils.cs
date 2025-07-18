using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Platform;

namespace ssprea_nvidia_control.Utils;

public static class AvaloniaAssetsUtils
{
    public static List<string> GetAvailableEmbeddedAssetsGuis()
    {
        var guisAssetFolderPath = new Uri("avares://ssprea-nvidia-control/Assets/MainWindowGuis");

        var foundGuis = new List<string>();
        
        foreach (var assetUri in AssetLoader.GetAssets(guisAssetFolderPath,null))
        {
            if (assetUri.AbsolutePath.EndsWith(".customgui"))
                foundGuis.Add(Path.GetFileName(Path.GetDirectoryName(assetUri.AbsolutePath)!));
        }
        
        return foundGuis.Distinct().ToList();
    }
}