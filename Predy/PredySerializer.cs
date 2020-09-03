using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Predy.Expressions;
using Predy.Visitors;

namespace Predy
{
	public class PredyConverter
	{
		public static PredyLambdaExpression Serialize<T>(LambdaExpression exp)
		{
			return SerializeLambda(exp, typeof(T));
		}

		public static LambdaExpression Deserialize(PredyLambdaExpression predy)
		{
			_visitor = new PredyParameterVisitor();
			return DeserializeLambda(predy);
		}

		#region SERIALIZER

		private static IPredyExpression SerializeInternal(Expression exp)
		{
			if (exp is LambdaExpression lambdaEx)
				return SerializeLambda(lambdaEx);
			if (exp is BinaryExpression binaryEx)
				return SerializeBinary(binaryEx);
			if (exp is MethodCallExpression methodCallEx)
				return SerializeMethodCall(methodCallEx);
			if (exp is MemberExpression memberEx)
				return SerializeMember(memberEx);
			if (exp is ConstantExpression contantEx)
				return SerializeConstant(contantEx);
			if (exp is UnaryExpression unaryEx)
				return SerializeUnary(unaryEx);
			if (exp is ParameterExpression parameterEx)
				return SerializeParameter(parameterEx);

			throw new NotSupportedException(nameof(SerializeInternal));
		}

		private static PredyLambdaExpression SerializeLambda(LambdaExpression exp, Type type = null)
		{
			PredyLambdaExpression predyLambda = new PredyLambdaExpression
			{
				Body = SerializeInternal(exp.Body),
				Parameters = exp.Parameters.Select(SerializeParameter).ToList(),
				Type = type
			};

			return predyLambda;
		}

		private static PredyUnaryExpression SerializeUnary(UnaryExpression unaryEx)
		{
			return new PredyUnaryExpression
			{
				Operand = SerializeInternal(unaryEx.Operand)
			};
		}

		private static PredyBinaryExpression SerializeBinary(BinaryExpression exp)
		{
			IPredyExpression left = SerializeInternal(exp.Left);
			IPredyExpression right = SerializeInternal(exp.Right);

			return new PredyBinaryExpression
			{
				Left = left,
				Right = right,
				NodeType = exp.NodeType
			};
		}

		private static PredyMethodCallExpression SerializeMethodCall(MethodCallExpression exp)
		{
			if (exp.Object is MemberExpression memberFromObject)
			{
				PredyMethodCallExpression methodPredy = new PredyMethodCallExpression
				{
					Member = (PredyMemberExpression) SerializeMember(memberFromObject),
					MethodName = exp.Method.Name
				};

				foreach (Expression argExp in exp.Arguments)
					methodPredy.Arguments.Add(SerializeInternal(argExp));

				return methodPredy;
			}

			if (exp.Arguments[0] is MemberExpression)
			{
				PredyMethodCallExpression methodPredy = new PredyMethodCallExpression
				{
					MethodName = exp.Method.Name
				};

				foreach (Expression argExp in exp.Arguments)
					methodPredy.Arguments.Add(SerializeInternal(argExp));

				return methodPredy;
			}

			throw new NotSupportedException(nameof(SerializeMethodCall));
		}

		private static IPredyExpression SerializeMember(MemberExpression memberEx)
		{
			Expression exp = memberEx.Expression;
			while (exp is MemberExpression innerMember)
				if (innerMember.Expression is MemberExpression)
				{
					exp = innerMember.Expression;
				}
				else if (innerMember.Expression is ConstantExpression)
				{
					object value = Expression.Lambda(memberEx).Compile().DynamicInvoke();
					ConstantExpression constant = Expression.Constant(value);
					return SerializeInternal(constant);
				}

			return new PredyMemberExpression
			{
				MemberName = memberEx.Member.Name,
				Expression = SerializeInternal(memberEx.Expression)
			};
		}

		private static PredyConstantExpression SerializeConstant(ConstantExpression constantEx)
		{
			return new PredyConstantExpression
			{
				Value = constantEx.Value
			};
		}

