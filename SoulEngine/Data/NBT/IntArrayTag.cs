namespace SoulEngine.Data.NBT;

public class IntArrayTag : ArrayTag<int>
{
    public IntArrayTag(string? name) : base(name, TagType.IntArray)
    {
    }
    
    public IntArrayTag(string? name, int[] value) : base(name, TagType.IntArray)
    {
        Value = value;
    }

    public override void Read(BinaryReader reader)
    {
        int length = reader.ReadInt32();
        Value = new int[length];
        for (int i = 0; i < length; i++)
            Value[i] = reader.ReadInt32();
    }

    public override void Write(BinaryWriter writer)
    {
        writer.Write(Value.Length);
        for (int i = 0; i < Value.Length; i++)
            writer.Write(Value[i]);
    }

    public override void Write(SNBTWriter writer)
    {
        writer.EndLine("[I; ");
        writer.Indent();
        writer.BeginLine("");

        int timeSinceLineBreak = 0;
        for (int i = 0; i < Value.Length; i++)
        {
            writer.Append(Value[i].ToString());

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