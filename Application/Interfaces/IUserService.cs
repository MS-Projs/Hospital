﻿using Domain.Models.API.Requests;
using Domain.Models.API.Results;
using Domain.Models.Common;

namespace Application.Interfaces;

public interface IUserService
{
    Task<Result<SignUpResult>> SignUp(SignUpRequest signUpRequest);
    Task<Result<SignInResult>> SignIn(SignInRequest request);

    // Profile Management
    Task<Result<UserProfileViewModel>> GetProfile(long userId, CancellationToken cancellationToken);
    Task<Result<UserProfileViewModel>> UpdateProfile(UpdateProfileRequest request, CancellationToken cancellationToken);

    Task<Result<UserProfileViewModel>> UploadProfilePhoto(UploadProfilePhotoRequest request,
        CancellationToken cancellationToken);

    Task<Result<bool>> DeleteProfilePhoto(long userId, CancellationToken cancellationToken);
    Task<Result<bool>> ChangePassword(ChangePasswordRequest request, CancellationToken cancellationToken);
    Task<Result<bool>> DeactivateAccount(long userId, CancellationToken cancellationToken);
    Task<Result<bool>> DeleteAccount(long userId, CancellationToken cancellationToken);
}