using System;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Ensure.Web.Options
{
    public class JwtOptions
    {
        public string Issuer { get; set; }

        public string Audience { get; set; }

        public string Key
        {
            get; set;
        }

        public int DaysToExpire { get; set; }

        public TokenValidationParameters GetTokenValidationParams()
        {
            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = Issuer,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key))
            };
        }

        public static JwtOptions FromConfig(IConfiguration config)
        {
            JwtOptions options = new();
            options.Issuer = config.GetValue<string>("Jwt:Issuer");
            options.Audience = config.GetValue<string>("Jwt:Audience");
            options.DaysToExpire = config.GetValue<int>("Jwt:DaysToExpire");
            options.Key = config.GetValue<string>("Jwt:Key");
            return options;
        }
    }
}
