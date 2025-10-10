using Administration.Helpers;
using Application.Interfaces;
using Domain.Models.API.Requests;
using Domain.Models.API.Results;
using Domain.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Administration.Controllers;

/// <summary>
/// Controller for user authentication and profile management
/// </summary>
[ApiController]
[Route("api/v1/[controller]/[action]")]
public class AuthorizationController : MyController<AuthorizationController>
{
    private readonly IUserService _userService;

    public AuthorizationController(IUserService userService)
    {
        _userService = userService;
    }

    /*#region Authentication Endpoints

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="request">User registration details</param>
    /// <returns>Registration result with token</returns>
    [HttpPost]
    [AllowAnonymous]
    public async Task<Result<SignUpResult>> SignUp(SignUpRequest request)
    {
        return await _userService.SignUp(request);
    }

    /// <summary>
    /// Sign in with phone and password
    /// </summary>
    /// <param name="request">Sign in credentials</param>
    /// <returns>Sign in result with token</returns>
    /*[HttpPost]
    [AllowAnonymous]
    public async Task<Result<SignUpResult>> SignIn(SignInRequest request)
    {
        return await _userService.SignIn(request);
    }

    /// <summary>
    /// Sign out current user (invalidate token)
    /// </summary>
    /// <returns>Success result</returns>
    [HttpPost]
    [Authorize]
    public async Task<Result<bool>> SignOut()
    {
        return await _userService.SignOut(UserId);
    }

    #endregion

    #region Profile Management Endpoints

    /// <summary>
    /// Get current user profile information
    /// </summary>
    /// <returns>Current user profile</returns>
    [HttpGet]
    [Authorize]
    public async Task<Result<UserProfileResult>> GetProfile()
    {
        return await _userService.GetProfile(UserId);
    }

    /// <summary>
    /// Update current user profile
    /// </summary>
    /// <param name="request">Updated profile information</param>
    /// <returns>Updated user profile</returns>
    [HttpPost]
    [Authorize]
    public async Task<Result<UserProfileResult>> UpdateProfile(UpdateProfileRequest request)
    {
        return await _userService.UpdateProfile(request, UserId);
    }

    /// <summary>
    /// Change user password
    /// </summary>
    /// <param name="request">Password change request</param>
    /// <returns>Success result</returns>
    [HttpPost]
    [Authorize]
    public async Task<Result<bool>> ChangePassword(ChangePasswordRequest request)
    {
        return await _userService.ChangePassword(request, UserId);
    }

    #endregion*/
}