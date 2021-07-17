using Project1.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Project1.Services;
using Project1.Helpers;
using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace Project1
{
	public class Startup
	{
		private readonly IWebHostEnvironment _env;
		private readonly IConfiguration _configuration;

		public Startup(IWebHostEnvironment env, IConfiguration configuration)
		{
			_configuration = configuration;
			_env = env;
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			// requires using Microsoft.Extensions.Options
			services.Configure<BookstoreDatabaseSettings>(
				_configuration.GetSection(nameof(BookstoreDatabaseSettings)));

			services.AddSingleton<IBookstoreDatabaseSettings>(sp =>
				sp.GetRequiredService<IOptions<BookstoreDatabaseSettings>>().Value);

			services.Configure<UserDatabaseSettings>(
				_configuration.GetSection(nameof(UserDatabaseSettings)));

			services.AddSingleton<IUserDatabaseSettings>(sp =>
				sp.GetRequiredService<IOptions<UserDatabaseSettings>>().Value);

			services.AddScoped<IBookService, BookService>();

			services.AddScoped<IUserService, UserService>();

			services.AddHttpContextAccessor();

			services.AddCors();

			// configure strongly typed settings objects
			var appSettingsSection = _configuration.GetSection("AppSettings");
			services.Configure<AppSettings>(appSettingsSection);

			// configure jwt authentication
			var appSettings = appSettingsSection.Get<AppSettings>();
			var key = Encoding.ASCII.GetBytes(appSettings.Secret);
			services.AddAuthentication(x =>
			{
				x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			})
			.AddJwtBearer(x =>
			{
				x.Events = new JwtBearerEvents
				{
					OnTokenValidated = context =>
					{
						ClaimsPrincipal claims = context.Principal;
						string userId = claims.FindFirstValue(ClaimTypes.NameIdentifier);

						IUserService userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
						User user = userService.GetById(userId);

						context.Request.RouteValues.TryGetValue("userId", out object requestedUserId);

						if (user == null || (requestedUserId != null && userId != requestedUserId.ToString()))
						{
							// return unauthorized if user no longer exists
							context.Response.StatusCode = 403;
							context.Fail("Unauthorized");
						}

						return Task.CompletedTask;
					}
				};

				x.RequireHttpsMetadata = false;
				x.SaveToken = true;
				x.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = new SymmetricSecurityKey(key),
					ValidateIssuer = false,
					ValidateAudience = false
				};
			});

			services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

			services.AddControllers()
				.AddNewtonsoftJson(options => options.UseMemberCasing());

			services.AddSwaggerDocument();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseOpenApi();

			app.UseSwaggerUi3();

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseAuthentication();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}
