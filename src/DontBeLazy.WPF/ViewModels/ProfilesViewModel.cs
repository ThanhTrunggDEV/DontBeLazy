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
        await _profileUseCase.CreateProfileAsync(NewProfileName, NewProfileIsDefault);
        IsAddProfileDialogOpen = false;
        await LoadDataAsync();
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
        await _profileEntryUseCase.AddProfileEntryAsync(SelectedProfile.Id, NewEntryType, NewEntryValue);
        IsAddEntryDialogOpen = false;
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task RemoveEntryAsync(ProfileEntryDto entry)
    {
        if (SelectedProfile == null) return;
        await _profileEntryUseCase.RemoveProfileEntryAsync(SelectedProfile.Id, entry.Id);
        await LoadDataAsync();
    }
}
