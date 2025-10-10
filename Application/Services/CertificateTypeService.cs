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

public class CertificateTypeService : ICertificateType
{
    private readonly EntityContext _context;

    public CertificateTypeService(EntityContext context)
    {
        _context = context;
    }

    public async Task<Result<CertificateTypeViewModel>> UpsertContractType(UpsertCertificateTypeRequest request,
        long currentUserId)
    {
        try
        {
            request.Id ??= 0;
            request.KeyWord = request.KeyWord!.Trim();
            request.ValueEn = request.ValueEn.Trim();
            request.ValueRu = request.ValueRu.Trim();
            request.ValueUz = request.ValueUz.Trim();
            request.ValueUzl = request.ValueUzl.Trim();

            if (!string.IsNullOrEmpty(request.KeyWord)
                || !string.IsNullOrEmpty(request.ValueEn)
                || !string.IsNullOrEmpty(request.ValueRu)
                || !string.IsNullOrEmpty(request.ValueUz)
                || !string.IsNullOrEmpty(request.ValueUzl))
                return new ErrorModel(ErrorEnum.BadRequest);

            CertificateType? contractType;

            if (request.Id == 0)
            {
                var existingGroup = await _context.CertificateTypes
                    .FirstOrDefaultAsync(ct => ct.KeyWord == request.KeyWord);
                if (existingGroup != null)
                    return new ErrorModel(ErrorEnum.BadRequest);
                contractType = new CertificateType
                {
                    KeyWord = request.KeyWord,
                    ValueEn = request.ValueEn,
                    ValueRu = request.ValueRu,
                    ValueUz = request.ValueUz,
                    ValueUzl = request.ValueUzl
                };
                await _context.CertificateTypes.AddAsync(contractType);
            }
            else
            {
                contractType = await _context.CertificateTypes.FirstOrDefaultAsync(ct => ct.Id == request.Id);

                if (contractType == null)
                    return new ErrorModel(ErrorEnum.CertificateTypeNotFound);
                contractType.ValueEn = request.ValueEn;
                contractType.ValueRu = request.ValueRu;
                contractType.ValueUz = request.ValueUz;
                contractType.ValueUzl = request.ValueUzl;
                contractType.UpdatedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();


            return new CertificateTypeViewModel(contractType);
        }
        catch (Exception ex)
        {
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<CertificateTypeViewModel>> GetType(long contractTypeId, long currentUserId)
    {
        try
        {
            var type = await _context.CertificateTypes.FirstOrDefaultAsync(ct => ct.Id == contractTypeId);
            if (type == null)
                return new ErrorModel(ErrorEnum.CertificateTypeNotFound);

            return new CertificateTypeViewModel(type);
        }
        catch (Exception ex)
        {
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<PagedResult<CertificateTypeViewModel>>> GetTypes(PagedRequest request, long currentUserId)
    {
        try
        {
            var types = await _context.CertificateTypes
                .OrderBy(ct => ct.KeyWord)
                .Select(x => new CertificateTypeViewModel(x))
                .ToListAsync();


            return request.All ? types.ToListResponse() : types.ToListResponse(request.PageNumber, request.PageSize);
        }
        catch (Exception ex)
        {
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<CertificateTypeViewModel>> ToggleContractTypeActivation(long contractTypeId,
        long currentUserId)
    {
        try
        {
            var type = await _context.CertificateTypes.FirstOrDefaultAsync(ct => ct.Id == contractTypeId);
            if (type == null)
                return new ErrorModel(ErrorEnum.CertificateTypeNotFound);

            type.Status = type.Status == EntityStatus.Deleted ? EntityStatus.Active : EntityStatus.Deleted;
            type.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

           

            
            return new CertificateTypeViewModel(type);
        }
        catch (Exception ex)
        {
           
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }
}