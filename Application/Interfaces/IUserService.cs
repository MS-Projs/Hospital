using Domain.Models.API.Requests;
using Domain.Models.API.Results;
using Domain.Models.Common;

namespace Application.Interfaces;

public interface IUserService
{
    Task<Result<SignUpResult>> SignUp(SignUpRequest signUpRequest);
    Task<Result<SignUpResult>> SignIn(SignInRequest request);
}