﻿using Ensure.Domain.Models;
using Ensure.Web.Data;
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
	[ApiController]
	public class AccountApiController : ControllerBase
	{
		private readonly UserManager<AppUser> _userManager;
		private readonly SignInManager<AppUser> _signInManager;
		private readonly IConfiguration config;

		public AccountApiController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IConfiguration config)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			this.config = config;
		}

		[HttpPost]
		[HttpGet]
		[Route("Login")]
		public async Task<ActionResult<ApiUserInfo>> Login(string username, string password)
		{
			var u = await _userManager.FindByNameAsync(username);
			if (await _userManager.CheckPasswordAsync(u, password))
			{
				var handler = new JwtSecurityTokenHandler();
				var key = Encoding.UTF8.GetBytes(config["Jwt:SecretKey"]);
				var tokenDescriptor = new JwtSecurityToken(null, null, claims: new List<Claim>()
					{
						new Claim(ClaimTypes.NameIdentifier, u.Id),
						new Claim(ClaimTypes.Name, u.UserName),
						new Claim(ClaimTypes.Email, u.Email),
				},
				expires: DateTime.UtcNow.AddYears(1),
				signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature));
				string jwtToken = (new JwtSecurityTokenHandler()).WriteToken(tokenDescriptor);
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

		[AllowAnonymous]
		[Route("CreateUserWithoutValidation")]
		public async Task<ActionResult<IdentityResult>> CreateUserWithoutValidation(string username, string pwd, string email)
		{
			return await _userManager.CreateAsync(new AppUser
			{
				UserName = username,
				Email = email
			}, pwd);
		}

		[Route("GetTarget")]
		[HttpGet]
		public async Task<ActionResult<short>> GetTarget()
		{
			return (await _userManager.FindByNameAsync(User.Identity.Name)).DailyTarget;
		}
	}
}
