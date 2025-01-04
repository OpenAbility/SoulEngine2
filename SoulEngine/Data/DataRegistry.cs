using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAbility.Logging;
using SoulEngine.Core;
using SoulEngine.Events;


namespace SoulEngine.Data;

public class DataRegistry
{
	public static DataRegistry CreateData(EventBus<GameEvent> eventBus, string path)
	{
		if (!Directory.Exists(Path.GetDirectoryName(path)))
			Directory.CreateDirectory(Path.GetDirectoryName(path)!);
		
		if(!File.Exists(path))
			File.WriteAllText(path, "{}");

		using Stream stream = File.OpenRead(path);

		DataRegistry registry = new DataRegistry(stream);
		
		eventBus.BeginListen((gameEvent, _) =>
		{
			if (gameEvent.EventType is GameEvent.Update or GameEvent.Finalizing && registry.Dirty)
			{
				Logger.Get<DataRegistry>().Debug("Flushing {}", path);
				using Stream globReg = File.OpenWrite(path);
				registry.Save(globReg);
			}
		});

		return registry;
	}
	

	private readonly Dictionary<string, DataNode> nodes = new Dictionary<string, DataNode>();
	public bool Dirty { get; private set; } = false;
	public DataRegistry(Stream input)
	{
		JObject jObject = JObject.Parse(new StreamReader(input).ReadToEnd());
		foreach (var child in jObject)
		{
			DataNodeType nodeType = (DataNodeType)Int32.Parse(child.Key[0].ToString());

			DataNode loaded = nodeType switch
			{
				DataNodeType.Int32 => new DataNode { int32 = child.Value!.Value<int>() },
				DataNodeType.Int64 => new DataNode { int64 = child.Value!.Value<long>() },
				DataNodeType.Float => new DataNode { float32 = child.Value!.Value<float>() },
				DataNodeType.Double => new DataNode { double64 = child.Value!.Value<double>() },
				DataNodeType.String => new DataNode { stringValue = child.Value!.Value<string>()! },
				DataNodeType.Blob => new DataNode { blob = Convert.FromBase64String(child.Value!.Value<string>()!) },
				_ => throw new ArgumentOutOfRangeException()
			};
			loaded.NodeType = nodeType;
			
			nodes.Add(child.Key[1..], loaded);
		}
	}

	public void Save(Stream output)
	{
		using JsonWriter writer = new JsonTextWriter(new StreamWriter(output));

		writer.Formatting = Formatting.Indented;
		writer.WriteStartObject();

		foreach (var node in nodes)
		{
			DataNodeType nodeType = node.Value.NodeType;
			
			writer.WritePropertyName((int)nodeType + node.Key);

			switch (nodeType)
			{
				case DataNodeType.Int32:
					writer.WriteValue(node.Value.int32);
					break;
				case DataNodeType.Int64:
					writer.WriteValue(node.Value.int64);
					break;
				case DataNodeType.Float:
					writer.WriteValue(node.Value.float32);
					break;
				case DataNodeType.Double:
					writer.WriteValue(node.Value.double64);
					break;
				case DataNodeType.String:
					writer.WriteValue(node.Value.stringValue);
					break;
				case DataNodeType.Blob:
					writer.WriteValue(Convert.ToBase64String(node.Value.blob));
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			
		}
		
		writer.WriteEndObject();
		writer.Flush();
		Dirty = false;
		

	}

	public bool Exists(string name)
	{
		return nodes.ContainsKey(name);
	}

	public DataNodeType GetType(string name)
	{
		return nodes[name].NodeType;
	}

	private DataNode VerifyType(string name, DataNodeType type)
	{
		DataNode node = nodes[name];
		if (node.NodeType != type)
			throw new Exception("Node is not of correct type!");
		return node;
	}
	
	private DataNode VerifyTypeS(string name, DataNodeType type)
	{
		if (!Exists(name))
		{
			DataNode newNode = new DataNode();
			newNode.NodeType = type;
			nodes[name] = newNode;
			return newNode;
		}
		
		DataNode node = nodes[name];
		if (node.NodeType != type)
			throw new Exception("Node is not of correct type!");
		return node;
	}
	
	public int GetInt32(string name)
	{
		return VerifyType(name, DataNodeType.Int32).int32;
	}
	
	public long GetInt64(string name)
	{
		return VerifyType(name, DataNodeType.Int64).int64;
	}
	
	public float GetFloat(string name)
	{
		return VerifyType(name, DataNodeType.Float).float32;
	}
	
	public double GetDouble(string name)
	{
		return VerifyType(name, DataNodeType.Double).double64;
	}
	
	public string GetString(string name)
	{
		return VerifyType(name, DataNodeType.String).stringValue;
	}
	
	public byte[] GetBlob(string name)
	{
		return VerifyType(name, DataNodeType.Blob).blob;
	}
	
	public DataRegistry SetInt32(string name, int value)
	{
		VerifyTypeS(name, DataNodeType.Int32).int32 = value;
		Dirty = true;
		return this;
	}
	
	public DataRegistry SetInt64(string name, long value)
	{
		VerifyTypeS(name, DataNodeType.Int64).int64 = value;
		Dirty = true;
		return this;
	}
	
	public DataRegistry SetFloat(string name, float value)
	{
		VerifyTypeS(name, DataNodeType.Float).float32 = value;
		Dirty = true;
		return this;
	}
	
	public DataRegistry SetDouble(string name, double value)
	{
		VerifyTypeS(name, DataNodeType.Double).double64 = value;
		Dirty = true;
		return this;
	}
	
	public DataRegistry SetString(string name, string value)
	{
		VerifyTypeS(name, DataNodeType.String).stringValue = value;
		Dirty = true;
		return this;
	}
	
	public DataRegistry SetBlob(string name, byte[] value)
	{
		VerifyTypeS(name, DataNodeType.Blob).blob = value;
		Dirty = true;
		return this;
	}


	private class DataNode
	{
		public DataNodeType NodeType;
		public int int32;
		public long int64;
		public float float32;
		public double double64;
		public string stringValue;
		public byte[] blob;
	}
	
}

public enum DataNodeType
{
	Int32 = 1,
	Int64 = 2,
	Float = 3,
	Double = 4,
	String = 5,
	Blob = 6
}