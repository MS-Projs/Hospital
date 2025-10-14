using Application.Interfaces;
using DataAccess;
using DataAccess.Enums;
using DataAccess.Schemas.Auth;
using Domain.Enums;
using Domain.Extensions;
using Domain.Models.API.Requests;
using Domain.Models.API.Results;
using Domain.Models.Common;
using Domain.Models.Infrastructure.Params;
using Infrastructure.Extensions;
using Infrastructure.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class UserService(
    ITokenService tokenService,
    EntityContext context,
    IHttpContextAccessor httpContextAccessor,
    IFileService fileService) : IUserService
{
    public async Task<Result<SignUpResult>> SignUp(SignUpRequest signUpRequest)
    {
        var isPhoneNotUnique = await context.Users.AnyAsync(x => x.Phone == signUpRequest.Phone);
        if (isPhoneNotUnique)
            return new ErrorModel(ErrorEnum.PhoneAlreadyExists);

        var newUser = signUpRequest.Adapt<User>();
        newUser.Password = newUser.Password.HashPassword();
        await context.Users.AddAsync(newUser);
        await context.SaveChangesAsync();

        return await GenerateTokenForUser(newUser);
    }

    public async Task<Result<SignInResult>> SignIn(SignInRequest request)
    {
        var user = await context.Users.FirstOrDefaultAsync(x => x.Phone == request.Phone);
        if (user == null || !request.Password.VerifyPassword(user.Password))
            return new ErrorModel(ErrorEnum.UserNotFound);
        
        var token = await GenerateTokenForUser(user);
        return token.Adapt<SignInResult>();
    }

    #region Profile Management

    public async Task<Result<UserProfileViewModel>> GetProfile(long userId, CancellationToken cancellationToken)
    {
        try
        {
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id == userId && u.Status != EntityStatus.Deleted, cancellationToken);
                
            if (user == null)
                return new ErrorModel(ErrorEnum.UserNotFound);

            var baseUrl = httpContextAccessor.GetRequestPath();
            return new UserProfileViewModel(user, baseUrl);
        }
        catch (Exception ex)
        {
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<UserProfileViewModel>> UpdateProfile(UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId && u.Status != EntityStatus.Deleted, cancellationToken);
                
            if (user == null)
                return new ErrorModel(ErrorEnum.UserNotFound);

            // Check if new phone is unique (if phone is being changed)
            if (request.Phone != user.Phone)
            {
                var phoneExists = await context.Users
                    .AnyAsync(u => u.Phone == request.Phone && u.Id != request.UserId, cancellationToken);
                    
                if (phoneExists)
                    return new ErrorModel(ErrorEnum.PhoneAlreadyExists);
            }

            // Update user properties
            user.FirstName = request.FirstName ?? user.FirstName;
            user.LastName = request.LastName ?? user.LastName;
            user.Phone = request.Phone ?? user.Phone;
            user.Email = request.Email ?? user.Email;
            user.DateOfBirth = request.DateOfBirth ?? user.DateOfBirth;
            user.Gender = request.Gender ?? user.Gender;
            user.Address = request.Address ?? user.Address;
            user.UpdatedDate = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);

            var baseUrl = httpContextAccessor.GetRequestPath();
            return new UserProfileViewModel(user, baseUrl);
        }
        catch (Exception ex)
        {
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<UserProfileViewModel>> UploadProfilePhoto(UploadProfilePhotoRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId && u.Status != EntityStatus.Deleted, cancellationToken);
                
            if (user == null)
                return new ErrorModel(ErrorEnum.UserNotFound);

            // Delete old photo if exists
            if (!string.IsNullOrEmpty(user.ProfilePhotoPath))
            {
                await fileService.DeleteFileAsync(user.ProfilePhotoPath);
            }

            // Save new photo
            var fileResult = await fileService.SaveFileAsync(request.Photo, "profile-photos");
            if (!fileResult.Success)
                return fileResult.Error!;

            user.ProfilePhotoPath = fileResult.Payload!;
            user.UpdatedDate = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);

            var baseUrl = httpContextAccessor.GetRequestPath();
            return new UserProfileViewModel(user, baseUrl);
        }
        catch (Exception ex)
        {
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<bool>> DeleteProfilePhoto(long userId, CancellationToken cancellationToken)
    {
        try
        {
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id == userId && u.Status != EntityStatus.Deleted, cancellationToken);
                
            if (user == null)
                return new ErrorModel(ErrorEnum.UserNotFound);

            if (string.IsNullOrEmpty(user.ProfilePhotoPath))
                return new ErrorModel(ErrorEnum.ProfilePhotoNotFound);

            // Delete physical file
            await fileService.DeleteFileAsync(user.ProfilePhotoPath);

            // Remove path from database
            user.ProfilePhotoPath = null;
            user.UpdatedDate = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<bool>> ChangePassword(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId && u.Status != EntityStatus.Deleted, cancellationToken);
                
            if (user == null)
                return new ErrorModel(ErrorEnum.UserNotFound);

            // Verify old password
            if (!request.CurrentPassword.VerifyPassword(user.Password))
                return new ErrorModel(ErrorEnum.InvalidCurrentPassword);

            // Hash and update new password
            user.Password = request.NewPassword.HashPassword();
            user.UpdatedDate = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<bool>> DeactivateAccount(long userId, CancellationToken cancellationToken)
    {
        try
        {
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id == userId && u.Status != EntityStatus.Deleted, cancellationToken);
                
            if (user == null)
                return new ErrorModel(ErrorEnum.UserNotFound);

            user.Status = EntityStatus.Inactive;
            user.UpdatedDate = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<bool>> DeleteAccount(long userId, CancellationToken cancellationToken)
    {
        try
        {
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id == userId && u.Status != EntityStatus.Deleted, cancellationToken);
                
            if (user == null)
                return new ErrorModel(ErrorEnum.UserNotFound);

            // Soft delete
            user.Status = EntityStatus.Deleted;
            user.UpdatedDate = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);

            // Delete profile photo if exists
            if (!string.IsNullOrEmpty(user.ProfilePhotoPath))
            {
                await fileService.DeleteFileAsync(user.ProfilePhotoPath);
            }

            return true;
        }
        catch (Exception ex)
        {
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    #endregion

    private async Task<SignUpResult> GenerateTokenForUser(User user)
    {
        var token = await tokenService.GenerateToken(user.Adapt<GenerateTokenParams>());
        return token.Payload.Adapt<SignUpResult>();
    }
}