using ImGuiNET;
using SoulEngine.Data.NBT;

namespace SoulEngine.Props;

public class ByteProperty : PropProperty<byte>
{
    public ByteProperty(string name, byte defaultValue) : base(name, defaultValue)
    {
    }

#if DEVELOPMENT
    public override unsafe void Edit()
    {
        fixed (void* ptr = &Value)
            ImGui.InputScalar(Name, ImGuiDataType.U8, (IntPtr)ptr);
    }
#endif

    public override Tag Save()
    {
        return new ByteTag(Name, Value);
    }

    public override void Load(Tag tag)
    {
        Value = ((ByteTag)tag).Data;
    }
}