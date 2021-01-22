using Ensure.Domain;
using Ensure.Domain.Enums;
using Ensure.Web.Data;
using Ensure.Web.Models;
using Ensure.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ensure.Web.Controllers
{
	/// <summary>
    /// The Web UI Controller
    /// </summary>
	[Authorize]
	[Route("/")]
	public class HomeController : Controller
	{
		private readonly SignInManager<AppUser> _signInManager;
		private readonly IEnsureService _ensureService;
		private readonly UserManager<AppUser> _userManager;

		public HomeController(SignInManager<AppUser> signInManager,
			IEnsureService ensureService, UserManager<AppUser> userManager)
		{
			_signInManager = signInManager;
			_ensureService = ensureService;
			_userManager = userManager;
		}

		/* Redirect App base (~/) to Logs (~/Logs). Cahce for performance */
		[ResponseCache(Duration = int.MaxValue, Location = ResponseCacheLocation.Any)]
		[Route("/")]
		public IActionResult Index() => RedirectToAction("Logs");

		/// <summary>
        /// Main Action - Displays date's logs
        /// </summary>
        /// <param name="date">The date to display</param>
		[Route("Logs/{date?}")]
		public async Task<IActionResult> Logs(string date)
		{
			var u = await _userManager.FindByNameAsync(User.Identity.Name);
			int userTimeZone = u.TimeZone;
			// user's time
			DateTime d = DateTime.UtcNow.Add(TimeSpan.FromHours(userTimeZone));
			try
			{
				d = DateTime.ParseExact(date, EnsureConstants.DateTimeUrlFormat, CultureInfo.InvariantCulture);
			}
			catch { }

			var currDayLogs = await _ensureService.GetUserDayLogsAsync(User.Identity.Name,
					d.Date.Subtract(TimeSpan.FromHours(userTimeZone)));

			currDayLogs.ForEach(l => l.Logged = l.Logged.AddHours(userTimeZone));

			var todayCount = d == DateTime.UtcNow.Add(TimeSpan.FromHours(userTimeZone)) ? currDayLogs.Count : await _ensureService.GetDayCountAsync(User.Identity.Name,
					DateTime.UtcNow.Date.Subtract(TimeSpan.FromHours(userTimeZone)));

			var vm = new HomeViewModel()
			{
				Logs = currDayLogs,
				CurrentDate = d.Date,
				// current day progress - just count
				UserDailyProgress = todayCount,
				UserDailyTarget = u.DailyTarget,
			};
			return View(vm);
		}

		/// <summary>
        /// User Error Page
        /// </summary>
        /// <returns></returns>
		[Route("Error")]
		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

        #region EnsuresCD
        /// <summary>
        /// Add Ensure Log
        /// </summary>
        /// <param name="taste">The taste</param>
        [Route("Add")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Add([FromForm] EnsureTaste taste)
		{
			await _ensureService.LogAsync(User.Identity.Name, taste);
			return RedirectToAction("Logs");
		}

		/// <summary>
        /// Remove Ensure Log
        /// </summary>
        /// <param name="id">The log identifier</param>
		[Route("Delete/{id}")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(string id)
		{
			var l = await _ensureService.FindByIdAsync(id);
			if (l == null || l.UserId != User.FindFirst(ClaimTypes.NameIdentifier).Value)
			{
				return BadRequest();
			}
			string date = l.Logged.ToString(EnsureConstants.DateTimeUrlFormat);
			await _ensureService.RemoveLogAsync(l);
			return RedirectToAction("Logs", new { date });
		}
        #endregion

        #region UserAndProfile
        /* Login: GET & POST */
        [HttpGet]
		[AllowAnonymous]
		[Route("Login")]
		public IActionResult Login() => View();

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		[Route("Login")]
		public async Task<IActionResult> Login(LoginViewModel model)
		{
			if (!ModelState.IsValid) return View(model);
			var res = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, false, false);
			if (!res.Succeeded)
			{
				if (!res.IsLockedOut) ModelState.AddModelError("", "Authentication failed.");
			}
			return RedirectToAction("Logs");
		}

		/* Logout */
		[AcceptVerbs("GET", "POST")]
		[Route("Logout")]
		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();
			return RedirectToAction("Logs");
		}

		/* Signup: GET & POST */
		[HttpGet]
		[AllowAnonymous]
		[Route("SignUp")]
		public IActionResult SignUp() => User.Identity.IsAuthenticated ? Redirect("View") : View();

		[HttpPost]
		[AllowAnonymous]
		[Route("SignUp")]
		public async Task<IActionResult> SignUp(SignUpViewModel model)
		{
			if (User.Identity.IsAuthenticated) return Redirect("~/");
			if (!ModelState.IsValid) return View(model);
			AppUser u = new()
			{
				Email = model.Email,
				UserName = model.UserName,
				DailyTarget = model.DailyTarget,
				TimeZone = model.TimeZone,
			};
			var res = await _userManager.CreateAsync(u, model.Password);
			if (res.Succeeded)
            {
				await _signInManager.SignInAsync(u, false);
				return Redirect("~/");
            }
			else
            {
                foreach (var err in res.Errors)
                {
					ModelState.AddModelError("", err.Description);
                }
				return View(model);
            }
		}

		/* Profile */
		/// <summary>
        /// User's information page
        /// </summary>
		[Route("Profile")]
		[HttpGet]
		public async Task<IActionResult> Profile() => View(await _userManager.FindByNameAsync(User.Identity.Name));

		/// <summary>
        /// Updates the user's target
        /// </summary>
        /// <param name="dailyTarget">The new target</param>
		[HttpPost]
		[Route("Profile/UpdateTarget")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdateTarget([FromForm] short dailyTarget)
		{
			if (dailyTarget > 0)
			{
				var u = await _userManager.FindByNameAsync(User.Identity.Name);
				u.DailyTarget = dailyTarget;
				await _userManager.UpdateAsync(u);
				return RedirectToAction("Profile");
			}
			return RedirectToAction("Profile");
		}


		#endregion

	}
}
