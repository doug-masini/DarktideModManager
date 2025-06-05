using Avalonia.Controls;
using System.Reactive;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using DarktideModManager.ViewModels;
using ReactiveUI;
using Avalonia.Interactivity;
using System;

namespace DarktideModManager.Views;

public partial class MainWindow : ReactiveWindow<MainViewModel>
{

    public MainWindow()
    {
        InitializeComponent();
        this.Loaded += MainWindow_Loaded;
        this.SizeToContent = SizeToContent.Manual;
        if(Screens.Primary != null)
        {
            Height = Screens.Primary.Bounds.Height * 0.75; //75% of screen height
            Width = Screens.Primary.Bounds.Width * 0.50; //50% of screen width
            WindowStartupLocation = WindowStartupLocation.Manual;
            Position = new PixelPoint((int)(Screens.Primary.Bounds.Width * 0.01), (int)(Screens.Primary.Bounds.Height * 0.01)); //1% from left and top
        }
    }

    private void MainWindow_Loaded(object? sender, RoutedEventArgs e)
    {
        ViewModel?.ShowOpenFileDialog.RegisterHandler(ShowOpenFileDialog);
    }

    private async Task ShowOpenFileDialog(IInteractionContext<Unit, string?> interaction)
    {
        IStorageProvider storageProvider = this.StorageProvider;
        var result = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            AllowMultiple = false,
            Title = "Select Folder",
        });
        interaction.SetOutput(result.FirstOrDefault()?.TryGetLocalPath());
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        ViewModel?.OnClosing();
        base.OnClosing(e);
    }
}