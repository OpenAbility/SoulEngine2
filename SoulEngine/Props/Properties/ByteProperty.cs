using ImGuiNET;
using SoulEngine.Data.NBT;

namespace SoulEngine.Props;

public class ByteProperty : SerializedProperty<byte>
{
    public ByteProperty(string name, byte defaultValue) : base(name, defaultValue)
    {
    }


    public override unsafe void Edit()
    {
        fixed (void* ptr = &Value)
            ImGui.InputScalar(Name, ImGuiDataType.U8, (IntPtr)ptr);
    }


    public override Tag Save()
    {
        return new ByteTag(Name, Value);
    }

    public override void Load(Tag tag)
    {
        Value = ((ByteTag)tag).Data;
    }
}