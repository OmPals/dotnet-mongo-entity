using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using Project1.Helpers;
using Project1.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Project1.Services
{
	public interface IUserService
	{
		User Authenticate(string username, string password);
		IEnumerable<User> GetAll();
		User GetById(string id);
		User Create(User user, string password);
		User Update(string id, User user, string password = null);
		void Delete(string id);
	}

	public class UserService : IUserService
	{
		private readonly IMongoCollection<User> _users;
		private readonly string _userId;

		public UserService(IUserDatabaseSettings settings, IHttpContextAccessor httpContextAccessor)
		{
			var client = new MongoClient(settings.ConnectionString);
			var database = client.GetDatabase(settings.DatabaseName);

			_userId = httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
			_users = database.GetCollection<User>(settings.UsersCollectionName);
		}

		public User GetByUsername(string username) =>
			_users.Find<User>(user => user.Username == username).FirstOrDefault();

		public User GetById(string id) =>
			_users.Find<User>(user => user.Id == id).FirstOrDefault();

		public User Create(User user)
		{
			_users.InsertOne(user);
			return user;
		}

		public void UpdateOne(string id, User userIn) =>
			_users.ReplaceOne(user => user.Id == id, userIn);

		public void Remove(string id) =>
			_users.DeleteOne(user => user.Id == id);

		public User Authenticate(string username, string password)
		{
			if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
				return null;

			User user = GetByUsername(username);

			// check if username exists
			if (user == null)
				return null;

			// check if password is correct
			if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
				return null;

			// authentication successful
			return user;
		}

		public User Create(User user, string password)
		{
			if (string.IsNullOrWhiteSpace(password))
				throw new Exception("password required");

			if (GetByUsername(user.Username) != null)
				throw new Exception("Username \"" + user.Username + "\" is already taken");

			byte[] passwordHash, passwordSalt;
			CreatePasswordHash(password, out passwordHash, out passwordSalt);

			user.PasswordHash = passwordHash;
			user.PasswordSalt = passwordSalt;

			return Create(user);
		}

		public User Update(string id, User userParam, string password = null)
		{
			User user = GetById(id);

			if (user == null)
				throw new AppException("User not found");

			// update username if it has changed
			if (!string.IsNullOrWhiteSpace(userParam.Username) && userParam.Username != user.Username)
			{
				User userWithSameUsername = GetByUsername(userParam.Username);

				// throw error if the new username is already taken
				if (userWithSameUsername != null && userWithSameUsername.Id != id)
					throw new AppException("Username " + userParam.Username + " is already taken");

				user.Username = userParam.Username;
			}

			// update user properties if provided
			if (!string.IsNullOrWhiteSpace(userParam.FirstName))
				user.FirstName = userParam.FirstName;

			if (!string.IsNullOrWhiteSpace(userParam.LastName))
				user.LastName = userParam.LastName;

			// update password if provided
			if (!string.IsNullOrWhiteSpace(password))
			{
				byte[] passwordHash, passwordSalt;
				CreatePasswordHash(password, out passwordHash, out passwordSalt);

				user.PasswordHash = passwordHash;
				user.PasswordSalt = passwordSalt;
			}

			UpdateOne(id, user);

			return user;
		}

		private static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
		{
			if (password == null) throw new ArgumentNullException("password");
			if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");
			if (storedHash.Length != 64) throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "passwordHash");
			if (storedSalt.Length != 128) throw new ArgumentException("Invalid length of password salt (128 bytes expected).", "passwordHash");

			using (var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt))
			{
				var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
				for (int i = 0; i < computedHash.Length; i++)
				{
					if (computedHash[i] != storedHash[i]) return false;
				}
			}

			return true;
		}

		private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
		{
			if (password == null) throw new ArgumentNullException("password");
			if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");

			using (var hmac = new System.Security.Cryptography.HMACSHA512())
			{
				passwordSalt = hmac.Key;
				passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
			}
		}

		public IEnumerable<User> GetAll()
		{
			throw new NotImplementedException();
		}

		public void Delete(string id)
		{
			Remove(id);
		}
	}
}
