using System.Collections;
using System.Runtime.CompilerServices;

namespace SoulEngine.Data.NBT;

public class ListTag : Tag, IList<Tag>
{
    public TagType Type { get; private set; }

    private List<Tag> tags = new List<Tag>();
    
    public ListTag(string? name) : base(name, TagType.List)
    {
    }

    public override void Read(BinaryReader reader)
    {
        Type = (TagType)reader.ReadByte();

        int size = reader.ReadInt32();
        for (int i = 0; i < size; i++)
        {
            Tag tag = NewTag(Type, null);
            tag.Read(reader);
            tags.Add(tag);
        }
    }

    public override void Write(BinaryWriter writer)
    {
        if (tags.Count > 0) Type = tags[0].Type;
        else Type = TagType.Byte;
        
        writer.Write((byte)Type);
        writer.Write(tags.Count);
        for (int i = 0; i < tags.Count; i++)
            tags[i].Write(writer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Tag ValidateTag(Tag tag)
    {
        if (tags.Count == 0)
        {
            Type = tag.Type;
        }

        if (tag.Type != Type)
            throw new Exception("List type mismatch!");
        return tag;
    }

    public IEnumerator<Tag> GetEnumerator() => tags.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => tags.GetEnumerator();

    public void Add(Tag item) => tags.Add(ValidateTag(item));

    public void Clear() => tags.Clear();

    public bool Contains(Tag item) => tags.Contains(item);

    public void CopyTo(Tag[] array, int arrayIndex) => tags.CopyTo(array, arrayIndex);

    public bool Remove(Tag item) => tags.Remove(item);

    public int Count => tags.Count;
    
    public bool IsReadOnly => false;
    
    public int IndexOf(Tag item) => tags.IndexOf(item);

    public void Insert(int index, Tag item) => tags.Insert(index, item);

    public void RemoveAt(int index) => tags.RemoveAt(index);

    public Tag this[int index]
    {
        get => tags[index];
        set
        {
            tags[index] = value;
        }
    }
}