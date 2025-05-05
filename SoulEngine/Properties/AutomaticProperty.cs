using System.Reflection;
using SoulEngine.Core;
using SoulEngine.Data;
using SoulEngine.Data.NBT;
using SoulEngine.Inspection;
using SoulEngine.Util;

namespace SoulEngine.Props;

public class AutomaticProperty : SerializedProperty
{
    private readonly MemberWrapper value;
    private readonly object target;
    private readonly object? defaultValue;
    private object? resetValue;
    private readonly Scene scene;

    public AutomaticProperty(Scene scene, string name, MemberWrapper wrapper, object target) : base(name)
    {
        value = wrapper;
        this.target = target;
        this.scene = scene;
        
        defaultValue = value.GetValue(target);
        resetValue = defaultValue;
    }

    public override void Edit()
    {
        object? current = value.GetValue(target);
        current = Inspector.Inspect(scene, value.MemberType, current, Name, out bool edited);

        if (edited)
        {
            value.SetValue(target, current);
        }
    }

    public override void MakeCurrentReset()
    {
        resetValue = value.GetValue(target);
    }

    public override void Reset()
    {
       value.SetValue(target, resetValue);
    }

    public override Tag Save()
    {
        object? current = value.GetValue(target);
        if (current == null)
            return new StringTag(Name, "$@!INVALID");
        
        Tag? tag = NBTSerialization.Serialize(current, scene);
        if (tag == null)
            return new StringTag(Name, "$@!INVALID");

        return tag;
    }

    public override void Load(Tag tag)
    {
        if (tag is StringTag { Data: "$@!INVALID" })
        {
            value.SetValue(target, defaultValue);
            return;
        }
        
        value.SetValue(target, NBTSerialization.Deserialize(tag, value.MemberType, scene));
    }
}