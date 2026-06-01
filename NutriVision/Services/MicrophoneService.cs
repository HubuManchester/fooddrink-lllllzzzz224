using System.Globalization;
using CommunityToolkit.Maui.Media;
using NutriVision.Services.Abstractions;

namespace NutriVision.Services;

public sealed class MicrophoneService : IMicrophoneService
{
    private const int HResultSpeechPrivacyDeclined = unchecked((int)0x80045509);
    private static readonly TimeSpan FirstListenWindow = TimeSpan.FromSeconds(6);
    private static readonly TimeSpan RetryListenWindow = TimeSpan.FromSeconds(8);
    private static readonly TimeSpan CompletionWaitWindow = TimeSpan.FromSeconds(3);

    public async Task<VoiceInputResult> ListenForFoodNameAsync(CancellationToken ct)
    {
        try
        {
            var speechToText = SpeechToText.Default;
            var granted = await speechToText.RequestPermissions(ct);
            if (!granted)
            {
                return VoiceInputResult.Fail(VoiceInputFailureReason.PermissionDenied);
            }

            string? latestPartial = null;
            var completed = new TaskCompletionSource<SpeechToTextResult>(TaskCreationOptions.RunContinuationsAsynchronously);

            void OnUpdated(object? _, SpeechToTextRecognitionResultUpdatedEventArgs e)
            {
                latestPartial = e.RecognitionResult;
            }

            void OnCompleted(object? _, SpeechToTextRecognitionResultCompletedEventArgs e)
            {
                completed.TrySetResult(e.RecognitionResult);
            }

            speechToText.RecognitionResultUpdated += OnUpdated;
            speechToText.RecognitionResultCompleted += OnCompleted;

            try
            {
                var firstAttempt = await ListenOnceAsync(
                    speechToText,
                    completed,
                    () => latestPartial,
                    FirstListenWindow,
                    ct);

                if (!string.IsNullOrWhiteSpace(firstAttempt))
                {
                    return VoiceInputResult.Success(firstAttempt.Trim().ToLowerInvariant());
                }

                // Retry once with a longer listen window for emulator/slow-start recognition.
                latestPartial = null;
                completed = new TaskCompletionSource<SpeechToTextResult>(TaskCreationOptions.RunContinuationsAsynchronously);
                var secondAttempt = await ListenOnceAsync(
                    speechToText,
                    completed,
                    () => latestPartial,
                    RetryListenWindow,
                    ct);

                return string.IsNullOrWhiteSpace(secondAttempt)
                    ? VoiceInputResult.Fail(VoiceInputFailureReason.NoSpeechDetected)
                    : VoiceInputResult.Success(secondAttempt.Trim().ToLowerInvariant());
            }
            finally
            {
                speechToText.RecognitionResultUpdated -= OnUpdated;
                speechToText.RecognitionResultCompleted -= OnCompleted;
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            return VoiceInputResult.Fail(VoiceInputFailureReason.PermissionDenied, ex.Message);
        }
        catch (FeatureNotSupportedException)
        {
            return VoiceInputResult.Fail(VoiceInputFailureReason.Unsupported);
        }
        catch (NotSupportedException)
        {
            return VoiceInputResult.Fail(VoiceInputFailureReason.Unsupported);
        }
        catch (Exception ex) when (ex.HResult == HResultSpeechPrivacyDeclined)
        {
            return VoiceInputResult.Fail(VoiceInputFailureReason.SpeechPrivacyDisabled, ex.Message);
        }
        catch (Exception ex) when (ex.Message.Contains("network", StringComparison.OrdinalIgnoreCase))
        {
            return VoiceInputResult.Fail(VoiceInputFailureReason.NetworkUnavailable, ex.Message);
        }
        catch (Exception ex)
        {
            var detail = $"{ex.GetType().Name} (0x{ex.HResult:X8}): {ex.Message}";
            return VoiceInputResult.Fail(VoiceInputFailureReason.Unknown, detail);
        }
    }

    private static async Task<string?> ListenOnceAsync(
        ISpeechToText speechToText,
        TaskCompletionSource<SpeechToTextResult> completed,
        Func<string?> getLatestPartial,
        TimeSpan listenWindow,
        CancellationToken ct)
    {
        var options = new SpeechToTextOptions
        {
            Culture = CultureInfo.CurrentCulture,
            ShouldReportPartialResults = true
        };

        await speechToText.StartListenAsync(options, ct);

        var timeoutTask = Task.Delay(listenWindow, ct);
        var finished = await Task.WhenAny(completed.Task, timeoutTask);
        if (finished == timeoutTask)
        {
            await speechToText.StopListenAsync(CancellationToken.None);
        }

        SpeechToTextResult? result = null;
        try
        {
            result = await completed.Task.WaitAsync(CompletionWaitWindow, ct);
        }
        catch
        {
            // Completion can be delayed; partial text remains a valid fallback.
        }

        return result?.Text ?? getLatestPartial();
    }
}
