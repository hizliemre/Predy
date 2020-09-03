using System;
using System.Linq.Expressions;
using Predy.Expressions;

namespace Predy
{
	public static class PredyExtensions
	{
		public static PredyLambdaExpression Serialize<T>(this Expression<Func<T, bool>> expression)
		{
			if (expression is LambdaExpression lambdaExpression)
				return PredyConverter.Serialize<T>(lambdaExpression);
			throw new InvalidOperationException("This method serialize only lambda expressions.");
		}

		public static LambdaExpression Deserialize(this PredyLambdaExpression predy)
		{
			return PredyConverter.Deserialize(predy);
		}
	}
}