using System;
using System.Linq.Expressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Predy.Expressions;

namespace Predy
{
	public enum PredyExpressions
	{
		Lambda,
		Binary,
		MethodCall,
		Member,
		Constant,
		Parameter,
		Unary
	}

	public class PredyJsonConverter : JsonConverter<IPredyExpression>
	{
		public override void WriteJson(JsonWriter writer, IPredyExpression value, JsonSerializer serializer)
		{
			if (value is PredyLambdaExpression lambda) WriteLambda(writer, lambda, serializer);
			else if (value is PredyBinaryExpression binary) WriteBinary(writer, binary, serializer);
			else if (value is PredyMethodCallExpression methodCall) WriteMethodCall(writer, methodCall, serializer);
			else if (value is PredyMemberExpression member) WriteMember(writer, member, serializer);
			else if (value is PredyConstantExpression constant) WriteConstant(writer, constant, serializer);
			else if (value is PredyParameterExpression parameter) WriteParameter(writer, parameter, serializer);
			else if (value is PredyUnaryExpression unary) WriteUnary(writer, unary, serializer);
		}

		public static void WriteUnary(JsonWriter writer, PredyUnaryExpression value, JsonSerializer serializer)
		{
			JObject jObject = new JObject
			{
				["ExType"] = JToken.FromObject(PredyExpressions.Unary),
				[nameof(PredyUnaryExpression.Operand)] = JToken.FromObject(value.Operand, serializer)
			};
			jObject.WriteTo(writer);
		}

		public static void WriteParameter(JsonWriter writer, PredyParameterExpression value, JsonSerializer serializer)
		{
			JObject jObject = new JObject
			{
				["ExType"] = JToken.FromObject(PredyExpressions.Parameter),
				[nameof(PredyParameterExpression.Name)] = JToken.FromObject(value.Name, serializer),
				[nameof(PredyParameterExpression.Type)] = JToken.FromObject(value.Type, serializer)
			};
			jObject.WriteTo(writer);
		}

		public static void WriteConstant(JsonWriter writer, PredyConstantExpression value, JsonSerializer serializer)
		{
			JObject jObject = new JObject
			{
				["ExType"] = JToken.FromObject(PredyExpressions.Constant),
				["Type"] = JToken.FromObject(value.Value.GetType()),
				[nameof(PredyConstantExpression.Value)] = JToken.FromObject(value.Value, serializer)
			};
			jObject.WriteTo(writer);
		}

		public static void WriteMember(JsonWriter writer, PredyMemberExpression value, JsonSerializer serializer)
		{
			JObject jObject = new JObject
			{
				["ExType"] = JToken.FromObject(PredyExpressions.Member),
				[nameof(PredyMemberExpression.Expression)] = JToken.FromObject(value.Expression, serializer),
				[nameof(PredyMemberExpression.MemberName)] = JToken.FromObject(value.MemberName, serializer)
			};
			jObject.WriteTo(writer);
		}

		public static void WriteMethodCall(JsonWriter writer, PredyMethodCallExpression value, JsonSerializer serializer)
		{
			JObject jObject = new JObject
			{
				["ExType"] = JToken.FromObject(PredyExpressions.MethodCall),
				[nameof(PredyMethodCallExpression.MethodName)] = JToken.FromObject(value.MethodName, serializer),
				[nameof(PredyMethodCallExpression.Member)] = JToken.FromObject(value.Member, serializer),
				[nameof(PredyMethodCallExpression.Arguments)] = JToken.FromObject(value.Arguments, serializer)
			};
			jObject.WriteTo(writer);
		}

		public static void WriteLambda(JsonWriter writer, PredyLambdaExpression value, JsonSerializer serializer)
		{
			JObject jObject = new JObject
			{
				["ExType"] = JToken.FromObject(PredyExpressions.Lambda),
				[nameof(PredyLambdaExpression.Body)] = JToken.FromObject(value.Body, serializer),
				[nameof(PredyLambdaExpression.Parameters)] = JToken.FromObject(value.Parameters, serializer),
				[nameof(PredyLambdaExpression.Type)] = JToken.FromObject(value.Type, serializer)
			};
			jObject.WriteTo(writer);
		}

		public static void WriteBinary(JsonWriter writer, PredyBinaryExpression value, JsonSerializer serializer)
		{
			JObject jObject = new JObject
			{
				["ExType"] = JToken.FromObject(PredyExpressions.Binary),
				[nameof(value.NodeType)] = JToken.FromObject(value.NodeType),
				[nameof(value.Left)] = JToken.FromObject(value.Left, serializer),
				[nameof(value.Right)] = JToken.FromObject(value.Right, serializer)
			};
			jObject.WriteTo(writer);
		}

