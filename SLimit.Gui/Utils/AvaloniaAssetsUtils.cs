using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Platform;

namespace SLimit.Gui.Utils;

public static class AvaloniaAssetsUtils
{
    public static List<string> GetAvailableEmbeddedAssetsGuis()
    {
        var guisAssetFolderPath = new Uri("avares://SLimit.Gui/Assets/MainWindowGuis");

        var foundGuis = new List<string>();
        
        foreach (var assetUri in AssetLoader.GetAssets(guisAssetFolderPath,null))
        {
            if (assetUri.AbsolutePath.EndsWith(".customgui", StringComparison.InvariantCultureIgnoreCase))
                foundGuis.Add(Path.GetFileName(Path.GetDirectoryName(assetUri.AbsolutePath)!));
        }
        
        return foundGuis.Distinct().ToList();
    }
}