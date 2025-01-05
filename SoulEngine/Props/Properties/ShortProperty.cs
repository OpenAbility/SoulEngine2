using ImGuiNET;
using SoulEngine.Data.NBT;

namespace SoulEngine.Props;

public class ShortProperty : PropProperty<short>
{
    public ShortProperty(string name, short defaultValue) : base(name, defaultValue)
    {
    }

#if DEVELOPMENT
    public override unsafe void Edit()
    {
        fixed (void* ptr = &Value)
            ImGui.InputScalar(Name, ImGuiDataType.S16, (IntPtr)ptr);
    }
#endif

    public override Tag Save()
    {
        return new ShortTag(Name, Value);
    }

    public override void Load(Tag tag)
    {
        Value = ((ShortTag)tag).Data;
    }
}