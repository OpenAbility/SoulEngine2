namespace SoulEngine.Data.NBT;

/// <summary>
/// Base NBT tag class
/// </summary>
public abstract class Tag
{
    /// <summary>
    /// The name of this tag
    /// </summary>
    public string? Name;

    public readonly TagType Type;

    public Tag(string? name, TagType type)
    {
        Name = name;
        Type = type;
    }

    public Tag SetName(string name)
    {
        Name = name;
        return this;
    }

    /// <summary>
    /// Read tag data
    /// </summary>
    /// <param name="reader">The reader to read from</param>
    public abstract void Read(BinaryReader reader);
    /// <summary>
    /// Write tag data
    /// </summary>
    /// <param name="writer">The writer to write to</param>
    public abstract void Write(BinaryWriter writer);

    public static Tag ReadNamedTag(BinaryReader reader)
    {
        TagType type = (TagType)reader.ReadByte();
        if (type == 0)
            return new EndTag();

        ushort nameLength = reader.ReadUInt16();
        string name = new string(reader.ReadChars(nameLength));

        Tag tag = NewTag(type, name);
        tag.Read(reader);

        return tag;
    }
    
    public static void WriteNamedTag(Tag tag, BinaryWriter writer)
    {
        writer.Write((byte)tag.Type);
        
        writer.Write((ushort)tag.Name!.Length);
        writer.Write(tag.Name.ToCharArray());
        
        tag.Write(writer);
    }

    public static Tag NewTag(TagType type, string? name)
    {
        return type switch
        {
            TagType.End => new EndTag(),
            TagType.Byte => new ByteTag(name),
            TagType.Short => new ShortTag(name),
            TagType.Int => new IntTag(name),
            TagType.Long => new LongTag(name),
            TagType.Float => new FloatTag(name),
            TagType.Double => new DoubleTag(name),
            TagType.ByteArray => new ByteArrayTag(name),
            TagType.String => new StringTag(name),
            TagType.List => new ListTag(name),
            TagType.Compound => new CompoundTag(name),
            TagType.IntArray => new IntArrayTag(name),
            TagType.LongArray => new LongArrayTag(name),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}

/// <summary>
/// Specifies tag types
/// </summary>
public enum TagType : byte
{
    End,
    Byte,
    Short,
    Int,
    Long,
    Float,
    Double,
    ByteArray,
    String,
    List,
    Compound,
    IntArray,
    LongArray
}