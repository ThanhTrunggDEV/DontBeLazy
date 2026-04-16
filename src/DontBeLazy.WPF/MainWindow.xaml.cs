using System.Windows;
using DontBeLazy.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace DontBeLazy.WPF;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<MainViewModel>();
        this.Closing += MainWindow_Closing;
    }

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        var sessionState = App.Services.GetRequiredService<DontBeLazy.UseCases.ActiveSessionState>();
        if (sessionState.CurrentSession != null)
        {
            e.Cancel = true;
            MessageBox.Show("Bạn không thể tắt ứng dụng khi đang trong phiên tập trung!\nHãy dũng cảm ấn từ bỏ phiên trong phần mềm nếu bạn muốn bỏ cuộc.", 
                "Kỷ luật sắt", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
