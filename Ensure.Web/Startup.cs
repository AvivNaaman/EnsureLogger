using Ensure.Web.Areas.Admin.Services;
using Ensure.Web.Data;
using Ensure.Web.Options;
using Ensure.Web.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ensure.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));

            services.AddDatabaseDeveloperPageExceptionFilter();
            services.AddHttpContextAccessor();

            services.AddOptions<JwtOptions>()
                .Bind(Configuration.GetSection("Jwt"));

            services.AddOptions<SendGridOptions>()
                .Bind(Configuration.GetSection("SendGrid"));

            JwtOptions jwtInstance = JwtOptions.FromConfig(Configuration);

            services.AddAuthentication(auth =>
            {
                auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(bearer =>
            {
                bearer.SaveToken = true;
                bearer.TokenValidationParameters = jwtInstance.GetTokenValidationParams();
            });

            services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedEmail = false;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

            services.AddFluentEmail(Configuration["SendGrid:FromAddress"])
                .AddRazorRenderer()
                .AddSendGridSender(Configuration["SendGrid:ApiKey"]);

            services.AddScoped<IEnsureService, EnsureService>();
            services.AddScoped<IAppUsersService, AppUsersService>();
            services.AddScoped<IAdminService, AdminService>();

            services.ConfigureApplicationCookie(cookie =>
            {
                cookie.LoginPath = "/Account/Login";
                cookie.LogoutPath = "/Account/Logout";
                cookie.AccessDeniedPath = "/Account/AccessDenied"; // TODO: Implement!
            });

            services.AddSwaggerGen(s =>
            {
                s.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Ensure Logger API",
                    Description = "The API fot the cross-platform drink logger",
                    License = new OpenApiLicense
                    {
                        Name = "MIT License",
                        Url = new Uri("https://opensource.org/licenses/MIT"),
                    },
                    Contact = new OpenApiContact
                    {
                         Name = "Aviv Naaman",
                         Email = "avivnaaman04@gmail.com",
                    },
                    Version = "v1"
                });
            });

            services.AddAntiforgery();

            services.AddControllersWithViews()
                .AddJsonOptions(json =>
                {
                    // we don't want to map shared helper props (such as ApiResponse.IsError)
                    // to save some time & data usage. This prop will be eavlauted on client if needed.
                    json.JsonSerializerOptions.IgnoreReadOnlyProperties = true;
                })
#if DEBUG
                .AddRazorRuntimeCompilation() // only when debugging!
#endif
                ;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
#if !DEBUG
            app.UseHttpsRedirection();
#endif
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapAreaControllerRoute(
                    name: "areas",
                    areaName: "Admin",
                    pattern: "{area}/{controller=Home}/{action=Index}/{id?}");
            });

            // Swagger for easy API doc & access
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Ensure Logger API");
            });
        }
    }
}
