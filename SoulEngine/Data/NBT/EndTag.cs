namespace SoulEngine.Data.NBT;

public class EndTag : Tag
{
    public EndTag() : base(null, TagType.End)
    {
    }

    public override void Read(BinaryReader reader)
    {
        throw new NotImplementedException();
    }

    public override void Write(BinaryWriter writer)
    {
        throw new NotImplementedException();
    }

    public override void Write(SNBTWriter writer)
    {
        throw new NotImplementedException();
    }
}