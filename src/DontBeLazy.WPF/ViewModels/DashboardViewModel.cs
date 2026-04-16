using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DontBeLazy.Domain.Entities;
using DontBeLazy.Domain.ValueObjects;
using DontBeLazy.Ports.Inbound;

namespace DontBeLazy.WPF.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IFocusTaskUseCase _taskUseCase;
    private readonly IProfileUseCase _profileUseCase;

    [ObservableProperty]
    private ObservableCollection<FocusTask> _tasks = new();

    [ObservableProperty]
    private ObservableCollection<Profile> _profiles = new();

    [ObservableProperty]
    private string _newTaskName = string.Empty;

    [ObservableProperty]
    private int _newTaskMinutes = 25;

    [ObservableProperty]
    private Profile? _selectedProfile;

    [ObservableProperty]
    private bool _isAddDialogOpen;

    [ObservableProperty]
    private bool _isEditDialogOpen;

    [ObservableProperty]
    private FocusTask? _editingTask;

    [ObservableProperty]
    private string _editTaskName = string.Empty;

    [ObservableProperty]
    private int _editTaskMinutes = 25;

    [ObservableProperty]
    private Profile? _editSelectedProfile;

    public DashboardViewModel(IFocusTaskUseCase taskUseCase, IProfileUseCase profileUseCase)
    {
        _taskUseCase = taskUseCase;
        _profileUseCase = profileUseCase;
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        var tasks = await _taskUseCase.GetAllTasksAsync();
        Tasks = new ObservableCollection<FocusTask>(tasks);

        var profiles = await _profileUseCase.GetAllProfilesAsync();
        Profiles = new ObservableCollection<Profile>(profiles);
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
    private void OpenEditDialog(FocusTask task)
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
    private async Task DeleteTaskAsync(FocusTask task)
    {
        await _taskUseCase.DeleteTaskAsync(task.Id);
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task MarkDoneAsync(FocusTask task)
    {
        await _taskUseCase.ChangeTaskStatusAsync(task.Id, Domain.Enums.TaskStatus.Done);
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task MarkAbandonedAsync(FocusTask task)
    {
        await _taskUseCase.ChangeTaskStatusAsync(task.Id, Domain.Enums.TaskStatus.Abandoned);
        await LoadDataAsync();
    }
}
