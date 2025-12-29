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
using Domain.Models.Integration.Sms;
using Infrastructure.Extensions;
using Infrastructure.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class UserService(
    ITokenService tokenService,
    EntityContext context,
    IHttpContextAccessor httpContextAccessor,
    IFileService fileService,
    ISmsService smsService,
    ILogger<UserService> logger) : IUserService
{
  
    public async Task<Result<CreateSessionResult>> CreateSession(CreateSessionRequest createSessionRequest)
    {
        var cleanedPhone= createSessionRequest.Phone.TrimStart('+', '0');
        var existingUser = await context.Users.FirstOrDefaultAsync(x => x.Phone == cleanedPhone);
            
        User user;
        if (existingUser == null)
        {
            user = createSessionRequest.Adapt<User>();
            user.Phone = cleanedPhone;
                
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();
        }
        else
        {
            user = existingUser;
            user.Status= EntityStatus.Active;
        }
            
        var newSession = user.Adapt<Session>();
            
        await context.Sessions.AddAsync(newSession);
        await context.SaveChangesAsync();
            
        //await smsService.SendSms((user, newSession).Adapt<SendSmsParams>());

        return newSession.Adapt<CreateSessionResult>();
    }

    public async Task<Result<VerifySessionResult>> VerifySession(VerifySessionRequest request)
    {
        try
        {
            var session = await context.Sessions
                .Include(s => s.User)
                .FirstOrDefaultAsync(x => x.Id == request.SessionId);
            
            if (session is null || session.OtpExpireDate < DateTime.UtcNow || session.IsVerified)
                return new ErrorModel(ErrorEnum.SessionNotFound);
            
            if (session.OtpCode != request.Code)
                return new ErrorModel(ErrorEnum.InvalidCode);
        
            var tokenResult = await tokenService.GenerateToken(session.User.Adapt<GenerateTokenParams>());
        
            if (!tokenResult.Success)
                return tokenResult.Error!;

            // Store refresh token in database
            var refreshToken = new RefreshToken
            {
                UserId = session.User.Id,
                Token = tokenResult.Payload.RefreshToken,
                ExpiresAt = tokenResult.Payload.RefreshTokenExpiry,
            };

            await context.RefreshTokens.AddAsync(refreshToken);
            context.Sessions.Remove(session);
            await context.SaveChangesAsync();
        
            return tokenResult.Payload.Adapt<VerifySessionResult>();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error verifying session {SessionId}", request.SessionId);
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<RefreshTokenResult>> RefreshToken(RefreshTokenRequest request)
    {
        try
        {
            var refreshToken = await context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

            if (refreshToken is null)
                return new ErrorModel(ErrorEnum.InvalidRefreshToken);

            if (refreshToken.ExpiresAt < DateTime.UtcNow)
                return new ErrorModel(ErrorEnum.RefreshTokenExpired);

            // Generate new tokens
            var tokenResult = await tokenService.GenerateToken(refreshToken.User.Adapt<GenerateTokenParams>());
            
            if (!tokenResult.Success)
                return tokenResult.Error!;

            context.Remove(refreshToken);

            // Store new refresh token
            var newRefreshToken = new RefreshToken
            {
                UserId = refreshToken.UserId,
                Token = tokenResult.Payload!.RefreshToken,
                ExpiresAt = tokenResult.Payload.RefreshTokenExpiry,
            };

            await context.RefreshTokens.AddAsync(newRefreshToken);
            await context.SaveChangesAsync();

            return tokenResult.Payload.Adapt<RefreshTokenResult>();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error refreshing token");
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
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