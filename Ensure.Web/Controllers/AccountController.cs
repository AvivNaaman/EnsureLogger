using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ensure.Web.Data;
using Ensure.Web.Models;
using Ensure.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Ensure.Web.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AccountController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IAppUsersService _appUsersService;
        private readonly IEnsureService _ensureService;
        private readonly UserManager<AppUser> _userManager;

        public AccountController(SignInManager<AppUser> signInManager, IAppUsersService appUsersService,
            IEnsureService ensureService, UserManager<AppUser> userManager)
        {
            _signInManager = signInManager;
            _appUsersService = appUsersService;
            _ensureService = ensureService;
            _userManager = userManager;
        }

        /* Login: GET & POST */
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            // try login. on failure return with message, otherwise redirect to main page.
            var res = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, false, false);
            if (!res.Succeeded)
            {
                ModelState.AddModelError("", "Authentication failed.");
                return View(model);
            }
            return RedirectToAction("Logs", "Home");
        }

        /* Logout */
        [Authorize]
        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Logs", "Home");
        }

        /* Signup: GET & POST */
        [HttpGet]
        public IActionResult SignUp() => User.Identity.IsAuthenticated ? Redirect("View") : View();

        [HttpPost]
        public async Task<IActionResult> SignUp(SignUpViewModel model)
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Logs", "Home");
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
                return RedirectToAction("Logs", "Home");
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
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile() => View(await _userManager.FindByNameAsync(User.Identity.Name));

        /// <summary>
        /// Updates the user's target
        /// </summary>
        /// <param name="dailyTarget">The new target</param>
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTarget([FromForm] int dailyTarget)
        {
            if (dailyTarget > 0)
            {
                await _appUsersService.SetUserTarget(dailyTarget, User.Identity.Name);
                return RedirectToAction(nameof(Profile));
            }
            return RedirectToAction(nameof(Profile));
        }

        #region PasswordReset
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword() => View();

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(string email)
        {
            var link = $"{Request.Scheme}://{Request.Host}{Request.PathBase}"
                + "/Account/ResetPasswordFinish";
            var u = await _userManager.FindByEmailAsync(email);
            await _appUsersService.SendPasswordResetEmail(u, link);
            return RedirectToAction(nameof(ResetPasswordSent));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordSent() => View();

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPasswordFinish(string email, string token)
        {
            var u = await _userManager.FindByEmailAsync(email);
            return  u is null ? // if user not found, return not found
                NotFound() : // otherwise, return the page with the info
                View(new PasswordResetViewModel { UserName = u.UserName, Token = token });
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPasswordFinish(PasswordResetViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.NewPassword = model.NewPasswordVertification = string.Empty;
                return View(model);
            }
            var u = await _userManager.FindByNameAsync(model.UserName);
            if (u is null) return NotFound();
            var res = await _userManager.ResetPasswordAsync(u, model.Token, model.NewPassword);
            if (!res.Succeeded)
            {
                res.Errors.ToList().ForEach(e => ModelState.AddModelError("", e.Description));
                model.NewPassword = model.NewPasswordVertification = string.Empty;
                return View(model);
            }
            return RedirectToAction("Logs", "Home");
        }
        #endregion
    }
}
