using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Predy.Expressions;

namespace Predy.Visitors
{
	public class PredyParameterVisitor : ExpressionVisitor
	{
		private readonly List<ParameterExpression> _parameters = new List<ParameterExpression>();

		protected override Expression VisitParameter(ParameterExpression node)
		{
			if (!_parameters.Any(x => x.Name == node.Name && x.Type == node.Type))
				_parameters.Add(node);

			return base.VisitParameter(node);
		}

		public ParameterExpression GetParameter(string name, Type type)
		{
			if (!HasParameter(name, type))
				CreateParameter(name, type);
			return _parameters.First(x => x.Name == name && x.Type == type);
		}

		public void CreateParameter(string name, Type type)
		{
			if (HasParameter(name, type)) return;
			ParameterExpression parameter = Expression.Parameter(type, name);
			_parameters.Add(parameter);
		}

		public bool HasParameter(string name, Type type)
		{
			return _parameters.Any(x => x.Name == name && x.Type == type);
		}

		public IEnumerable<ParameterExpression> GetParameters(List<PredyParameterExpression> parameters)
		{
			parameters.ForEach(x => CreateParameter(x.Name, x.Type));
			return _parameters.Where(x => parameters.Any(y => y.Name == x.Name && y.Type == x.Type));
		}
	}
}