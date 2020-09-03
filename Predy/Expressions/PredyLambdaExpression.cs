using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Predy.Expressions
{
	[JsonConverter(typeof(PredyLambdaJsonConverter))]
	public class PredyLambdaExpression: IPredyExpression
	{
		public PredyLambdaExpression()
		{
			Parameters = new List<PredyParameterExpression>();
		}

		public Type Type { get; set; }

		public IPredyExpression Body { get; set; }

		public List<PredyParameterExpression> Parameters { get; set; }
	}
}