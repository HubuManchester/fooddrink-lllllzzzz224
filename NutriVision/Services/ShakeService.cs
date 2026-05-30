using NutriVision.Services.Abstractions;

namespace NutriVision.Services;

public sealed class ShakeService : IShakeService
{
    private const double Threshold = 2.0;
    private DateTime _lastFired = DateTime.MinValue;
    private bool _subscribed;

    public event EventHandler? Shaken;

    public void Start()
    {
        if (!Accelerometer.Default.IsSupported || Accelerometer.IsMonitoring)
        {
            return;
        }

        try
        {
            if (!_subscribed)
            {
                Accelerometer.ReadingChanged += OnReadingChanged;
                _subscribed = true;
            }

            Accelerometer.Start(SensorSpeed.Game);
        }
        catch (FeatureNotSupportedException)
        {
            // Some targets (or emulators) do not expose accelerometer.
            Unsubscribe();
        }
        catch (NotSupportedException)
        {
            Unsubscribe();
        }
    }

    public void Stop()
    {
        try
        {
            if (Accelerometer.IsMonitoring)
            {
                Accelerometer.Stop();
            }
        }
        catch
        {
            // Ignore stop failures on unsupported platforms.
        }

        Unsubscribe();
    }

    private void OnReadingChanged(object? sender, AccelerometerChangedEventArgs e)
    {
        var a = e.Reading.Acceleration;
        var magnitude = Math.Sqrt((a.X * a.X) + (a.Y * a.Y) + (a.Z * a.Z));
        if (magnitude < Threshold)
        {
            return;
        }

        var now = DateTime.UtcNow;
        if ((now - _lastFired).TotalMilliseconds < 900)
        {
            return;
        }

        _lastFired = now;
        Shaken?.Invoke(this, EventArgs.Empty);
    }

    private void Unsubscribe()
    {
        if (_subscribed)
        {
            Accelerometer.ReadingChanged -= OnReadingChanged;
            _subscribed = false;
        }
    }
}
