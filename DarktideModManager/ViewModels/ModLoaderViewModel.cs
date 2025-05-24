using DarktideModManager.Models;

namespace DarktideModManager.ViewModels;

public class ModLoaderViewModel : ViewModelBase
{
    private ModLoaderSettings _settings;
    public ModLoaderViewModel(ref ModLoaderSettings settings)
    {
        _settings = settings;
    }
}