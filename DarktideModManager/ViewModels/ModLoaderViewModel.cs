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
    private ObservableCollection<string> ZipFiles { get; set; } = [];
    private readonly ModManager _modManager = null!;
    
    private string _gameDirectoryText = "Test";
    public string GameDirectoryText
    {
        get => _gameDirectoryText;
        set
        {
            this.RaiseAndSetIfChanged(ref _gameDirectoryText, value);
            if(_settings != null)
                _settings.ModLoaderGameDirectory = value;
        }
    }
    
    private string _statusText = string.Empty;
    public string StatusText
    {
        get => _statusText;
        set => this.RaiseAndSetIfChanged(ref _statusText, value);
    }
    public Interaction<Unit, string?> ParentShowOpenFileDialog { get; private set; } = null!;
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
        if (!string.IsNullOrEmpty(_settings.ModLoaderGameDirectory))
            GameDirectoryText = _settings.ModLoaderGameDirectory;
        else
            StatusText = GAME_DIRECTORY_TEXT;
            
        ZipFiles = _modManager.ZipFiles;
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
        if (folderName == null) return;
        if (folderName.Contains(ModManager.WARHAMMER_FOLDER_NAME))
            GameDirectoryText = folderName;
        else
            StatusText = GAME_DIRECTORY_TEXT;
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
            _modManager.InstallMods();
            StatusText = "Mods Installed";
        }
    }
}