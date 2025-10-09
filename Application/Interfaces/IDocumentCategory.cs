using Domain.Models.API.Requests;
using Domain.Models.API.Results;
using Domain.Models.Common;

namespace Application.Interfaces;

public interface IDocumentCategory
{
    Task<Result<DocumentCategoryViewModel>> UpsertDocumentCategory(UpsertDocumentCategoryRequest request,
        long currentUserId);

    Task<Result<DocumentCategoryViewModel>> GetDocumentCategory(long documentCategoryId, long currentUserId);
    
    Task<Result<PagedResult<DocumentCategoryViewModel>>> GetDocumentCategories(PagedRequest request, long currentUserId);

    Task<Result<DocumentCategoryViewModel>> ToggleDocumentCategoryActivation(long documentCategoryId,
        long currentUserId);
}
