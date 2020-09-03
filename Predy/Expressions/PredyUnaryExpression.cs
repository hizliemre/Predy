using System.Text.Json.Serialization;

namespace Predy.Expressions
{
	[JsonConverter(typeof(PredyUnaryJsonConverter))]
	public class PredyUnaryExpression: IPredyExpression
	{
		public IPredyExpression Operand { get; set; }
	}
}