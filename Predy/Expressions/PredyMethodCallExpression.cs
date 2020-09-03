using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Predy.Expressions
{
	[JsonConverter(typeof(PredyMethodCallJsonConverter))]
	public class PredyMethodCallExpression: IPredyExpression
	{
		public PredyMethodCallExpression()
		{
			Arguments = new List<IPredyExpression>();
		}

		public List<IPredyExpression> Arguments { get; }
		public string MethodName { get; set; }
		public PredyMemberExpression Member { get; set; }
	}
}