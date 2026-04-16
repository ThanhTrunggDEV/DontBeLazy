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

    // ── Add Task ────────────────────────────────────────────────
    [ObservableProperty] private bool _isAddDialogOpen;
    [ObservableProperty] private string _newTaskName = string.Empty;
    [ObservableProperty] private int _newTaskMinutes = 25;
    [ObservableProperty] private ProfileDto? _selectedProfile;
    [ObservableProperty] private bool _newTaskIsRecurring;
    [ObservableProperty] private string _newRecurringType = "Daily";   // Daily/Weekly/Custom
    [ObservableProperty] private string _newWeekDays = string.Empty;   // e.g. "Mon,Wed,Fri"
    [ObservableProperty] private int _newCustomDays = 2;

    // ── Edit Task ────────────────────────────────────────────────
    [ObservableProperty] private bool _isEditDialogOpen;
    [ObservableProperty] private FocusTaskDto? _editingTask;
    [ObservableProperty] private string _editTaskName = string.Empty;
    [ObservableProperty] private int _editTaskMinutes = 25;
    [ObservableProperty] private ProfileDto? _editSelectedProfile;
    [ObservableProperty] private bool _editTaskIsRecurring;
    [ObservableProperty] private string _editRecurringType = "Daily";
    [ObservableProperty] private string _editWeekDays = string.Empty;
    [ObservableProperty] private int _editCustomDays = 2;

    // ── AI ───────────────────────────────────────────────────────
    [ObservableProperty] private bool _isAiDialogOpen;
    [ObservableProperty] private bool _isAiLoading;
    [ObservableProperty] private string _aiDialogTitle = string.Empty;
    [ObservableProperty] private string _aiResultText = string.Empty;

    public IReadOnlyList<string> RecurringTypes { get; } = ["Daily", "Weekly", "Custom"];

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

    // ── Add ──────────────────────────────────────────────────────
    [RelayCommand]
    private void OpenAddDialog()
    {
        NewTaskName = string.Empty;
        NewTaskMinutes = 25;
        SelectedProfile = null;
        NewTaskIsRecurring = false;
        NewRecurringType = "Daily";
        NewWeekDays = string.Empty;
        NewCustomDays = 2;
        IsAddDialogOpen = true;
    }

    [RelayCommand]
    private async Task AddTaskAsync()
    {
        if (string.IsNullOrWhiteSpace(NewTaskName)) return;
        var task = await _taskUseCase.CreateTaskAsync(NewTaskName, NewTaskMinutes, SelectedProfile?.Id);
        if (NewTaskIsRecurring)
        {
            var config = NewRecurringType switch
            {
                "Weekly" => NewWeekDays,
                "Custom" => NewCustomDays.ToString(),
                _ => string.Empty
            };
            await _taskUseCase.SetTaskRecurringAsync(task.Id, NewRecurringType, config);
        }
        IsAddDialogOpen = false;
        await LoadDataAsync();
    }

    // ── Edit ─────────────────────────────────────────────────────
    [RelayCommand]
    private void OpenEditDialog(FocusTaskDto task)
    {
        if (task.Status == TaskStatusDto.Active) return; // disabled per UC01
        EditingTask = task;
        EditTaskName = task.Name;
        EditTaskMinutes = task.ExpectedMinutes;
        EditSelectedProfile = Profiles.FirstOrDefault(p => p.Id == task.ProfileId);
        // Reset recurring fields (not tracked in DTO currently — default to non-recurring)
        EditTaskIsRecurring = false;
        EditRecurringType = "Daily";
        EditWeekDays = string.Empty;
        EditCustomDays = 2;
        IsEditDialogOpen = true;
    }

    [RelayCommand]
    private async Task SaveEditAsync()
    {
        if (EditingTask == null || string.IsNullOrWhiteSpace(EditTaskName)) return;
        await _taskUseCase.UpdateTaskAsync(EditingTask.Id, EditTaskName, EditTaskMinutes, EditSelectedProfile?.Id, null);
        if (EditTaskIsRecurring)
        {
            var config = EditRecurringType switch
            {
                "Weekly" => EditWeekDays,
                "Custom" => EditCustomDays.ToString(),
                _ => string.Empty
            };
            await _taskUseCase.SetTaskRecurringAsync(EditingTask.Id, EditRecurringType, config);
        }
        IsEditDialogOpen = false;
        await LoadDataAsync();
    }

    // ── Delete ───────────────────────────────────────────────────
    [RelayCommand]
    private async Task DeleteTaskAsync(FocusTaskDto task)
    {
        if (task.Status == TaskStatusDto.Active) return;
        await _taskUseCase.DeleteTaskAsync(task.Id);
        await LoadDataAsync();
    }

    // ── Status changes ───────────────────────────────────────────
    [RelayCommand]
    private async Task ToggleDoneAsync(FocusTaskDto task)
    {
        var newStatus = task.Status == TaskStatusDto.Done
            ? TaskStatusDto.Pending
            : TaskStatusDto.Done;
        await _taskUseCase.ChangeTaskStatusAsync(task.Id, newStatus);
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task RetryAbandonedAsync(FocusTaskDto task)
    {
        await _taskUseCase.ChangeTaskStatusAsync(task.Id, TaskStatusDto.Pending);
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task AiSuggestPriorityAsync()
    {
        AiDialogTitle = "AI Sắp xếp ưu tiên";
        AiResultText = "Đang phân tích các công việc...";
        IsAiLoading = true;
        IsAiDialogOpen = true;
        try 
        { 
            var result = await _taskUseCase.AiSuggestPriorityAsync(); 
            var cleanStr = result.Replace("```json", "").Replace("```", "").Trim();
            try
            {
                var parsed = System.Text.Json.JsonDocument.Parse(cleanStr);
                if (parsed.RootElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    int index = 1;
                    int appliedCount = 0;
                    foreach (var element in parsed.RootElement.EnumerateArray())
                    {
                        if (element.TryGetProperty("Id", out var idProp) && idProp.TryGetGuid(out var id))
                        {
                            await _taskUseCase.UpdateTaskSortOrderAsync(id, index);
                            appliedCount++;
                        }
                        // Fallback matching by name just in case Gemini dropped Id
                        else if (element.TryGetProperty("Name", out var nameProp))
                        {
                            string name = nameProp.GetString() ?? "";
                            var match = Tasks.FirstOrDefault(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));
                            if (match != null)
                            {
                                await _taskUseCase.UpdateTaskSortOrderAsync(match.Id, index);
                                appliedCount++;
                            }
                        }
                        index++;
                    }
                    
                    AiResultText = $"Đã tự động thay đổi Sort Order của {appliedCount} công việc để tối ưu hiệu suất!";
                    await LoadDataAsync();
                    
                    // Task list is loaded. We may need to sort it locally if the View depends on the bound collection order,
                    // but depending on how the view binds, maybe ReloadDataAsync is enough.
                    var sortedList = Tasks.OrderBy(t => t.SortOrder).ToList();
                    Tasks = new ObservableCollection<FocusTaskDto>(sortedList);
                }
                else
                {
                    AiResultText = System.Text.RegularExpressions.Regex.Unescape(cleanStr);
                }
            }
            catch
            {
                AiResultText = result;
            }
        }
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
        try 
        { 
            var jsonResult = await _taskUseCase.AiBreakdownTaskAsync(task.Id); 
            // Clean Markdown
            var cleanStr = jsonResult.Replace("```json", "").Replace("```", "").Trim();
            
            var parsed = System.Text.Json.JsonDocument.Parse(cleanStr);
            var subtasks = parsed.RootElement.EnumerateArray().Select(x => x.GetString());
            
            int addedCount = 0;
            foreach (var sub in subtasks)
            {
                if (!string.IsNullOrWhiteSpace(sub))
                {
                    // Allocate 25 minutes for each subtask and inherit profile if any
                    await _taskUseCase.CreateTaskAsync(sub, 25, task.ProfileId, null);
                    addedCount++;
                }
            }

            AiResultText = $"Đã chia nhỏ thành công {addedCount} sub-tasks cho nhiệm vụ này.";
            await LoadDataAsync();
        }
        catch (Exception ex) 
        { 
            AiResultText = $"Lỗi: Không thể phân tích dữ liệu AI ({ex.Message}).\n\nOriginal Text:\n{AiResultText}"; 
        }
        finally 
        { 
            IsAiLoading = false; 
        }
    }

    [RelayCommand]
    private void CloseAiDialog() => IsAiDialogOpen = false;
}
