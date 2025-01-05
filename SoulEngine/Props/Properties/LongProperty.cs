using ImGuiNET;
using SoulEngine.Data.NBT;

namespace SoulEngine.Props;

public class LongProperty : PropProperty<long>
{
    public LongProperty(string name, long defaultValue) : base(name, defaultValue)
    {
    }

    #if DEVELOPMENT
    public override unsafe void Edit()
    {
        fixed (long* ptr = &Value)
            ImGui.InputScalar(Name, ImGuiDataType.S64, (IntPtr)ptr);
    }
    #endif

    public override Tag Save()
    {
        return new LongTag(Name, Value);
    }

    public override void Load(Tag tag)
    {
        Value = ((LongTag)tag).Data;
    }
}