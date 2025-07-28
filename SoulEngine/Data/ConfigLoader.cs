namespace SoulEngine.Data;

public class ConfigLoader
{
	private readonly Dictionary<string, object> values = new Dictionary<string, object>();

	public void LoadFile(string data)
	{
		var result = ConfigParser.Parse(data);
		foreach (var variable in result)
		{
			values[variable.Key] = variable.Value;
		}
	}

	public ICollection<string> GetKeys()
	{
		return values.Keys;
	}
	
	public object this[string key]
	{
		get
		{
			if (values.TryGetValue(key, out object? item))
				return item;
			return null!;
		}
		set
		{
			values[key] = value;
		}
	}

	private static readonly Type[] AllowedTypes =
	[
		typeof(string), typeof(int), typeof(short), typeof(byte), typeof(float), typeof(double), typeof(bool)
	];

	public string Serialize()
	{
		string output = "# Automatically serialized by ConfyLoader\n";

		foreach (var variable in values)
		{
			if(!AllowedTypes.Contains(variable.Value.GetType()))
				continue;
			output += variable.Key + "=";
			if (variable.Value is string s)
				output += s;
			if (variable.Value is int i)
				output += i + "i";
			if (variable.Value is short sh)
				output += sh + "s";
			if (variable.Value is byte b)
				output += b + "b";
			if (variable.Value is float f)
				output += f + "f";
			if (variable.Value is double d)
				output += d + "d";
			if (variable.Value is bool bo)
				output += bo ? "true" : "false";
			output += "\n";
		}


		return output;
	}
}

public static class ConfigParser
{
	public static Dictionary<string, object> Parse(string data)
	{
		string[] lines = data.Split("\n");
		Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();

		foreach (string lineRaw in lines)
		{
			string line = lineRaw.Trim();
			
			if(string.IsNullOrEmpty(line))
				continue;
			
			if(line.StartsWith("#"))
				continue;

			string[] kvp = line.Split("=", 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

			if (kvp.Length == 1)
				throw new FormatException($"Key {kvp[0]} is missing value");
			keyValuePairs[kvp[0]] = kvp[1];
		}
		
		Dictionary<string, object> objects = new Dictionary<string, object>();


		foreach (var kvp in keyValuePairs)
		{

			objects.Add(kvp.Key, GetObjectFromValue(kvp.Value));
		}

		return objects;
	}

	public static object GetObjectFromValue(string value)
	{
		if (value == "true")
			return true;
		if (value == "false")
			return false;
		
		if(value.EndsWith("sb"))
			if (SByte.TryParse(value.Remove(value.Length - 2), out sbyte resultSByte))
				return resultSByte;
		
		if(value.EndsWith("us"))
			if (UInt16.TryParse(value.Remove(value.Length - 2), out ushort resultUShort))
				return resultUShort;
		
		if(value.EndsWith("ui"))
			if (UInt32.TryParse(value.Remove(value.Length - 2), out uint resultUIntForced))
				return resultUIntForced;

		if(value.EndsWith("ul"))
			if (UInt64.TryParse(value.Remove(value.Length - 2), out ulong resultULong))
				return resultULong;

		
		if(value.EndsWith("b"))
			if (Byte.TryParse(value.Remove(value.Length - 1), out byte resultByte))
				return resultByte;
		
		if(value.EndsWith("s"))
			if (Int16.TryParse(value.Remove(value.Length - 1), out short resultShortForced))
				return resultShortForced;

		if(value.EndsWith("i"))
			if (Int32.TryParse(value.Remove(value.Length - 1), out int resultIntForced))
				return resultIntForced;
		
		if(value.EndsWith("l"))
			if (Int64.TryParse(value.Remove(value.Length - 1), out long resultLong))
				return resultLong;
			
		if(value.EndsWith("f"))
			if (Single.TryParse(value.Remove(value.Length - 1), out float resultFloatForced))
				return resultFloatForced;
			
		if(value.EndsWith("d"))
			if (Double.TryParse(value.Remove(value.Length - 1), out double resultFloatForced))
				return resultFloatForced;
			
		if (Int32.TryParse(value, out int resultInt))
			return resultInt;
		
		if (Single.TryParse(value, out float resultFloat))
			return resultFloat;
		
		if (Double.TryParse(value, out double resultDouble))
			return resultDouble;


		return value;

	}
}