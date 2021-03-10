using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ensure.Web.Areas.Admin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Ensure.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IAdminService adminService;

        public HomeController(IAdminService adminService)
        {
            this.adminService = adminService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await adminService.GetStatsForHome());
        }
    }
}
