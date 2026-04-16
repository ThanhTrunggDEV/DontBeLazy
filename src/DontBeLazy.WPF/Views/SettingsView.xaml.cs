using System.Windows.Controls;
using DontBeLazy.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace DontBeLazy.WPF.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<SettingsViewModel>();
    }

    private async void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm)
            await vm.LoadDataCommand.ExecuteAsync(null);
    }
}
