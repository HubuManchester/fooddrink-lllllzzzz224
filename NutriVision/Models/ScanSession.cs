using SQLite;

namespace NutriVision.Models;

[Table("scan_sessions")]
public sealed class ScanSession
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string? PhotoPath { get; set; }
    public string? RecognizedFood { get; set; }
    public double Calories { get; set; }
    public double Protein { get; set; }
    public double Fat { get; set; }
    public double Carbs { get; set; }
    public string? Location { get; set; }
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public ScanStatus Status { get; set; } = ScanStatus.Pending;
    public string? ErrorCode { get; set; }
}

