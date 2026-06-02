using Microsoft.Maui.Devices.Sensors;
using NutriVision.Services.Abstractions;

namespace NutriVision.Services;

public sealed class CompassService : ICompassService
{
    private bool _subscribed;

    public bool IsSupported => Compass.Default.IsSupported;

    public bool IsMonitoring => Compass.Default.IsMonitoring;

    public event EventHandler<CompassReadingChangedEventArgs>? ReadingChanged;

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
                Compass.Default.ReadingChanged += OnReadingChanged;
                _subscribed = true;
            }

            Compass.Default.Start(SensorSpeed.UI, applyLowPassFilter: true);
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
                Compass.Default.Stop();
            }
        }
        catch
        {
        }

        Unsubscribe();
    }

    private void OnReadingChanged(object? sender, CompassChangedEventArgs e)
    {
        ReadingChanged?.Invoke(this, new CompassReadingChangedEventArgs(e.Reading.HeadingMagneticNorth));
    }

    private void Unsubscribe()
    {
        if (_subscribed)
        {
            Compass.Default.ReadingChanged -= OnReadingChanged;
            _subscribed = false;
        }
    }
}
