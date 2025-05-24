using System;
using System.IO;
using System.Net;
using System.Text.Json;

namespace DarktideModManager.Models;

public class ModManager
{
    public const string SETTINGS_FILE = "settings.json";
    private ModLoaderSettings _settings;
    public ModManager(ref ModLoaderSettings settings)
    {
        _settings = settings;
        LoadSettings();
    }

    private void LoadSettings()
    {
        if (File.Exists(Path.Combine(Environment.CurrentDirectory, SETTINGS_FILE)))
        {
            // Load settings from file
            try
            {
                var settings = JsonSerializer.Deserialize<ModLoaderSettings>(File.ReadAllText(Path.Combine(Environment.CurrentDirectory, SETTINGS_FILE)));
                if (settings != null)
                    _settings = settings;
            }
            catch (Exception e)
            {
                Console.WriteLine($"could not load settings: {e.Message}");
            }
        }
        else
        {
            Console.WriteLine($"No settings file found.");
        }
    }
    
    
    
}