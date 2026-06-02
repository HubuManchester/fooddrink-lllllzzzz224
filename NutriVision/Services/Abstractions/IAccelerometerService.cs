namespace NutriVision.Services.Abstractions;

public interface IAccelerometerService
{
    bool IsSupported { get; }
    bool IsMonitoring { get; }
    event EventHandler<AccelerometerReadingChangedEventArgs>? ReadingChanged;
    void Start();
    void Stop();
}

public sealed class AccelerometerReadingChangedEventArgs : EventArgs
{
    public AccelerometerReadingChangedEventArgs(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public double X { get; }
    public double Y { get; }
    public double Z { get; }
}
