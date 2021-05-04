﻿using System;
using Microsoft.AspNetCore.Authentication;

namespace Ensure.Web.Security
{

    public class SessionAuthOptions : AuthenticationSchemeOptions
    {
        public string CookieName { get; set; } = SessionAuthConstants.DefaultCookieName;

        public byte[] KeyBytes { get; set; }
        public string LoginPath { get; set; } = "/Account/Login";
        public string ReturnUrlParameter { get; set; } = "returnUrl";
    }

}
