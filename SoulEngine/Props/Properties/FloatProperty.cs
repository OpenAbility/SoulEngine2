using ImGuiNET;
using SoulEngine.Data.NBT;

namespace SoulEngine.Props;

public class FloatProperty : SerializedProperty<float>
{
    public FloatProperty(string name, float defaultValue) : base(name, defaultValue)
    {
    }


    public override unsafe void Edit()
    {
        fixed (void* ptr = &Value)
            ImGui.InputScalar(Name, ImGuiDataType.Float, (IntPtr)ptr);
    }


    public override Tag Save()
    {
        return new FloatTag(Name, Value);
    }

    public override void Load(Tag tag)
    {
        Value = ((FloatTag)tag).Data;
    }
}