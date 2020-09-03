using Newtonsoft.Json;

namespace Predy.Expressions
{
	[JsonConverter(typeof(PredyJsonConverter))]
	public interface IPredyExpression { }
}