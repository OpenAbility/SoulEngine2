using Hexa.NET.ImGui;
using SoulEngine.Data.NBT;

namespace SoulEngine.Props;

public class ShortProperty : SerializedProperty<short>
{
    public ShortProperty(string name, short defaultValue) : base(name, defaultValue)
    {
    }


    public override unsafe void Edit()
    {
        fixed (void* ptr = &Value)
            ImGui.InputScalar(Name, ImGuiDataType.S16, ptr);
    }


    public override Tag Save()
    {
        return new ShortTag(Name, Value);
    }

    public override void Load(Tag tag)
    {
        Value = ((ShortTag)tag).Data;
    }
}