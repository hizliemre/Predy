using System.Text.Json.Serialization;

namespace Predy.Expressions
{
	[JsonConverter(typeof(PredyMemberJsonConverter))]
	public class PredyMemberExpression : IPredyExpression
	{
		public string MemberName { get; set; }
		public IPredyExpression Expression { get; set; }
	}
}