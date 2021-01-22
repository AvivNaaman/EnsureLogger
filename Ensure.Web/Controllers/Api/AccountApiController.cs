using Ensure.Domain.Models;
using Ensure.Web.Data;
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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    public class AccountApiController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IAppUsersService appUsersService;

        public AccountApiController(UserManager<AppUser> userManager
            , IAppUsersService appUsersService)
        {
            _userManager = userManager;
            this.appUsersService = appUsersService;
        }

        [AcceptVerbs("POST", "GET")]
        [AllowAnonymous]
        [Route("Login")]
        public async Task<ActionResult<ApiUserInfo>> Login(string username, string password)
        {
            var u = await _userManager.FindByNameAsync(username);
            if (await _userManager.CheckPasswordAsync(u, password))
            {
                string jwtToken = appUsersService.GenerateBearerToken(u);
                var info = new ApiUserInfo()
                {
                    Email = u.Email,
                    UserName = u.UserName,
                    DailyTarget = u.DailyTarget,
                    JwtToken = jwtToken,
                };
                return info;
            }
            else
            {
                return BadRequest();
            }
        }

        [Route("GetTarget")]
        [HttpGet]
        public async Task<ActionResult<int>> GetTarget()
        {
            return await appUsersService.GetUserTarget(User.Identity.Name);
        }

        [Route("SetTarget")]
        [HttpPost]
        public async Task<ActionResult> SetTarget(short target)
        {
            await appUsersService.SetUserTarget(target, User.Identity.Name);
            return Ok();
        }

        [Route("Register")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<ApiUserInfo>> Register(SignUpViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            AppUser u = new()
            {
                UserName = model.UserName,
                Email = model.Email,
                DailyTarget = model.DailyTarget,
                TimeZone = model.TimeZone
            };
            var res = await _userManager.CreateAsync(u, model.Password);
            if (res.Succeeded)
            {
                return await Login(model.UserName, model.Password);
            }
            else return BadRequest();
        }
    }
}
