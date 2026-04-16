using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DontBeLazy.Ports.DTOs;
using DontBeLazy.Ports.Inbound;

namespace DontBeLazy.WPF.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IFocusTaskUseCase _taskUseCase;
    private readonly IProfileUseCase _profileUseCase;

    [ObservableProperty] private ObservableCollection<FocusTaskDto> _tasks = new();
    [ObservableProperty] private ObservableCollection<ProfileDto> _profiles = new();
    [ObservableProperty] private string _newTaskName = string.Empty;
    [ObservableProperty] private int _newTaskMinutes = 25;
    [ObservableProperty] private ProfileDto? _selectedProfile;
    [ObservableProperty] private bool _isAddDialogOpen;
    [ObservableProperty] private bool _isEditDialogOpen;
    [ObservableProperty] private FocusTaskDto? _editingTask;
    [ObservableProperty] private string _editTaskName = string.Empty;
    [ObservableProperty] private int _editTaskMinutes = 25;
    [ObservableProperty] private ProfileDto? _editSelectedProfile;

    // AI
    [ObservableProperty] private bool _isAiDialogOpen;
    [ObservableProperty] private bool _isAiLoading;
    [ObservableProperty] private string _aiDialogTitle = string.Empty;
    [ObservableProperty] private string _aiResultText = string.Empty;

    public DashboardViewModel(IFocusTaskUseCase taskUseCase, IProfileUseCase profileUseCase)
    {
        _taskUseCase = taskUseCase;
        _profileUseCase = profileUseCase;
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        var tasks = await _taskUseCase.GetAllTasksAsync();
        Tasks = new ObservableCollection<FocusTaskDto>(tasks);

        var profiles = await _profileUseCase.GetAllProfilesAsync();
        Profiles = new ObservableCollection<ProfileDto>(profiles);
    }

    [RelayCommand]
    private void OpenAddDialog()
    {
        NewTaskName = string.Empty;
        NewTaskMinutes = 25;
        SelectedProfile = null;
        IsAddDialogOpen = true;
    }

    [RelayCommand]
    private async Task AddTaskAsync()
    {
        if (string.IsNullOrWhiteSpace(NewTaskName)) return;
        await _taskUseCase.CreateTaskAsync(NewTaskName, NewTaskMinutes, SelectedProfile?.Id);
        IsAddDialogOpen = false;
        await LoadDataAsync();
    }

    [RelayCommand]
    private void OpenEditDialog(FocusTaskDto task)
    {
        EditingTask = task;
        EditTaskName = task.Name;
        EditTaskMinutes = task.ExpectedMinutes;
        EditSelectedProfile = Profiles.FirstOrDefault(p => p.Id == task.ProfileId);
        IsEditDialogOpen = true;
    }

    [RelayCommand]
    private async Task SaveEditAsync()
    {
        if (EditingTask == null || string.IsNullOrWhiteSpace(EditTaskName)) return;
        await _taskUseCase.UpdateTaskAsync(EditingTask.Id, EditTaskName, EditTaskMinutes, EditSelectedProfile?.Id, null);
        IsEditDialogOpen = false;
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task DeleteTaskAsync(FocusTaskDto task)
    {
        await _taskUseCase.DeleteTaskAsync(task.Id);
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task MarkDoneAsync(FocusTaskDto task)
    {
        await _taskUseCase.ChangeTaskStatusAsync(task.Id, TaskStatusDto.Done);
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task MarkAbandonedAsync(FocusTaskDto task)
    {
        await _taskUseCase.ChangeTaskStatusAsync(task.Id, TaskStatusDto.Abandoned);
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task AiSuggestPriorityAsync()
    {
        AiDialogTitle = "AI Suggest Priority";
        AiResultText = string.Empty;
        IsAiLoading = true;
        IsAiDialogOpen = true;
        try { AiResultText = await _taskUseCase.AiSuggestPriorityAsync(); }
        catch (Exception ex) { AiResultText = $"Lỗi: {ex.Message}"; }
        finally { IsAiLoading = false; }
    }

    [RelayCommand]
    private async Task AiBreakdownTaskAsync(FocusTaskDto task)
    {
        AiDialogTitle = $"AI Breakdown: {task.Name}";
        AiResultText = string.Empty;
        IsAiLoading = true;
        IsAiDialogOpen = true;
        try { AiResultText = await _taskUseCase.AiBreakdownTaskAsync(task.Id); }
        catch (Exception ex) { AiResultText = $"Lỗi: {ex.Message}"; }
        finally { IsAiLoading = false; }
    }

    [RelayCommand]
    private void CloseAiDialog() => IsAiDialogOpen = false;
}
