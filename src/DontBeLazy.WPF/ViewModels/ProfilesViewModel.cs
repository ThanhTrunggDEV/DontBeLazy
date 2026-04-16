using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DontBeLazy.Ports.DTOs;
using DontBeLazy.Ports.Inbound;

namespace DontBeLazy.WPF.ViewModels;

public partial class ProfilesViewModel : ObservableObject
{
    private readonly IProfileUseCase _profileUseCase;
    private readonly IProfileEntryUseCase _profileEntryUseCase;

    [ObservableProperty] private ObservableCollection<ProfileDto> _profiles = new();
    [ObservableProperty] private ProfileDto? _selectedProfile;
    [ObservableProperty] private bool _isAddProfileDialogOpen;
    [ObservableProperty] private string _newProfileName = string.Empty;
    [ObservableProperty] private bool _newProfileIsDefault;
    [ObservableProperty] private string _newEntryValue = string.Empty;
    [ObservableProperty] private ProfileEntryTypeDto _newEntryType = ProfileEntryTypeDto.Website;
    [ObservableProperty] private bool _isAddEntryDialogOpen;

    // Edit Entry
    [ObservableProperty] private bool _isEditEntryDialogOpen;
    [ObservableProperty] private ProfileEntryDto? _editingEntry;
    [ObservableProperty] private string _editEntryValue = string.Empty;
    [ObservableProperty] private ProfileEntryTypeDto _editEntryType = ProfileEntryTypeDto.Website;

    // AI Smart Profile
    [ObservableProperty] private bool _isAiProfileDialogOpen;
    [ObservableProperty] private bool _isAiProfileLoading;
    [ObservableProperty] private string _aiProfileIntent = string.Empty;
    [ObservableProperty] private string _aiProfileResult = string.Empty;

    // Rename Profile
    [ObservableProperty] private bool _isRenameProfileDialogOpen;
    [ObservableProperty] private string _renameProfileName = string.Empty;
    private ProfileDto? _renamingProfile;

    public IReadOnlyList<ProfileEntryTypeDto> EntryTypes { get; } =
        Enum.GetValues<ProfileEntryTypeDto>().ToList();

    public ProfilesViewModel(IProfileUseCase profileUseCase, IProfileEntryUseCase profileEntryUseCase)
    {
        _profileUseCase = profileUseCase;
        _profileEntryUseCase = profileEntryUseCase;
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        var profiles = await _profileUseCase.GetAllProfilesAsync();
        Profiles = new ObservableCollection<ProfileDto>(profiles);
        if (SelectedProfile != null)
            SelectedProfile = Profiles.FirstOrDefault(p => p.Id == SelectedProfile.Id);
    }

    [RelayCommand]
    private void OpenAddProfileDialog()
    {
        NewProfileName = string.Empty;
        NewProfileIsDefault = false;
        IsAddProfileDialogOpen = true;
    }

    [RelayCommand]
    private async Task CreateProfileAsync()
    {
        if (string.IsNullOrWhiteSpace(NewProfileName)) return;
        var newName = NewProfileName;
        await _profileUseCase.CreateProfileAsync(NewProfileName, NewProfileIsDefault);
        IsAddProfileDialogOpen = false;
        await LoadDataAsync();
        SelectedProfile = Profiles.FirstOrDefault(p => p.Name == newName);
    }

    [RelayCommand]
    private async Task DeleteProfileAsync(ProfileDto profile)
    {
        await _profileUseCase.DeleteProfileAsync(profile.Id);
        if (SelectedProfile?.Id == profile.Id) SelectedProfile = null;
        await LoadDataAsync();
    }

    [RelayCommand]
    private void OpenAddEntryDialog()
    {
        if (SelectedProfile == null) return;
        NewEntryValue = string.Empty;
        NewEntryType = ProfileEntryTypeDto.Website;
        IsAddEntryDialogOpen = true;
    }

