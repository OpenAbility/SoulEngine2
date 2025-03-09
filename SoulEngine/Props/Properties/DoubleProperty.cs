using Hexa.NET.ImGui;
using SoulEngine.Data.NBT;

namespace SoulEngine.Props;

public class DoubleProperty : SerializedProperty<double>
{
    public DoubleProperty(string name, double defaultValue) : base(name, defaultValue)
    {
    }


    public override unsafe void Edit()
    {
        fixed (void* ptr = &Value)
            ImGui.InputScalar(Name, ImGuiDataType.Double, ptr);
    }


    public override Tag Save()
    {
        return new DoubleTag(Name, Value);
    }

    public override void Load(Tag tag)
    {
        Value = ((DoubleTag)tag).Data;
    }
}