using Application.Interfaces;
using DataAccess;
using DataAccess.Enums;
using DataAccess.Schemas.Public;
using Domain.Enums;
using Domain.Extensions;
using Domain.Models.API.Requests;
using Domain.Models.API.Results;
using Domain.Models.Common;
using Infrastructure.Extensions;
using Infrastructure.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class PatientService : IPatient
{
    private readonly EntityContext _context;
    private readonly IFileService _fileService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<PatientService> _logger;

    public PatientService(
        EntityContext context,
        IFileService fileService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<PatientService> logger)
    {
        _context = context;
        _fileService = fileService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<Result<PatientViewModel>> UpsertPatient(
        UpsertPatientRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Normalize input
            request.Id ??= 0;
            request.FullName = request.FullName?.Trim() ?? string.Empty;
            request.Gender = request.Gender?.Trim().ToLowerInvariant() ?? string.Empty;
            request.AdditionalNotes = request.AdditionalNotes?.Trim();

            // Validate user exists
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == request.UserId && u.Status != EntityStatus.Deleted,
                    cancellationToken);

            if (user == null)
                return new ErrorModel(ErrorEnum.UserNotFound);

            PatientViewModel result;

            if (request.Id == 0)
            {
                // Check for existing patient
                var existingPatient = await _context.Patients
                    .AsNoTracking()
                    .AnyAsync(x => x.UserId == request.UserId && x.Status != EntityStatus.Deleted,
                        cancellationToken);

                if (existingPatient)
                    return new ErrorModel(ErrorEnum.PatientAlreadyExist);

                result = await CreatePatient(request, cancellationToken);
            }
            else
            {
                // Verify patient exists
                var patientExists = await _context.Patients
                    .AnyAsync(c => c.Id == request.Id.Value && c.Status != EntityStatus.Deleted,
                        cancellationToken);

                if (!patientExists)
                    return new ErrorModel(ErrorEnum.PatientNotFound);

                result = await UpdatePatient(request, cancellationToken);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upserting patient for user {UserId}", request.UserId);
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<PatientSingleViewModel>> GetPatientById(
        long patientId,
        CancellationToken cancellationToken)
    {
        try
        {
            var patient = await _context.Patients
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == patientId && d.Status != EntityStatus.Deleted,
                    cancellationToken);

            if (patient == null)
                return new ErrorModel(ErrorEnum.PatientNotFound);

            return new PatientSingleViewModel(patient);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting patient {PatientId}", patientId);
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<PagedResult<PatientViewModel>>> GetPatients(
        FilterPatientRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Normalize search criteria
            request.FullName = request.FullName?.Trim();
            request.Gender = request.Gender?.Trim().ToLowerInvariant();

            var query = BuildPatientQuery(request);

            var patients = await query
                .OrderBy(x => x.FullName)
                .Select(x => new PatientViewModel(x))
                .ToListAsync(cancellationToken);

            return request.All
                ? patients.ToListResponse()
                : patients.ToListResponse(request.PageNumber, request.PageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting patients with filter");
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<PatientViewModel>> PatientToggleActivation(
        long patientId,
        CancellationToken cancellationToken)
    {
        try
        {
            var patient = await _context.Patients
                .FirstOrDefaultAsync(c => c.Id == patientId, cancellationToken);

            if (patient == null)
                return new ErrorModel(ErrorEnum.PatientNotFound);

            patient.Status = patient.Status == EntityStatus.Deleted
                ? EntityStatus.Active
                : EntityStatus.Deleted;
            patient.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return new PatientViewModel(patient);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling patient {PatientId} activation", patientId);
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    #region Document Management

    public async Task<Result<DocumentViewModel>> UploadPatientDocument(
        UploadPatientDocumentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Verify patient exists
            var patientExists = await _context.Patients
                .AsNoTracking()
                .AnyAsync(p => p.Id == request.PatientId && p.Status != EntityStatus.Deleted,
                    cancellationToken);

            if (!patientExists)
                return new ErrorModel(ErrorEnum.PatientNotFound);

            // Save file
            var fileResult = await _fileService.SaveFileAsync(request.File, "patient-documents");
            if (!fileResult.Success)
                return fileResult.Error!;

            // Create document record
            var document = new PatientDocument
            {
                PatientId = request.PatientId,
                FilePath = fileResult.Payload!,
                FileType = request.FileType?.Trim() ?? "Unknown",
                DocumentCategoryId = request.CategoryId,
                EncryptedKey = _fileService.GenerateEncryptionKey(),
                UploadedAt = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow,
                Status = EntityStatus.Active
            };

            await _context.PatientDocuments.AddAsync(document, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // Load with includes
            var savedDocument = await _context.PatientDocuments
                .Include(d => d.DocumentCategory)
                .AsNoTracking()
                .FirstAsync(d => d.Id == document.Id, cancellationToken);

            var baseUrl = _httpContextAccessor.GetRequestPath();
            return new DocumentViewModel(savedDocument, baseUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document for patient {PatientId}", request.PatientId);
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<List<DocumentViewModel>>> GetPatientDocuments(
        long patientId,
        CancellationToken cancellationToken)
    {
        try
        {
            var patientExists = await _context.Patients
                .AsNoTracking()
                .AnyAsync(p => p.Id == patientId && p.Status != EntityStatus.Deleted,
                    cancellationToken);

            if (!patientExists)
                return new ErrorModel(ErrorEnum.PatientNotFound);

            var documents = await _context.PatientDocuments
                .Include(d => d.DocumentCategory)
                .Where(d => d.PatientId == patientId && d.Status != EntityStatus.Deleted)
                .OrderByDescending(d => d.UploadedAt)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var baseUrl = _httpContextAccessor.GetRequestPath();
            var documentViewModels = documents
                .Select(d => new DocumentViewModel(d, baseUrl))
                .ToList();

            return documentViewModels;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting documents for patient {PatientId}", patientId);
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<(Stream stream, string fileName, string contentType)>> DownloadPatientDocument(
        long documentId,
        CancellationToken cancellationToken)
    {
        try
        {
            var document = await _context.PatientDocuments
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == documentId && d.Status != EntityStatus.Deleted,
                    cancellationToken);

            if (document == null)
                return new ErrorModel(ErrorEnum.DocumentNotFound);

            return await _fileService.GetFileAsync(document.FilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading document {DocumentId}", documentId);
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<bool>> DeletePatientDocument(
        long documentId,
        CancellationToken cancellationToken)
    {
        try
        {
            var document = await _context.PatientDocuments
                .FirstOrDefaultAsync(d => d.Id == documentId && d.Status != EntityStatus.Deleted,
                    cancellationToken);

            if (document == null)
                return new ErrorModel(ErrorEnum.DocumentNotFound);

            // Soft delete
            document.Status = EntityStatus.Deleted;
            document.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            // Delete physical file (background operation)
            _ = Task.Run(() => _fileService.DeleteFileAsync(document.FilePath), CancellationToken.None);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document {DocumentId}", documentId);
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    #endregion

    #region Private Helper Methods

    private async Task<PatientViewModel> CreatePatient(
        UpsertPatientRequest request,
        CancellationToken cancellationToken)
    {
        var patient = request.Adapt<Patient>();
        patient.CreatedDate = DateTime.UtcNow;
        patient.Status = EntityStatus.Active;

        await _context.Patients.AddAsync(patient, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new PatientViewModel(patient);
    }

    private async Task<PatientViewModel> UpdatePatient(
        UpsertPatientRequest request,
        CancellationToken cancellationToken)
    {
        var patient = await _context.Patients
            .FirstAsync(c => c.Id == request.Id!.Value, cancellationToken);

        patient.UserId = request.UserId;
        patient.FullName = request.FullName;
        patient.AdditionalNotes = request.AdditionalNotes;
        patient.Age = request.Age;
        patient.Gender = request.Gender;
        patient.Address = request.Address?.Trim();
        patient.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new PatientViewModel(patient);
    }

    private IQueryable<Patient> BuildPatientQuery(FilterPatientRequest request)
    {
        var query = _context.Patients
            .Where(x => x.Status != EntityStatus.Deleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.FullName))
            query = query.Where(x => EF.Functions.ILike(x.FullName, $"%{request.FullName}%"));

        if (!string.IsNullOrWhiteSpace(request.Gender))
            query = query.Where(x => x.Gender == request.Gender);

        if (request.Age.HasValue)
            query = query.Where(x => x.Age >= request.Age.Value);

        return query;
    }

    #endregion
}