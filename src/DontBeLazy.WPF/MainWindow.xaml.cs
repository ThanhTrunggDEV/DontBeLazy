using System.Windows;
using DontBeLazy.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;

using System.IO;

namespace DontBeLazy.WPF;

public partial class MainWindow : Window
{
    private System.Windows.Forms.NotifyIcon _notifyIcon = null!;
    private bool _realExit;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<MainViewModel>();
        
        SetupTrayIcon();

        this.Closing += MainWindow_Closing;
        this.Loaded += async (s, e) => await ((MainViewModel)DataContext).InitializeAsync();
    }

    private void SetupTrayIcon()
    {
        _notifyIcon = new System.Windows.Forms.NotifyIcon();
        _notifyIcon.Text = "Don't Be Lazy";
        _notifyIcon.Visible = true;
        
        var icoPath = Path.Combine(System.AppContext.BaseDirectory, "app.ico");
        if (File.Exists(icoPath))
        {
            _notifyIcon.Icon = new System.Drawing.Icon(icoPath);
        }

        _notifyIcon.DoubleClick += (s, e) => ShowWindow();

        var contextMenu = new System.Windows.Forms.ContextMenuStrip();
        contextMenu.Items.Add("Mở ứng dụng", null, (s, e) => ShowWindow());
        contextMenu.Items.Add("Thoát hoàn toàn", null, (s, e) => 
        {
            _realExit = true;
            Application.Current.Shutdown();
        });

        _notifyIcon.ContextMenuStrip = contextMenu;
    }

    private void ShowWindow()
    {
        this.Show();
        if (this.WindowState == WindowState.Minimized)
            this.WindowState = WindowState.Normal;
        this.Activate();
    }

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_realExit) return;

        var sessionState = App.Services.GetRequiredService<DontBeLazy.UseCases.ActiveSessionState>();
        if (sessionState.CurrentSession != null)
        {
            e.Cancel = true;
            MessageBox.Show("Bạn không thể tắt ứng dụng khi đang trong phiên tập trung!\nHãy dũng cảm ấn từ bỏ phiên trong phần mềm nếu bạn muốn bỏ cuộc.", 
                "Kỷ luật sắt", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        e.Cancel = true;
        this.Hide();
    }

    protected override void OnClosed(System.EventArgs e)
    {
        _notifyIcon?.Dispose();
        base.OnClosed(e);
    }
}
