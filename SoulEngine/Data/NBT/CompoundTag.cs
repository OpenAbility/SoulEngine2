using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace SoulEngine.Data.NBT;

public class CompoundTag : Tag, IDictionary<string, Tag>, ICollection<Tag>
{
    private Dictionary<string, Tag> tags = new Dictionary<string, Tag>();

    public CompoundTag(string? name) : base(name, TagType.Compound)
    {
    }


    public override void Read(BinaryReader reader)
    {
        tags.Clear();
        Tag tag;
        while ((tag = ReadNamedTag(reader)).Type != TagType.End)
        {
            tags[tag.Name!] = tag;
        }
    }

    public override void Write(BinaryWriter writer)
    {
        foreach (var tag in tags.Values)
        {
            WriteNamedTag(tag, writer);
        }
        writer.Write((byte)TagType.End);
    }


    public void SetByte(string name, byte value)
    {
        tags[name] = new ByteTag(name, value);
    } 
    
    public void SetShort(string name, short value)
    {
        tags[name] = new ShortTag(name, value);
    } 
    
    public void SetInt(string name, int value)
    {
        tags[name] = new IntTag(name, value);
    } 
    
    public void SetLong(string name, long value)
    {
        tags[name] = new LongTag(name, value);
    } 
    
    public void SetFloat(string name, float value)
    {
        tags[name] = new FloatTag(name, value);
    } 
    
    public void SetDouble(string name, double value)
    {
        tags[name] = new DoubleTag(name, value);
    } 
    
    public void SetString(string name, string value)
    {
        tags[name] = new StringTag(name, value);
    } 
    
    public void SetByteArray(string name, byte[] value)
    {
        tags[name] = new ByteArrayTag(name, value);
    } 
    
    public void SetIntArray(string name, int[] value)
    {
        tags[name] = new IntArrayTag(name, value);
    } 
    
    public void SetLongArray(string name, long[] value)
    {
        tags[name] = new LongArrayTag(name, value);
    } 
    
    public void SetCompound(string name, CompoundTag value)
    {
        tags[name] = value.SetName(name);
    } 
    
    public void SetBool(string name, bool value)
    {
        SetByte(name, value ? (byte)1 : (byte)0);
    }

    public T? GetTag<T>(string name) where T : Tag
    {
        if (TryGetValue(name, out Tag? tag))
            return tag as T;
        return null;
    }

    public byte? GetByte(string name) => GetTag<ByteTag>(name)?.Data;
    public short? GetShort(string name) => GetTag<ShortTag>(name)?.Data;
    public int? GetInt(string name) => GetTag<IntTag>(name)?.Data;
    public long? GetLong(string name) => GetTag<LongTag>(name)?.Data;
    public float? GetFloat(string name) => GetTag<FloatTag>(name)?.Data;
    public double? GetDouble(string name) => GetTag<DoubleTag>(name)?.Data;
    public byte[]? GetByteArray(string name) => GetTag<ByteArrayTag>(name)?.Value;
    public int[]? GetIntArray(string name) => GetTag<IntArrayTag>(name)?.Value;
    public long[]? GetLongArray(string name) => GetTag<LongArrayTag>(name)?.Value;

    public bool? GetBool(string name)
    {
        byte? b = GetByte(name);
        if (b == null)
            return null;
        return b != 0;
    }
    
    
    IEnumerator<Tag> IEnumerable<Tag>.GetEnumerator() => tags.Values.GetEnumerator();

    IEnumerator<KeyValuePair<string, Tag>> IEnumerable<KeyValuePair<string, Tag>>.GetEnumerator() =>
        tags.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => tags.GetEnumerator();

    public void Add(KeyValuePair<string, Tag> item) => tags.Add(item.Key, item.Value);

    public void Add(Tag item) => tags.Add(item.Name!, item);

    void ICollection<Tag>.Clear() => Values.Clear();

    public bool Contains(Tag item) => tags.ContainsValue(item);

    public void CopyTo(Tag[] array, int arrayIndex) => tags.Values.CopyTo(array, arrayIndex);

    public bool Remove(Tag item) => tags.Remove(item.Name!);

    int ICollection<Tag>.Count => tags.Count;

    bool ICollection<Tag>.IsReadOnly => false;

    void ICollection<KeyValuePair<string, Tag>>.Clear() => tags.Clear();

    public bool Contains(KeyValuePair<string, Tag> item) => tags.Contains(item);

    public void CopyTo(KeyValuePair<string, Tag>[] array, int arrayIndex)
    {
        foreach (var kvp in tags)
            array[arrayIndex++] = kvp;
    }

    public bool Remove(KeyValuePair<string, Tag> item) => tags.Remove(item.Key);

    int ICollection<KeyValuePair<string, Tag>>.Count => tags.Count;

    bool ICollection<KeyValuePair<string, Tag>>.IsReadOnly => false;

    public void Add(string key, Tag value) => tags.Add(key, value);

    public bool ContainsKey(string key) => tags.ContainsKey(key);

    public bool Remove(string key) => tags.Remove(key);

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out Tag value) => tags.TryGetValue(key, out value);

    public Tag this[string key]
    {
        get => tags[key];
        set
        {
            tags[key] = value;
        }
    }

    public ICollection<string> Keys => tags.Keys;
    public ICollection<Tag> Values => tags.Values;
}