    [RelayCommand]
    private async Task AddEntryAsync()
    {
        if (SelectedProfile == null || string.IsNullOrWhiteSpace(NewEntryValue)) return;
        
        var val = NewEntryValue.Trim();
        if (NewEntryType == ProfileEntryTypeDto.Website) {
            val = val.Replace("http://", "").Replace("https://", "").Replace("www.", "").Split('/')[0].Trim();
        }

        try {
            await _profileEntryUseCase.AddProfileEntryAsync(SelectedProfile.Id, NewEntryType, val);
            IsAddEntryDialogOpen = false;
            await LoadDataAsync();
        } catch (Exception ex) {
            System.Windows.MessageBox.Show(ex.Message, "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void OpenEditEntryDialog(ProfileEntryDto entry)
    {
        if (SelectedProfile == null) return;
        EditingEntry = entry;
        EditEntryType = entry.Type;
        EditEntryValue = entry.Value;
        IsEditEntryDialogOpen = true;
    }

    [RelayCommand]
    private async Task SaveEditEntryAsync()
    {
        if (SelectedProfile == null || EditingEntry == null || string.IsNullOrWhiteSpace(EditEntryValue)) return;
        
        var val = EditEntryValue.Trim();
        if (EditEntryType == ProfileEntryTypeDto.Website) {
            val = val.Replace("http://", "").Replace("https://", "").Replace("www.", "").Split('/')[0].Trim();
        }

        try {
            await _profileEntryUseCase.UpdateProfileEntryAsync(SelectedProfile.Id, EditingEntry.Id, EditEntryType, val);
            IsEditEntryDialogOpen = false;
            await LoadDataAsync();
        } catch (Exception ex) {
            System.Windows.MessageBox.Show(ex.Message, "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task RemoveEntryAsync(ProfileEntryDto entry)
    {
        if (SelectedProfile == null) return;
        await _profileEntryUseCase.RemoveProfileEntryAsync(SelectedProfile.Id, entry.Id);
        await LoadDataAsync();
    }

    [RelayCommand]
    private void OpenAiProfileDialog()
    {
        AiProfileIntent = string.Empty;
        AiProfileResult = string.Empty;
        IsAiProfileDialogOpen = true;
    }

    [RelayCommand]
    private async Task AiGenerateProfileAsync()
    {
        if (string.IsNullOrWhiteSpace(AiProfileIntent)) return;
        if (SelectedProfile == null)
        {
            AiProfileResult = "Vui lòng chọn một Profile trong danh sách bên trái trước khi tạo gợi ý AI.";
            return;
        }

        IsAiProfileLoading = true;
        AiProfileResult = string.Empty;
        try 
        { 
            var jsonResult = await _profileUseCase.AiGenerateProfileSuggestionAsync(AiProfileIntent); 
            // Clean markdown if present
            var cleanStr = jsonResult.Replace("```json", "").Replace("```", "").Trim();
            
            var parsed = System.Text.Json.JsonDocument.Parse(cleanStr);
            var websites = parsed.RootElement.GetProperty("websites").EnumerateArray().Select(x => x.GetString());
            var apps = parsed.RootElement.GetProperty("apps").EnumerateArray().Select(x => x.GetString());
            
            int addedCount = 0;
            foreach(var w in websites)
            {
                if (!string.IsNullOrWhiteSpace(w)) {
                    await _profileEntryUseCase.AddProfileEntryAsync(SelectedProfile.Id, ProfileEntryTypeDto.Website, w);
                    addedCount++;
                }
            }
            foreach(var a in apps)
            {
                if (!string.IsNullOrWhiteSpace(a)) {
                    await _profileEntryUseCase.AddProfileEntryAsync(SelectedProfile.Id, ProfileEntryTypeDto.App, a);
                    addedCount++;
                }
            }

            AiProfileResult = $"Đã thêm thành công {addedCount} ngoại lệ vào Profile {SelectedProfile.Name}.";
            await LoadDataAsync();
        }
        catch (Exception ex) 
        { 
            AiProfileResult = $"Lỗi: Không thể phân tích dữ liệu AI ({ex.Message}).\n\nOriginal Text:\n{AiProfileResult}"; 
        }
        finally 
        { 
            IsAiProfileLoading = false; 
        }
    }

    [RelayCommand]
    private void CloseAiProfileDialog() => IsAiProfileDialogOpen = false;

    [RelayCommand]
    private void OpenRenameProfileDialog(ProfileDto profile)
    {
        _renamingProfile = profile;
        RenameProfileName = profile.Name;
        IsRenameProfileDialogOpen = true;
    }

    [RelayCommand]
    private async Task SaveRenameProfileAsync()
    {
        if (_renamingProfile == null || string.IsNullOrWhiteSpace(RenameProfileName)) return;
        await _profileUseCase.UpdateProfileNameAsync(_renamingProfile.Id, RenameProfileName.Trim());
        IsRenameProfileDialogOpen = false;
        await LoadDataAsync();
    }

    [RelayCommand]
    private void CloseRenameProfileDialog() => IsRenameProfileDialogOpen = false;
}
