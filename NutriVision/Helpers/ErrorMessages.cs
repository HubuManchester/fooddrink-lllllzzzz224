namespace NutriVision.Helpers;

public static class ErrorMessages
{
    public const string CameraPermissionDenied = "未获得相机权限，请在系统设置中允许后重试。";
    public const string CameraUnavailable = "当前设备无法使用相机，请切换设备后重试。";
    public const string NetworkUnavailable = "当前网络不可用，已切换到本地识别。";
    public const string RecognitionEmpty = "没有识别到食物，请重新拍摄清晰图片。";
    public const string RecognitionFailed = "识别失败，请稍后重试。";
    public const string LocationUnavailable = "无法获取位置，将以“位置不可用”保存记录。";
    public const string DatabaseWriteFailed = "保存失败，请稍后再试。";
}

