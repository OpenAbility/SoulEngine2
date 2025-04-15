using OpenAbility.Logging;
using SoulEngine.Content;

namespace SoulEngine.Data;

/// <summary>
/// Global engine variables indexed by a string name
/// </summary>
public class EngineVarContext
{
	private readonly Dictionary<string, EngineVarEntry> Entries = new Dictionary<string, EngineVarEntry>();
	private static readonly Logger Logger = Logger.Get("EngineVar");

	public static EngineVarContext Global;

	public EngineVarContext(ContentContext context)
	{

		ConfigLoader configLoader = new ConfigLoader();
		
		string[] configs = context.LoadAllStrings("config.cfg");

		foreach (var config in configs)
		{
			configLoader.LoadFile(config);
		}

		foreach (var key in configLoader.GetKeys())
		{
			Set(key, configLoader[key]);
		}

		File.WriteAllText("cfg_latest_backup.cfg", configLoader.Serialize());
		
		//Events.InvokeEvent(EventNames.LoadEVarDefaults);
	}

	public string GetString(string name, string @default = "") => Get(name, @default);
	public bool GetBool(string name, bool @default = false) => Get(name, @default);
	public int GetInt(string name, int @default = 0) => Get(name, @default);
	public uint GetUInt(string name, uint @default = 0) => Get(name, @default);
	public byte GetByte(string name, byte @default = 0) => Get(name, @default);
	public sbyte GetSByte(string name, sbyte @default = 0) => Get(name, @default);
	public short GetShort(string name, short @default = 0) => Get(name, @default);
	public ushort GetUShort(string name, ushort @default = 0) => Get(name, @default);
	public long GetLong(string name, long @default = 0) => Get(name, @default);
	public ulong GetULong(string name, ulong @default = 0) => Get(name, @default);
	public float GetFloat(string name, float @default = 0) => Get(name, @default);
	public double GetDouble(string name, double @default = 0) => Get(name, @default);

	public void SetString(string name, string value) => Set(name, value);
	public void SetBool(string name, bool value) => Set(name, value);
	public void SetInt(string name, int value) => Set(name, value);
	public void SetUInt(string name, uint value) => Set(name, value);
	public void SetShort(string name, short value) => Set(name, value);
	public void SetUShort(string name, ushort value) => Set(name, value);
	public void SetByte(string name, byte value) => Set(name, value);
	public void SetSByte(string name, sbyte value) => Set(name, value);
	public void SetLong(string name, long value) => Set(name, value);
	public void SetULong(string name, ulong value) => Set(name, value);
	public void SetFloat(string name, float value) => Set(name, value);
	public void SetDouble(string name, double value) => Set(name, value);
	public void SetEnum<T>(string name, T value) where T : Enum => Set(name, value, typeof(T));
	public T GetEnum<T>(string name, T @default) where T : Enum
	{
		var entry = GetEntry(name);
		if (entry == null)
		{
			Set(name, @default, typeof(T));
			return @default;
		}
		if(!Validate(typeof(T), entry.Value.Type))
			return @default;
		return (T)entry.Value.Value;
	}


	private T Get<T>(string name, T @default)
	{
		var entry = GetEntry(name);
		if (entry == null)
		{
			Set(name, @default!);
			return @default;
		}
		if(!Validate(typeof(T), entry.Value.Type))
			return @default;
		return (T)entry.Value.Value;
	}
	
	public T? Get<T>(string name)
	{
		return Get<T>(name, default!);
	}

	public bool SetFast(string name, object value) => SetFast(name, value, null);

	public bool SetFast(string name, object value, Type? enumType)
	{
		if (value == null)
			throw new ArgumentNullException(nameof(value));
		
		EngineVarType engineVarType = GetType(value.GetType());
		EngineVarEntry? entry = GetEntry(name);
		if (entry == null)
		{
			RegisterEntry(name, engineVarType, value, enumType);
			return true;
		}

		if (entry.Value.Locked)
			return false;
		
		if(entry.Value.Type != engineVarType)
			return false;
		
		var actualEntry = entry.Value;
		actualEntry.Value = value;
		Entries[name] = actualEntry;

		return true;
	}
	
