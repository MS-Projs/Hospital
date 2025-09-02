using DataAccess.Enums;
using Microsoft.AspNetCore.Http;

namespace Domain.Models.Infrastructure.Params;

public record SaveItemFileParams(Guid ItemId, IFormFile File, ItemFileType Type);