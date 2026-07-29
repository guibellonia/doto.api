using Doto.Domain.Entities;

namespace Doto.Domain.Interfaces;

public interface IReportExportRepository
{
    Task<ReportExport?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ReportExport>> GetBySubjectAsync(Guid subjectUserId, CancellationToken cancellationToken = default);

    Task AddAsync(ReportExport export, CancellationToken cancellationToken = default);

    Task UpdateAsync(ReportExport export, CancellationToken cancellationToken = default);
}
