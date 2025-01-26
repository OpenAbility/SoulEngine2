namespace SoulEngine.Data.NBT;

public class LongArrayTag : ArrayTag<long>
{
    public LongArrayTag(string? name) : base(name, TagType.LongArray)
    {
    }
    
    public LongArrayTag(string? name, long[] value) : base(name, TagType.LongArray)
    {
        Value = value;
    }

    public override void Read(BinaryReader reader)
    {
        int length = reader.ReadInt32();
        Value = new long[length];
        for (int i = 0; i < length; i++)
            Value[i] = reader.ReadInt64();
    }

    public override void Write(BinaryWriter writer)
    {
        writer.Write(Value.Length);
        for (int i = 0; i < Value.Length; i++)
            writer.Write(Value[i]);
    }

    public override void Write(SNBTWriter writer)
    {
        writer.EndLine("[L; ");
        writer.Indent();
        writer.BeginLine("");

        int timeSinceLineBreak = 0;
        for (int i = 0; i < Value.Length; i++)
        {
            writer.Append(Value[i] + "l");

            if (i != Value.Length - 1)
                writer.Append(", ");

            timeSinceLineBreak++;

            if (timeSinceLineBreak >= 32)
            {
                timeSinceLineBreak = 0;
                writer.EndLine("").BeginLine("");
            }
        }

        writer.PopIndent();
        
        writer.EndLine("").BeginLine("]");
    }
}