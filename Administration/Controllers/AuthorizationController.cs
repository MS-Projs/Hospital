using Administration.Helpers;
using Application.Interfaces;
using Domain.Models.API.Requests;
using Domain.Models.API.Results;
using Domain.Models.Common;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignInResult = Domain.Models.API.Results.SignInResult;

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
    [HttpPost("send-otp")]
    public async Task<Result<CreateSessionResult>> CreateSession(CreateSessionRequest request)
        => await _userService.CreateSession(request);

    /// <summary>
    /// verify otp
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("verify-otp")]
    public async Task<Result<VerifySessionResult>> VerifySession(VerifySessionRequest request)
        => await _userService.VerifySession(request);

    /// <summary>
    /// Refresh token
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("refresh-token")]
    public async Task<Result<RefreshTokenResult>> RefreshToken(RefreshTokenRequest request)
        => await _userService.RefreshToken(request);
    
    #region Profile Management

    /// <summary>
    /// Get user profile information
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User profile details</returns>
    [HttpGet("profile"),Authorize]
    public async Task<Result<UserProfileViewModel>> GetProfile(
        [FromQuery] long userId,
        CancellationToken cancellationToken = default)
    {
        return await _userService.GetProfile(userId, cancellationToken);
    }

    /// <summary>
    /// Update user profile information
    /// </summary>
    /// <param name="request">Profile update request with new user details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated user profile</returns>
    [HttpPut("profile"),Authorize]
    public async Task<Result<UserProfileViewModel>> UpdateProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        return await _userService.UpdateProfile(request, cancellationToken);
    }

    /// <summary>
    /// Upload or update user profile photo
    /// </summary>
    /// <param name="request">Profile photo upload request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated user profile with new photo URL</returns>
    [HttpPost("profile-photo"),Authorize]
    public async Task<Result<UserProfileViewModel>> UploadProfilePhoto(
        [FromForm] UploadProfilePhotoRequest request,
        CancellationToken cancellationToken = default)
    {
        return await _userService.UploadProfilePhoto(request, cancellationToken);
    }

    /// <summary>
    /// Delete user profile photo
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpDelete("profile-photo"),Authorize]
    public async Task<Result<bool>> DeleteProfilePhoto(
        [FromQuery] long userId,
        CancellationToken cancellationToken = default)
    {
        return await _userService.DeleteProfilePhoto(userId, cancellationToken);
    }

    /// <summary>
    /// Change user password
    /// </summary>
    /// <param name="request">Password change request with old and new passwords</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpPost("change-password")]
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
    [HttpPost("deactivate")]
    public async Task<Result<bool>> DeactivateAccount(
        [FromQuery] long userId,
        CancellationToken cancellationToken = default)
    {
        return await _userService.DeactivateAccount(userId, cancellationToken);
    }

    /// <summary>
    /// Delete user account permanently (soft delete)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpDelete("account")]
    public async Task<Result<bool>> DeleteAccount(
        [FromQuery] long userId,
        CancellationToken cancellationToken = default)
    {
        return await _userService.DeleteAccount(userId, cancellationToken);
    }

    #endregion
}