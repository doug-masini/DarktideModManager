using System;
using System.IO;
using System.Reactive;
using DarktideModManager.Models;
using ReactiveUI;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DarktideModManager.ViewModels;
public class MainViewModel : ViewModelBase
{
    private ViewModelBase _currentView = null!;
    private readonly ModLoaderViewModel _modLoaderViewModel;
    private readonly ModLoaderSettings _modLoaderSettings;
    
    public Interaction<Unit, string?> ShowOpenFileDialog { get; }
    public ViewModelBase CurrentView 
    {
        get => _currentView;
        set => this.RaiseAndSetIfChanged(ref _currentView, value);
    }

    public MainViewModel()
    {
        ShowOpenFileDialog = new Interaction<Unit, string?>();
        _modLoaderSettings = ModManager.LoadSettings();
        _modLoaderViewModel = new ModLoaderViewModel(ref _modLoaderSettings, ShowOpenFileDialog);
        CurrentView = _modLoaderViewModel;
    }
    
    public void OnClosing()
    {
        try
        {
            // Save current device settings 
            var json = JsonSerializer.Serialize(_modLoaderSettings);
            File.WriteAllText(Path.Combine(Environment.CurrentDirectory, ModManager.SETTINGS_FILE), json);
        }
        catch (Exception e)
        {
           Console.WriteLine($"failed to save settings: {e.Message}");
        }
    }
}