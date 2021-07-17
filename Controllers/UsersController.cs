using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Project1.Helpers;
using Project1.Models;
using Project1.Services;

namespace Project1.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class UsersController : ControllerBase
	{
		private readonly IUserService _userService;
		private readonly IOptions<AppSettings> _appSettings;
		private readonly IMapper _mapper;


		public UsersController(IUserService userService,
			IOptions<AppSettings> appSettings,
			IMapper mapper)
		{
			_userService = userService;
			_appSettings = appSettings;
			_mapper = mapper;
		}

		[AllowAnonymous]
		[HttpPost("authenticate")]
		public IActionResult Authenticate([FromBody] AuthenticateModel model)
		{
			User user = _userService.Authenticate(model.Username, model.Password);

			if (user == null)
				return Unauthorized(new { message = "Username or password is incorrect" });

			string tokenString = GenerateUserToken(user);

			// return basic user info and authentication token
			return Ok(new
			{
				accessToken = tokenString,
				id = user.Id,
				username = user.Username,
				fullName = user.FullName,
			});
		}

		[AllowAnonymous]
		[HttpPost("register")]
		public IActionResult Register([FromBody] RegisterModel model)
		{
			// map model to entity
			User user = _mapper.Map<User>(model);

			try
			{
				// create user
				_userService.Create(user, model.Password);
				return Ok();
			}
			catch (Exception ex)
			{
				// return error message if there was an exception
				return BadRequest(new { message = ex.Message });
			}
		}

		public string GenerateUserToken(User user)
		{
			JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
			byte[] key = Encoding.ASCII.GetBytes(_appSettings.Value.Secret);
			SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
			{
				Issuer = "Project1",
				Audience = "Project1",
				CompressionAlgorithm = SecurityAlgorithms.HmacSha256Signature,
				Subject = new ClaimsIdentity(new Claim[]
				{
					new Claim(ClaimTypes.NameIdentifier, user.Id),
					new Claim(ClaimTypes.Name, user.Username)
				}),
				Expires = DateTime.UtcNow.AddDays(7),
				SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
			};
			SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
			string tokenString = tokenHandler.WriteToken(token);

			return tokenString;
		}

		[HttpPut("{userId:length(24)}")]
		public IActionResult Update([FromBody] UpdateModel updateModel, [FromRoute] string userId)
		{
			if (updateModel == null)
				return BadRequest(new { message = "request body cannot be null" });

			User user = _mapper.Map<User>(updateModel);

			try
			{
				user = _userService.Update(userId, user);
				string tokenString = GenerateUserToken(user);

				return Ok(new
				{
					accessToken = tokenString,
					username = user.Username,
					fullName = user.FullName,
					id = user.Id
				});
			}
			catch (AppException ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		[HttpDelete("{userId:length(24)}")]
		public IActionResult Delete([FromRoute] string userId)
		{
			User user = _userService.GetById(userId);

			if (user == null)
			{
				return NotFound();
			}

			_userService.Delete(userId);

			return NoContent();
		}
	}
}
