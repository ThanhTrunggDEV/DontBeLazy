using MaterialDesignThemes.Wpf;
using System.Windows.Controls;
using DontBeLazy.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace DontBeLazy.WPF.Views;

public partial class ProfilesView : UserControl
{
    public ProfilesView()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<ProfilesViewModel>();
    }

    private async void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is ProfilesViewModel vm)
            await vm.LoadDataCommand.ExecuteAsync(null);
    }

    private void OnAiProfileDialogClosing(object sender, DialogClosingEventArgs e)
    {
        if (DataContext is ProfilesViewModel vm)
            vm.IsAiProfileDialogOpen = false;
    }

    private void OnAddProfileDialogClosing(object sender, DialogClosingEventArgs e)
    {
        if (DataContext is ProfilesViewModel vm)
            vm.IsAddProfileDialogOpen = false;
    }

    private void OnAddEntryDialogClosing(object sender, DialogClosingEventArgs e)
    {
        if (DataContext is ProfilesViewModel vm)
            vm.IsAddEntryDialogOpen = false;
    }

    private void OnRenameProfileDialogClosing(object sender, DialogClosingEventArgs e)
    {
        if (DataContext is ProfilesViewModel vm)
            vm.IsRenameProfileDialogOpen = false;
    }
}
