using System.Threading.Tasks;
using Ensure.Web.Data;
using Ensure.Web.Models;
using Ensure.Web.Security;
using Ensure.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ensure.Web.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)] // We don't want mapping of MVC controllers (returning HTML) to API doc!
    public class AccountController : Controller
    {
        private readonly IAppUsersService _appUsersService;
        private readonly ISigninService _signinService;

        public AccountController(IAppUsersService appUsersService, ISigninService signinService)
        {
            _appUsersService = appUsersService;
            _signinService = signinService;
        }

        /* Login: GET & POST */
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login() => View();

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var password = model.Password;
            model.Password = string.Empty;
            if (!ModelState.IsValid) return View(model);
            // try login. on failure return with message, otherwise redirect to main page.
            var user = await _appUsersService.FindByNameAsync(model.UserName);
            if (user is not null)
            {
                var res = _signinService.SessionPasswordLogin(user, password);
                if (res)
                {
                    return RedirectToAction("Logs", "Home");
                }
            }
            ModelState.AddModelError("", "Authentication failed.");
            return View(model);
        }

        /* Logout */
        [Authorize(AuthenticationSchemes = SessionAuthConstants.Scheme)]
        [AcceptVerbs("GET", "POST")]
        public IActionResult Logout()
        {
            _signinService.SessionLogout();
            return RedirectToAction("Logs", "Home");
        }

        /* Access Denied */
        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied() => View();

        /* Signup: GET & POST */
        [HttpGet]
        [AllowAnonymous]
        public IActionResult SignUp() => User.Identity.IsAuthenticated ? Redirect("View") : View();

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> SignUp(SignUpViewModel model)
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Logs", "Home");
            if (!ModelState.IsValid) return View(model);
            AppUser u = new()
            {
                Email = model.Email,
                UserName = model.UserName,
                DailyTarget = model.DailyTarget
            };

            var res = await _appUsersService.CreateAsync(u, model.Password);
            if (res.Succeeded)
            {
                _signinService.SessionLogin(u);
                return RedirectToAction("Logs", "Home");
            }
            else
            {
                res.Errors.ForEach(e => ModelState.AddModelError("", e));
                return View(model);
            }
        }

        /* Profile */
        /// <summary>
        /// User's information page
        /// </summary>
        [Authorize(AuthenticationSchemes = SessionAuthConstants.Scheme)]
        [HttpGet]
        public async Task<IActionResult> Profile() => View(await _appUsersService.FindByNameAsync(User.Identity.Name));

        /// <summary>
        /// Updates the user's target
        /// </summary>
        /// <param name="dailyTarget">The new target</param>
        [HttpPost]
        [Authorize(AuthenticationSchemes = SessionAuthConstants.Scheme)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTarget([FromForm] int dailyTarget, bool returnHome)
        {
            var res = await _appUsersService.SetUserTarget(dailyTarget, User.Identity.Name);
            if (!res.Succeeded)
            {
                res.Errors.ForEach(e => ModelState.AddModelError("", e));
            }

            if (returnHome)
                return RedirectToAction("Logs", "Home");
            else
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
            var u = await _appUsersService.FindByEmailAsync(email);

            if (u is not null)
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
            var u = await _appUsersService.FindByEmailAsync(email);
            return u is null ? // if user not found, return not found
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
            var u = await _appUsersService.FindByNameAsync(model.UserName);
            if (u is null) return NotFound();
            var res = await _appUsersService.ResetPasswordAsync(u, model.Token, model.NewPassword);
            if (!res.Succeeded)
            {
                res.Errors.ForEach(e => ModelState.AddModelError("", e));
                model.NewPassword = model.NewPasswordVertification = string.Empty;
                return View(model);
            }
            return RedirectToAction("Logs", "Home");
        }
        #endregion
    }
}
