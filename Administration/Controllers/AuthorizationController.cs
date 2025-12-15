using Administration.Helpers;
using Application.Interfaces;
using Domain.Models.API.Requests;
using Domain.Models.API.Results;
using Domain.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Administration.Controllers;

[ApiController]
[Route("api/v1/[controller]/[action]")]
public class AuthorizationController: MyController<AuthorizationController>
{
    private readonly IUserService _userService;
    public AuthorizationController(IUserService userService)
    {
        _userService = userService;
    }
    /// <summary>
    /// send otp to phone number
    /// </summary>
    /// <param name="request">phoneNumber should send as 998... format </param>
    /// <returns></returns>
    [HttpPost]
    public async Task<Result<CreateSessionResult>> CreateSession(CreateSessionRequest request)
        => await _userService.CreateSession(request);

    /// <summary>
    /// verify otp
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<Result<VerifySessionResult>> VerifySession(VerifySessionRequest request)
        => await _userService.VerifySession(request);

    /// <summary>
    /// Refresh token
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<Result<RefreshTokenResult>> RefreshToken(RefreshTokenRequest request)
        => await _userService.RefreshToken(request);
    
    #region Profile Management

    /// <summary>
    /// Get user profile information
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User profile details</returns>
    [HttpGet,Authorize]
    public async Task<Result<UserProfileViewModel>> GetProfile(
        CancellationToken cancellationToken = default)
    {
        return await _userService.GetProfile(UserId, cancellationToken);
    }

    /// <summary>
    /// Update user profile information
    /// </summary>
    /// <param name="request">Profile update request with new user details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated user profile</returns>
    [HttpPut,Authorize]
    public async Task<Result<UserProfileViewModel>> UpdateProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var newRequest = request;
        newRequest.UserId = UserId;
        return await _userService.UpdateProfile(newRequest, cancellationToken);
    }

    /// <summary>
    /// Upload or update user profile photo
    /// </summary>
    /// <param name="request">Profile photo upload request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated user profile with new photo URL</returns>
    [HttpPost,Authorize]
    public async Task<Result<UserProfileViewModel>> UploadProfilePhoto(
        [FromForm] UploadProfilePhotoRequest request,
        CancellationToken cancellationToken = default)
    {
        var newRequest = request;
        newRequest.UserId = UserId;
        return await _userService.UploadProfilePhoto(request, cancellationToken);
    }

    /// <summary>
    /// Delete user profile photo
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpDelete,Authorize]
    public async Task<Result<bool>> DeleteProfilePhoto(
        CancellationToken cancellationToken = default)
    {
        return await _userService.DeleteProfilePhoto(UserId,cancellationToken);
    }

    /// <summary>
    /// Change user password
    /// </summary>
    /// <param name="request">Password change request with old and new passwords</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpPost,Authorize]
    public async Task<Result<bool>> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        return await _userService.ChangePassword(request, cancellationToken);
    }

    /// <summary>
    /// Deactivate user account (can be reactivated later)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpPost,Authorize]
    public async Task<Result<bool>> DeactivateAccount(
        CancellationToken cancellationToken = default)
    {
        return await _userService.DeactivateAccount(UserId, cancellationToken);
    }

    /// <summary>
    /// Delete user account permanently (soft delete)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpDelete,Authorize]
    public async Task<Result<bool>> DeleteAccount(
        CancellationToken cancellationToken = default)
    {
        return await _userService.DeleteAccount(UserId, cancellationToken);
    }

    #endregion
}