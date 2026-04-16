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
    }
}
