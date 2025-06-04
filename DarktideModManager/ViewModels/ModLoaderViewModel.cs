using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using DarktideModManager.Models;
using ReactiveUI;

namespace DarktideModManager.ViewModels;

public class ModLoaderViewModel : ViewModelBase
{
    private ModLoaderSettings _settings;
    private ObservableCollection<string> ZipFiles { get; set; } = [];
    private ModManager _modManager;
    
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
    public Interaction<Unit, string?> ParentShowOpenFileDialog { get; private set; }
    public ReactiveCommand<Unit, Unit> OpenFolderCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> InstallModsCommand { get; private set; }

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
        GameDirectoryText = _settings.ModLoaderGameDirectory ?? "Select the Darktide Game Directory";
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
        if (folderName != null)
        {
            GameDirectoryText = folderName;
        }
    }
    private void RunInstallMods()
    {
        if(ZipFiles.Count > 0)
            _modManager.InstallMods();
    }
}