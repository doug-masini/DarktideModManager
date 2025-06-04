using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text.Json;

namespace DarktideModManager.Models;

public class ModManager
{
    public const string SETTINGS_FILE = "settings.json";
    private ModLoaderSettings _settings;
    public ObservableCollection<string> ZipFiles { get; private set; } = [];
    public ModManager(ref ModLoaderSettings settings)
    {
        _settings = settings;
        ScanForZipFiles();
    }
    
    public void ScanForZipFiles()
    {
        ZipFiles.Clear();
        if(Directory.Exists(_settings.ModLoaderModsDirectory))
        {
            string[] files = Directory.GetFiles(_settings.ModLoaderModsDirectory);
            foreach (var file in files) 
            {
                if (file.Contains(".zip"))
                    ZipFiles.Add(file);
            }
        }
    }
    
    public void InstallMods()
    {
        if(ZipFiles.Count > 0 && Directory.Exists(_settings.ModLoaderGameDirectory))
        {
            //Base Mod Loader
            foreach (var file in ZipFiles)
            {
                if (file.Contains("Darktide Mod Loader"))
                {
                    try
                    {
                        ZipFile.ExtractToDirectory(file, _settings.ModLoaderGameDirectory, true);
                        Process process = new Process();
                        process.StartInfo.WorkingDirectory = $"{_settings.ModLoaderGameDirectory}\\";
                        process.StartInfo.FileName = $"{_settings.ModLoaderGameDirectory}\\toggle_darktide_mods.bat";
                        process.Start();
                        process.WaitForExit();
                        ZipFiles.Remove(file);
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }

            //Mod FrameWork
            foreach (var file in ZipFiles)
            {
                if (file.Contains("Darktide Mod Framework"))
                {
                    try
                    {
                        ZipFile.ExtractToDirectory(file, $"{_settings.ModLoaderGameDirectory}\\mods", true);
                        ZipFiles.Remove(file);
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }

            //prerequisites
            foreach (var file in ZipFiles)
            {
                if (file.Contains("ui_extensions"))
                {
                    try
                    {
                        ZipFile.ExtractToDirectory(file, $"{_settings.ModLoaderGameDirectory}\\mods", true);
                        AppendModOrder(file, "ui_extension");
                        ZipFiles.Remove(file);
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }

            foreach (var file in ZipFiles)
            {
                if (file.Contains("animation_events"))
                {
                    try
                    {
                        ZipFile.ExtractToDirectory(file, $"{_settings.ModLoaderGameDirectory}\\mods", true);
                        AppendModOrder(file);
                        ZipFiles.Remove(file);
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }


            foreach (var file in ZipFiles)
            {

                    try
                    {
                        ZipFile.ExtractToDirectory(file, $"{_settings.ModLoaderGameDirectory}\\mods", true);
                        ZipArchive zipArchive = ZipFile.OpenRead(file);
                        ZipArchiveEntry entry = zipArchive.Entries[0];
                        
                        AppendModOrder(file, entry.FullName.Split('/')[0]);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                
            }
            ZipFiles.Clear();
        }

    }
    
    private void AppendModOrder(string file, string? optionalName = null)
    {
        string name; 
        if(optionalName != null)
        {
            name = optionalName;
        }
        else
        {
            name = Path.GetFileName(file).Split('-')[0];
        }

        bool lineExists = false;
        string[] lines = File.ReadAllLines($"{_settings.ModLoaderGameDirectory}\\mods\\mod_load_order.txt");
        foreach (string line in lines)
        {
            if (line.Contains(name))
            { lineExists = true; break; }
        }
        if (!lineExists)
        {
            using (StreamWriter sw = File.AppendText($"{_settings.ModLoaderGameDirectory}\\mods\\mod_load_order.txt"))
            {
                sw.WriteLine(name);
            }
        }
    }

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