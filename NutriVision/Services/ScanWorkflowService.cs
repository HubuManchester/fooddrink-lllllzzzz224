using NutriVision.Helpers;
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
        var available = await _cameraService.IsAvailableAsync(ct);
        if (!available)
        {
            return (null, ErrorMessages.CameraUnavailable);
        }

        var photo = await _cameraService.CaptureAsync(ct);
        if (photo is null)
        {
            return (null, ErrorMessages.CameraPermissionDenied);
        }

        await using (photo.Content)
        {
            var recognition = await _foodRecognitionService.RecognizeAsync(photo.Content, ct);
            if (!recognition.IsSuccess || string.IsNullOrWhiteSpace(recognition.FoodName))
            {
                return (null, ErrorMessages.RecognitionEmpty);
            }

            var nutrition = await _nutritionService.GetNutritionAsync(recognition.FoodName, ct) ?? new NutritionInfo();
            var address = await _locationService.GetCurrentAddressAsync(ct) ?? "位置不可用";

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

