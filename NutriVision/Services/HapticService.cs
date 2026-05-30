using NutriVision.Services.Abstractions;

namespace NutriVision.Services;

public sealed class HapticService : IHapticService
{
    public void NotifySuccess()
    {
        try
        {
            Vibration.Vibrate(TimeSpan.FromMilliseconds(120));
            HapticFeedback.Perform(HapticFeedbackType.Click);
        }
        catch
        {
            // Some emulator targets do not support this hardware feedback.
        }
    }
}

