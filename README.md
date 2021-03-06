# Predy
Predy is a simple lambda expression serializer for predicates. You can serialize to json your simple lambda expression predicates.

### A simple usage

    internal class Program
    {
      private static void Main(string[] args)
      {
        IEnumerable<Person> persons = new List<Person>
        {
          new Person {Name = "Emre", Lastname = "Hizli", Age = 29},
          new Person {Name = "Tony", Lastname = "Stark", Age = 40},
          new Person {Name = "Karen", Lastname = "Hizli", Age = 1}
        };
        
        Expression<Func<Person, bool>> predicate = m => m.Age > 10 && m.Name.StartsWith("Em");
        
        PredyLambdaExpression serialized = predicate.Serialize(); // Serialize with predy
        string json = JsonConvert.SerializeObject(serialized); // Convert to json 
        PredyLambdaExpression deserializedJson = JsonConvert.DeserializeObject<PredyLambdaExpression>(json); // Deserialize to predy
        Expression<Func<Person, bool>> deserialized = (Expression<Func<Person, bool>>) deserializedJson.Deserialize(); // Deserialize to lambda with predy

        List<Person> result = persons.Where(deserialized.Compile()).ToList(); // Compile and use
      }
    }
