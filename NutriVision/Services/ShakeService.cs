using NutriVision.Services.Abstractions;

namespace NutriVision.Services;

public sealed class ShakeService : IShakeService
{
    private const double Threshold = 2.0;
    private DateTime _lastFired = DateTime.MinValue;

    public event EventHandler? Shaken;

    public void Start()
    {
        if (Accelerometer.IsMonitoring)
        {
            return;
        }

        Accelerometer.ReadingChanged += OnReadingChanged;
        Accelerometer.Start(SensorSpeed.Game);
    }

    public void Stop()
    {
        if (!Accelerometer.IsMonitoring)
        {
            return;
        }

        Accelerometer.Stop();
        Accelerometer.ReadingChanged -= OnReadingChanged;
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
}

