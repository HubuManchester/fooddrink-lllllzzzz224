using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NutriVision.Models;
using NutriVision.Services.Abstractions;

namespace NutriVision.ViewModels;

public partial class ScanViewModel : BaseViewModel
{
    private readonly ICameraService _cameraService;
    private readonly IScanWorkflowService _scanWorkflowService;
    private readonly IHistoryRepository _historyRepository;
    private readonly INutritionService _nutritionService;
    private readonly ILocationService _locationService;
    private readonly ISpeechService _speechService;
    private readonly IHapticService _hapticService;
    private readonly IShakeService _shakeService;
    private readonly IMicrophoneService _microphoneService;

    [ObservableProperty]
    private string statusText = "Ready to scan food";

    [ObservableProperty]
    private string? recognizedFood;

    [ObservableProperty]
    private string? location;

    [ObservableProperty]
    private string nutritionText = "-";

    [ObservableProperty]
    private bool isFlashSupported;

    [ObservableProperty]
    private bool isFlashOn;

    [ObservableProperty]
    private string flashButtonText = "Flash unavailable";

    [ObservableProperty]
    private string cameraHint = "Camera preview is loading.";

    public ScanViewModel(
        ICameraService cameraService,
        IScanWorkflowService scanWorkflowService,
        IHistoryRepository historyRepository,
        INutritionService nutritionService,
        ILocationService locationService,
        ISpeechService speechService,
        IHapticService hapticService,
        IShakeService shakeService,
        IMicrophoneService microphoneService)
    {
        _cameraService = cameraService;
        _scanWorkflowService = scanWorkflowService;
        _historyRepository = historyRepository;
        _nutritionService = nutritionService;
        _locationService = locationService;
        _speechService = speechService;
        _hapticService = hapticService;
        _shakeService = shakeService;
        _microphoneService = microphoneService;

        _shakeService.Shaken += OnShaken;
        _shakeService.Start();
    }

    [RelayCommand]
    private async Task InitializeCameraAsync()
    {
        ErrorMessage = null;

        try
        {
            var available = await _cameraService.IsAvailableAsync(CancellationToken.None);
            if (!available)
            {
                IsFlashSupported = false;
                FlashButtonText = "Flash unavailable";
                CameraHint = "Camera preview is unavailable on this device.";
                return;
            }

            await _cameraService.StartPreviewAsync(CancellationToken.None);
            IsFlashSupported = await _cameraService.CanToggleFlashAsync(CancellationToken.None);
            FlashButtonText = IsFlashSupported
                ? (IsFlashOn ? "Flash On" : "Flash Off")
                : "Flash unavailable";
            CameraHint = IsFlashSupported
                ? "Live camera preview is active. Toggle flash before capturing."
                : "Live camera preview is active. This camera does not support flash.";
        }
        catch
        {
            IsFlashSupported = false;
            FlashButtonText = "Flash unavailable";
            CameraHint = "Camera preview failed to start.";
        }
    }

    [RelayCommand]
    private async Task StopCameraAsync()
    {
        try
        {
            await _cameraService.StopPreviewAsync(CancellationToken.None);
        }
        catch
        {
        }
    }

    [RelayCommand]
    private async Task ToggleFlashAsync()
    {
        if (!IsFlashSupported)
        {
            ErrorMessage = "Flash is unavailable on this camera.";
            return;
        }

        try
        {
            var nextValue = !IsFlashOn;
            await _cameraService.SetFlashAsync(nextValue, CancellationToken.None);
            IsFlashOn = nextValue;
            FlashButtonText = nextValue ? "Flash On" : "Flash Off";
            CameraHint = nextValue
                ? "Flash torch is enabled for low-light capture."
                : "Flash torch is disabled.";
        }
        catch
        {
            ErrorMessage = "Flash toggle failed. Please retry.";
        }
    }

    [RelayCommand]
    private async Task ScanAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        StatusText = "Recognizing from camera...";

