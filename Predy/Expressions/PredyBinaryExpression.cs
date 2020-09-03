using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace Predy.Expressions
{
	[JsonConverter(typeof(PredyBinaryJsonConverter))]
	public class PredyBinaryExpression : IPredyExpression
	{
		public IPredyExpression Left { get; set; }
		public IPredyExpression Right { get; set; }
		public ExpressionType NodeType { get; set; }
	}
}