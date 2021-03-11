using Ensure.Domain.Models;
using Ensure.Web.Data;
using Ensure.Web.Helpers;
using Ensure.Web.Models;
using Ensure.Web.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
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
        private readonly UserManager<AppUser> _userManager;
        private readonly IAppUsersService _appUsersService;

        public AccountApiController(UserManager<AppUser> userManager
            , IAppUsersService appUsersService)
        {
            _userManager = userManager;
            _appUsersService = appUsersService;
        }

        [HttpGet]
        [Route("GetInfo")]
        public async Task<ActionResult<ApiUserInfo>> GetInfo()
        {
            return await _appUsersService.GetUserInfo(User.GetId(), null);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("Login")]
        public async Task<ActionResult<ApiResponse<ApiUserInfo>>> Login(string username, string password)
        {
            var u = await _userManager.FindByNameAsync(username);
            if (await _userManager.CheckPasswordAsync(u, password))
            {
                string jwtToken = _appUsersService.GenerateBearerToken(u);
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
            var res = await _userManager.CreateAsync(u, model.Password);
            if (res.Succeeded)
            {
                return await Login(model.UserName, model.Password);
            }
            else return BadRequest(new ApiResponse<ApiUserInfo>(string.Join(" \n", res.Errors.Select(ie => ie.Description))));
        }

        [Route("ResetPassword")]
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult> ResetPassword(string email)
        {
            var link = $"{Request.Scheme}://{Request.Host}{Request.PathBase}"
                   + "/Account/ResetPasswordFinish";
            var u = await _userManager.FindByEmailAsync(email);
            await _appUsersService.SendPasswordResetEmail(u, link);
            return Ok();
        }
    }
}
