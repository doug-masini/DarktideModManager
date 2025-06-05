using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DarktideModManager.Models;
using ReactiveUI;

namespace DarktideModManager.ViewModels;

public class ModLoaderViewModel : ViewModelBase
{
    private const string GAME_DIRECTORY_TEXT = "Select the Darktide Game Directory";
    private readonly ModLoaderSettings _settings = null!;
    private ObservableCollection<string> ZipFiles { get; } = [];
    private ObservableCollection<string> InstalledMods { get; } = [];
    private readonly ModManager _modManager = null!;
    
    private string _gameDirectoryText = GAME_DIRECTORY_TEXT;
    public string GameDirectoryText
    {
        get => _gameDirectoryText;
        set
        {
            this.RaiseAndSetIfChanged(ref _gameDirectoryText, value);
            _settings.ModLoaderGameDirectory = value;
        }
    }
    
    private bool _isValidFolderSelected;
    public bool IsValidFolderSelected { get => _isValidFolderSelected; private set => this.RaiseAndSetIfChanged(ref _isValidFolderSelected, value); }
    
    private string _statusText = string.Empty;
    public string StatusText
    {
        get => _statusText;
        set => this.RaiseAndSetIfChanged(ref _statusText, value);
    }

    private Interaction<Unit, string?> ParentShowOpenFileDialog { get; } = null!;
    public ReactiveCommand<Unit, Unit> OpenFolderCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> InstallModsCommand { get; private set; } = null!;

    /// <summary>
    /// Constructor for designer only
    /// </summary>
    public ModLoaderViewModel()
    {
    }
    public ModLoaderViewModel(ref ModLoaderSettings settings, Interaction<Unit, string?> showOpenFileDialog)
    {
        ParentShowOpenFileDialog = showOpenFileDialog;
        _settings = settings;
        _modManager = new ModManager(ref _settings);
        OpenFolderCommand = ReactiveCommand.CreateFromTask(RunOpenFolder);
        InstallModsCommand = ReactiveCommand.Create(RunInstallMods);
        SetGameFolder(_settings.ModLoaderGameDirectory);
        
        ZipFiles = _modManager.ZipFiles;
        InstalledMods = _modManager.InstalledMods;
    }

    

    private async Task RunOpenFolder()
    {
        var folderName = await ParentShowOpenFileDialog.Handle(Unit.Default);
   
        if (folderName != null)
        {
            SetGameFolder(folderName);
        }
    }
    private void SetGameFolder(string? folderName)
    {
        if (string.IsNullOrEmpty(folderName))
        {
            StatusText = GAME_DIRECTORY_TEXT;
            IsValidFolderSelected = false;
            return;
        }
        
        if (folderName.Contains(ModManager.WARHAMMER_FOLDER_NAME))
        {
            GameDirectoryText = folderName;
            IsValidFolderSelected = true;
        }
        else
        {
            StatusText = GAME_DIRECTORY_TEXT;
            IsValidFolderSelected = false;
        }
    }
    private void RunInstallMods()
    {
        if (string.IsNullOrEmpty(_settings.ModLoaderGameDirectory)) return;
        if (ZipFiles.Count == 0)
        {
            StatusText = "No Mods Found";
        }
        else if( !_settings.ModLoaderGameDirectory.Contains(ModManager.WARHAMMER_FOLDER_NAME))
        {
            StatusText = "Invalid Game Directory";
        }
        else
        {
            try
            {
                _modManager.InstallMods();
                StatusText = "Mods Installed";
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(e.Message);
                StatusText = e.Message;
            }
         
        }
    }
}