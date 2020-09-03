using System.Text.Json.Serialization;

namespace Predy.Expressions
{
	[JsonConverter(typeof(PredyConstantJsonConverter))]
	public class PredyConstantExpression: IPredyExpression
	{
		public object Value { get; set; }
	}
}