		private static PredyParameterExpression SerializeParameter(ParameterExpression parameterEx)
		{
			return new PredyParameterExpression
			{
				Name = parameterEx.Name,
				Type = parameterEx.Type
			};
		}

		#endregion

		#region DESERIALIZER

		private static PredyParameterVisitor _visitor;

		private static LambdaExpression DeserializeLambda(PredyLambdaExpression lambdaPredy)
		{
			Expression body = DeserializeInternal(lambdaPredy.Body);
			return Expression.Lambda(body, _visitor.GetParameters(lambdaPredy.Parameters));
		}

		private static Expression DeserializeUnary(PredyUnaryExpression unaryPredy)
		{
			return Expression.Not(DeserializeInternal(unaryPredy.Operand));
		}

		private static Expression DeserializeMethodCall(PredyMethodCallExpression methodCallPredy)
		{
			Expression[] args = methodCallPredy.Arguments.Select(DeserializeInternal).ToArray();
			Expression memberEx = args[0];

			if (methodCallPredy.Member == null && memberEx.Type.IsGenericType)
			{
				MethodInfo method = FindMethod(typeof(Enumerable), methodCallPredy.MethodName, args.Length);
				Type[] genericTypes = memberEx.Type.GenericTypeArguments;
				MethodInfo genericMethod = method.MakeGenericMethod(genericTypes);
				return Expression.Call(genericMethod, args);
			}

			if (methodCallPredy.Member != null)
			{
				Type[] argTypes = args.Select(x => x.Type).ToArray();
				memberEx = DeserializeMember(methodCallPredy.Member);
				MethodInfo method = FindMethod(memberEx.Type, methodCallPredy.MethodName, argTypes);
				return Expression.Call(memberEx, method, args);
			}

			throw new NotSupportedException(nameof(DeserializeMethodCall));
		}

		public static Expression DeserializeBinary(PredyBinaryExpression binaryPredy)
		{
			Expression left = DeserializeInternal(binaryPredy.Left);
			Expression right = DeserializeInternal(binaryPredy.Right);

			BinaryExpression binary = Expression.MakeBinary(binaryPredy.NodeType, left, right);
			return binary;
		}

		public static Expression DeserializeMember(PredyMemberExpression memberPredy)
		{
			Expression expression = DeserializeInternal(memberPredy.Expression);
			return Expression.PropertyOrField(expression, memberPredy.MemberName);
		}

		public static ParameterExpression DeserializeParameter(PredyParameterExpression parameterPredy)
		{
			return _visitor.GetParameter(parameterPredy.Name, parameterPredy.Type);
		}

		private static Expression DeserializeConstant(PredyConstantExpression constantPredy)
		{
			return Expression.Constant(constantPredy.Value);
		}

		private static Expression DeserializeInternal(IPredyExpression predy)
		{
			if (predy is PredyParameterExpression parameterPredy)
				return DeserializeParameter(parameterPredy);
			if (predy is PredyMemberExpression memberPredy)
				return DeserializeMember(memberPredy);
			if (predy is PredyConstantExpression constantPredy)
				return DeserializeConstant(constantPredy);
			if (predy is PredyBinaryExpression binaryPredy)
				return DeserializeBinary(binaryPredy);
			if (predy is PredyMethodCallExpression methodCallPredy)
				return DeserializeMethodCall(methodCallPredy);
			if (predy is PredyLambdaExpression predyLambda)
				return DeserializeLambda(predyLambda);
			if (predy is PredyUnaryExpression unaryPredy)
				return DeserializeUnary(unaryPredy);

			return Expression.Constant(false);
		}

		private static MethodInfo FindMethod(Type type, string name, Type[] parameters)
		{
			return type.GetMethod(name, parameters);
		}

		private static MethodInfo FindMethod(Type type, string name, int parameterCount)
		{
			return type.GetMethods().Single(x => x.Name == name && x.GetParameters().Length == parameterCount);
		}

		#endregion
	}
}