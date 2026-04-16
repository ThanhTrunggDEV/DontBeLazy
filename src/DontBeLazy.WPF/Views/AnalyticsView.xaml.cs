using System.Windows.Controls;
using DontBeLazy.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace DontBeLazy.WPF.Views;

public partial class AnalyticsView : UserControl
{
    public AnalyticsView()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<AnalyticsViewModel>();
    }

    private async void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is AnalyticsViewModel vm)
            await vm.LoadDataCommand.ExecuteAsync(null);
    }
}
