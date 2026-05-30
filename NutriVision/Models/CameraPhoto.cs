namespace NutriVision.Models;

public sealed class CameraPhoto
{
    public string Path { get; init; } = string.Empty;
    public Stream Content { get; init; } = Stream.Null;
}

