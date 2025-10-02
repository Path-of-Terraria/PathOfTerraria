using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Terraria.DataStructures;
using Terraria.ModLoader.Config;

#nullable enable

namespace PathOfTerraria.Common.Encounters;

internal static class EncounterIO
{
	public static readonly string Extension = ".encounter.json";

	private static readonly Dictionary<string, EncounterDescription> cache = new(StringComparer.InvariantCultureIgnoreCase);
	private static readonly JsonSerializerSettings serializerSettings = new()
	{
		DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
		Converters = [new FixedXnaJsonConverter(), new EntityDefinitionJsonConverter()],
	};

	public static string ToJson(Encounter encounter)
	{
		string jsonText = JsonConvert.SerializeObject(encounter.Description, Formatting.Indented, serializerSettings);

		return jsonText;
	}

	public static EncounterDescription FromJson(string json)
	{
		EncounterDescription result = JsonConvert.DeserializeObject<EncounterDescription>(json, serializerSettings);

		return result;
	}

	public static EncounterDescription GetEncounterFromModPath(Mod mod, string filePathWithoutExtension)
	{
		string identifier = $"{mod.Name}/{filePathWithoutExtension}";

		if (!cache.TryGetValue(identifier, out EncounterDescription description))
		{
			cache[identifier] = description = FromJson(Encoding.UTF8.GetString(mod.GetFileBytes(filePathWithoutExtension + Extension)));
		}

		return description;
	}

	public static Encounter CreateEncounterFromModPath(Mod mod, string filePathWithoutExtension)
	{
		return EnemyEncounters.CreateEncounter(GetEncounterFromModPath(mod, filePathWithoutExtension));
	}
}

/// <summary> A converter that prevents XNA/FNA formats from being serialized as strings, instead using single-line arrays where possible. </summary>
internal sealed class FixedXnaJsonConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		Type checkedType = objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Nullable<>) ? objectType.GenericTypeArguments[0] : objectType;

		return checkedType == typeof(Point)
			|| checkedType == typeof(Point16)
			|| checkedType == typeof(Vector2)
			|| checkedType == typeof(Vector3)
			|| checkedType == typeof(Vector4)
			|| checkedType == typeof(Rectangle);
	}

	public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
	{
		if (value is null)
		{
			writer.WriteNull();
			return;
		}

		writer.WriteStartArray();

		switch (value)
		{
			case Point p32: writer.WriteRaw($@" {p32.X}, {p32.Y} "); break;
			case Point16 p16: writer.WriteRaw($@" {p16.X}, {p16.Y} "); break;
			case Vector2 vec2: writer.WriteRaw($@" {vec2.X}, {vec2.Y} "); break;
			case Vector3 vec3: writer.WriteRaw($@" {vec3.X}, {vec3.Y}, {vec3.Z} "); break;
			case Vector4 vec4: writer.WriteRaw($@"  {vec4.X}, {vec4.Y}, {vec4.Z}, {vec4.W} "); break;
			case Rectangle rect: writer.WriteRaw($@" {rect.X}, {rect.Y}, {rect.Width}, {rect.Height} "); break;
		}

		writer.WriteEndArray();
	}

	public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.Null) { return null; }

		var arr = JArray.Load(reader);

		T ReadValue<T>(int index)
		{
			return arr[index] is JValue jValue ? (T)Convert.ChangeType(jValue.Value, typeof(T))! : default!;
		}

		Type checkedType = objectType.IsGenericType ? objectType.GenericTypeArguments[0] : objectType;

		if (checkedType == typeof(Point))
		{
			return new Point(ReadValue<int>(0), ReadValue<int>(1));
		}
		else if (checkedType == typeof(Point16))
		{
			return new Point16(ReadValue<int>(0), ReadValue<int>(1));
		}
		else if (checkedType == typeof(Vector2))
		{
			return new Vector2(ReadValue<float>(0), ReadValue<float>(1));
		}
		else if (checkedType == typeof(Vector3))
		{
			return new Vector3(ReadValue<float>(0), ReadValue<float>(1), ReadValue<float>(2));
		}
		else if (checkedType == typeof(Vector4))
		{
			return new Vector4(ReadValue<float>(0), ReadValue<float>(1), ReadValue<float>(2), ReadValue<float>(3));
		}
		else if (checkedType == typeof(Rectangle))
		{
			return new Rectangle(ReadValue<int>(0), ReadValue<int>(1), ReadValue<int>(2), ReadValue<int>(3));
		}

		throw new NotImplementedException();
	}
}

internal sealed class EntityDefinitionJsonConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return typeof(EntityDefinition).IsAssignableFrom(objectType);
	}

	public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
	{
		if (value is EntityDefinition definition)
		{
			writer.WriteValue(definition.ToString());
		}
		else
		{
			writer.WriteNull();
		}
	}

	public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
	{
		if (JToken.Load(reader) is not JValue val || val.Type is not JTokenType.String and not JTokenType.Null)
		{
			throw new InvalidOperationException($"Expected a string, but got '{reader.TokenType}' instead.");
		}

		object? result = val.Value != null ? objectType.GetConstructor([typeof(string)])!.Invoke([val.Value]) : null;

		return result;
	}
}
