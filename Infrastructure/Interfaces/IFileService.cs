using Domain.Models.Common;
using Domain.Models.Infrastructure.Params;
using Domain.Models.Infrastructure.Results;

namespace Infrastructure.Interfaces;

public interface IFileService
{
    ValueTask<bool> SaveFile(SaveItemFileParams saveItemFileParams);
    Task<Result<IReadOnlyCollection<FileViewModel>>> GetItemFiles(GetItemFilesParams request);
}