// using DataAccess;
// using DataAccess.Schemas.Public;
// using Domain.Models.Common;
// using Domain.Models.Infrastructure.Params;
// using Domain.Models.Infrastructure.Results;
// using Infrastructure.Extensions;
// using Infrastructure.Interfaces;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Logging;
//
// namespace Infrastructure.Services;
//
// public class FileService(
//     EntityContext entityContext,
//     ILogger<FileService> logger) : IFileService
// {
//     public async ValueTask<bool> SaveFile(SaveItemFileParams saveItemFileParams)
//     {
//         try
//         {
//             var extension = Path.GetExtension(saveItemFileParams.File.FileName);
//             var file = $"{Guid.NewGuid()}{extension}";
//             var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files");
//             var filePath = Path.Combine(directoryPath, file);
//
//             // Ensure directory exists
//             if (!Directory.Exists(directoryPath))
//                 Directory.CreateDirectory(directoryPath);
//
//             await using var stream = File.Create(filePath);
//             await streamaveItemFileParams.File.CopyToAsync(stream);
//
//             var itemFile = new ItemFile
//             {
//                 ItemId = saveItemFileParams.ItemId,
//                 Location = file,
//                 Extension = extension,
//                 Type = saveItemFileParams.Type,
//                 OriginalName = saveItemFileParams.File.FileName,
//                 Size = saveItemFileParams.File.Length
//             };
//
//             await entityContext.ItemFiles.AddAsync(itemFile);
//             await entityContext.SaveChangesAsync();
//
//             return true;
//         }
//         catch (Exception ex)
//         {
//             logger.LogCritical(ex, "Error saving file for item {ItemId}", saveItemFileParams.ItemId);
//             return false;
//         }
//     }
//     
//     public async Task<Result<IReadOnlyCollection<FileViewModel>>> GetItemFiles(GetItemFilesParams request)
//     {
//         var baseUrl = HttpContextExtension.GetRequestPath();
//         
//         var files = await entityContext.ItemFiles
//             .Where(x => x.Id == request.ItemId)
//             .Select(x => new FileViewModel($"{baseUrl}/files/{x.Location}", x.OriginalName, x.Size, x.Extension))
//             .ToListAsync();
//         return files;
//     }
// }