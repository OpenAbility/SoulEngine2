using System.Collections;
using System.Numerics;

namespace SoulEngine.Data.NBT;

public abstract class ArrayTag<T> : Tag, IReadOnlyList<T> where T : unmanaged, INumber<T>
{

    public T[] Value;
    
    protected ArrayTag(string? name, TagType type) : base(name, type)
    {
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < Value.Length; i++)
            yield return Value[i];
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => Value.Length;

    public T this[int index] => Value[index];
}