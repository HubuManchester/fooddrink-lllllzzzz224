using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NutriVision.Models;
using NutriVision.Services.Abstractions;

namespace NutriVision.ViewModels;

public partial class ScanViewModel : BaseViewModel
{
    private readonly IScanWorkflowService _scanWorkflowService;
    private readonly IHistoryRepository _historyRepository;
    private readonly ISpeechService _speechService;
    private readonly IHapticService _hapticService;
    private readonly IShakeService _shakeService;

    [ObservableProperty]
    private string statusText = "准备拍照识别";

    [ObservableProperty]
    private string? recognizedFood;

    [ObservableProperty]
    private string? location;

    [ObservableProperty]
    private string nutritionText = "-";

    public ScanViewModel(
        IScanWorkflowService scanWorkflowService,
        IHistoryRepository historyRepository,
        ISpeechService speechService,
        IHapticService hapticService,
        IShakeService shakeService)
    {
        _scanWorkflowService = scanWorkflowService;
        _historyRepository = historyRepository;
        _speechService = speechService;
        _hapticService = hapticService;
        _shakeService = shakeService;
        _shakeService.Shaken += OnShaken;
        _shakeService.Start();
    }

    [RelayCommand]
    private async Task ScanAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        StatusText = "识别中，请稍候...";

        try
        {
            var (session, userError) = await _scanWorkflowService.RunAsync(CancellationToken.None);
            if (session is null)
            {
                ErrorMessage = userError ?? "识别失败，请重试。";
                StatusText = "识别失败";
                return;
            }

            await _historyRepository.AddAsync(session, CancellationToken.None);

            RecognizedFood = session.RecognizedFood;
            Location = session.Location;
            NutritionText = $"热量 {session.Calories:F0} kcal | 蛋白 {session.Protein:F1}g | 脂肪 {session.Fat:F1}g | 碳水 {session.Carbs:F1}g";
            StatusText = "识别成功并已保存";

            await _speechService.SpeakAsync($"识别到 {session.RecognizedFood}，热量 {session.Calories:F0} 千卡。", CancellationToken.None);
            _hapticService.NotifySuccess();
        }
        catch (Exception)
        {
            ErrorMessage = "处理失败，请稍后重试。";
            StatusText = "处理失败";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void OnShaken(object? sender, EventArgs e)
    {
        if (IsBusy)
        {
            return;
        }

        StatusText = "检测到摇一摇，重新开始识别";
        await ScanAsync();
    }
}