        try
        {
            var (session, userError) = await _scanWorkflowService.RunAsync(CancellationToken.None);
            if (session is null)
            {
                ErrorMessage = userError ?? "Recognition failed. Please retry.";
                StatusText = "Scan failed";
                return;
            }

            await SaveAndPresentAsync(session, CancellationToken.None);
            StatusText = "Scan success and saved";
        }
        catch (Exception ex)
        {
            ErrorMessage = "Processing failed. Please retry.";
            StatusText = "Processing failed";
#if DEBUG
            StatusText = $"Processing failed ({ex.GetType().Name})";
#endif
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task VoiceInputAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        StatusText = "Listening for food name...";

        try
        {
            var voiceResult = await _microphoneService.ListenForFoodNameAsync(CancellationToken.None);
            if (!voiceResult.IsSuccess || string.IsNullOrWhiteSpace(voiceResult.FoodName))
            {
                ErrorMessage = voiceResult.FailureReason switch
                {
                    VoiceInputFailureReason.PermissionDenied => "Microphone permission was denied. Please enable it in system settings.",
                    VoiceInputFailureReason.Unsupported => "Voice recognition is unavailable on this device.",
                    VoiceInputFailureReason.NoSpeechDetected => "No speech recognized. Please try again and speak clearly.",
                    VoiceInputFailureReason.SpeechPrivacyDisabled => "Windows online speech recognition is disabled. Please enable Privacy > Speech and try again.",
                    VoiceInputFailureReason.NetworkUnavailable => "Voice recognition needs network access. Please check your connection and try again.",
                    _ => "Voice input failed. Please try again."
                };
                StatusText = "Voice input failed";
#if DEBUG
                if (voiceResult.FailureReason == VoiceInputFailureReason.Unknown && !string.IsNullOrWhiteSpace(voiceResult.TechnicalMessage))
                {
                    StatusText = $"Voice input failed ({voiceResult.TechnicalMessage})";
                }
#endif
                return;
            }

            var foodName = voiceResult.FoodName!;
            var nutrition = await _nutritionService.GetNutritionAsync(foodName, CancellationToken.None);
            if (nutrition is null)
            {
                ErrorMessage = $"'{foodName}' is not in nutrition dictionary yet.";
                StatusText = "Voice recognized but unsupported food";
                return;
            }

            var address = await _locationService.GetCurrentAddressAsync(CancellationToken.None) ?? "Location unavailable";
            var session = new ScanSession
            {
                RecognizedFood = foodName,
                Calories = nutrition.Calories,
                Protein = nutrition.Protein,
                Fat = nutrition.Fat,
                Carbs = nutrition.Carbs,
                Location = address,
                TimestampUtc = DateTime.UtcNow,
                Status = ScanStatus.Success
            };

            await SaveAndPresentAsync(session, CancellationToken.None);
            StatusText = "Voice input success and saved";
        }
        catch (Exception ex)
        {
            ErrorMessage = "Voice input failed. Please try again.";
            StatusText = "Voice input unavailable";
#if DEBUG
            StatusText = $"Voice input failed ({ex.GetType().Name})";
#endif
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SaveAndPresentAsync(ScanSession session, CancellationToken ct)
    {
        await _historyRepository.AddAsync(session, ct);
        RecognizedFood = session.RecognizedFood;
        Location = session.Location;
        NutritionText = $"Calories {session.Calories:F0} kcal | Protein {session.Protein:F1}g | Fat {session.Fat:F1}g | Carbs {session.Carbs:F1}g";
        var spoken = await _speechService.SpeakAsync($"Recognized {session.RecognizedFood}, calories {session.Calories:F0}.", ct);
        if (!spoken)
        {
            ErrorMessage = "Speech is unavailable on this device.";
        }
        _hapticService.NotifySuccess();
    }

    private async void OnShaken(object? sender, EventArgs e)
    {
        if (IsBusy)
        {
            return;
        }

        StatusText = "Shake detected, restarting scan";
        await ScanAsync();
    }
}
