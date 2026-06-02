namespace NutriVision.Services.Abstractions;

public interface ICompassService
{
    bool IsSupported { get; }
    bool IsMonitoring { get; }
    event EventHandler<CompassReadingChangedEventArgs>? ReadingChanged;
    void Start();
    void Stop();
}

public sealed class CompassReadingChangedEventArgs : EventArgs
{
    public CompassReadingChangedEventArgs(double heading)
    {
        Heading = heading;
    }

    public double Heading { get; }
}
