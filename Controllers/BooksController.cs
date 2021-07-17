using Project1.Models;
using Project1.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Project1.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class BooksController : ControllerBase
	{
		private readonly IBookService _bookService;

		public BooksController(IBookService bookService)
		{
			_bookService = bookService;
		}

		[AllowAnonymous]
		[HttpGet]
		public ActionResult<List<Book>> Get()
		{
			List<Book> books = _bookService.Get();

			return Ok(books);
		}

		[AllowAnonymous]
		[HttpGet("{bookId:length(24)}", Name = "GetBook")]
		public ActionResult<Book> GetOne(string bookId)
		{
			Book book = _bookService.Get(bookId);

			if (book == null)
			{
				return NotFound();
			}

			return book;
		}

		[HttpPost]
		public ActionResult<Book> Create(Book book)
		{
			_bookService.Create(User.FindFirstValue(ClaimTypes.NameIdentifier), book);

			return CreatedAtRoute("GetBook", new { bookId = book.Id.ToString() }, book);
		}

		[HttpPut("{bookId:length(24)}")]
		public IActionResult Update(string bookId, Book bookIn)
		{
			Book newBook = _bookService.Update(User.FindFirstValue(ClaimTypes.NameIdentifier), bookId, bookIn);

			if (newBook == null)
				return BadRequest(new { message = "Request to update the resource is denied!" });

			return Ok(newBook);
		}

		[HttpDelete("{bookId:length(24)}")]
		public IActionResult Delete(string bookId)
		{
			bool isDeleted = _bookService.Remove(User.FindFirstValue(ClaimTypes.NameIdentifier), bookId);

			if (!isDeleted)
				return BadRequest(new { message = "Request to delete the resource is denied!" });

			return NoContent();
		}
	}
}