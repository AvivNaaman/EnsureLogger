using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Ensure.Web.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Ensure.Web.Areas.Admin.Controllers
{
    [Authorize(AuthenticationSchemes = SessionAuthConstants.Scheme)]
    public class UsersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}