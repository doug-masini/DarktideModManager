using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using DarktideModManager.ViewModels;

namespace DarktideModManager.Views;

public partial class ModLoaderView : UserControl
{
    private ModLoaderViewModel? _viewModel;
    public ModLoaderView()
    {
        InitializeComponent();
        this.Loaded += ModLoaderView_Loaded;
    }


    private void ModLoaderView_Loaded(object? sender, RoutedEventArgs e)
    {
        _viewModel = DataContext as ModLoaderViewModel;
    }
}