	public bool SetEntry(string name, EngineVarEntry entry)
	{
		EngineVarEntry? existing = GetEntry(name);

		if (existing != null)
		{
			if (existing.Value.Locked)
				return false;

			if (existing.Value.Type != entry.Type)
				return false;
		}

		Entries[name] = entry;

		return true;
	}

	public void Set(string name, object value) => Set(name, value, null);

	public void Set(string name, object value, Type? enumType)
	{
		if (SetFast(name, value, enumType))
		{
			//EventBus.Event(EventNames.AnyEngineVarUpdate, name);
			//EventBus.Event(EventNames.EngineVarUpdate(name), value);
		}
	}

	public bool Exists(string name)
	{
		return Entries.ContainsKey(name);
	}
	
	public bool Exists<T>(string name)
	{
		return Entries.ContainsKey(name) && GetEntry(name)!.Value.Type == GetType(typeof(T));
	}
	
	public bool Exists(string name, Type type)
	{
		return Entries.ContainsKey(name) && GetEntry(name)!.Value.Type == GetType(type);
	}
	
	public bool Exists(string name, EngineVarType type)
	{
		return Entries.ContainsKey(name) && GetEntry(name)!.Value.Type == type;
	}

	private static EngineVarType GetType(Type type)
	{
		if (type == typeof(string))
			return EngineVarType.String;
		if (type == typeof(bool))
			return EngineVarType.Bool;
		if (type == typeof(int))
			return EngineVarType.Int;
		if (type == typeof(uint))
			return EngineVarType.UInt;
		if (type == typeof(short))
			return EngineVarType.Short;
		if (type == typeof(ushort))
			return EngineVarType.UShort;
		if (type == typeof(long))
			return EngineVarType.Long;
		if (type == typeof(ulong))
			return EngineVarType.ULong;
		if (type == typeof(sbyte))
			return EngineVarType.SByte;
		if (type == typeof(byte))
			return EngineVarType.Byte;
		if (type == typeof(float))
			return EngineVarType.Float;
		if (type == typeof(double))
			return EngineVarType.Double;
		return type.IsEnum ? EngineVarType.Enum : EngineVarType.Invalid;
	}

	public void SetLocked(string entry, bool locked = true)
	{
		var e = GetEntry(entry);
		if(!e.HasValue)
			return;
		var eVar = e.Value;
		eVar.Locked = locked;

		Entries[entry] = eVar;
	}

	private bool Validate(Type type, EngineVarType eVarType)
	{
		return GetType(type) == eVarType;
	}

	private void RegisterEntry(string name, EngineVarType type, object value, Type? enumType)
	{
		if (value == null)
			throw new ArgumentNullException(nameof(value));
		
		if(Entries.ContainsKey(name))
			return;
		
		if(!Validate(value.GetType(), type))
			return;
		
		EngineVarEntry entry = new EngineVarEntry(type, name, value, enumType);
		Entries[name] = entry;
	}

	public EngineVarEntry? GetEntry(string name)
	{
		if (Entries.TryGetValue(name, out EngineVarEntry entry))
			return entry;
		return null;
	}

	public IEnumerable<string> GetEntries()
	{
		return Entries.Keys;
	}

	public struct EngineVarEntry
	{
		public readonly EngineVarType Type;
		public readonly string Name;
		public object Value;
		public readonly Type? EnumType;
		public bool Locked = false;
		
		public EngineVarEntry(EngineVarType type, string name, object value, Type? enumType)
		{
			Type = type;
			Name = name;
			Value = value;
			EnumType = enumType;
		}
	}

	public enum EngineVarType
	{
		Invalid,
		String,
		Bool,
		Int,
		UInt,
		Short,
		UShort,
		Long,
		ULong,
		Byte,
		SByte,
		Float,
		Double,
		Enum
	}


}