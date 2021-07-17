using Project1.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Project1.Services
{
	public interface IBookService
	{
		List<Book> Get();
		Book Get(string bookId);
		void Create(string userId, Book book);
		Book Update(string userId, string bookId, Book bookIn);
		bool Remove(string userId, string bookId);
	}

	public class BookService : IBookService
	{
		private readonly IMongoCollection<Book> _books;
		private readonly string _userId;

		public BookService(IBookstoreDatabaseSettings settings, IHttpContextAccessor httpContextAccessor)
		{
			var client = new MongoClient(settings.ConnectionString);
			var database = client.GetDatabase(settings.DatabaseName);

			_userId = httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
			_books = database.GetCollection<Book>(settings.BooksCollectionName);
		}

		public List<Book> Get() =>
			_books.Find(book => true).ToList();

		public Book Get(string id) =>
			_books.Find<Book>(book => book.Id == id).FirstOrDefault();

		public Book CreateOne(Book book)
		{
			_books.InsertOne(book);
			return book;
		}

		public void UpdateOne(string id, Book bookIn) =>
			_books.ReplaceOne(book => book.Id == id, bookIn);

		public void RemoveOne(string id) =>
			_books.DeleteOne(book => book.Id == id);

		public void Create(string userId, Book book)
		{
			book.PostedBy = userId;
			CreateOne(book);
		}

		public Book Update(string userId, string bookId, Book bookParam)
		{
			Book book = Get(bookId);

			if (book == null)
				return null;

			if (userId != book.PostedBy)
				return null;

			if (!string.IsNullOrWhiteSpace(bookParam.BookName))
				book.BookName = bookParam.BookName;

			if (!string.IsNullOrWhiteSpace(bookParam.Category))
				book.Category = bookParam.Category;

			if (!string.IsNullOrWhiteSpace(bookParam.Author))
				book.Author = bookParam.Author;

			if (bookParam.Price >= 0)
				book.Price = bookParam.Price;

			UpdateOne(bookId, book);

			return book;
		}

		public bool Remove(string userId, string bookId)
		{
			Book oldBook = Get(bookId);

			if (userId != oldBook.PostedBy)
				return false;

			RemoveOne(bookId);

			return true;
		}
	}
}