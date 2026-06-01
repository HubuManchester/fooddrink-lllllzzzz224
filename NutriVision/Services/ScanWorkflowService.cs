using NutriVision.Models;
using NutriVision.Services.Abstractions;

namespace NutriVision.Services;

public sealed class ScanWorkflowService : IScanWorkflowService
{
    private readonly ICameraService _cameraService;
    private readonly IFoodRecognitionService _foodRecognitionService;
    private readonly INutritionService _nutritionService;
    private readonly ILocationService _locationService;

    public ScanWorkflowService(
        ICameraService cameraService,
        IFoodRecognitionService foodRecognitionService,
        INutritionService nutritionService,
        ILocationService locationService)
    {
        _cameraService = cameraService;
        _foodRecognitionService = foodRecognitionService;
        _nutritionService = nutritionService;
        _locationService = locationService;
    }

    public async Task<(ScanSession? Session, string? UserError)> RunAsync(CancellationToken ct)
    {
        bool available;
        try
        {
            available = await _cameraService.IsAvailableAsync(ct);
        }
        catch
        {
            return (null, "Camera check failed. Please retry.");
        }

        if (!available)
        {
            return (null, "Camera is unavailable on this device.");
        }

        CameraPhoto? photo;
        try
        {
            photo = await _cameraService.CaptureAsync(ct);
        }
        catch
        {
            return (null, "Camera capture failed. Please check permission and retry.");
        }

        if (photo is null)
        {
            return (null, "Camera permission denied or capture canceled.");
        }

        await using (photo.Content)
        {
            RecognitionResult recognition;
            try
            {
                recognition = await _foodRecognitionService.RecognizeAsync(photo.Content, ct);
            }
            catch
            {
                return (null, "Food recognition failed. Please retry.");
            }

            if (!recognition.IsSuccess || string.IsNullOrWhiteSpace(recognition.FoodName))
            {
                return (null, "No food was recognized. Please retake the photo.");
            }

            NutritionInfo nutrition;
            try
            {
                nutrition = await _nutritionService.GetNutritionAsync(recognition.FoodName, ct) ?? new NutritionInfo();
            }
            catch
            {
                nutrition = new NutritionInfo();
            }

            string address;
            try
            {
                address = await _locationService.GetCurrentAddressAsync(ct) ?? "Location unavailable";
            }
            catch
            {
                address = "Location unavailable";
            }

            return (new ScanSession
            {
                PhotoPath = photo.Path,
                RecognizedFood = recognition.FoodName,
                Calories = nutrition.Calories,
                Protein = nutrition.Protein,
                Fat = nutrition.Fat,
                Carbs = nutrition.Carbs,
                Location = address,
                TimestampUtc = DateTime.UtcNow,
                Status = ScanStatus.Success
            }, null);
        }
    }
}
