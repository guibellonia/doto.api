using Doto.Domain.Common;
using Doto.Domain.Enums;

namespace Doto.Domain.Entities;

public class ReportExport : EntityBase
{
    private ReportExport()
    {
    }

    private ReportExport(
        Guid requestedByUserId,
        Guid subjectUserId,
        ReportType reportType,
        DateOnly periodStart,
        DateOnly periodEnd)
        : base(Guid.NewGuid(), DateTime.UtcNow)
    {
        if (requestedByUserId == Guid.Empty)
        {
            throw new ArgumentException("O export precisa de um solicitante.", nameof(requestedByUserId));
        }

        if (subjectUserId == Guid.Empty)
        {
            throw new ArgumentException("O export precisa de um sujeito.", nameof(subjectUserId));
        }

        if (periodEnd < periodStart)
        {
            throw new ArgumentException("O fim do período não pode ser anterior ao início.", nameof(periodEnd));
        }

        RequestedByUserId = requestedByUserId;
        SubjectUserId = subjectUserId;
        ReportType = reportType;
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
        Status = ReportExportStatus.Pending;
    }

    public Guid RequestedByUserId { get; private set; }

    public Guid SubjectUserId { get; private set; }

    public ReportType ReportType { get; private set; }

    public DateOnly PeriodStart { get; private set; }

    public DateOnly PeriodEnd { get; private set; }

    public ReportExportStatus Status { get; private set; }

    public string? FileName { get; private set; }

    public string? StoragePath { get; private set; }

    public DateTime? GeneratedAtUtc { get; private set; }

    public string? ErrorMessage { get; private set; }

    public AppUser? RequestedByUser { get; private set; }

    public AppUser? SubjectUser { get; private set; }

    public static ReportExport Request(
        Guid requestedByUserId,
        Guid subjectUserId,
        ReportType reportType,
        DateOnly periodStart,
        DateOnly periodEnd)
        => new(requestedByUserId, subjectUserId, reportType, periodStart, periodEnd);

    public void MarkCompleted(string fileName, string storagePath, DateTime generatedAtUtc)
    {
        Status = ReportExportStatus.Completed;
        FileName = fileName;
        StoragePath = storagePath;
        GeneratedAtUtc = generatedAtUtc;
        ErrorMessage = null;
    }

    public void MarkFailed(string errorMessage, DateTime failedAtUtc)
    {
        Status = ReportExportStatus.Failed;
        ErrorMessage = errorMessage;
        GeneratedAtUtc = failedAtUtc;
    }
}
