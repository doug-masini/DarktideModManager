using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;

namespace DarktideModManager.Models;

public class ModManager
{
    private const string MOD_LOADER_NAME = "Darktide Mod Loader";
    private const string MOD_FRAMEWORK_NAME = "Darktide Mod Framework";
    private const string UI_EXTENSIONS_NAME = "ui_extensions";
    private const string ANIMATION_EVENTS_NAME = "animation_events";
    private const string MOD_LOADER_SCRIPT_NAME = "\\toggle_darktide_mods.bat";
    private const string DARKTIDE_MODS_DIRECTORY_NAME = "\\mods";
    private const string MOD_LOAD_ORDER_NAME = "\\mod_load_order.txt";
    public const string SETTINGS_FILE = "settings.json";
    public const string WARHAMMER_FOLDER_NAME = "Warhammer 40,000 DARKTIDE";
    private readonly ModLoaderSettings _settings;
    public ObservableCollection<string> ZipFiles { get; private set; } = [];
    public ObservableCollection<string> InstalledMods { get; private set; } = [];

    public ModManager(ref ModLoaderSettings settings)
    {
        _settings = settings;
        ScanForZipFiles();
    }
    
    public void ScanForZipFiles()
    {
        ZipFiles.Clear();
        if (!Directory.Exists(_settings.ModLoaderModsDirectory)) return;
        var files = Directory.GetFiles(_settings.ModLoaderModsDirectory);
        foreach (var file in files) 
        {
            if (file.Contains(".zip"))
                ZipFiles.Add(file);
        }
    }
    
    public void InstallMods()
    {
        // check for valid directory and at least one mod
        if (ZipFiles.Count <= 0) 
            throw new InvalidOperationException($"No More Zip Files Found in the mods directory {_settings.ModLoaderModsDirectory}");
        if (!Directory.Exists(_settings.ModLoaderGameDirectory)) 
            throw new InvalidOperationException("Invalid Warhammer Game Directory");
        InstalledMods.Clear();
        
        //Base Mod Loader
        string? modLoaderFileName = ZipFiles.FirstOrDefault(x => x.Contains(MOD_LOADER_NAME, StringComparison.Ordinal));
        if (string.IsNullOrEmpty(modLoaderFileName))
            throw new InvalidOperationException("Darktide Mod Loader Not Found");
        try
        {
            ZipFile.ExtractToDirectory(modLoaderFileName, _settings.ModLoaderGameDirectory, true);
            using (Process process = new Process())
            {
                process.StartInfo.WorkingDirectory = $"{_settings.ModLoaderGameDirectory}\\";
                process.StartInfo.FileName = $"{_settings.ModLoaderGameDirectory}{MOD_LOADER_SCRIPT_NAME}";
                process.Start();
                process.WaitForExit();
            }
            ZipFiles.Remove(modLoaderFileName);
            InstalledMods.Add(Path.GetFileNameWithoutExtension(modLoaderFileName));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
        
        //Mod FrameWork
        string? modFrameWorkFileName = ZipFiles.FirstOrDefault(x => x.Contains(MOD_FRAMEWORK_NAME, StringComparison.Ordinal));
        if (string.IsNullOrEmpty(modFrameWorkFileName))
            throw new InvalidOperationException("Darktide Mod Framework Not Found");
        
        try
        {
            ZipFile.ExtractToDirectory(modFrameWorkFileName, $"{_settings.ModLoaderGameDirectory}{DARKTIDE_MODS_DIRECTORY_NAME}", true);
            ZipFiles.Remove(modFrameWorkFileName);
            InstalledMods.Add(Path.GetFileNameWithoutExtension(modFrameWorkFileName));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }


        // ui extension mod prerequisite
        string? uiExtensionFileName = ZipFiles.FirstOrDefault(x => x.Contains(UI_EXTENSIONS_NAME, StringComparison.Ordinal));
        if (string.IsNullOrEmpty(uiExtensionFileName))
            throw new InvalidOperationException("Darktide UI Extensions Mod Not Found");
        
        try
        {
            ZipFile.ExtractToDirectory(uiExtensionFileName, $"{_settings.ModLoaderGameDirectory}{DARKTIDE_MODS_DIRECTORY_NAME}", true);
            AppendModOrder(uiExtensionFileName, "ui_extension");
            ZipFiles.Remove(uiExtensionFileName);
            InstalledMods.Add(Path.GetFileNameWithoutExtension(uiExtensionFileName));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
        
        
        // animation events mod prerequisite
        string? animationEventsFileName = ZipFiles.FirstOrDefault(x => x.Contains(ANIMATION_EVENTS_NAME, StringComparison.Ordinal));
        if (string.IsNullOrEmpty(animationEventsFileName))
            throw new InvalidOperationException("Darktide UI Extensions Mod Not Found");
        
        try
        {
            ZipFile.ExtractToDirectory(animationEventsFileName, $"{_settings.ModLoaderGameDirectory}{DARKTIDE_MODS_DIRECTORY_NAME}", true);
            AppendModOrder(animationEventsFileName);
            ZipFiles.Remove(animationEventsFileName);
            InstalledMods.Add(Path.GetFileName(animationEventsFileName));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
        
        foreach (var file in ZipFiles)
        {
            try
            {
                ZipFile.ExtractToDirectory(file, $"{_settings.ModLoaderGameDirectory}{DARKTIDE_MODS_DIRECTORY_NAME}", true);
                ZipArchive zipArchive = ZipFile.OpenRead(file);
                ZipArchiveEntry entry = zipArchive.Entries[0];
                        
                AppendModOrder(file, entry.FullName.Split('/')[0]);
                InstalledMods.Add(Path.GetFileNameWithoutExtension(file));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        ZipFiles.Clear();
    }
    
    /// <summary>
    /// Appends the mod name to the mod load order text file using the name of the mod zip file
    /// </summary>
    /// <param name="file"></param>
    /// <param name="optionalName"></param>
    private void AppendModOrder(string file, string? optionalName = null)
    {
        var name = optionalName ?? Path.GetFileName(file).Split('-')[0];

        string[] lines = File.ReadAllLines($"{_settings.ModLoaderGameDirectory}{DARKTIDE_MODS_DIRECTORY_NAME}{MOD_LOAD_ORDER_NAME}");
        bool lineExists = lines.Any(line => line.Contains(name));
        if (lineExists) return;
        using StreamWriter sw = File.AppendText($"{_settings.ModLoaderGameDirectory}{DARKTIDE_MODS_DIRECTORY_NAME}{MOD_LOAD_ORDER_NAME}");
        sw.WriteLine(name);
    }
    

    /// <summary>
    /// Loads application settings from setting.json file located in the exe directory if it exists
    /// </summary>
    /// <returns></returns>
    public static ModLoaderSettings LoadSettings()
    {
        if (File.Exists(Path.Combine(Environment.CurrentDirectory, SETTINGS_FILE)))
        {
            // Load settings
            try
            {
                var settings = JsonSerializer.Deserialize<ModLoaderSettings>(File.ReadAllText(Path.Combine(Environment.CurrentDirectory, SETTINGS_FILE)));
                return settings ?? new ModLoaderSettings(string.Empty);
            }
            catch (Exception e)
            {
                Console.WriteLine($"could not load settings: {e.Message}");
                return new ModLoaderSettings(string.Empty);
            }
        }
        else
        {
            Console.WriteLine($"No settings file found.");
            return new ModLoaderSettings(string.Empty);
        }
    }
}