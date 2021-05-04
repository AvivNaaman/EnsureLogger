using Ensure.Domain.Models;
using Ensure.Web.Data;
using Ensure.Web.Helpers;
using Ensure.Web.Models;
using Ensure.Web.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Ensure.Web.Controllers
{
    [Route("api/Account")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // Bearer - Web API Auth
    [ApiController]
    public class AccountApiController : ControllerBase
    {
        private readonly IAppUsersService _appUsersService;
        private readonly ISigninService _signinService;

        public AccountApiController(IAppUsersService appUsersService, ISigninService signinService)
        {
            _appUsersService = appUsersService;
            _signinService = signinService;
        }

        [HttpGet]
        [Route("GetInfo")]
        public async Task<ActionResult<ApiUserInfo>> GetInfo()
        {
            var info = await _appUsersService.GetUserInfo(User.GetId(), null);
            return info is null ? StatusCode((int)HttpStatusCode.Unauthorized) : info;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("Login")]
        public async Task<ActionResult<ApiResponse<ApiUserInfo>>> Login(string username, string password)
        {
            var u = await _appUsersService.FindByNameAsync(username);
            if (u is not null && _appUsersService.CheckPassword(u, password))
            {
                string jwtToken = _signinService.GenerateApiLoginToken(u);
                return new ApiResponse<ApiUserInfo>(_appUsersService.GetUserInfo(u, jwtToken));
            }
            else
            {
                return BadRequest(new ApiResponse<ApiUserInfo>("User Name and Password do not match any existing user."));
            }
        }

        [Obsolete("Should be replaced by Get/SetInfo")]
        [Route("GetTarget")]
        [HttpGet]
        public async Task<ActionResult<int>> GetTarget()
        {
            return await _appUsersService.GetUserTarget(User.Identity.Name);
        }

        [Route("SetTarget")]
        [HttpPost]
        public async Task<ActionResult<ApiResponse>> SetTarget(int target)
        {
            await _appUsersService.SetUserTarget(target, User.Identity.Name);
            return new ApiResponse();
        }

        [Route("Register")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<ApiUserInfo>>> Register(SignUpViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            AppUser u = new()
            {
                UserName = model.UserName,
                Email = model.Email,
                DailyTarget = model.DailyTarget
            };
            var res = await _appUsersService.CreateAsync(u, model.Password);
            if (res)
            {
                return await Login(model.UserName, model.Password);
            }
            else return BadRequest(new ApiResponse<ApiUserInfo>(string.Join(" \n", res/*.Errors.Select(ie => ie.Description)*/)));
        }

        [Route("ResetPassword")]
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult> ResetPassword(string email)
        {
            var link = $"{Request.Scheme}://{Request.Host}{Request.PathBase}"
                   + "/Account/ResetPasswordFinish";
            var u = await _appUsersService.FindByEmailAsync(email);
            await _appUsersService.SendPasswordResetEmail(u, link);
            return Ok();
        }
    }
}
