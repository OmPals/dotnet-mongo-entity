using Microsoft.AspNetCore.Mvc;

namespace Project1.Controllers
{
	[ApiController]
	public class HomeController : ControllerBase
	{
		[HttpGet]
		[Route("/")]
		public IActionResult Get()
		{
			return Redirect("/swagger");
		}
	}
}
