using NutriVision.Models;

namespace NutriVision.Services.Abstractions;

public interface IScanWorkflowService
{
    Task<(ScanSession? Session, string? UserError)> RunAsync(CancellationToken ct);
}

