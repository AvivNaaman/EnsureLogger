using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Ensure.Web.Models;
using Ensure.Web.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ensure.Web.Security
{

    public class SessionAuthHandler : AuthenticationHandler<SessionAuthOptions>
    {
        private readonly HttpContext _httpContext;

        public SessionAuthHandler(IOptionsMonitor<SessionAuthOptions> options,
            ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock,
            IHttpContextAccessor httpContextAccessor) :
            base(options, logger, encoder, clock)
        {
            _httpContext = httpContextAccessor.HttpContext;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // get auth cookie value
            var cookie = _httpContext.Request.Cookies[Options.CookieName];

            if (cookie is null or "") return Task.FromResult(AuthenticateResult.Fail(new ArgumentNullException()));

            // split token from cookie
            var s = cookie.Split('.');

            var (json, signature) = (s[0], s[1]);

            // validate signature
            var jsonBytes = Convert.FromBase64String(json);
            if (signature != Convert.ToBase64String(new HMACSHA512(Options.KeyBytes).ComputeHash(jsonBytes)))
            {
                return Task.FromResult(AuthenticateResult.Fail(new AuthenticationException()));
            }

            // get contents
            var model = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonBytes);
            if (model is null || model.Count is 0) AuthenticateResult.Fail(new FormatException());

            var claims = model.Select(kvp => new Claim(kvp.Key, kvp.Value));

            return Task.FromResult(AuthenticateResult.Success(new(new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name)), Scheme.Name)));
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            var redirectUri = properties.RedirectUri;
            if (string.IsNullOrEmpty(redirectUri))
            {
                redirectUri = OriginalPathBase + OriginalPath + Request.QueryString;
            }

            var loginUri = Options.LoginPath + QueryString.Create(Options.ReturnUrlParameter, redirectUri);
            var redirectContext = new RedirectContext<SessionAuthOptions>(Context, Scheme, Options, properties, BuildRedirectUri(loginUri));
            redirectContext.Response.Redirect(redirectContext.RedirectUri);
            return Task.CompletedTask;
        }
    }
}
