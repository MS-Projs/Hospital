using Administration.Helpers;
using Application.Interfaces;
using Domain.Models.API.Requests;
using Domain.Models.API.Results;
using Domain.Models.Common;
using Microsoft.AspNetCore.Mvc;
using SignInResult = Domain.Models.API.Results.SignInResult;

namespace Administration.Controllers;

[ApiController]
[Route("api/v1/[controller]/[action]")]
public class AuthorizationController(
    IUserService userService) : MyController<AuthorizationController>
{
    [HttpPost]
    public async Task<Result<SignUpResult>>
        SignUp(SignUpRequest request) =>
        await userService.SignUp(request);

    [HttpPost]
    public async Task<Result<SignInResult>> 
        SignIn(SignInRequest request) =>
        await userService.SignIn(request);
}