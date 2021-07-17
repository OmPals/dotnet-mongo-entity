using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project1.Services;

namespace Project1.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize(Roles = "super-user")]
	public class SuperController : ControllerBase
	{
		private readonly BookService _bookService;

		public SuperController(BookService bookService)
		{
			_bookService = bookService;
		}

		[HttpDelete("{id:length(24)}")]
		public IActionResult DeleteSuper(string id)
		{
			_bookService.RemoveOne(id);
			return NoContent();
		}
	}
}
