using Application.Interfaces;
using DataAccess;
using DataAccess.Enums;
using DataAccess.Schemas.Public;
using Domain.Enums;
using Domain.Extensions;
using Domain.Models.API.Requests;
using Domain.Models.API.Results;
using Domain.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class DocumentCategoryService : IDocumentCategory
{
    private readonly EntityContext _context;

    public DocumentCategoryService(EntityContext context)
    {
        _context = context;
    }

    public async Task<Result<DocumentCategoryViewModel>> UpsertDocumentCategory(UpsertDocumentCategoryRequest request,
        long currentUserId)
    {
        try
        {
            request.Id ??= 0;
            request.KeyWord = request.KeyWord?.Trim();
            request.ValueEn = request.ValueEn?.Trim();
            request.ValueRu = request.ValueRu?.Trim();
            request.ValueUz = request.ValueUz?.Trim();
            request.ValueUzl = request.ValueUzl?.Trim();

            if (string.IsNullOrEmpty(request.KeyWord) ||
                string.IsNullOrEmpty(request.ValueEn) ||
                string.IsNullOrEmpty(request.ValueRu) ||
                string.IsNullOrEmpty(request.ValueUz) ||
                string.IsNullOrEmpty(request.ValueUzl))
                return new ErrorModel(ErrorEnum.BadRequest);

            DocumentCategory? documentCategory;

            if (request.Id == 0)
            {
                var existingCategory = await _context.DocumentCategories
                    .FirstOrDefaultAsync(dc => dc.KeyWord == request.KeyWord && dc.Status == EntityStatus.Active);
                if (existingCategory != null)
                    return new ErrorModel(ErrorEnum.BadRequest);
                
                documentCategory = new DocumentCategory
                {
                    KeyWord = request.KeyWord,
                    ValueEn = request.ValueEn,
                    ValueRu = request.ValueRu,
                    ValueUz = request.ValueUz,
                    ValueUzl = request.ValueUzl,
                    Status = EntityStatus.Active
                };
                await _context.DocumentCategories.AddAsync(documentCategory);
            }
            else
            {
                documentCategory = await _context.DocumentCategories
                    .FirstOrDefaultAsync(dc => dc.Id == request.Id);

                if (documentCategory == null)
                    return new ErrorModel(ErrorEnum.DocumentCategoryNotFound);

                documentCategory.KeyWord = request.KeyWord;
                documentCategory.ValueEn = request.ValueEn;
                documentCategory.ValueRu = request.ValueRu;
                documentCategory.ValueUz = request.ValueUz;
                documentCategory.ValueUzl = request.ValueUzl;
                documentCategory.UpdatedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return new DocumentCategoryViewModel(documentCategory);
        }
        catch (Exception ex)
        {
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<DocumentCategoryViewModel>> GetDocumentCategory(long documentCategoryId, long currentUserId)
    {
        try
        {
            var category = await _context.DocumentCategories
                .FirstOrDefaultAsync(dc => dc.Id == documentCategoryId && dc.Status == EntityStatus.Active);
            
            if (category == null)
                return new ErrorModel(ErrorEnum.DocumentCategoryNotFound);

            return new DocumentCategoryViewModel(category);
        }
        catch (Exception ex)
        {
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<PagedResult<DocumentCategoryViewModel>>> GetDocumentCategories(PagedRequest request, long currentUserId)
    {
        try
        {
            var categories = await _context.DocumentCategories
                .Where(dc => dc.Status == EntityStatus.Active)
                .OrderBy(dc => dc.KeyWord)
                .Select(x => new DocumentCategoryViewModel(x))
                .ToListAsync();

            return request.All ? categories.ToListResponse() : categories.ToListResponse(request.PageNumber, request.PageSize);
        }
        catch (Exception ex)
        {
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<DocumentCategoryViewModel>> ToggleDocumentCategoryActivation(long documentCategoryId,
        long currentUserId)
    {
        try
        {
            var category = await _context.DocumentCategories
                .FirstOrDefaultAsync(dc => dc.Id == documentCategoryId);
            
            if (category == null)
                return new ErrorModel(ErrorEnum.DocumentCategoryNotFound);

            category.Status = category.Status == EntityStatus.Deleted ? EntityStatus.Active : EntityStatus.Deleted;
            category.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new DocumentCategoryViewModel(category);
        }
        catch (Exception ex)
        {
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }
}
