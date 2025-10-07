using Application.Interfaces;
using DataAccess;
using DataAccess.Schemas.Auth;
using Domain.Enums;
using Domain.Models.API.Requests;
using Domain.Models.API.Results;
using Domain.Models.Common;
using Domain.Models.Infrastructure.Params;
using Infrastructure.Interfaces;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class UserService(
    ITokenService tokenService,
    EntityContext context) : IUserService
{
    public async Task<Result<SignUpResult>> SignUp(SignUpRequest signUpRequest)
    {
        var isPhoneNotUnique = await context.Users.AnyAsync(x => x.Phone == signUpRequest.Phone);
        if (isPhoneNotUnique)
            return new ErrorModel(ErrorEnum.PhoneAlreadyExists);

        var newUser = signUpRequest.Adapt<User>();
        await context.Users.AddAsync(newUser);
        await context.SaveChangesAsync();

        return await GenerateTokenForUser(newUser);
    }

    public async Task<Result<SignUpResult>> SignIn(SignInRequest request)
    {
        var user = await context.Users.FirstOrDefaultAsync(x => x.Phone== request.Phone && x.Password == request.Password);
        if(user == null)
            return new ErrorModel(ErrorEnum.UserNotFound);
        return await GenerateTokenForUser(user);
    }

    async private Task<SignUpResult>
        GenerateTokenForUser(User user)
    {
        var token = await tokenService.GenerateToken(user.Adapt<GenerateTokenParams>());
        return token.Payload.Adapt<SignUpResult>();
    }
}