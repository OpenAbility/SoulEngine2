using Hexa.NET.ImGui;
using SoulEngine.Data.NBT;

namespace SoulEngine.Props;

public class StringProperty : SerializedProperty<string>
{
    public StringProperty(string name, string defaultValue) : base(name, defaultValue)
    {
    }


    public override unsafe void Edit()
    {
        ImGui.InputText(Name, ref Value, 2048);
    }


    public override Tag Save()
    {
        return new StringTag(Name, Value);
    }

    public override void Load(Tag tag)
    {
        Value = ((StringTag)tag).Data;
    }
}