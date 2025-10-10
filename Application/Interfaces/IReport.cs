using Domain.Models.API.Requests;
using Domain.Models.API.Results;
using Domain.Models.Common;

namespace Application.Interfaces;

public interface IReport
{
    Task<Result<ReportViewModel>> CreateReport(CreateReportRequest request, CancellationToken cancellationToken);
    Task<Result<ReportViewModel>> GetReportById(long reportId, CancellationToken cancellationToken);
    Task<Result<PagedResult<ReportViewModel>>> GetReports(FilterReportRequest request, CancellationToken cancellationToken);
    Task<Result<(Stream stream, string fileName, string contentType)>> DownloadReportPdf(long reportId, CancellationToken cancellationToken);
    Task<Result<bool>> DeleteReport(long reportId, CancellationToken cancellationToken);
    Task<Result<ReportViewModel>> UploadReportFile(UploadReportFileRequest request, CancellationToken cancellationToken);
}
