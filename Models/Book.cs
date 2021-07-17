using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Project1.Models
{
	public class Book
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string Id { get; set; }

		[BsonElement("Name")]
		[JsonProperty("Name")]
		public string BookName { get; set; }

		public decimal Price { get; set; }

		public string Category { get; set; }

		public string Author { get; set; }

		public string PostedBy { get; set; }
	}
}