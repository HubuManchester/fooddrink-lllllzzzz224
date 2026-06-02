using Microsoft.Maui.Devices.Sensors;
using NutriVision.Services.Abstractions;

namespace NutriVision.Services;

public sealed class AccelerometerService : IAccelerometerService
{
    private bool _subscribed;

    public bool IsSupported => Accelerometer.Default.IsSupported;

    public bool IsMonitoring => Accelerometer.Default.IsMonitoring;

    public event EventHandler<AccelerometerReadingChangedEventArgs>? ReadingChanged;

    public void Start()
    {
        if (!IsSupported || IsMonitoring)
        {
            return;
        }

        try
        {
            if (!_subscribed)
            {
                Accelerometer.Default.ReadingChanged += OnReadingChanged;
                _subscribed = true;
            }

            Accelerometer.Default.Start(SensorSpeed.UI);
        }
        catch (FeatureNotSupportedException)
        {
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
            if (IsMonitoring)
            {
                Accelerometer.Default.Stop();
            }
        }
        catch
        {
        }

        Unsubscribe();
    }

    private void OnReadingChanged(object? sender, AccelerometerChangedEventArgs e)
    {
        var reading = e.Reading.Acceleration;
        ReadingChanged?.Invoke(this, new AccelerometerReadingChangedEventArgs(reading.X, reading.Y, reading.Z));
    }

    private void Unsubscribe()
    {
        if (_subscribed)
        {
            Accelerometer.Default.ReadingChanged -= OnReadingChanged;
            _subscribed = false;
        }
    }
}
