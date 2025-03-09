using Hexa.NET.ImGui;
using SoulEngine.Data.NBT;

namespace SoulEngine.Props;

public class IntProperty : SerializedProperty<int>
{
    public IntProperty(string name, int defaultValue) : base(name, defaultValue)
    {
    }


    public override unsafe void Edit()
    {
        fixed (void* ptr = &Value)
            ImGui.InputScalar(Name, ImGuiDataType.S32, ptr);
    }


    public override Tag Save()
    {
        return new IntTag(Name, Value);
    }

    public override void Load(Tag tag)
    {
        Value = ((IntTag)tag).Data;
    }
}