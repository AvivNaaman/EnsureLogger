using Ensure.Domain;
using Ensure.Domain.Enums;
using Ensure.Web.Data;
using Ensure.Web.Helpers;
using Ensure.Web.Models;
using Ensure.Web.Security;
using Ensure.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Ensure.Web.Controllers
{
    /// <summary>
    /// The Web UI Controller
    /// </summary>
    [Authorize(AuthenticationSchemes = SessionAuthConstants.Scheme)]
    [Route("/")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class HomeController : Controller
    {
        private readonly IEnsureService _ensureService;
        private readonly IAppUsersService usersService;

        public HomeController(IEnsureService ensureService, IAppUsersService usersService)
        {
            _ensureService = ensureService;
            this.usersService = usersService;
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
            // try parse, on failue just get today.
            DateTime d = date.FastParseFormattedDate() ?? DateTime.UtcNow;

            var currDayLogs = await _ensureService.GetLogsByDay(User.GetId(), d.Date);

            var u = await usersService.FindByIdReadonlyAsync(User.GetId());

            var vm = new HomeViewModel()
            {
                Logs = currDayLogs,
                CurrentDate = d.Date,
                // current day progress - just count
                UserDailyProgress = currDayLogs.Count,
                UserDailyTarget = u.DailyTarget,
            };
            return View(vm);
        }

        /// <summary>
        /// User Error Page
        /// </summary>
        /// <returns></returns>
        [Route("Error")]
        [AllowAnonymous]
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
            // wait until operation is finished before letting the user continue.
            // discar (_ = ) to prevent unwanted messages from the compiler/analyzer
            _ = await _ensureService.LogAsync(User.GetId(), taste);
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
            if (l == null || l.UserId != User.GetId())
            {
                return BadRequest();
            }
            string date = l.Logged.ToString(EnsureConstants.DateTimeUrlFormat);
            await _ensureService.RemoveLogAsync(l);
            return RedirectToAction("Logs", new { date });
        }
        #endregion
    }
}
