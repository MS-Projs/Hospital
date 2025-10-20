using Administration.Helpers;
using Application.Interfaces;
using Domain.Models.API.Requests;
using Domain.Models.API.Results;
using Domain.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Administration.Controllers;

/// <summary>
/// Controller for document category management operations
/// </summary>
[ApiController]
[Route("api/v1/[controller]/[action]")]

public class DocumentCategoryController : MyController<DocumentCategoryController>
{
    private readonly IDocumentCategory _documentCategoryService;

    public DocumentCategoryController(IDocumentCategory documentCategoryService)
    {
        _documentCategoryService = documentCategoryService;
    }

    #region CRUD Operations

    /// <summary>
    /// Create or update a document category
    /// </summary>
    /// <param name="request">Document category information</param>
    /// <returns>Document category view model</returns>
    [HttpPost]
    public async Task<Result<DocumentCategoryViewModel>> UpsertDocumentCategory(
        UpsertDocumentCategoryRequest request)
    {
        return await _documentCategoryService.UpsertDocumentCategory(request, UserId);
    }

    /// <summary>
    /// Get document category by ID
    /// </summary>
    /// <param name="documentCategoryId">Document category ID</param>
    /// <returns>Document category view model</returns>
    [HttpGet]
    public async Task<Result<DocumentCategoryViewModel>> GetDocumentCategory(
        [FromQuery] long documentCategoryId)
    {
        return await _documentCategoryService.GetDocumentCategory(documentCategoryId, UserId);
    }

    /// <summary>
    /// Get paginated list of document categories
    /// </summary>
    /// <param name="request">Pagination parameters</param>
    /// <returns>Paginated list of document categories</returns>
    [HttpPost]
    public async Task<Result<PagedResult<DocumentCategoryViewModel>>> GetDocumentCategories(
        PagedRequest request)
    {
        return await _documentCategoryService.GetDocumentCategories(request, UserId);
    }

    #endregion

    #region Status Management

    /// <summary>
    /// Toggle document category activation status (activate/deactivate)
    /// </summary>
    /// <param name="documentCategoryId">Document category ID</param>
    /// <returns>Updated document category view model</returns>
    [HttpPost]
    public async Task<Result<DocumentCategoryViewModel>> ToggleActivation(
        [FromQuery] long documentCategoryId)
    {
        return await _documentCategoryService.ToggleDocumentCategoryActivation(documentCategoryId, UserId);
    }

    #endregion
}
