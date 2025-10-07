using DataAccess.Enums;

namespace Domain.Models.Infrastructure.Params;

public record GetItemFilesParams(Guid ItemId, ItemFileType? Type);