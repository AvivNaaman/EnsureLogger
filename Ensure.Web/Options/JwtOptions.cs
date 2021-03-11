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

        public byte[] Key { get; set; }

        public int DaysToExpire { get; set; }

        public TokenValidationParameters GetTokenValidationParams()
        {
            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = Issuer,
                ValidAudience = Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Key)
            };
        }

        public static JwtOptions FromConfig(IConfiguration config)
        {
            JwtOptions options = new();
            options.Issuer = config.GetValue<string>("Jwt:Issuer");
            options.Audience = config.GetValue<string>("Jwt:Audidenc");
            options.DaysToExpire = config.GetValue<int>("Jwt:DaysToExpire");
            options.Key = Encoding.UTF8.GetBytes(config.GetValue<string>("Jwt:Key"));
            return options;
        }
    }
}
