using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DontBeLazy.Domain.Entities;
using DontBeLazy.Domain.Enums;
using DontBeLazy.Domain.ValueObjects;
using DontBeLazy.Ports.Inbound;

namespace DontBeLazy.WPF.ViewModels;

public partial class FocusSessionViewModel : ObservableObject
{
    private readonly IFocusSessionUseCase _sessionUseCase;
    private readonly IFocusTaskUseCase _taskUseCase;
    private readonly IProfileUseCase _profileUseCase;
    private readonly IQuoteUseCase _quoteUseCase;

    private DispatcherTimer? _timer;
    private SessionHistory? _currentSession;

    [ObservableProperty]
    private System.Collections.ObjectModel.ObservableCollection<FocusTask> _availableTasks = new();

    [ObservableProperty]
    private System.Collections.ObjectModel.ObservableCollection<Profile> _availableProfiles = new();

    [ObservableProperty]
    private FocusTask? _selectedTask;

    [ObservableProperty]
    private Profile? _selectedProfile;

    [ObservableProperty]
    private int _durationMinutes = 25;

    [ObservableProperty]
    private string _intentionText = string.Empty;

    [ObservableProperty]
    private bool _isSessionActive;

    [ObservableProperty]
    private bool _isPaused;

    [ObservableProperty]
    private string _timerDisplay = "25:00";

    [ObservableProperty]
    private double _timerProgress;

    [ObservableProperty]
    private int _remainingSeconds;

    [ObservableProperty]
    private bool _isGuiltTripVisible;

    [ObservableProperty]
    private string _guiltTripQuote = string.Empty;

    [ObservableProperty]
    private bool _isIntentionDialogOpen;

    private int _totalSeconds;

    public FocusSessionViewModel(
        IFocusSessionUseCase sessionUseCase,
        IFocusTaskUseCase taskUseCase,
        IProfileUseCase profileUseCase,
        IQuoteUseCase quoteUseCase)
    {
        _sessionUseCase = sessionUseCase;
        _taskUseCase = taskUseCase;
        _profileUseCase = profileUseCase;
        _quoteUseCase = quoteUseCase;
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        var tasks = await _taskUseCase.GetAllTasksAsync();
        AvailableTasks = new System.Collections.ObjectModel.ObservableCollection<FocusTask>(
            tasks.Where(t => t.Status == Domain.Enums.TaskStatus.Pending || t.Status == Domain.Enums.TaskStatus.Active));

        var profiles = await _profileUseCase.GetAllProfilesAsync();
        AvailableProfiles = new System.Collections.ObjectModel.ObservableCollection<Profile>(profiles);
    }

    [RelayCommand]
    private void OpenIntentionDialog()
    {
        var taskName = SelectedTask?.Name ?? "công việc của tôi";
        IntentionText = $"Trong {DurationMinutes} phút tới, tôi sẽ hoàn thành {taskName}";
        IsIntentionDialogOpen = true;
    }

    [RelayCommand]
    private async Task StartSessionAsync()
    {
        IsIntentionDialogOpen = false;

        _totalSeconds = DurationMinutes * 60;
        RemainingSeconds = _totalSeconds;
        UpdateTimerDisplay();

        _currentSession = await _sessionUseCase.StartSessionAsync(
            SelectedTask?.Id,
            SelectedTask?.Name ?? "Phiên tập trung",
            SelectedProfile?.Id,
            _totalSeconds);

        IsSessionActive = true;
        IsPaused = false;

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += OnTimerTick;
        _timer.Start();
    }

    private async void OnTimerTick(object? sender, EventArgs e)
    {
        if (_currentSession == null) return;

        RemainingSeconds--;
        TimerProgress = ((double)(_totalSeconds - RemainingSeconds) / _totalSeconds) * 100;
        UpdateTimerDisplay();

        await _sessionUseCase.SyncSessionTimeAsync(_currentSession.Id, 1);

        if (RemainingSeconds <= 0)
            await CompleteSessionAsync();
    }

    [RelayCommand]
    private async Task PauseResumeAsync()
    {
        if (_currentSession == null) return;

        if (IsPaused)
        {
            await _sessionUseCase.ResumeSessionAsync(_currentSession.Id);
            _timer?.Start();
            IsPaused = false;
        }
        else
        {
            await _sessionUseCase.PauseSessionAsync(_currentSession.Id);
            _timer?.Stop();
            IsPaused = true;
        }
    }

    [RelayCommand]
    private async Task AttemptStopAsync()
    {
        // Psychological trick: Show guilt-trip quote before allowing stop
        var quote = await _quoteUseCase.GetQuoteForEventAsync(QuoteEventType.GiveUp, "vi");
        GuiltTripQuote = quote?.Content
            ?? $"Bạn chỉ còn {RemainingSeconds / 60} phút nữa là xong! Bạn thực sự muốn bỏ cuộc sao?";
        IsGuiltTripVisible = true;
    }

    [RelayCommand]
    private void ContinueSession()
    {
        IsGuiltTripVisible = false;
    }

    [RelayCommand]
    private async Task ConfirmAbandonAsync()
    {
        IsGuiltTripVisible = false;
        await AbandonSessionAsync();
    }

    private async Task AbandonSessionAsync()
    {
        _timer?.Stop();
        if (_currentSession != null)
            await _sessionUseCase.CompleteSessionAsync(_currentSession.Id, CompletionStatus.Abandoned);

        ResetSession();
    }

    private async Task CompleteSessionAsync()
    {
        _timer?.Stop();
        if (_currentSession != null)
            await _sessionUseCase.CompleteSessionAsync(_currentSession.Id, CompletionStatus.Completed);

        ResetSession();
    }

    private void ResetSession()
    {
        IsSessionActive = false;
        IsPaused = false;
        _currentSession = null;
        RemainingSeconds = DurationMinutes * 60;
        TimerProgress = 0;
        UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
    {
        var minutes = RemainingSeconds / 60;
        var seconds = RemainingSeconds % 60;
        TimerDisplay = $"{minutes:D2}:{seconds:D2}";
    }
}
