using System;
using System.Text.Json.Serialization;

namespace Predy.Expressions
{
	[JsonConverter(typeof(PredyParameterJsonConverter))]
	public class PredyParameterExpression : IPredyExpression
	{
		public string Name { get; set; }
		public Type Type { get; set; }
	}
}