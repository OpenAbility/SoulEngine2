namespace SoulEngine.Data.NBT;

public class ByteArrayTag : ArrayTag<byte>
{
    public ByteArrayTag(string? name) : base(name, TagType.ByteArray)
    {
    }
    
    public ByteArrayTag(string? name, byte[] value) : base(name, TagType.ByteArray)
    {
        Value = value;
    }

    public override void Read(BinaryReader reader)
    {
        int length = reader.ReadInt32();
        Value = reader.ReadBytes(length);
    }

    public override void Write(BinaryWriter writer)
    {
        writer.Write(Value.Length);
        writer.Write(Value);
    }

    public override void Write(SNBTWriter writer)
    {
        writer.EndLine("[B; ");
        writer.Indent();
        writer.BeginLine("");

        int timeSinceLineBreak = 0;
        for (int i = 0; i < Value.Length; i++)
        {
            writer.Append(Value[i] + "b");

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