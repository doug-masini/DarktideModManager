using System;
using System.IO;
using System.Text.Json.Serialization;

namespace DarktideModManager.Models;

public class ModLoaderSettings
{
    public string? ModLoaderGameDirectory { get; set; }
    public string? ModLoaderModsDirectory { get; private set; }

    [JsonConstructor]
    public ModLoaderSettings(string modLoaderGameDirectory)
    {
        ModLoaderGameDirectory = modLoaderGameDirectory;
        ModLoaderModsDirectory = $"{Environment.CurrentDirectory}/mods/";
        InitializeModDirectory();
    }
    
    private void InitializeModDirectory()
    {
        if (!string.IsNullOrEmpty(ModLoaderModsDirectory) && !Directory.Exists(ModLoaderModsDirectory))
        {
            try
            {
                Directory.CreateDirectory(ModLoaderModsDirectory);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not create mods directory: {e.Message}");
            }
        }
    }
}