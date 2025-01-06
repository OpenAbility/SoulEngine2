using ImGuiNET;
using SoulEngine.Data.NBT;

namespace SoulEngine.Props;

public class BoolProperty : SerializedProperty<bool>
{
    public BoolProperty(string name, bool defaultValue) : base(name, defaultValue)
    {
    }


    public override unsafe void Edit()
    {
        fixed (void* ptr = &Value)
            ImGui.InputScalar(Name, ImGuiDataType.U8, (IntPtr)ptr);
    }


    public override Tag Save()
    {
        return new ByteTag(Name, Value ? (byte)1 : (byte)0);
    }

    public override void Load(Tag tag)
    {
        Value = ((ByteTag)tag).Data > 0;
    }
}