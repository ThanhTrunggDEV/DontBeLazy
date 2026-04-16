using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DontBeLazy.Ports.DTOs;
using DontBeLazy.Ports.Inbound;

namespace DontBeLazy.WPF.ViewModels;

public partial class FocusSessionViewModel : ObservableObject
{
    private readonly IFocusSessionUseCase _sessionUseCase;
    private readonly IFocusTaskUseCase _taskUseCase;
    private readonly IProfileUseCase _profileUseCase;
    private readonly IQuoteUseCase _quoteUseCase;

    private DispatcherTimer? _timer;
    private SessionHistoryDto? _currentSession;

    [ObservableProperty] private ObservableCollection<FocusTaskDto> _availableTasks = new();
    [ObservableProperty] private ObservableCollection<ProfileDto> _availableProfiles = new();
    [ObservableProperty] private FocusTaskDto? _selectedTask;
    [ObservableProperty] private ProfileDto? _selectedProfile;
    [ObservableProperty] private int _durationMinutes = 25;
    [ObservableProperty] private string _intentionText = string.Empty;
    [ObservableProperty] private bool _isSessionActive;
    [ObservableProperty] private bool _isPaused;
    [ObservableProperty] private string _timerDisplay = "25:00";
    [ObservableProperty] private double _timerProgress;
    [ObservableProperty] private int _remainingSeconds;
    [ObservableProperty] private bool _isGuiltTripVisible;
    [ObservableProperty] private bool _isGuiltTripLoading;
    [ObservableProperty] private string _guiltTripQuote = string.Empty;
    [ObservableProperty] private bool _isIntentionDialogOpen;
    [ObservableProperty] private string _frictionInput = string.Empty;

    private const string FrictionPhrase = "Tôi là kẻ lười biếng và tôi chấp nhận bỏ cuộc";
    public bool CanConfirmAbandon => FrictionInput.Trim() == FrictionPhrase;

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
        AvailableTasks = new ObservableCollection<FocusTaskDto>(
            tasks.Where(t => t.Status == TaskStatusDto.Pending || t.Status == TaskStatusDto.Active));

        var profiles = await _profileUseCase.GetAllProfilesAsync();
        AvailableProfiles = new ObservableCollection<ProfileDto>(profiles);
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
        if (RemainingSeconds <= 0) await CompleteSessionAsync();
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
        _timer?.Stop();
        IsGuiltTripLoading = true;
        IsGuiltTripVisible = true;
        GuiltTripQuote = string.Empty;

        try
        {
            var taskName = SelectedTask?.Name ?? "công việc của bạn";
            GuiltTripQuote = await _quoteUseCase.GenerateAiGuiltTripAsync(taskName, "vi");
        }
        finally
        {
            IsGuiltTripLoading = false;
        }

        if (!IsGuiltTripVisible) _timer?.Start(); // user cancelled while loading
    }

    partial void OnFrictionInputChanged(string value)
        => OnPropertyChanged(nameof(CanConfirmAbandon));

    [RelayCommand]
    private void ContinueSession()
    {
        IsGuiltTripVisible = false;
        FrictionInput = string.Empty;
        _timer?.Start();
    }

    [RelayCommand]
    private async Task ConfirmAbandonAsync()
    {
        if (!CanConfirmAbandon) return;
        IsGuiltTripVisible = false;
        FrictionInput = string.Empty;
        await AbandonSessionAsync();
    }

    private async Task AbandonSessionAsync()
    {
        _timer?.Stop();
        if (_currentSession != null)
            await _sessionUseCase.CompleteSessionAsync(_currentSession.Id, CompletionStatusDto.Abandoned);
        ResetSession();
    }

    private async Task CompleteSessionAsync()
    {
        _timer?.Stop();
        if (_currentSession != null)
            await _sessionUseCase.CompleteSessionAsync(_currentSession.Id, CompletionStatusDto.Completed);
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
        var m = RemainingSeconds / 60;
        var s = RemainingSeconds % 60;
        TimerDisplay = $"{m:D2}:{s:D2}";
    }
}
