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
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class UserService : IUserService
{
    private readonly ITokenService _tokenService;
    private readonly EntityContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IFileService _fileService;
    private readonly ISmsService _smsService;
    private readonly ILogger<UserService> _logger;

    public UserService(
        ITokenService tokenService,
        EntityContext context,
        IHttpContextAccessor httpContextAccessor,
        IFileService fileService,
        ISmsService smsService,
        ILogger<UserService> logger)
    {
        _tokenService = tokenService;
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _fileService = fileService;
        _smsService = smsService;
        _logger = logger;
    }

    public async Task<Result<CreateSessionResult>> CreateSession(CreateSessionRequest createSessionRequest)
    {
        try
        {
            var cleanedPhone = createSessionRequest.Phone.TrimStart('+', '0');

            var existingUser = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Phone == cleanedPhone);

            User user;
            if (existingUser == null)
            {
                user = createSessionRequest.Adapt<User>();
                user.Phone = cleanedPhone;
                user.Status = EntityStatus.Active;
                user.CreatedDate = DateTime.UtcNow;

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
            }
            else
            {
                user = await _context.Users.FirstAsync(u => u.Id == existingUser.Id);
                user.Status = EntityStatus.Active;
                await _context.SaveChangesAsync();
            }

            var newSession = user.Adapt<Session>();

            await _context.Sessions.AddAsync(newSession);
            await _context.SaveChangesAsync();

            // Send SMS (commented for development)
            // await _smsService.SendSms((user, newSession).Adapt<SendSmsParams>());

            return newSession.Adapt<CreateSessionResult>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating session for phone {Phone}", createSessionRequest.Phone);
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<VerifySessionResult>> VerifySession(VerifySessionRequest request)
    {
        try
        {
            var session = await _context.Sessions
                .Include(s => s.User)
                .FirstOrDefaultAsync(x => x.Id == request.SessionId);

            if (session == null || session.OtpExpireDate < DateTime.UtcNow || session.IsVerified)
                return new ErrorModel(ErrorEnum.SessionNotFound);

            if (session.OtpCode != request.Code)
                return new ErrorModel(ErrorEnum.InvalidCode);

            var tokenResult = await _tokenService.GenerateToken(session.User!.Adapt<GenerateTokenParams>());

            if (!tokenResult.Success)
                return tokenResult.Error!;

            // Store refresh token
            var refreshToken = new RefreshToken
            {
                UserId = session.User.Id,
                Token = tokenResult.Payload!.RefreshToken,
                ExpiresAt = tokenResult.Payload.RefreshTokenExpiry,
                CreatedDate = DateTime.UtcNow,
                Status = EntityStatus.Active
            };

            await _context.RefreshTokens.AddAsync(refreshToken);
            _context.Sessions.Remove(session);
            await _context.SaveChangesAsync();

            return tokenResult.Payload.Adapt<VerifySessionResult>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying session {SessionId}", request.SessionId);
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<RefreshTokenResult>> RefreshToken(RefreshTokenRequest request)
    {
        try
        {
            var refreshToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

            if (refreshToken == null)
                return new ErrorModel(ErrorEnum.InvalidRefreshToken);

            if (refreshToken.ExpiresAt < DateTime.UtcNow)
                return new ErrorModel(ErrorEnum.RefreshTokenExpired);

            // Generate new tokens
            var tokenResult = await _tokenService.GenerateToken(refreshToken.User.Adapt<GenerateTokenParams>());

            if (!tokenResult.Success)
                return tokenResult.Error!;

            _context.Remove(refreshToken);

            // Store new refresh token
            var newRefreshToken = new RefreshToken
            {
                UserId = refreshToken.UserId,
                Token = tokenResult.Payload!.RefreshToken,
                ExpiresAt = tokenResult.Payload.RefreshTokenExpiry,
                CreatedDate = DateTime.UtcNow,
                Status = EntityStatus.Active
            };

            await _context.RefreshTokens.AddAsync(newRefreshToken);
            await _context.SaveChangesAsync();

            return tokenResult.Payload.Adapt<RefreshTokenResult>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    #region Profile Management

    public async Task<Result<UserProfileViewModel>> GetProfile(
        long userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId && u.Status != EntityStatus.Deleted,
                    cancellationToken);

            if (user == null)
                return new ErrorModel(ErrorEnum.UserNotFound);

            var baseUrl = _httpContextAccessor.GetRequestPath();
            return new UserProfileViewModel(user, baseUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting profile for user {UserId}", userId);
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<UserProfileViewModel>> UpdateProfile(
        UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId && u.Status != EntityStatus.Deleted,
                    cancellationToken);

            if (user == null)
                return new ErrorModel(ErrorEnum.UserNotFound);

            // Check phone uniqueness if changed
            if (!string.IsNullOrWhiteSpace(request.Phone) && request.Phone != user.Phone)
            {
                var phoneExists = await _context.Users
                    .AsNoTracking()
                    .AnyAsync(u => u.Phone == request.Phone && u.Id != request.UserId,
                        cancellationToken);

                if (phoneExists)
                    return new ErrorModel(ErrorEnum.PhoneAlreadyExists);

                user.Phone = request.Phone;
            }

            // Update only provided fields
            if (!string.IsNullOrWhiteSpace(request.FirstName))
                user.FirstName = request.FirstName.Trim();

            if (!string.IsNullOrWhiteSpace(request.LastName))
                user.LastName = request.LastName.Trim();

            if (!string.IsNullOrWhiteSpace(request.Email))
                user.Email = request.Email.Trim();

            if (request.DateOfBirth.HasValue)
                user.DateOfBirth = request.DateOfBirth;

            if (request.Gender.HasValue)
                user.Gender = request.Gender;

            if (!string.IsNullOrWhiteSpace(request.Address))
                user.Address = request.Address.Trim();

            user.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            var baseUrl = _httpContextAccessor.GetRequestPath();
            return new UserProfileViewModel(user, baseUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile for user {UserId}", request.UserId);
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<UserProfileViewModel>> UploadProfilePhoto(
        UploadProfilePhotoRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId && u.Status != EntityStatus.Deleted,
                    cancellationToken);

            if (user == null)
                return new ErrorModel(ErrorEnum.UserNotFound);

            // Delete old photo if exists
            if (!string.IsNullOrEmpty(user.ProfilePhotoPath))
            {
                await _fileService.DeleteFileAsync(user.ProfilePhotoPath);
            }

            // Save new photo
            var fileResult = await _fileService.SaveFileAsync(request.Photo, "profile-photos");
            if (!fileResult.Success)
                return fileResult.Error!;

            user.ProfilePhotoPath = fileResult.Payload!;
            user.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            var baseUrl = _httpContextAccessor.GetRequestPath();
            return new UserProfileViewModel(user, baseUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading profile photo for user {UserId}", request.UserId);
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<bool>> DeleteProfilePhoto(
        long userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId && u.Status != EntityStatus.Deleted,
                    cancellationToken);

            if (user == null)
                return new ErrorModel(ErrorEnum.UserNotFound);

            if (string.IsNullOrEmpty(user.ProfilePhotoPath))
                return new ErrorModel(ErrorEnum.ProfilePhotoNotFound);

            // Delete physical file
            await _fileService.DeleteFileAsync(user.ProfilePhotoPath);

            // Remove path from database
            user.ProfilePhotoPath = null;
            user.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting profile photo for user {UserId}", userId);
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<bool>> ChangePassword(
        ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId && u.Status != EntityStatus.Deleted,
                    cancellationToken);

            if (user == null)
                return new ErrorModel(ErrorEnum.UserNotFound);

            // Verify old password
            if (string.IsNullOrEmpty(user.Password) ||
                !request.CurrentPassword.VerifyPassword(user.Password))
                return new ErrorModel(ErrorEnum.InvalidCurrentPassword);

            // Hash and update new password
            user.Password = request.NewPassword.HashPassword();
            user.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user {UserId}", request.UserId);
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<bool>> DeactivateAccount(
        long userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId && u.Status != EntityStatus.Deleted,
                    cancellationToken);

            if (user == null)
                return new ErrorModel(ErrorEnum.UserNotFound);

            user.Status = EntityStatus.Inactive;
            user.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating account for user {UserId}", userId);
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<bool>> DeleteAccount(
        long userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId && u.Status != EntityStatus.Deleted,
                    cancellationToken);

            if (user == null)
                return new ErrorModel(ErrorEnum.UserNotFound);

            // Soft delete
            user.Status = EntityStatus.Deleted;
            user.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            // Delete profile photo if exists (background operation)
            if (!string.IsNullOrEmpty(user.ProfilePhotoPath))
            {
                _ = Task.Run(() => _fileService.DeleteFileAsync(user.ProfilePhotoPath), CancellationToken.None);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting account for user {UserId}", userId);
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    #endregion
}