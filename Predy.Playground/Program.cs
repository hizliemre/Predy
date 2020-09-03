using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Predy.Expressions;

namespace Predy.Playground
{
	class Person
	{
		public string Name { get; set; }
		public string Lastname { get; set; }
		public int Age { get; set; }
	}

	class Program
	{
		static void Main(string[] args)
		{
			IEnumerable<Person> persons = new List<Person>
			{
				new Person {Name = "Emre", Lastname = "Hizli", Age = 29},
				new Person {Name = "Tony", Lastname = "Stark", Age = 40},
				new Person {Name = "Karen", Lastname = "Hizli", Age = 1},
			};

			Expression<Func<Person, bool>> predicate = m => m.Age > 10 && m.Name.StartsWith("Em");
			var serialized = predicate.Serialize();
			var json = JsonConvert.SerializeObject(serialized);
			var deserializedJson = JsonConvert.DeserializeObject<PredyLambdaExpression>(json);
			var deserialized = (Expression<Func<Person, bool>>) deserializedJson.Deserialize();
			var result = persons.Where(deserialized.Compile()).ToList();
		}
	}
}