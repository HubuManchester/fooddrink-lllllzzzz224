namespace NutriVision.Models;

public sealed class AppSettings
{
    public ThemeMode ThemeMode { get; set; } = ThemeMode.System;
    public FontScale FontScale { get; set; } = FontScale.Medium;
    public bool HighContrastEnabled { get; set; }
    public bool TtsEnabled { get; set; } = true;
}

public enum ThemeMode
{
    System = 0,
    Light = 1,
    Dark = 2
}

public enum FontScale
{
    Small = 0,
    Medium = 1,
    Large = 2
}