		public override IPredyExpression ReadJson(JsonReader reader, Type objectType, IPredyExpression existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			JObject jObject = JObject.Load(reader);
			PredyExpressions expressionType = serializer.Deserialize<PredyExpressions>(jObject.SelectToken("ExType").CreateReader());
			return expressionType switch
			{
				PredyExpressions.Lambda     => PredyLambdaJsonConverter.Read(jObject, serializer),
				PredyExpressions.Binary     => PredyBinaryJsonConverter.Read(jObject, serializer),
				PredyExpressions.MethodCall => PredyMethodCallJsonConverter.Read(jObject, serializer),
				PredyExpressions.Member     => PredyMemberJsonConverter.Read(jObject, serializer),
				PredyExpressions.Constant   => PredyConstantJsonConverter.Read(jObject, serializer),
				PredyExpressions.Parameter  => PredyParameterJsonConverter.Read(jObject, serializer),
				PredyExpressions.Unary      => PredyUnaryJsonConverter.Read(jObject, serializer),
				var _                       => throw new ArgumentOutOfRangeException()
			};
		}
	}

	public class PredyLambdaJsonConverter : JsonConverter<PredyLambdaExpression>
	{
		public override void WriteJson(JsonWriter writer, PredyLambdaExpression value, JsonSerializer serializer)
		{
			PredyJsonConverter.WriteLambda(writer, value, serializer);
		}

		public override PredyLambdaExpression ReadJson(JsonReader reader, Type objectType, PredyLambdaExpression existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			JObject jObject = JObject.Load(reader);
			return Read(jObject, serializer);
		}

		public static PredyLambdaExpression Read(JObject jObject, JsonSerializer serializer)
		{
			PredyLambdaExpression deserialized = new PredyLambdaExpression
			{
				Body = serializer.Deserialize<IPredyExpression>(jObject.SelectToken(nameof(PredyLambdaExpression.Body)).CreateReader()),
				Type = serializer.Deserialize<Type>(jObject.SelectToken(nameof(PredyLambdaExpression.Type)).CreateReader())
			};
			JArray jArray = jObject.SelectToken(nameof(PredyLambdaExpression.Parameters)).Value<JArray>();
			foreach (JToken item in jArray)
			{
				PredyParameterExpression deserializedItem = serializer.Deserialize<PredyParameterExpression>(item.CreateReader());
				deserialized.Parameters.Add(deserializedItem);
			}

			return deserialized;
		}
	}

	public class PredyBinaryJsonConverter : JsonConverter<PredyBinaryExpression>
	{
		public override void WriteJson(JsonWriter writer, PredyBinaryExpression value, JsonSerializer serializer)
		{
			PredyJsonConverter.WriteBinary(writer, value, serializer);
		}

		public override PredyBinaryExpression ReadJson(JsonReader reader, Type objectType, PredyBinaryExpression existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			JObject jObject = JObject.Load(reader);
			return Read(jObject, serializer);
		}

		public static PredyBinaryExpression Read(JObject jObject, JsonSerializer serializer)
		{
			PredyBinaryExpression deserialized = new PredyBinaryExpression
			{
				Left = serializer.Deserialize<IPredyExpression>(jObject.SelectToken(nameof(PredyBinaryExpression.Left)).CreateReader()),
				Right = serializer.Deserialize<IPredyExpression>(jObject.SelectToken(nameof(PredyBinaryExpression.Right)).CreateReader()),
				NodeType = serializer.Deserialize<ExpressionType>(jObject.SelectToken(nameof(PredyBinaryExpression.NodeType)).CreateReader())
			};
			return deserialized;
		}
	}

	public class PredyMethodCallJsonConverter : JsonConverter<PredyMethodCallExpression>
	{
		public override void WriteJson(JsonWriter writer, PredyMethodCallExpression value, JsonSerializer serializer)
		{
			PredyJsonConverter.WriteMethodCall(writer, value, serializer);
		}

		public override PredyMethodCallExpression ReadJson(JsonReader reader, Type objectType, PredyMethodCallExpression existingValue, bool hasExistingValue,
			JsonSerializer serializer)
		{
			JObject jObject = JObject.Load(reader);
			return Read(jObject, serializer);
		}

