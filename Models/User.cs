using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Project1.Models
{
	public class User
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string Id { get; set; }
		public string Username { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string FullName => $"{FirstName} {LastName}".Trim();

		[BsonRepresentation(BsonType.DateTime)]
		public DateTime DOB { get; set; }
		public int Age => CalculateAge(DOB);
		public byte[] PasswordHash { get; set; }
		public byte[] PasswordSalt { get; set; }
		public string AccessToken { get; set; }
		public string RefreshToken { get; set; }
		public string Email { get; set; }
		public string Contact { get; set; }
		public bool IsVerified { get; set; }

		private static int CalculateAge(DateTime dateOfBirth)
		{
			if (dateOfBirth == null) return 0;
			int age = DateTime.Now.Year - dateOfBirth.Year;
			if (DateTime.Now.DayOfYear < dateOfBirth.DayOfYear)
				age -= 1;

			return age;
		}
	}
}
