using Doto.Application.DTOs;
using Doto.Application.DTOs.Requests.Report;
using Doto.Application.DTOs.Responses;

namespace Doto.Application.Interfaces;

public interface IReportService
{
    Task<BaseResponse<ReportDtoResponse>> GenerateReportAsync(
        Guid personId, 
        GenerateReportRequest request);
}