		public static PredyMethodCallExpression Read(JObject jObject, JsonSerializer serializer)
		{
			PredyMethodCallExpression deserialized = new PredyMethodCallExpression
			{
				Member = serializer.Deserialize<PredyMemberExpression>(jObject.SelectToken(nameof(PredyMethodCallExpression.Member)).CreateReader()),
				MethodName = serializer.Deserialize<string>(jObject.SelectToken(nameof(PredyMethodCallExpression.MethodName)).CreateReader())
			};
			JArray jArray = jObject.SelectToken(nameof(PredyMethodCallExpression.Arguments)).Value<JArray>();
			foreach (JToken item in jArray)
			{
				IPredyExpression deserializedItem = serializer.Deserialize<IPredyExpression>(item.CreateReader());
				deserialized.Arguments.Add(deserializedItem);
			}

			return deserialized;
		}
	}

	public class PredyMemberJsonConverter : JsonConverter<PredyMemberExpression>
	{
		public override void WriteJson(JsonWriter writer, PredyMemberExpression value, JsonSerializer serializer)
		{
			PredyJsonConverter.WriteMember(writer, value, serializer);
		}

		public override PredyMemberExpression ReadJson(JsonReader reader, Type objectType, PredyMemberExpression existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			JObject jObject = JObject.Load(reader);
			return Read(jObject, serializer);
		}

		public static PredyMemberExpression Read(JObject jObject, JsonSerializer serializer)
		{
			PredyMemberExpression deserialized = new PredyMemberExpression
			{
				MemberName = serializer.Deserialize<string>(jObject.SelectToken(nameof(PredyMemberExpression.MemberName)).CreateReader()),
				Expression = serializer.Deserialize<IPredyExpression>(jObject.SelectToken(nameof(PredyMemberExpression.Expression)).CreateReader())
			};
			return deserialized;
		}
	}

	public class PredyConstantJsonConverter : JsonConverter<PredyConstantExpression>
	{
		public override void WriteJson(JsonWriter writer, PredyConstantExpression value, JsonSerializer serializer)
		{
			PredyJsonConverter.WriteConstant(writer, value, serializer);
		}

		public override PredyConstantExpression ReadJson(JsonReader reader, Type objectType, PredyConstantExpression existingValue, bool hasExistingValue,
			JsonSerializer serializer)
		{
			JObject jObject = JObject.Load(reader);
			return Read(jObject, serializer);
		}

		public static PredyConstantExpression Read(JObject jObject, JsonSerializer serializer)
		{
			Type valueType = serializer.Deserialize<Type>(jObject.SelectToken("Type").CreateReader());
			JsonReader valueReader = jObject.SelectToken(nameof(PredyConstantExpression.Value)).CreateReader();
			PredyConstantExpression deserialized = new PredyConstantExpression
			{
				Value = serializer.Deserialize(valueReader, valueType)
			};
			return deserialized;
		}
	}

	public class PredyUnaryJsonConverter : JsonConverter<PredyUnaryExpression>
	{
		public override void WriteJson(JsonWriter writer, PredyUnaryExpression value, JsonSerializer serializer)
		{
			PredyJsonConverter.WriteUnary(writer, value, serializer);
		}

		public override PredyUnaryExpression ReadJson(JsonReader reader, Type objectType, PredyUnaryExpression existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			JObject jObject = JObject.Load(reader);
			return Read(jObject, serializer);
		}

		public static PredyUnaryExpression Read(JObject jObject, JsonSerializer serializer)
		{
			PredyUnaryExpression deserialized = new PredyUnaryExpression
			{
				Operand = serializer.Deserialize<IPredyExpression>(jObject.SelectToken(nameof(PredyUnaryExpression.Operand)).CreateReader())
			};
			return deserialized;
		}
	}

	public class PredyParameterJsonConverter : JsonConverter<PredyParameterExpression>
	{
		public override void WriteJson(JsonWriter writer, PredyParameterExpression value, JsonSerializer serializer)
		{
			PredyJsonConverter.WriteParameter(writer, value, serializer);
		}

		public override PredyParameterExpression ReadJson(JsonReader reader, Type objectType, PredyParameterExpression existingValue, bool hasExistingValue,
			JsonSerializer serializer)
		{
			JObject jObject = JObject.Load(reader);
			return Read(jObject, serializer);
		}

		public static PredyParameterExpression Read(JObject jObject, JsonSerializer serializer)
		{
			return new PredyParameterExpression
			{
				Name = serializer.Deserialize<string>(jObject.SelectToken(nameof(PredyParameterExpression.Name)).CreateReader()),
				Type = serializer.Deserialize<Type>(jObject.SelectToken(nameof(PredyParameterExpression.Type)).CreateReader())
			};
		}
	}
}