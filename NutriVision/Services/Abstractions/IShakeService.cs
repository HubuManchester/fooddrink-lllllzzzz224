namespace NutriVision.Services.Abstractions;

public interface IShakeService
{
    event EventHandler? Shaken;
    void Start();
    void Stop();
}

