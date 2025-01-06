using ImGuiNET;
using SoulEngine.Data.NBT;

namespace SoulEngine.Props;

public class LongProperty : SerializedProperty<long>
{
    public LongProperty(string name, long defaultValue) : base(name, defaultValue)
    {
    }

    
    public override unsafe void Edit()
    {
        fixed (long* ptr = &Value)
            ImGui.InputScalar(Name, ImGuiDataType.S64, (IntPtr)ptr);
    }
    

    public override Tag Save()
    {
        return new LongTag(Name, Value);
    }

    public override void Load(Tag tag)
    {
        Value = ((LongTag)tag).Data;
    }
}