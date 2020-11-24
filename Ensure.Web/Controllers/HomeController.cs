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
	[Authorize()]
	[Route("/")]
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly SignInManager<AppUser> _signInManager;
		private readonly IEnsureService _ensureService;
		private readonly ITimeService _timeService;
		private readonly UserManager<AppUser> _userManager;

		public HomeController(ILogger<HomeController> logger, SignInManager<AppUser> signInManager,
			IEnsureService ensureService, ITimeService timeService, UserManager<AppUser> userManager)
		{
			_logger = logger;
			_signInManager = signInManager;
			_ensureService = ensureService;
			_timeService = timeService;
			_userManager = userManager;
		}

		[ResponseCache(Duration = int.MaxValue, Location = ResponseCacheLocation.Any)]
		[Route("/")]
		public IActionResult Index() => RedirectToAction("Logs");

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

		[HttpGet]
		[Route("Logout")]
		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();
			return RedirectToAction("Logs");
		}

		[Route("Error")]
		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

		// Add via form
		[Route("Add")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Add([FromForm] EnsureTaste taste)
		{
			await _ensureService.LogAsync(User.Identity.Name, taste);
			return RedirectToAction("Logs");
		}

		// Add via form
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

		// TODO: Implement UI
		[Route("Profile")]
		[HttpGet]
		public async Task<IActionResult> Profile() => View(await _userManager.FindByNameAsync(User.Identity.Name));

		// TODO: Implement UI
		[HttpGet]
		[AllowAnonymous]
		[Route("SignUp")]
		public IActionResult SignUp() => View();

		// TODO: Implement Logic
		[HttpPost]
		[AllowAnonymous]
		[Route("SignUp")]
		public IActionResult SignUp(SignUpViewModel model)
		{
			if (!ModelState.IsValid) return View(model);
			return Redirect("");
		}
	}
}
