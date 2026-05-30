using System.Globalization;
using CommunityToolkit.Maui.Media;
using NutriVision.Services.Abstractions;

namespace NutriVision.Services;

public sealed class MicrophoneService : IMicrophoneService
{
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
                var options = new SpeechToTextOptions
                {
                    Culture = CultureInfo.CurrentCulture,
                    ShouldReportPartialResults = true
                };

                await speechToText.StartListenAsync(options, ct);
                await Task.Delay(TimeSpan.FromSeconds(4), ct);
                await speechToText.StopListenAsync(CancellationToken.None);

                SpeechToTextResult? result = null;
                try
                {
                    result = await completed.Task.WaitAsync(TimeSpan.FromSeconds(4), ct);
                }
                catch
                {
                    // If completion callback is late, fallback to partial text.
                }

                var finalText = result?.Text ?? latestPartial;
                return string.IsNullOrWhiteSpace(finalText)
                    ? VoiceInputResult.Fail(VoiceInputFailureReason.NoSpeechDetected)
                    : VoiceInputResult.Success(finalText.Trim().ToLowerInvariant());
            }
            finally
            {
                speechToText.RecognitionResultUpdated -= OnUpdated;
                speechToText.RecognitionResultCompleted -= OnCompleted;
            }
        }
        catch (FeatureNotSupportedException)
        {
            return VoiceInputResult.Fail(VoiceInputFailureReason.Unsupported);
        }
        catch (NotSupportedException)
        {
            return VoiceInputResult.Fail(VoiceInputFailureReason.Unsupported);
        }
        catch
        {
            return VoiceInputResult.Fail(VoiceInputFailureReason.Unknown);
        }
    }
}
