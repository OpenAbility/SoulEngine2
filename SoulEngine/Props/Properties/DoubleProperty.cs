using ImGuiNET;
using SoulEngine.Data.NBT;

namespace SoulEngine.Props;

public class DoubleProperty : PropProperty<double>
{
    public DoubleProperty(string name, double defaultValue) : base(name, defaultValue)
    {
    }

#if DEVELOPMENT
    public override unsafe void Edit()
    {
        fixed (void* ptr = &Value)
            ImGui.InputScalar(Name, ImGuiDataType.Double, (IntPtr)ptr);
    }
#endif

    public override Tag Save()
    {
        return new DoubleTag(Name, Value);
    }

    public override void Load(Tag tag)
    {
        Value = ((DoubleTag)tag).Data;
    }
}